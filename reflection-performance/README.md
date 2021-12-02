# Introduction

Have you ever said or heard that reflection is slow? I have to admit that I had made that claim in the past. But I found out it is rather easy to use reflection in a way that doesn't decrease performance. And I created a benchmark for calling a method via reflection and its alternatives only to find out that the cost is not that high after all. I am certain it is OK to use reflection in non-performance-sensitive code. And I have a better idea how some libraries use code generation in order to achieve superior performance.

# Directions

[BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) is used for measuring actual timing. Results and conclusion are at the bottom of this page.

# Types of methods being benchmarked

I've deliberately instructed the CLR not to inline the methods (where applicable) in order to actually time the call.

### Method with a parameter and return type

```C#
private static readonly int _response = 1;

[MethodImpl(MethodImplOptions.NoInlining)]
public static int Parameters(int request) => _response;
```

### Void method without parameters

```C#
[MethodImpl(MethodImplOptions.NoInlining)]
public static void Void() {}
```

# What is known about the method

### The actual method is known at compile time

This includes rather easy scenarios which act as a good base line.

* Static method
* Non-virtual method
* Interface method
* Delegate

### Only the method signature is known at compile time

Sometimes we only have the `MethodInfo` instance and do not know the actual method at compile time. In these cases knowing the signature is quite helpful since there are some tricks to make things run faster.

* Slow reflection - This is the standard way of calling a method via `MethodInfo.Invoke`
* Reflection delegate - As [Jon Skeet](https://stackoverflow.com/users/22656/jon-skeet) has said more than 10 years ago - [we can make reflection fly](https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/). Basically, a delegate is created using the `MethodInfo.CreateDelegate` method which requires a knowledge of the method signature. Then that delegate is called instead of `MethodInfo.Invoke`.
* Compiled expression tree - [C# expression trees](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/) can be compiled to a delegate, too. Knowing the signature of the target method is required.

### The method signature is not known at compile time

The tricky case is when we know only something about the signature. For example a method which accepts only one class parameter and returns void. But without knowing the exact type of the only parameter we cannot use the approaches above. Thankfully there are alternatives, which are costly in terms of development effort, but useful in certain scenarios.

* Compiled static method - Using the .NET API [C# code can be compiled dynamically](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp?view=roslyn-dotnet) from a string. Thus we can dynamically create C# code with the desired functionality and then compile a class. The assembly, which is the result of the compilation, can be loaded into the application process. When that is done we obtain a delegate from the dynamically created `MethodInfo` reference.
* Emitted static method - Instead of compiling code, which takes a noticeable amount of time while the application is running, the .NET API can be used for [emitting IL directly](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit?view=netcore-3.0). This would require engineering knowledge of MSIL. At the end we use the same approach and create a delegate from the emitted `MethodInfo` referece. An easy way to emit the MSIL is to manually create one (or several) possible variation(s) of the functionality we need and observe the result (after we build in Release) using an IL decompiler. There are a number of IL decompilers available - ildasm, ILSpy, dotPeek etc.

# Results

I am using .NET Core 3.1 on Windows 10. The hardware is a bit older but still competitive.

```
Intel Core i5-3570 CPU 3.40GHz (Ivy Bridge), 1 CPU, 4 logical and 4 physical cores
.NET Core SDK=3.1.301
```

### Method with a parameter and return type

|                         Method |       Mean |     Error |    StdDev |
|------------------------------- |-----------:|----------:|----------:|
|                         Static |   1.951 ns | 0.0189 ns | 0.0168 ns |
|                     NonVirtual |   1.983 ns | 0.0264 ns | 0.0247 ns |
|                      Interface |   1.958 ns | 0.0136 ns | 0.0127 ns |
|                       Delegate |   2.542 ns | 0.0267 ns | 0.0250 ns |
|             ReflectionDelegate |   2.535 ns | 0.0215 ns | 0.0201 ns |
| CompiledExpressionTreeDelegate |   2.271 ns | 0.0270 ns | 0.0253 ns |
|           CompiledCodeDelegate |   2.481 ns | 0.0161 ns | 0.0151 ns |
|            EmittedCodeDelegate |   2.537 ns | 0.0335 ns | 0.0314 ns |
|                 SlowReflection | 175.244 ns | 1.8769 ns | 1.7557 ns |

### Void method without parameters

|                         Method |      Mean |     Error |    StdDev |
|------------------------------- |----------:|----------:|----------:|
|                         Static |  1.118 ns | 0.0163 ns | 0.0144 ns |
|                     NonVirtual |  1.370 ns | 0.0196 ns | 0.0174 ns |
|                      Interface |  1.433 ns | 0.0046 ns | 0.0043 ns |
|                       Delegate |  1.931 ns | 0.0157 ns | 0.0147 ns |
|             ReflectionDelegate |  1.972 ns | 0.0201 ns | 0.0188 ns |
| CompiledExpressionTreeDelegate |  2.523 ns | 0.0207 ns | 0.0194 ns |
|           CompiledCodeDelegate |  1.948 ns | 0.0268 ns | 0.0251 ns |
|            EmittedCodeDelegate |  1.956 ns | 0.0299 ns | 0.0280 ns |
|                 SlowReflection | 86.080 ns | 0.5168 ns | 0.4834 ns |

# Conclusion

Static, non-virtual and interface methods are the fastest, which is of course no surprise. After that it becomes clear that whatever approach we use, if we end up creating a delegate the performance is equivalent. Finally, a cost of 100-200 ns is rather cheap in my opinion if we don't know the method signature, we don't want to invest time into code generation and our code is not performance-sensitive.