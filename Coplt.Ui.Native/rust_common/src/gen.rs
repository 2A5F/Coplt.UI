#![allow(unused)]

use std::{
    marker::PhantomData,
    pin::{self, Pin},
    task::{Context, RawWaker, RawWakerVTable, Waker},
};

pub trait IterableGenerator<'a> {
    type Yield;
    type Return;

    /// This is safe because this cannot be called twice.  
    /// The first call will make the future, then the future while always pinned.  
    ///
    /// ```compile_fail
    /// let mut a = a_gen(async |ctx: AGen<_>| {
    ///     for i in 0..10 {
    ///         ctx.Yield(i).await;
    ///     }
    /// });
    /// a.iter();
    /// a.iter(); // cannot borrow `a` as mutable more than once at a time
    /// ```
    ///
    fn iter(&'a mut self) -> impl Iterator<Item = Self::Yield>;
    fn coroutine(self) -> impl Coroutine<Yield = Self::Yield, Return = Self::Return>;
}

#[derive(Debug, Clone, Copy)]
pub struct Generator<'a, C>(C, PhantomData<&'a mut ()>);

impl<'a, C: Coroutine> IterableGenerator<'a> for Generator<'a, C> {
    type Yield = C::Yield;
    type Return = C::Return;

    fn iter(&'a mut self) -> impl Iterator<Item = Self::Yield> {
        unsafe { Pin::new_unchecked(&mut self.0) }.to_iter()
    }

    fn coroutine(self) -> impl Coroutine<Yield = Self::Yield, Return = Self::Return> {
        self.0
    }
}

impl<'a, C: Coroutine<R>, R> Coroutine<R> for Generator<'a, C> {
    type Yield = C::Yield;
    type Return = C::Return;

    fn resume(self: Pin<&mut Self>, arg: R) -> CoroutineState<Self::Yield, Self::Return> {
        let c = unsafe { self.map_unchecked_mut(|a| &mut a.0) };
        c.resume(arg)
    }
}

pub trait Coroutine<R = ()> {
    type Yield;
    type Return;

    fn resume(self: Pin<&mut Self>, arg: R) -> CoroutineState<Self::Yield, Self::Return>;
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub enum CoroutineState<Y, R> {
    Yielded(Y),
    Complete(R),
}

#[derive(Debug)]
pub struct AGen<T: Unpin, R: Unpin = ()>(PhantomData<(*mut T, *mut R)>);

impl<T: Unpin, R: Unpin> AGen<T, R> {
    #[allow(non_snake_case)]
    pub fn Yield(&self, val: T) -> impl Future {
        AGenYield::<T, R>::Input(val)
    }
}

enum AGenYield<T, R> {
    None(PhantomData<R>),
    Input(T),
}

impl<T: Unpin, R: Unpin> Future for AGenYield<T, R> {
    type Output = R;

