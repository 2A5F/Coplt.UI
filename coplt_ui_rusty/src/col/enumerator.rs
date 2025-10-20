use std::fmt::Debug;

pub trait Enumerator {
    type Item;

    fn move_next(&mut self) -> bool;
    fn current(&self) -> Self::Item;
}

pub trait EnumeratorIter {
    type Iterator: Iterator;

    fn iter(self) -> Self::Iterator;
}

pub mod iter {
    use super::*;

    #[derive(Debug)]
    pub struct EnumeratorIterator<T>(T);

    impl<T: Enumerator> Iterator for EnumeratorIterator<T> {
        type Item = <T as Enumerator>::Item;

        fn next(&mut self) -> Option<Self::Item> {
            if self.0.move_next() {
                Some(self.0.current())
            } else {
                None
            }
        }
    }

    impl<T: Enumerator> EnumeratorIter for T {
        type Iterator = EnumeratorIterator<T>;

        fn iter(self) -> Self::Iterator {
            EnumeratorIterator(self)
        }
    }
}
