using System;
using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ReflectionPerformance.Approaches;

namespace ReflectionPerformance
{
	[SimpleJob(RuntimeMoniker.NetCoreApp31)]
	public class MethodParametersJob
	{
		private int _request;
		private MethodInfo _staticMethod;

		// non-virtual
		private NonVirtualMethod _nonVirtualMethod;

		// interface
		private IInterfaceMethod _interfaceMethod;

		// delegates
		private Func<int, int> _delegate;

		// reflection delegates
		private Func<int, int> _reflectionDelegate;

		// compiled code
		private Func<int, int> _compiledCodeDelegate;

		// emitted code
		private Func<int, int> _emittedCodeDelegate;

		// compiled expression tree
		private Func<int, int> _compiledExpressionTreeDelegate;

		// slow reflection
		private object[] _slowArgs;

		[GlobalSetup]
		public void GlobalSetup()
		{
			_request = 1;
			_staticMethod = typeof(StaticMethod).GetMethod(nameof(StaticMethod.Parameters));
			if (_staticMethod == null)
			{
				throw new InvalidOperationException("Could not get static method with parameters.");
			}

			// non-virtual
			_nonVirtualMethod = new NonVirtualMethod();

			// interface
			_interfaceMethod = new InterfaceMethod();

			// delegates
			_delegate = StaticMethod.Parameters;

			// reflection delegates
			_reflectionDelegate = (Func<int, int>) _staticMethod.CreateDelegate(typeof(Func<int, int>));

			// compiled expression tree
			int response = 1;
			Expression<Func<int, int>> expressionTree = request => response;
			_compiledExpressionTreeDelegate = expressionTree.Compile(preferInterpretation: false);

			// compiled code
			CompiledCodeGenerator.CompileCode();
			_compiledCodeDelegate = CompiledCodeGenerator.CompiledMethodParameters;

			// emitted code
			EmittedCodeGenerator.EmitCode();
			_emittedCodeDelegate = EmittedCodeGenerator.EmittedMethodParameters;

			// slow reflection
			_slowArgs = new object[] {1};
		}

		[Benchmark]
		public void Static()
		{
			StaticMethod.Parameters(_request);
		}

		[Benchmark]
		public void NonVirtual()
		{
			_nonVirtualMethod.Parameters(_request);
		}

		[Benchmark]
		public void Interface()
		{
			_interfaceMethod.Parameters(_request);
		}

		[Benchmark]
		public void Delegate()
		{
			_delegate(_request);
		}

		[Benchmark]
		public void ReflectionDelegate()
		{
			_reflectionDelegate(_request);
		}

		[Benchmark]
		public void CompiledExpressionTreeDelegate()
		{
			_compiledExpressionTreeDelegate(_request);
		}

		[Benchmark]
		public void CompiledCodeDelegate()
		{
			_compiledCodeDelegate(_request);
		}

		[Benchmark]
		public void EmittedCodeDelegate()
		{
			_emittedCodeDelegate(_request);
		}

		[Benchmark]
		public void SlowReflection()
		{
			_staticMethod.Invoke(obj: null, parameters: _slowArgs);
		}
	}
}
