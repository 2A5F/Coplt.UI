use std::{
    marker::PhantomData,
    pin::{Pin, pin},
    task::{Context, RawWaker, RawWakerVTable, Waker},
};

pub fn a_gen<T>(f: impl AsyncFnOnce(AGen<T>) -> Option<()>) -> impl Iterator<Item = T> {
    AGenIter {
        yield_value: Option::<T>::None,
        fu: f(AGen(PhantomData)),
    }
}

pub struct AGen<T>(PhantomData<*mut T>);

impl<T> AGen<T> {
    #[allow(non_snake_case)]
    pub fn Yield(&self, val: T) -> impl Future {
        AGenYield(val)
    }
}

pub struct AGenYield<T>(T);

impl<T> Future for AGenYield<T> {
    type Output = ();

    fn poll(self: Pin<&mut Self>, cx: &mut Context<'_>) -> std::task::Poll<Self::Output> {
        let iter = cx.waker().data() as *mut Option<T>;
        std::task::Poll::Pending
    }
}

pub struct AGenIter<T, F: Future> {
    yield_value: Option<T>,
    fu: F,
}

impl<T, F: Future> Iterator for AGenIter<T, F> {
    type Item = T;

    fn next(&mut self) -> Option<Self::Item> {
        unsafe {
            const VTBL: RawWakerVTable = RawWakerVTable::new(clone, wake, wake_by_ref, drop);
            fn clone(p: *const ()) -> RawWaker {
                panic!("not support clone")
            }
            fn drop(_: *const ()) {}
            fn wake(_: *const ()) {
                panic!("not support wake")
            }
            fn wake_by_ref(_: *const ()) {
                panic!("not support wake")
            }

            let waker = Waker::new((&mut self.yield_value) as *mut _ as _, &VTBL);
            let mut cx = Context::from_waker(&waker);

            while let std::task::Poll::Pending = Pin::new_unchecked(&mut self.fu).poll(&mut cx) {
                if let Some(v) = self.yield_value.take() {
                    return Some(v);
                }
            }
        }
        None
    }
}

// #[test]
// fn test_a_gen() {
//     let v: Vec<i32> = a_gen(async |ctx| {
//         for i in 0..10 {
//             ctx.Yield(i).await;
//         }
//         Some(())
//     })
//     .collect();
//     println!("{v:?}");
// }
