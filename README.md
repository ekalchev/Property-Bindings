# Property-Bindings

Idea is taken from https://github.com/praeclarum/Bind but with heavy modifications and bug fixes

# Version 0.2
  * You can now have working bindings if there is null value in property path. Example: 
  Binding.Create(() => instance1.Test.Name, () => instance2.Test.Name) - that will work even if intance1.Test is null 
# Version 0.1

  * New syntax - PropertyBinding.Create(() => obj1.Property1.Property2.Name, () => obj2.Property1.Property2.Name)
  * Use of ConditionalWeakTable to hold WeakReferences instead of hard references - this way if your binding instance go out of scope the left and the right side instances will be eligble for GC and they won't leak
  * Ability to bind with property path PropertyBinding.Create(() => obj1.Property1.Property2.Name, () => obj2.Property1.Property2.Name) where some properties in the path can be null. Original code doens't support that.
  * Bug fix for memory leak when property subscriptions was not removed from the static dictionary on unsubscribe
  * Added Unit tests
