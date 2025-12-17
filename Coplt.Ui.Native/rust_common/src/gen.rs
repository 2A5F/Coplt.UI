#![allow(unused)]

use std::{
    marker::PhantomData,
    pin::{self, Pin},
    task::{Context, RawWaker, RawWakerVTable, Waker},
};

pub trait Generator<R = ()> {
    type Yield;
    type Return;

    fn resume(self: Pin<&mut Self>, arg: R) -> GeneratorState<Self::Yield, Self::Return>;
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub enum GeneratorState<Y, R> {
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

impl<T: Unpin, YR: Unpin, R: Unpin, F, Fu> Generator<YR> for AGenImpl<T, F, Fu, YR>
where
    F: for<'a> FnOnce(AGen<T, YR>) -> Fu,
    Fu: Future<Output = R>,
{
    type Yield = T;
    type Return = Fu::Output;

    fn resume(self: Pin<&mut Self>, arg: YR) -> GeneratorState<Self::Yield, Self::Return> {
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
                            return GeneratorState::Complete(r);
                        }
                        std::task::Poll::Pending => {
                            debug_assert!(!matches!(state, AGenYeildState::Input(_)));
                            match state {
                                AGenYeildState::None => continue,
                                AGenYeildState::Output(v) => {
                                    let v = (v as *mut T).read();
                                    (state as *mut AGenYeildState<T, YR>)
                                        .write(AGenYeildState::Input(arg));
                                    return GeneratorState::Yielded(v);
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

pub fn a_gen<T: Unpin, R: Unpin, YR: Unpin, F, Fu>(
    f: F,
) -> impl Generator<YR, Yield = T, Return = R>
where
    F: FnOnce(AGen<T, YR>) -> Fu,
    Fu: Future<Output = R>,
{
    AGenImpl::None(f)
}

#[derive(Debug)]
pub struct GeneratorIter<G>(Option<G>);

impl<G> Iterator for GeneratorIter<G>
where
    G: Generator,
{
    type Item = G::Yield;

    fn next(&mut self) -> Option<Self::Item> {
        unsafe {
            match &mut self.0 {
                Some(this) => {
                    let this = Pin::new_unchecked(this);
                    match this.resume(()) {
                        GeneratorState::Yielded(v) => Some(v),
                        GeneratorState::Complete(_) => {
                            (self as *mut Self).replace(Self(None));
                            None
                        }
                    }
                }
                None => None,
            }
        }
    }
}

pub trait MakeGeneratorIter<T> {
    fn to_iter(self) -> impl Iterator<Item = T>;
}

impl<T, G> MakeGeneratorIter<T> for G
where
    G: Generator<Yield = T>,
{
    fn to_iter(self) -> impl Iterator<Item = T> {
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
    .to_iter()
    .collect();
    println!("{v:?}");
    assert_eq!(v, [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
}
