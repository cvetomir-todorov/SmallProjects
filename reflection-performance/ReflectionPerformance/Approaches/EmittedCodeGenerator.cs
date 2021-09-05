using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ReflectionPerformance.Approaches
{
	public static class EmittedCodeGenerator
	{
		/// <summary>
		/// The type we are going to emit. It's useful to observe it using a IL decompiler so that we know what IL opcodes to emit.
		/// </summary>
		private static class EmittedClass
		{
			public static readonly int Response = 1;

			public static int EmittedMethodParameters(int request) => Response;

			public static void EmittedMethodVoid() {}
		}

		public static Func<int, int> EmittedMethodParameters { get; private set; }

		public static Action EmittedMethodVoid { get; private set; }

		public static void EmitCode()
		{
			// build assembly, module, type
			AssemblyName assemblyName = new AssemblyName("EmittedAssembly");
			AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("EmittedModule");
			TypeBuilder typeBuilder = moduleBuilder.DefineType(nameof(EmittedClass), TypeAttributes.Public);

			// build type members
			FieldBuilder responseFieldBuilder = typeBuilder.DefineField(
				nameof(EmittedClass.Response), typeof(int), FieldAttributes.Private | FieldAttributes.Static);
			EmitTypeInitializer(typeBuilder, responseFieldBuilder);
			MethodBuilder methodParametersBuilder = EmitMethodParameters(typeBuilder, responseFieldBuilder);
			MethodBuilder methodVoidBuilder = EmitMethodVoid(typeBuilder);

			CreateDelegates(typeBuilder, methodParametersBuilder, methodVoidBuilder);
		}

		private static void EmitTypeInitializer(TypeBuilder typeBuilder, FieldBuilder responseFieldBuilder)
		{
			ConstructorBuilder typeInitializerBuilder = typeBuilder.DefineTypeInitializer();
			ILGenerator initializerIL = typeInitializerBuilder.GetILGenerator();

			initializerIL.Emit(OpCodes.Ldc_I4_1); // load int32 value of 1 onto the stack
			initializerIL.Emit(OpCodes.Stsfld, responseFieldBuilder); // set value to the response static field
			initializerIL.Emit(OpCodes.Ret); // return
		}

		private static MethodBuilder EmitMethodParameters(TypeBuilder typeBuilder, FieldBuilder responseFieldBuilder)
		{
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(
				name: nameof(EmittedClass.EmittedMethodParameters),
				attributes: MethodAttributes.Public | MethodAttributes.Static,
				returnType: typeof(int), parameterTypes: new[] {typeof(int)});
			ILGenerator methodIL = methodBuilder.GetILGenerator();

			methodIL.Emit(OpCodes.Ldsfld, responseFieldBuilder); // load the response static field
			methodIL.Emit(OpCodes.Ret); // return it

			return methodBuilder;
		}

		private static MethodBuilder EmitMethodVoid(TypeBuilder typeBuilder)
		{
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(
				name: nameof(EmittedClass.EmittedMethodVoid),
				attributes: MethodAttributes.Public | MethodAttributes.Static,
				returnType: typeof(void), parameterTypes: new Type[0]);
			ILGenerator methodIL = methodBuilder.GetILGenerator();

			methodIL.Emit(OpCodes.Ret); // simple return

			return methodBuilder;
		}

		private static void CreateDelegates(TypeBuilder typeBuilder, MethodBuilder methodParametersBuilder, MethodBuilder methodVoidBuilder)
		{
			Type createdType = typeBuilder.CreateType();
			if (createdType == null)
			{
				throw new InvalidOperationException("Could not get emitted class.");
			}

			MethodInfo methodParameters = createdType.GetMethod(methodParametersBuilder.Name);
			if (methodParameters == null)
			{
				throw new InvalidOperationException($"Could not get method {methodParametersBuilder.Name} from type {typeBuilder.Name}.");
			}
			EmittedMethodParameters = (Func<int, int>) methodParameters.CreateDelegate(typeof(Func<int, int>));

			MethodInfo methodVoid = createdType.GetMethod(methodVoidBuilder.Name);
			if (methodVoid == null)
			{
				throw new InvalidOperationException($"Could not get method {methodVoidBuilder.Name} from type {typeBuilder.Name}.");
			}
			EmittedMethodVoid = (Action) methodVoid.CreateDelegate(typeof(Action));
		}
	}
}
