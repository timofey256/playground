# What are delegates in C#?
Delegate is a method placeholder for a given type signature. They allow you to pass methods as parameters, store them, or create callback handling mechanisms, allowing for greater modularity.
## When to use delegates?
Use delegates whenever you need to separate the method being called from its conditional logic, or when you want to make your functions extensible with callbacks and event handling.
- **Callbacks** - when you need to pass a method as a parameter to another method.
- **Event handling** - when you need to pass an abstract action to a UI element.
- **Dynamic invocation** - when you have a collection of similar methods and want to call them conditionally, you just assign it to a delegate.
- **Asynchronous programming** - when you need to execute a method on a separate thread.

## How to use delegates?
```c#
delegate void FilterDelegate(int number);

void IsEven(int number)
{
  return number % 2 == 0;
}
void IsOdd(int number)
{
  return number % 2 != 0;
}

static List<int> Filter(List<int> numbers, FilterDelegate filter)
{
    List<int> filteredList = new List<int>();
    foreach (int number in numbers)
    {
        if (filter(number))
        {
            filteredList.Add(number);
        }
    }
    return filteredList;
}
List<int> numbers = new List<int>() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
Filter(numbers, IsOdd); // {1,3,5,7,9}
Filter(numbers, IsEven); // {2,4,6,8,10}
```

## Are delegates mutable or immutable?
Delegates are immutable.

## Delegates internals
In .NET delegate types always derive from `System.MulticastDelegate`, which in turn derives from `System.Delegate`. `System.Delegate` is also implements `ICloneable` and `ISerializable` interfaces.

`System.Delegate` has a `MethodInfo` (`System.Reflaction.MethodInfo`) property which describes the signature of a particular method: its name, parameters and the return type. Then, it also should have a pointer to a function itself. `Target` property of `System.Delegate` contains it.

## What are lambda expressions and what they are for?
The purpose of a lambda expression is to eliminate the hassle of declaring an entirely new member when you need to make a delegate from a very simple method. Lambda expressions are divided into two types: statement lambdas and expression lambdas.
For example, instead of declaring a large delegate for a really simple function, you can use lambda expressions:
```
BubbleSort(items,
    (int first, int second) =>
    {
        return first < second;
    })
```
The return type is missing, but the compiler can see that the lambda expression is being converted to a delegate whose signature requires the return type bool. The compiler verifies that the expressions of every return statement in the statement lambda’s block would be legal in a bool-returning method. The public modifier is missing; given that the method is no longer an accessible member of the containing class, there is no need to describe its accessibility. Similarly, the static modifier is no longer necessary. The amount of “ceremony” around the method is already greatly reduced.


