# Ben.Demystifier
[![NuGet version (Ben.Demystifier)](https://img.shields.io/nuget/v/Ben.Demystifier.svg?style=flat-square)](https://www.nuget.org/packages/Ben.Demystifier/)
[![build](https://github.com/benaadams/Ben.Demystifier/workflows/Demystifier%20PR%20Build/badge.svg)](https://github.com/benaadams/Ben.Demystifier/actions)

Output the modern C# 7.0+ features in stack traces that looks like the C# source code that generated them rather than IL formatted.

## High performance understanding for stack traces 

.NET stack traces output the compiler transformed methods; rather than the source code methods, which make them slow to mentally parse and match back to the source code.

The current output was good for C# 1.0; but has become progressively worse since C# 2.0 (iterators, generics) as new features are added to the .NET languages and at C# 7.1 the stack traces are esoteric (see: [Problems with current stack traces](#problems-with-current-stack-traces)).

### Make error logs more productive

Output the modern C# 7.0+ features in stack traces in an understandable fashion that looks like the C# source code that generated them.

[![Demystified stacktrace](https://aoa.blob.core.windows.net/aspnet/stacktrace-demystified.png)](https://aoa.blob.core.windows.net/aspnet/stacktrace-demystified.png)

### Usage

```
exception.Demystify()
```
Or instead of Environment.StackTrace
```
EnhancedStackTrace.Current()
```
Resolves the stack back to the C# source format of the calls (and is an inspectable list of stack frames)

Calling `.ToString()` on the Demystified exception will produce a string stacktrace similar to the following (without the comments):

```csharp
System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
   at bool System.Collections.Generic.List<T>+Enumerator.MoveNextRare()
   at IEnumerable<string> Program.Iterator(int startAt)+MoveNext()                       // Resolved enumerator
   at bool System.Linq.Enumerable+SelectEnumerableIterator<TSource, TResult>.MoveNext()  // Resolved enumerator
   at string string.Join(string separator, IEnumerable<string> values)                    
   at string Program+GenericClass<TSuperType>.GenericMethod<TSubType>(ref TSubType value) 
   at async Task<string> Program.MethodAsync(int value)                                  // Resolved async 
   at async Task<string> Program.MethodAsync<TValue>(TValue value)                       // Resolved async 
   at string Program.Method(string value)+()=>{} [0]                                     // lambda source + ordinal
   at string Program.Method(string value)+()=>{} [1]                                     // lambda source + ordinal 
   at string Program.RunLambda(Func<string> lambda)                                       
   at (string val, bool) Program.Method(string value)                                    // Tuple returning
   at ref string Program.RefMethod(in string value)+LocalFuncRefReturn()                 // ref return local func
   at int Program.RefMethod(in string value)+LocalFuncParam(string val)                  // local function
   at string Program.RefMethod(in string value)                                          // in param (readonly ref)    
   at (string val, bool) static Program()+(string s, bool b)=>{}                         // tuple return static lambda
   at void static Program()+(string s, bool b)=>{}                                       // void static lambda
   at void Program.Start((string val, bool) param)                                       // Resolved tuple param
   at void Program.Start((string val, bool) param)+LocalFunc1(long l)                    // void local function 
   at bool Program.Start((string val, bool) param)+LocalFunc2(bool b1, bool b2)          // bool return local function 
   at string Program.Start()                                                              
   at void Program()+()=>{}                                                              // ctor defined lambda  
   at void Program(Action action)+(object state)=>{}                                     // ctor defined lambda 
   at void Program.RunAction(Action<object> lambda, object state)                         
   at new Program(Action action)                                                         // constructor 
   at new Program()                                                                      // constructor 
   at void Program.Main(String[] args)                                                    
```

Calling `.ToString()` on the same exception would produce the following output

```csharp
System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
   at System.ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion() // ? low value
   at System.Collections.Generic.List`1.Enumerator.MoveNextRare()                         
   at Program.<Iterator>d__3.MoveNext()                                                   // which enumerator?
   at System.Linq.Enumerable.SelectEnumerableIterator`2.MoveNext()                        // which enumerator?
   at System.String.Join(String separator, IEnumerable`1 values)                          
   at Program.GenericClass`1.GenericMethod[TSubType](TSubType& value)                     
   at Program.<MethodAsync>d__4.MoveNext()                                                // which async overload?
--- End of stack trace from previous location where exception was thrown ---              // ? no value
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()                      // ? no value
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task) // ? no value
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()                           // ? no value
   at Program.<MethodAsync>d__5`1.MoveNext()                                              // which async overload?
--- End of stack trace from previous location where exception was thrown ---              // ? no value
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()                      // ? no value
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task) // ? no value
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()                           // ? no value
   at Program.<>c__DisplayClass8_0.<Method>b__0()                                         //  ¯\_(ツ)_/¯
   at Program.<>c__DisplayClass8_0.<Method>b__1()                                         //  ¯\_(ツ)_/¯
   at Program.RunLambda(Func`1 lambda) 
   at Program.Method(String value)
   at Program.<RefMethod>g__LocalFuncRefReturn|10_1(<>c__DisplayClass10_0& )              // local function
   at Program.<RefMethod>g__LocalFuncParam|10_0(String val, <>c__DisplayClass10_0& )      // local function
   at Program.RefMethod(String value)
   at Program.<>c.<.cctor>b__18_1(String s, Boolean b)                                    //  ¯\_(ツ)_/¯
   at Program.<>c.<.cctor>b__18_0(String s, Boolean b)                                    //  ¯\_(ツ)_/¯
   at Program.Start(ValueTuple`2 param)                                                   // Tuple param?
   at Program.<Start>g__LocalFunc1|11_0(Int64 l)                                          // local function
   at Program.<Start>g__LocalFunc2|11_1(Boolean b1, Boolean b2)                           // local function
   at Program.Start()
   at Program.<>c.<.ctor>b__1_0()                                                         //  ¯\_(ツ)_/¯
   at Program.<>c__DisplayClass2_0.<.ctor>b__0(Object state)                              //  ¯\_(ツ)_/¯
   at Program.RunAction(Action`1 lambda, Object state)
   at Program..ctor(Action action)                                                        // constructor
   at Program..ctor()                                                                     // constructor
   at Program.Main(String[] args)
```
Which is far less helpful, and close to jibberish in places


### Problems with current stack traces: 

* **constructors** 

   Does not match code, output as `.ctor` and `.cctor`
   
* **parameters** 

   Do not specify qualifier `ref`, `out` or `in`
   
* **iterators** 

   Cannot determine overload `<Iterator>d__3.MoveNext()` rather than `Iterator(int startAt)+MoveNext()`
* **Linq**

   Cannot determine overload 
   
   `Linq.Enumerable.SelectEnumerableIterator``2.MoveNext()` 
   
   rather than
   
   `Linq.Enumerable+SelectEnumerableIterator<TSource, TResult>.MoveNext()`
* **async**

   Cannot determine overload and no modifier such as `async` 
   
   `<MethodAsync>d__5``1.MoveNext()` 
   
   rather than
   
   `async Task<string> Program.MethodAsync(int value)`

   Noise!
   ```
   --- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw() 
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task) 
   at System.Runtime.CompilerServices.TaskAwaiter.ValidateEnd(Task task) 
   at System.Runtime.CompilerServices.TaskAwaiter.GetResult() 
   ```

* **lambdas**

   Mostly jibberish `<>c__DisplayClass2_0.<.ctor>b__0(Object state)` with a suggestion of where they are declared but no hint if there are multiple overloads of the method.
* **local functions**

   Mostly jibberish `<RefMethod>g__LocalFuncParam|10_0(String val, <>c__DisplayClass10_0& )` with a suggestion of where they are declared but no hint if there are multiple overloads of the method.
   
* **generic parameters**

   Not resolved, only an indication of the number `RunLambda(Func``1 lambda)` rather than `RunLambda(Func<string> lambda)`
* **value tuples**

   Do not match code, output as `ValueTuple``2 param` rather than `(string val, bool) param`
* **primitive types**

   Do not match code, output as `Int64`, `Boolean`, `String` rather than `long`, `bool`, `string`
* **return types**

   Skipped entirely from method signature

### Benchmarks

To run benchmarks from the repository root:
```
dotnet run -p .\test\Ben.Demystifier.Benchmarks\ -c Release -f netcoreapp2.0 All
```
<sub>Note: we're only kicking off via `netcoreapp2.0`, benchmarks will run for all configured platforms like `net462`.</sub>
