# Property-Bindings

Idea is taken from https://github.com/praeclarum/Bind.
Original code suffered from memory leaks, performance issues and not supporting property paths with NULL values.

# Performance

Creating the binding is the slowest part, because it uses reflection and compilation of expression trees
```
for (int i = 0; i < 1000; i++)
{
    bind = Binding.Create(() => left.Name, () => right.Name);
}
```
For 1000 Binding.Create it takes 370ms on Intel i7-4710HQ. This can get slower if you have long property paths like this, because
more expressions will be compiled

```
Binding.Create(() => left.Property1.Property2.Property3.Name, () => right.Property1.Property2.Property3.Name);
```

Property updates are fast enough, just 3x slower that manual property updates

```C#
string[] values = new string[1000];
var binding = Binding.Create(() => left.Name, () => right.Name);
...

// this is 1x
for (int i = 0; i < 1000; i++)
{
   left.Name = values[i];
   right.Name = left.Name;
}

// this is 3x
for (int i = 0; i < 1000; i++)
{
    left.Name = values[i];
}

```
# Version 1.0
  * Optimization - Use FastExpressionCompiler to speed up Lambda.Compile
  * Optimization - Do not use Lambda.Compile for Constant Expressions
# Version 0.2
  * You can now have working bindings if there is null value in property path. Example: 
  Binding.Create(() => instance1.Test.Name, () => instance2.Test.Name) - that will work even if intance1.Test is null 
# Version 0.1

  * New syntax - PropertyBinding.Create(() => obj1.Property1.Property2.Name, () => obj2.Property1.Property2.Name)
  * Use of ConditionalWeakTable to hold WeakReferences instead of hard references - this way if your binding instance go out of scope the left and the right side instances will be eligble for GC and they won't leak
  * Ability to bind with property path PropertyBinding.Create(() => obj1.Property1.Property2.Name, () => obj2.Property1.Property2.Name) where some properties in the path can be null. Original code doens't support that.
  * Bug fix for memory leak when property subscriptions was not removed from the static dictionary on unsubscribe
  * Added Unit tests
