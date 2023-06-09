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
