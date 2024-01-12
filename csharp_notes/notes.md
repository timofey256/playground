# Other resources
- Tomáš Slama's notes from the same class 3 years ago : https://slama.dev/notes/the-cs-programming-language/.
- 

# Lecture 5: static types, constructors and enums

### `const` vs `readonly` vs `readonly static`
How are they stored in memory:
	`readonly` will take up space for every instance of an object.
	`readonly static` will be stored in one specific place in memory which will be shared among all instances of object.
	`static` won't take up any space and will be replaced with actual values during compilation.

During compile-time compiler does so-called **constant-folding**. If makes computations on constants wherever it can. For example `int variable = 2*2*2` will be folded to `int variable = 8`.

Note that we cannot assign a runtime values (for example output of functions) to a  constant. So you can't create a constant like `const int sinusOfRightAngle = Math.Sin(90)` If you, however, still want to do this, use `static readonly`.

**Exception**: `string` can be assigned to a constant despite being a reference type.

If you will use constants in your library, after changing them there, you should also recompile a program that uses this library. Otherwise, program will use old values of these constants. It does not occur with `readonly` fields


### (Static) constructors 
Static constructors aren't called on the start of the program but whenever you need to use static field or property. JIT compiler will analyze your code and call static constructor before you will use any method/field/property of object. In other words, constructors are **guaranteed** to be called before you call anything from an object.

In intermediate language class constructor(or static constructor, or type constructor) is called `.cctor` (usual constructors are denoted `.ctor`)

When you "redirect" constructor with `this()` (for example: `public A : this("Hello world") { body }`), you call another constructor and then, after initializing the object in this another constructor, the body is called (so not before!).
 
 **Ancestor's constructor is always called** (except for `object`). If class has no `base()` or `this()` in constructor, then constructor of `object()` will be called.

### **Enums** are value type.
  Use them to define a bunch of constant values when you don't want to mix values of the same type but different semantics(like month and day, both are strings). Also, consider `struct` for these purposes.

-----
# Lecture 6: NuGet, Benchmarks, Optimizations and virtual methods tables
### NuGet package manager
It is a place where people can publish their libraries. Every package has `.nupkg` extension.

### Benchmarks (`BenchmarkDotNet` library)
**Microbenchmark** - measure performance of some particular piece of code(like unit test)
**Macrobenchmark** -  measure performance of the entire program (like integration test)

### JIT Optimizations
**Inline expansion**: when we call function B() from function A() multiple times (let's say, is is inside a loop), compiler will try to to unfold function B() if it is small (rule of thumb: small := "under 20B"). The reason for doing it is that if B() is small and we iteratively call it, then we will spend much more time preparing environment to call this function(like initializing stack in the beginning and then releasing it in the end; so-called prolog and epilog of functions) and not in its actual behavior. 

If you want to force inlining, you can add attribute `[MethodImpl(MethodImplOptions.AggressiveInlining]` . If you do not want any inlining whatsoever, then add `[MethodImpl(MethodImplOptions.NoInlining]` to a method. **Usually you do not want to do it (!)** because these optimization are very tricky to use properly.

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

**A big downside of virtual methods** is that it's impossible to inline them. So they are on average slower than usual methods.

------

# Lecture 7
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

----
# Lecture 8

**Can I call a virtual method from a constructor?** In .NET, yes. In some other languages, no. Why? Because VMT might not have been initialized yet.

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

----

# Lecture 9

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

------
# Lecture 10

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
	Inc(ref val); // val == 3
}
```
- "call by reference"
- formal parameter is an alias for the actual parameter (address of actual parameter is passed)
- actual parameter must be a variable (so could **not** be `Inc(123123)`)

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
 
-----
# Lecture 11