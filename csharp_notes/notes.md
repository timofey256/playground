# Lecture 5: static types, constructors and enums

### `const` vs `readonly` vs `readonly static`
How are they stored in memory:
	`readonly` will occupy space for every instance of an object.
	`readonly static` will be stored in one specific place in memory, shared among all object instances..
	`const` won't occupy any space and will be replaced with actual values during compilation.

During compile-time, the compiler performs **constant-folding**, making computations on constants wherever possible. For example, `int variable = 2 * 2 * 2` will be folded to `int variable = 8`.

Note that you cannot assign runtime values (e.g., the output of functions) to a constant. Therefore, creating a constant like `const int sinusOfRightAngle = Math.Sin(90)` is not allowed. If needed, use `static readonly`.

**Exception**: `string` can be assigned to a constant despite being a reference type.

If you use constants in your library, after changing them, you should also recompile a program that uses this library. Otherwise, the program will use old values of these constants. This does not occur with `readonly` fields. That's why it is not a good idea to use public constants.

### (Static) constructors 
Static constructors aren't called at the start of the program but whenever you need to use a static field or property.  The JIT compiler will analyze your code and generate will generate CIL code that will lazily call the static constructor before the first access to a method, field, or property. In other words, constructors are **guaranteed** to be called before you call anything from an object. If JIT compilation occurs after the static constructor has already been called, no additional code is generated.

*Side note: This has an interesting implication on the order of code execution, potentially influencing seemingly unrelated parts of the program's performance. Ensuring that all static constructors have run before JIT-compiling the hot path can, in certain cases (though not always measurably), enhance performance. In more recent .NET versions, the introduction of tiered compilation addresses this concern by compiling all methods twice, with the second pass likely covering the execution of almost all static constructors. This diminishes the necessity of explicitly managing the order of static constructor calls before JIT compilation.*

In intermediate language, a class constructor (or static constructor, or type constructor) is called `.cctor` (usual constructors are denoted `.ctor`).

When you "redirect" constructor with `this()` (for example: `public A : this("Hello world") { body }`), you call another constructor and then, after initializing the object in this another constructor, the body is called (so not before!).
 
 **Ancestor's constructor is always called** (except for `object`). If class has no `base()` or `this()` in constructor, then constructor of `object()` will be called.

### **Enums** are value type.
Use them to define a set of constant values when you don't want to mix values of the same type but different semantics (like month and day, both as strings). Also, consider using `struct` for these purposes:
```csharp
struct Month {
	int Num; // or string Name;
}
struct Day {
	int Num;
}

enum Month {
	January, February, ... 
}
enum Day {
	First, Second, ...
}
```

<div style="page-break-after: always"></div>

# Lecture 6: NuGet, Benchmarks, Optimizations and virtual methods tables
### NuGet package manager
It is a place where people can publish their libraries. Every package has `.nupkg` extension.

### Benchmarks (`BenchmarkDotNet` library)
**Microbenchmark** - measure performance of some particular piece of code(like unit test)
**Macrobenchmark** -  measure performance of the entire program (like integration test)

