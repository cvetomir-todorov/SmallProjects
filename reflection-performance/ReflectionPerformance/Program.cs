using System;
using BenchmarkDotNet.Running;
using ReflectionPerformance.Approaches;

namespace ReflectionPerformance
{
	public static class Program
	{
		public static void Main(string[] args)
		{
#if DEBUG
			CompiledCodeGenerator.CompileCode();
			CompiledCodeGenerator.CompiledMethodParameters(1);
			CompiledCodeGenerator.CompiledMethodVoid();

			EmittedCodeGenerator.EmitCode();
			EmittedCodeGenerator.EmittedMethodParameters(1);
			EmittedCodeGenerator.EmittedMethodVoid();
#elif RELEASE
			BenchmarkRunner.Run<MethodParametersJob>();
			//BenchmarkRunner.Run<MethodVoidJob>();
#endif
		}
	}
}
