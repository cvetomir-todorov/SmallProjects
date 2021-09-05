using System;
using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ReflectionPerformance.Approaches;

namespace ReflectionPerformance
{
	[SimpleJob(RuntimeMoniker.NetCoreApp31)]
	public class MethodVoidJob
	{
		private MethodInfo _staticMethod;

		// non-virtual
		private NonVirtualMethod _nonVirtualMethod;

		// interface
		private IInterfaceMethod _interfaceMethod;

		// delegates
		private Action _delegate;

		// reflection delegates
		private Action _reflectionDelegate;

		// compiled code
		private Action _compiledCodeDelegate;

		// emitted code
		private Action _emittedCodeDelegate;

		// compiled expression tree
		private Action _compiledExpressionTreeDelegate;
		private void EmptyMethod() {}
		// slow reflection
		private object[] _slowArgs;

		[GlobalSetup]
		public void GlobalSetup()
		{
			_staticMethod = typeof(StaticMethod).GetMethod(nameof(StaticMethod.Void));
			if (_staticMethod == null)
			{
				throw new InvalidOperationException("Could not get static method void.");
			}

			// non-virtual
			_nonVirtualMethod = new NonVirtualMethod();

			// interface
			_interfaceMethod = new InterfaceMethod();

			// delegates
			_delegate = StaticMethod.Void;

			// reflection delegates
			_reflectionDelegate = (Action)_staticMethod.CreateDelegate(typeof(Action));

			// compiled expression tree
			Expression<Action> expressionTree = () => EmptyMethod();
			_compiledExpressionTreeDelegate = expressionTree.Compile(preferInterpretation: false);

			// compiled code
			CompiledCodeGenerator.CompileCode();
			_compiledCodeDelegate = CompiledCodeGenerator.CompiledMethodVoid;

			// emitted code
			EmittedCodeGenerator.EmitCode();
			_emittedCodeDelegate = EmittedCodeGenerator.EmittedMethodVoid;

			// slow reflection
			_slowArgs = new object[0];
		}

		[Benchmark]
		public void Static()
		{
			StaticMethod.Void();
		}

		[Benchmark]
		public void NonVirtual()
		{
			_nonVirtualMethod.Void();
		}

		[Benchmark]
		public void Interface()
		{
			_interfaceMethod.Void();
		}

		[Benchmark]
		public void Delegate()
		{
			_delegate();
		}

		[Benchmark]
		public void ReflectionDelegate()
		{
			_reflectionDelegate();
		}

		[Benchmark]
		public void CompiledExpressionTreeDelegate()
		{
			_compiledExpressionTreeDelegate();
		}

		[Benchmark]
		public void CompiledCodeDelegate()
		{
			_compiledCodeDelegate();
		}

		[Benchmark]
		public void EmittedCodeDelegate()
		{
			_emittedCodeDelegate();
		}

		[Benchmark]
		public void SlowReflection()
		{
			_staticMethod.Invoke(obj: null, parameters: _slowArgs);
		}
	}
}