    fn poll(self: Pin<&mut Self>, cx: &mut Context<'_>) -> std::task::Poll<Self::Output> {
        match core::mem::replace(self.get_mut(), AGenYield::None(PhantomData)) {
            AGenYield::None(_) => {
                let state = cx.waker().data() as *mut AGenYeildState<T, R>;
                let r = unsafe { std::ptr::replace(state, AGenYeildState::None) };
                debug_assert!(matches!(r, AGenYeildState::Input(_)));
                match r {
                    AGenYeildState::Input(r) => std::task::Poll::Ready(r),
                    _ => unreachable!(),
                }
            }
            AGenYield::Input(v) => {
                let state = cx.waker().data() as *mut AGenYeildState<T, R>;
                debug_assert!(matches!(unsafe { &*state }, AGenYeildState::None));
                unsafe { std::ptr::replace(state, AGenYeildState::Output(v)) };
                std::task::Poll::Pending
            }
        }
    }
}

#[derive(Debug)]
enum AGenYeildState<T, YR> {
    None,
    Output(T),
    Input(YR),
}

#[derive(Debug)]
enum AGenImpl<T, F, Fu: Future, YR = ()> {
    None(F),
    Running(Fu, AGenYeildState<T, YR>),
    End,
}

impl<T: Unpin, YR: Unpin, R: Unpin, F, Fu> Coroutine<YR> for AGenImpl<T, F, Fu, YR>
where
    F: for<'a> FnOnce(AGen<T, YR>) -> Fu,
    Fu: Future<Output = R>,
{
    type Yield = T;
    type Return = Fu::Output;

    fn resume(self: Pin<&mut Self>, arg: YR) -> CoroutineState<Self::Yield, Self::Return> {
        unsafe {
            const VTBL: RawWakerVTable = RawWakerVTable::new(clone, wake, wake_by_ref, drop);
            fn clone(_: *const ()) -> RawWaker {
                panic!("not support clone")
            }
            fn drop(_: *const ()) {}
            fn wake(_: *const ()) {
                panic!("not support wake")
            }
            fn wake_by_ref(_: *const ()) {
                panic!("not support wake")
            }

            let this = self.get_unchecked_mut();
            debug_assert!(!matches!(this, AGenImpl::End));
            if let AGenImpl::None(f) = this {
                let f = (f as *mut F).read();
                let fu = f(AGen(PhantomData));
                (this as *mut Self).write(Self::Running(fu, AGenYeildState::None));
            }
            if let Self::Running(fu, state) = this {
                let waker = Waker::new(state as *mut _ as _, &VTBL);
                let mut cx = Context::from_waker(&waker);

                loop {
                    match Pin::new_unchecked(&mut *fu).poll(&mut cx) {
                        std::task::Poll::Ready(r) => {
                            (this as *mut Self).replace(Self::End);
                            return CoroutineState::Complete(r);
                        }
                        std::task::Poll::Pending => {
                            debug_assert!(!matches!(state, AGenYeildState::Input(_)));
                            match state {
                                AGenYeildState::None => continue,
                                AGenYeildState::Output(v) => {
                                    let v = (v as *mut T).read();
                                    (state as *mut AGenYeildState<T, YR>)
                                        .write(AGenYeildState::Input(arg));
                                    return CoroutineState::Yielded(v);
                                }
                                AGenYeildState::Input(_) => unreachable!(),
                            }
                        }
                    }
                }
            } else {
                unreachable!()
            }
        }
    }
}

pub fn a_gen<'a, T: Unpin, R: Unpin, YR: Unpin, F, Fu>(
    f: F,
) -> Generator<'a, impl Coroutine<YR, Yield = T, Return = R>>
where
    F: FnOnce(AGen<T, YR>) -> Fu,
    Fu: Future<Output = R>,
{
    Generator(AGenImpl::None(f), PhantomData)
}

#[derive(Debug)]
pub struct GeneratorIter<'a, G>(Option<Pin<&'a mut G>>);

impl<'a, G> Iterator for GeneratorIter<'a, G>
where
    G: Coroutine,
{
    type Item = G::Yield;

    fn next(&mut self) -> Option<Self::Item> {
        unsafe {
            match &mut self.0 {
                Some(this) => match this.as_mut().resume(()) {
                    CoroutineState::Yielded(v) => Some(v),
                    CoroutineState::Complete(_) => {
                        (self as *mut Self).replace(Self(None));
                        None
                    }
                },
                None => None,
            }
        }
    }
}

pub trait GeneratorToIter<T> {
    fn to_iter(self: Pin<&mut Self>) -> impl Iterator<Item = T>;
}

impl<T, G> GeneratorToIter<T> for G
where
    G: Coroutine<Yield = T>,
{
    fn to_iter(self: Pin<&mut Self>) -> impl Iterator<Item = T> {
        GeneratorIter(Some(self))
    }
}

#[test]
fn test_a_gen() {
    let v: Vec<_> = a_gen(async |ctx: AGen<_>| {
        for i in 0..10 {
            ctx.Yield(i).await;
        }
    })
    .iter()
    .collect();
    println!("{v:?}");
    assert_eq!(v, [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
}