### JIT Optimizations
**Inline expansion**: when we call function B() from function A() multiple times (let's say, is is inside a loop), compiler will try to to unfold function B() if it is small (rule of thumb: small := "under 20B"). The reason for doing it is that if B() is small and we iteratively call it, then we will spend much more time preparing environment to call this function(like initializing stack in the beginning and then releasing it in the end; so-called prolog and epilog of functions) and not in its actual behavior. 

If you want to force JIT to make more inlining (you can't really force to do it always, JIT always can decide not to do it), you can add attribute `[MethodImpl(MethodImplOptions.AggressiveInlining]` . If you do not want any inlining whatsoever, then add `[MethodImpl(MethodImplOptions.NoInlining]` to a method. **Usually you do not want to do it (!)** because these optimization are very tricky to use properly.

**Devirtualization**: This optimization allows the JIT compiler to decide whether to generate code that uses a specific method implementation instead of abstract virtual dispatch. This can be illustrated in the following code:
```csharp
MyAbstractClass c = ...;
if (c.GetType() == typeof(ConcreteDerivedClass))
    c.F(); // Calls (potentially inlines) F because we know the exact method implementation
else
    c.F(); // Virtual dispatch
```
In this example, the JIT compiler performs devirtualization when it compares the type of the object `c` with the type `ConcreteDerivedClass`. If they match, it may decide to invoke the `F()` method directly on the concrete implementation, allowing optimizations like code inlining and improving performance.

### Virtual methods
Virtual methods are used to create extensible interfaces. They use so-called **virtual methods table**(VMT or sometimes `vtable`). Then, when some class inherits from another which has at least one virtual method, we copy a pointer to the parent class' VMT. If the child class also has a virtual method, then we will store both table (parent's VMT and newly created child's).

*Note that `abstract` methods are always virtual*

Then, in CIL (common intermediate language), when calling virtual methods, we will see:
```
CIL:
callvirt A.f()        // where A.f() is a virtual method of class A


In x86 it will be converted to usual call(pseudocode):
call[this.GetType().VMT[index of function virtual table]]   // where `this`                                                                     refers to class A 
```

**How is virtual methods table implemented?**
VMT is used to determine at **run time** which method from which class in the hierarchy to call. If class has at least 1 virtual method, it will also have pointer to its VMT. This pointer will always be at the beginning of the object address (see an image below). All object instances of the same class will share the same `vtable`.

![[Pasted image 20240104175210.png]]

Further reading : ["Virtual, new and override in C#"](https://pnguyen.io/posts/virtual-new-override-csharp/) (it also shows what happens during casting)

**A big downside of virtual methods** is that it's (usually) not possible to inline them since JIT does inlining only when it's 100% sure what method will be called. That's why virtual methods are on average slower than usual methods. 
Although, JIT can inline virtual methods if they are `sealed` (or the entire class is sealed). For example:
```csharp
MySealedClass c = ...;
c.MyVirtualMethod() // JIT is able to inline this!
```

<div style="page-break-after: always"></div>

# Lecture 7: hiding members of children, abstract vs interface, `base` keyword
### Hiding members in descendant (keyword `new`)
In the code below we **hid member** `f()` of `A`'s parent. Why it only shows **warning** and does not throw error? Because if it did, it could cause a lot of problems in large code bases. Let's say `B` is some class from some library we use and `A` is our class which inherits from `B`. If we had created method `f()` before it was in `B` and then developers of the library decided to create a member with this name, then our code is broken. That's bad and that's the reason hiding members is warning, not error.
```
class B {
	public void f()
}

class A : B {
	public void f()
}
```

### sealed override
**`sealed override`** keyword allows you to prevent descendants of class from overriding some function. You can use it when you want to say that "this implementation is the final one and no one should overwrite it". The advantage of it is that even though in CIL it will still be a `callvirt` command, while translating CIL to x86 JIT can possibly inline it; thus, increasing performance (by a small margin though; but good to know that it happens at all).

### abstract method vs interface method
![[Pasted image 20240104191647.png]]

Why there can be no data in interface? Because if there were, then we have the diamond problem: several classes share the same data, then you make a class that inherits from both of them, and now this new class has several instances of the same object. This also applies to methods, not only data, [see description on wiki](https://en.wikipedia.org/wiki/Multiple_inheritance#The_diamond_problem) . 

### Calling methods of the base(parent) class
If you want to call a member from the base (also called "parent") class, you can try several things. Let's say class `A` inherits from `B`:
- If you want to call `B.f()` and there's no function `A.f()`, just call `f()`.
- If there is function with the same name in `A`, then you can either call `((B)this).f()`, or `base.f()`. The former won't work if `A.f()` overrides `B.f()` because you essentially access VMT which is already overwritten with `A.f()`. It will lead to infinite loop. The later is the solution. Even if `A.f()` overwrites `B.f()`, `base.f()` will call implementation from `B` class.

**Note that `base` does not work for the base class of the base class. In other words, `base.base.f()` won't work.**

<div style="page-break-after: always"></div>

# Lecture 8: factory, `callvirt`, properties, access modifiers

**Can I call a virtual method from a constructor?** In .NET, yes. In some other languages, no. Why? Virtual Method Table (VMT), which keeps track of virtual methods and their implementations, might not have been fully initialized during the construction phase.

*It's worth mentioning that most **linters** (tools that analyze code for potential issues) usually give a warning in such cases. This is because calling a virtual method from a constructor can lead to certain invariants not being set in the derived class. For instance, fields that get initialized in the constructor may still be `null`, and this is something a programmer might not expect when writing an `override` method. Therefore, caution is advised when invoking virtual methods within constructors to ensure proper initialization and avoid unexpected behavior in the derived class.*

### Factory design pattern
If you repeatedly generate a specific kind (or class) of objects, you can consider implementing the other object that will generate these objects on demand. For example, you need to generate apples and sometimes golden apples (once every N apple) for your tree. Perfect use-case for factory pattern. Other examples and further explanation : https://refactoring.guru/design-patterns/factory-method

### `callvirt` instead of `call`
Sometimes you can see `callvirt` in CIL for a non-virtual method. It is used to prevent errors associated with libraries. If you had library `L1` with non-virtual methods that were used in library `L2` and then decided to change these non-virtual methods to virtual, CIL of `L2` would still contain `call` which can lead to unexpected behavior like calling code of ancestor instead of descendant.

### Properties
The following 2 code pieces are equivalent:
```csharp
public int X { get; set; }
```

```csharp
private int _x;
public int X {
	get {
		// return _x
	}
	set {
		// set `value` to _x
	}
}
```

Both pieces generate `int get_X()` and `void set_X(int value)` in CIL.
Important good practices: Properties should **not** be slow because the syntax looks instant. Properties should **not** have any side effects.

- `=>` operator
	You can use `=>` to shorten your code. Examples of equivalent code:
	```csharp
	int X { get => _x; }
	
	int X => _x
	
	
	void foo() {
		bar();
	} 
	
	void foo() => bar()
	```

- `NaN` in doubles
	When you want to use invalid value for `double`, consider using `double.NaN` instead of `null` for nullable `double?`. Then, you can check it with `double.IsNaN(value)`

### Access modifiers
| Modifier | Description |
| ---- | ---- |
| public | The code is accessible for all classes |
| private | The code is only accessible within the same class |
| protected | The code is accessible within the same class, or in a class that is inherited from that class |
| internal | The code is only accessible within its own assembly, but not from another assembly. |
| protected internal | Access is limited to the current assembly OR types derived from the containing class  |
| private protected | Access is limited to the current assembly AND types derived from the containing class |

**Default access modifiers for different types:**
- Inside class : `private`
- Inside interface : `public`
- class : `internal`

### Variables lifetime optimizations
JIT compiler can do optimizations of some variables like:
```csharp
while (...) {
	int b = 7; // int b, ALLOC -> SUB SP, 4; // b = 7
}

// instead of allocating memory every iteration, compiler will allocation 
// one place in memory for variable b:

// ALLOC, SUB SP 4
while (...) {
	int b = 7;  // b=7
}
```

<div style="page-break-after: always"></div>

# Lecture 9: value and reference types, pointers, introduction to tracking reference

### Explicit scope
If you want to separate some chunks of code and create an artificial scope, you can just use curly brackets
```csharp
void foo() {
	// some code...
	int b = 0;
	{
		// more code...
		int c = 0;
		Console.WriteLine(c);
	}
	// rest of the code...
}
```

### Value and reference types
**Value type**:
```csharp
struct S {
	int a1, a2, a3; // entire struct is 12B big
}

int b = 7; // 4B

S s1 = new S(); // don't conflate with `new` in reference types, for value types `new` doesn't allocate anything! 
```

**Reference types:**
- address to the entire object in garbage-collected heap.
- explicit allocation with `new`.
- How is it store in memory?
	```csharp
	class A {
		int a1, a2;
	}
	
	// ---------------------------------------------------
	// | sync block | reference to Type object | a1 | a2 |
	// ---------------------------------------------------
	```
- How to get address where object is stored? You can't.
- `Object` is reference type but value types also inherit from it. How so? To make it work, C# uses (un)boxing.
```csharp
// boxing:
int a = 5;
object o = a; // boxing `a`. Runtime creates an instance of System.Int32 on heap. And address to this instance will be stored in `o`.

// unboxing:
object o = 5;
int i = (int)o; // unboxing always requires explicit casting.

// note that unboxing also requires accurately determining the type; for example, you cannot unbox an `int` into a `long` without precise casting.
long a = (long)o; // error!
long a = (long)(int)o // OK
```
- You want to prevent repeated boxing/unboxing in your code because it significantly slows it down. Example:
```csharp
object o = 5;
for (int i=0; i<1000; i++) {
	o = ((int)o)+1 // here we first unbox `o` to increment it. then, runtime check for types. then, we box it to save it to `o`.
}
```
- The other downside of allowing (un)boxing is that you now can't see potential runtime errors associated with casting to different types.

### Pointers in C\#
- It is address.
- Syntax : `int* a, b;`
- Explicit reference/dereference : `a=&...; *b=...`
- No limitations to what it can be pointer.
- Garbage collector is not responsible for pointers!

### Something in between pointers and reference types : tracking reference
- Can point to GC heap, static field, local variable.
- 4B or 8B (depends on if system is 32b or 64b)
- Can't access address of reference in memory.
- Used in C++/CLI language (C++ and C# mixture language)
- Solution for C# : reference parameters

<div style="page-break-after: always"></div>

# Lecture 10: tracking reference, hidden `this` and why structs are sealed

### Value vs reference parameters
**Value parameters**:
```csharp
void Inc(int x) { x = x + 1; }
void f() {
	int val = 3;
	Inc(val); // val == 3
}
```
- "call by value"
- value is copied inside a called function
- actual parameter is an expression (so could be `Inc(123123)`)

**Reference parameters:**
```csharp
void Inc(ref int x) { x = x + 1; }
void f() {
	int val = 3;
	Inc(ref val); // val == 4
}
```
- "call by reference"
- formal parameter is an alias for the actual parameter (address of actual parameter is passed)
- actual parameter must be a variable or method that returns corresponding type (so could **not** be `Inc(123123)` but can be `Inc(ref somevar)`, `Inc(ref MyClass.MyMethodThatReturnsRefInt())`);

**Discard variable**: If you want to discard some return value, use `_`. Example : `int.TryParse("123", out int _)`.

***in* and *ref readonly*** : almost the same except some technicalities.

### Quick note about inheritance of interfaces
Despite interfaces not inherit from `object` we can call function that are implemented there. Why? Because interface is always implemented by some object. This object should inherit from `object` type by default; as a consequence, it is always possible to call from interface all functions defined in `object` like `.ToString()`, `.GetHashCode()`, `GetType()`, etc. 

### Why structs are sealed?
Consider the following code:
```csharp
struct S1 {
	int x;
}

struct S2 : S1 {
	int y;
}

S1 s1 = new S1(); // size 4B
S2 s2 = new S2(); // size 8B

s1 = s2 // what should happen? s1 is 4B and s2 is 8B. it would be complex and is frequent cause of bugs (for example in C++ where structs aren't sealed), so this structs are sealed.

// note that this does not happen in reference types because all variables of this type are equal in their size (cuz they are pointers to memory)
```

### Hidden `this` as a parameter in methods
When you call some member(property, field or function) of the current type inside a method, you also use a hidden `this`.
```csharp
class C {
	public void foo() { // in fact, it is `public void foo(C this)`
		bar(); // in fact, it is `this.bar()`
	}
	
	public void bar() { // same logic here : `public void bar(C this)`
		Console.WriteLine("Hello from bar()!");
	}
}
```

In classes `this` has clearly type of its class - reference type variable. But what type do structs have? Consider the following example:

```csharp
struct S {
	int _x = 0;
	
	public void foo() {
		bar();
	}
	
	public void bar() {
		_x++;
	}
}
```
 If it `foo()` also had `this` as a hidden parameter of type `S` to call `this.bar()`, then struct `S` would have been copied to `this` and the variable `_x` would have been changed there and we would not be able to see changes in our struct. Because of that **structs have tracking reference as a hidden parameter**: `public void foo(ref S this) { ... }`
 
<div style="page-break-after: always"></div>

# Lecture 11: arrays, constructors, goto and switch

### Arrays
- Arrays are reference type and are allocated in GC heap. 
- Array objects are stored almost the same as a regular reference type object:
```
// ---------------------------------------------------
// | sync block | reference to Type object | Length of array |a1|a2|...|a_n|
// ---------------------------------------------------
```
- Arrays are always initialized after declaring.
- arrays are a descendant of `System.Array`.
- **Multidimensional arrays**: 
	- Jagged: `int[][] a = new int[2][]`. Literally, arrays of arrays (as a reference types). We store pointers to arrays in the bigger array. It allows to these smaller arrays have different sizes because they are independent objects.
	- Rectangular: `int[,] = new int[2,3]`.
	
	Because of back compatibility with VSBasic rectangular arrays are paradoxically slower for access than jagged. They are still more memory efficient but keep in mind that access in 2-3x slower.
- When you access elements in array, runtime checks if you are accessing elements still inside array (if no, throws `IndexOutOfRangeException`)

**`default` keyword**: `default(int)` or just `default` initialized variable with its default value. For `int` it is 0, for `string` it is `null` (same for all reference types). For all nullable types(i.e. `int?`) default() is `null`.

### Implicit and explicit constructors
Object always has an implicit constructor without parameters which sets zero to memory that is allocated for the object for initializing. Also, object can have an explicit constructor without parameters. But it is called only explicitly too. Example:
```csharp
class C {
	public C() { Console.WriteLine("Hello from C.ctor!"); }
}

struct S1 {
	public C c;
}

struct S2 {
	public C c = new C();
}

S1 s1 = new S1(); // no message
S2 s2 = new S2(); // "Hello from C.ctor!"
```

### `goto`
Isn't as bad as people say. Can be used in certain situations. 

In C# `goto` can't go inside some scope because it was a common cause of plentiful of bugs in languages like C where `goto` can jump anywhere. So in C# you can move inside the current scope and step out of it. 

In CIL `goto` implemented with `jmp`.

Good use-cases of `goto`:
1. If you can simulate some process using non-complex finite-state machine, `goto` can be helpful. Or `switch` statement.
2. Jump outside of several loops (break will stop only the first outer one).
3. In switch statements. If you have several cases but want to redirect some case to another, use `goto`.

### `switch` statement

You can use `switch` how it is used the most, as void statement:
```csharp
switch(variable) {
	case smth1:
		break;
	case smth2:
	case smth3:
		break;
	default:
		// ...
}
```

Or, also, in C# you can use it as expression which returns result:
```csharp
int x = 10;
var result = x switch {
	> 10 and < 20 => 1000,
	int b => b + 10,
	null => -1
} 
```

Here we used so-called **pattern matching**. It is a technique that allows you to test an expression to determine if it has certain characteristics.

<div style="page-break-after: always"></div>

# Lecture 12: pattern matching, exceptions

### Pattern matching
- a is `X { property1 : ..., property2 : ... }`
- a is `X(...)`. 
	Have to have `Deconstruct(out a, out b, ...)` method! 
	It is an example of duck typing.
	  
	Imagine you have a `Person` class and want to get name of a specific person with easy syntax: `var (firstName, lastName) = p`. To do so, you should implement `Deconstruct()` method:
```csharp
class Person {
	public string FirstName { get; set; }
	public string LastName { get; set; }
	
	public void Deconstruct(out string firstName, out string lastName) {
		firstName = FirstName;
		lastName = LastName;
	}
}
```
- a is [condition, condition, type, condition, ....]. 
	This is so-called list pattern. You can compare list in the variable on the left to the conditions on the right. 
	  
	Should have at least 3 elements (conditions and/or types)
	Note that types in this matching could allocated on GC heap!
	Example:
```csharp
int[] numbers = { 1, 2, 3 };

Console.WriteLine(numbers is [1, 2, 3]);  // True
Console.WriteLine(numbers is [1, 2, 4]);  // False
Console.WriteLine(numbers is [1, 2, 3, 4]);  // False
Console.WriteLine(numbers is [0 or 1, <= 2, >= 3]);  // True
```

### Exceptions
- All exceptions are reference type objects and have to be descendants of `System.Exception` or descendants of other types that inherit from `System.Exception`.
- If exception is thrown, then the rest of the code of function will be skipped (except for the `finally` block if it's present. it will be executed after `try` and `catch` blocks) as well as the rest of the code of functions which called the function where this error has occurred. 
- Exception objects also have members like `message`, `StackTrace`, `InnerException`. `StackTrace` gets a string representation of the immediate frames on the call stack. It helps to find out how, where and why error has occurred. `InnerException` is the other exception which caused the exception. For example, you create your custom exception `UserNotFoundException` when you don't find your user in data. Exception you get might be `FileNotFoundException`, `SqlException` and so on. Then, you return `UserNotFoundException` with the specific `InnerException` which caused it. 
- **Why exceptions are so slow?** Part of the reason is that it fills `StackTrace` and we can't just literally jump to the end of the function when error was thrown. It could have leaded to unallocated variables and overall problems with memory. Also, we have to determine where is the the nearest `catch` and `finally` blocks. So this "jump" is quite complex from the runtime standpoint. 

#### Code contracts
Basic principles of automated analysis and verification of programs (model checking, static analysis, dynamic analysis, and deductive methods) and their practical applications (e.g., detecting concurrency errors).

<div style="page-break-after: always"></div>

# Lecture 13: more about exceptions, `finally` and `using` keywords

### Problems caused by exceptions
If you was constructing or updating some data structure when error occurred, it could have caused invalidness of this data structure. How to prevent it?
- **Defensive programming**: use keywords and other techniques to ensure that you can't have logically invalid data in data structure. For example, `required` keyword.
- Implement undo: throw a specific error, then catch it with try-catch block and make undo in catch block. If needed, throw error further.

Note that if you will send your error further from `catch` block, then it will create a new `StackTrace`:
```csharp
try {
	// ...
}
catch (Exception e) {
	// process error
	throw e; // creates new StackTrace!
	throw; // preserves StackTrace, throws `e` further
}
```
Sometimes it is useful if your client has an access to errors and you don't want him to know internals of your program.

### `try` and `finally`
When to use `finally` block? When you want to execute some code regardless of we got exception of not. Good example is closing opened files (`StreamReader`, `StreamWriter`, `XmldataWriter`, etc.) after using them. 

**Disposable pattern:** when some object holds some resources, it is useful to implement a function (often called `Dispose()`) to release these resources. If you use files, close your files in `Dispose()` method. If you don't use pointers, you usually don't want to implement Disposable pattern according to documentation. It will be enough to implement `Dispose()` method using `IDisposable` interface. Also, don't forget that someone can want to use the object after calling `Dispose()`. In this case, return `ObjectDisposedException`.

### `using` statement
The `using` statement ensures the correct use of an [IDisposable](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable) instance.
Often it is used to work with files: file is opened only for the duration of the code inside `using`, then `.Dispose()` is called.

**How to use it?**
- using with brackets:
```csharp
var numbers = new List<int>();
using (StreamReader reader = File.OpenText("reader.txt"))
{
	// ...
}
```

- using without brackets are allowed too.
```csharp
using StreamReader sr = new StreamReader(...);
// sr lives until the end of the scope
```

- several usings: you can either nest them, or inline outer ones:
```csharp

using (StreamWriter writer = File.OpenText("writer.txt"))
using (StreamReader reader = File.OpenText("reader.txt"))
{
	// ...
}
```

- **aliases**: you can use `using` as a declaration of alias for something. It should be defined in the global scope. From C#12 you can declare it also for generic types.
```csharp
using Point = (int x, int y);
```

- import namespace:
```csharp
using System;
```

<div style="page-break-after: always"></div>

# Lecture 14: garbage collection and strings

### Garbage collector
**Garbage collection** (**GC**) is a mechanism for automatic memory management. The garbage collector attempts to reclaim memory which was allocated by the program, but is no longer referenced; such memory is called garbage.

Main .NET GC developer is Maoni Stephens.  

#### Heap types
- Small Object Heap (SOH) for small objects
- Large Object Heap (LOH) for large objects : >85000 bytes (value is empiric). **No heap-compacting here (!)** except if not forced with settings. Also, is garbage-collected only after garbage collection occurred in generation 2 of objects (see below).

Note that if you have, for example, array of large objects, then it does not mean that this array will end up in the LOH too. Because it contains references and not objects themselves, it could be in SOH.

#### Kernel memory allocation calls
- reserve VA(virtual address) : returns address and size of memory that **can** (not "is") allocated.
- commit VA : allocation in memory.
- decommit/free : free a specific region

**Segment (old definition) / region (modern definition)** is a block of allocated memory in .NET. 

#### Garbage collector modes
Server mode (optimized for fast response time, high throughput and scalability):
	Can have several heaps. GC has its separate threads. Also, GC operates from the thread that called GC.
Workstation mode (designed for client apps):
	Has only one heap. When GC is called, then all threads are paused.
Read more [here](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/workstation-server-gc)

#### When GC is executed?
**When memory is needed:** This occurs when you attempt to allocate memory, often done using the `new` keyword. The initiation of garbage collection (GC) is typically prompted by various factors. These include the passage of a certain amount of time since the last GC execution, the allocation of a specific amount of memory since the last GC, and various other heuristics. Additionally, the Windows operating system may send messages to all applications, instructing them to run their garbage collectors.

Garbage collection proves highly effective for managing **short-lived objects**. When the GC is executed, a significant amount of memory is freed, reducing the need to copy a large number of objects. This efficiency is particularly advantageous for short-lived objects, as it helps maintain a more streamlined and responsive memory management process.

#### How objects are stored?
- GC builds a graph of objects where vertices are objects and edges are references to other objects (for example, field with some reference type object).
- Static variable are allocated separately.
- For every thread we also have call stack.
- JIT compiles let's GC now all of the data above. They are called **GC roots**.

#### How it works?
- **(1st phase):** Traverse the graph of objects and mark all objects that are still in use.
- **(2nd phase):** Move the live objects to a compacted space, overwriting the non-marked objects. The space vacated by non-marked objects is considered free, and new objects can be allocated in these spaces.
This process ensures efficient memory usage by compacting the live objects and effectively managing the allocation of new objects in the reclaimed memory spaces.

**Problems:**
- Holes in memory : when you free some blocks, your memory can become tattered. And then you can have enough memory for allocating some new object but because this memory isn't sequential, you will fail to allocate this new object. Solution: **heap compacting**: GC squeezes objects together, trying not to leave free memory between them.

#### Generational GC
(text taken from [here](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/fundamentals#generations))
The GC algorithm is based on several considerations:

- It's faster to compact the memory for a portion of the managed heap than for the entire managed heap.
- Newer objects have shorter lifetimes, and older objects have longer lifetimes.
- Newer objects tend to be related to each other and accessed by the application around the same time.

Garbage collection primarily occurs with the reclamation of short-lived objects. To optimize the performance of the garbage collector, the managed heap is divided into three generations, 0, 1, and 2, so it can handle long-lived and short-lived objects separately. The garbage collector stores new objects in generation 0. Objects created early in the application's lifetime that survive collections are promoted and stored in generations 1 and 2. Because it's faster to compact a portion of the managed heap than the entire heap, this scheme allows the garbage collector to release the memory in a specific generation rather than release the memory for the entire managed heap each time it performs a collection.

- **Generation 0**: This generation is the youngest and contains short-lived objects. An example of a short-lived object is a temporary variable. Garbage collection occurs most frequently in this generation.
- **Generation 1**: This generation contains short-lived objects and serves as a buffer between short-lived objects and long-lived objects.
- **Generation 2**: This generation contains long-lived objects. An example of a long-lived object is an object in a server application that contains static data that's live for the duration of the process.

#### Potential memory leaks and inefficiencies
When can occur? Typical cases:
- Reference from a variable. Memory leak.
- Long-lived objects. If we don't reach 2. generation, long-lived objects can be there for a very long time. Inefficiency.
- When you call `List<T>.Clear()`. `Clear()` sets Count of the list to zero but objects that were in the list still can be pointed to other objects which prevents them from being garbage-collected. Ideally, if you know that you won't fill all cleared cells in list, you should set them to `null`. Memory leak.

### Strings
- In memory stored as a block : [length, char0, char1, ...].
- Always immutable.
- `StringBuilder` is a "mutable string". Inside is something like `List<char>`.
- There are a lot of optimizations for `string`: for example, if you write something like `"hello from C# notes" + "yeah, it's the last lecture" + "uWu"` compiler will replace it with `string.Concat()` of this strings which is faster. Or compile even can use constant-folding (similarly to how `int num = 100 * 100 * 100` will be translated to `int num = 1000000`).
- When you are using `String.Format` be careful to not to use strings that you don't control. Prefer to write String.Format like on the second line to the what is on the third one.
```csharp
string name = "{0}";
Console.WriteLine("{0}: hello {1}", DateTime.Now, name); // 01/18/2024 18:36:20: hello {0}
Console.WriteLine("{0}: hello" + name + "!", DateTime.Now); // 01/18/2024 18:36:20: hello01/18/2024 18:36:20!
```
- `$"{string1} ... {string2}"` is another way to concatenate strings. Behind the scenes uses `InterpolatedStringHandler`.