# Boxing/unboxing

## What is boxing (unboxing)?

What happens when we want to pass a value type as a reference type? For example, when casting to `object` type:
```
int num = 10;
object o = num;

```
The result of the conversion has to be a reference to a storage location that contains something that looks like an instance of a reference type, but the variable contains a value of value type. The opposite case is called unboxing.

## What happens when you box some value type?
Well, as expected, you should allocate some space in the heap for the value of the value type itself and for overhead to make it look like object(namely, a `SyncBlockIndex` and VMT). Then, you need to copy the value to the allocated space. As a result, you will get a reference to your newly created reference type object that contains some value type.

## What happens when you unbox some reference type?
You, firstly, should check if you even can assign a value within the reference type object to some variable and then acopy it. So 2 operations.


