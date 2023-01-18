pub type Predicate<T> = fn(v: &T) -> bool;

pub trait Entity<T> {
    fn new<'t>(&self, name: String, family: String) -> T;
    fn add<'t>(&mut self, entity: &'t T) -> Option<&'t T>;
    fn update<'t>(entity: &'t T) -> Option<bool>;
    fn remove(id: i32) -> Option<bool>;
    fn list() -> Option<&'static [T]>;
    fn find<'t>(id: i32) -> Result<&'t T, bool>;
    fn filter(predicate: Predicate<T>) -> Option<&'static [T]>;
}
