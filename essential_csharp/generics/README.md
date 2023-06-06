# What generics in C# are?

For examples, see `./MyGenericClass/Program.cs`

To facilitate code reuse, especially the reuse of algorithms, C# includes a feature called generics. **Generics in C# is a feature that allows the creation of reusable, type-safe code components by parameterizing types.** Just as methods are powerful because they can take arguments, so types and methods that take type arguments have significantly more functionality.

## When I want to use generics?
Generally, you use generics when you don't want to be dependent on only 1 type in your data structure or function. In other words, you want to generalize a its behaviour for different types. Consider that you have some stack. It would be more useful to have an ability to put more than 1 type into this stack: `ints`, `strings` etc. (However, sometimes you want to put only several types to this stack. Constraints (see below) are the way to solve this problem.)

## Is there sth similar in other languages?
Generics are lexically similar to generic types in Java and templates in C++.

## Is there alternatives to Generics?
Yes.
1) You can declare functions for an `object` type. Consider you want to implement a stack.
```
class Stack
{
    private object[] _items;

    public void Push(object item) {
        // push to _items
    }
}
```
However, it will complicate several things. Firstly, it doesn't validate type it saves. Using `object` will force you to (a) handle possible type exceptions (since you'll be able to put anything into stack, while poping you should cast a returned value. it can lead to exceptions), (b) deal with boxing/unboxing of elements both when pushing and poping them.

2) You can declare your own stack class, i.e. 
```
public class CellStack
{
    public virtual Cell Pop();
    public virtual void Push(Cell cell);
}
```
However, are you really eager to define `CellStack.Push()` and `CellStack.Pop()`?

Generic classes are a solution Example of the implementation of a generic stack see in `./MyGenericClass/Program.cs`. 

## Where can I use Generics?

You can use Generics for defining classses, interface and functions. Generic 

## What is `default(T)` and when use it?

`default(T)` allows you to return the default value of the type. For example, continuing our example with a stack, when you pop from an empty stack, you can want to return a `default(T)` (instead of throwing an exception).

## How to use multiple generic types?

See `./MyGenericClass/Program.cs` 

## How can I constraint a type in generics? When, for example, I want only similar objects be putted in my generic class?

Imagine you have a generic implementation queue that actually implements a queue of people in some market. You don't want to add anything but people in this line. You can either add some validation of recieved type for `Push()` function, or use generic constraints.

```
class Queue<T> 
    where T : IPerson
{
    ...
}
```

You can constraint to different objects:

1) `where T : [some interface]`

2) `where T : [some class]`. Meaning that a given object should be an instance of this class or inherited from it.
3) `where T : class`. 
Meaning that a given object should be a reference type
4) `where T : struct`. 

## Can I declare a generic method within a non-generic class?

Yes, you can. Generic function is a function that is declared with type parameters. See example in `Math` class in `./MyGenericClass/Program.cs` 

## How are generics implemented internally?

See:
1) https://essentialcsharp.com/generic-internals
2) https://stackoverflow.com/questions/41461595/where-are-generic-methods-stored
3) https://stackoverflow.com/questions/54976683/how-c-sharp-generic-works