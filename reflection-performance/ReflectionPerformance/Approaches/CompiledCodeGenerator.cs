using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace ReflectionPerformance.Approaches
{
	public static class CompiledCodeGenerator
	{
		/// <summary>
		/// Copy-pasted in the string below which gets compiled. Class and method names are used in the code.
		/// </summary>
		public static class CompiledClass
		{
			private static readonly int _response = 1;

			public static int CompiledMethodParameters(int request) => _response;

			public static void CompiledMethodVoid() {}
		}

		private const string GeneratedCode =
@"
public static class CompiledClass
{
	private static readonly int _response = 1;

	public static int CompiledMethodParameters(int request) => _response;

	public static void CompiledMethodVoid() {}
}
";

		public static Func<int, int> CompiledMethodParameters { get; private set; }

		public static Action CompiledMethodVoid { get; private set; }

		public static void CompileCode()
		{
			Assembly compiledAssembly = LoadCompiledAssembly();
			Type compiledClass = compiledAssembly.GetType(nameof(CompiledClass));
			if (compiledClass == null)
			{
				throw new InvalidOperationException("Could not get compiled class.");
			}

			MethodInfo compiledMethodParameters = compiledClass.GetMethod(nameof(CompiledClass.CompiledMethodParameters));
			if (compiledMethodParameters == null)
			{
				throw new InvalidOperationException("Could not get compiled method with parameters.");
			}
			CompiledMethodParameters = (Func<int, int>) compiledMethodParameters.CreateDelegate(typeof(Func<int, int>));

			MethodInfo compiledMethodVoid = compiledClass.GetMethod(nameof(CompiledClass.CompiledMethodVoid));
			if (compiledMethodVoid == null)
			{
				throw new InvalidOperationException("Could not get compiled method void.");
			}
			CompiledMethodVoid = (Action) compiledMethodVoid.CreateDelegate(typeof(Action));
		}

		private static Assembly LoadCompiledAssembly()
		{
			using MemoryStream assemblyStream = new MemoryStream();
			CompileAssembly(assemblyStream);
			assemblyStream.Seek(0, SeekOrigin.Begin);

			AssemblyLoadContext loadContext = new AssemblyLoadContext("dynamic-load-context");
			Assembly assembly = loadContext.LoadFromStream(assemblyStream);
			return assembly;
		}

		private static void CompileAssembly(Stream assemblyStream)
		{
			SourceText source = SourceText.From(GeneratedCode);
			CSharpParseOptions parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7);
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

			List<MetadataReference> references = CreateReferences();

			CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(
				outputKind: OutputKind.DynamicallyLinkedLibrary,
				optimizationLevel: OptimizationLevel.Release,
				assemblyIdentityComparer: AssemblyIdentityComparer.Default);

			CSharpCompilation compilation = CSharpCompilation.Create(
				assemblyName: "CompiledAssembly.dll",
				syntaxTrees: new[] {syntaxTree},
				references, compilationOptions);

			EmitResult emitResult = compilation.Emit(assemblyStream);
			if (!emitResult.Success)
			{
				foreach (Diagnostic diagnostic in emitResult.Diagnostics)
				{
					Console.WriteLine("{0} {1}:{2}", diagnostic.Severity, diagnostic.Id, diagnostic.GetMessage());
				}

				throw new InvalidOperationException("Compilation failed with errors.");
			}
		}

		private static List<MetadataReference> CreateReferences()
		{
			List<MetadataReference> references = new List<MetadataReference>();

			foreach (Assembly referencedAssembly in AssemblyLoadContext.Default.Assemblies)
			{
				if (referencedAssembly.IsDynamic)
					continue;

				MetadataReference reference = MetadataReference.CreateFromFile(referencedAssembly.Location);
				references.Add(reference);
			}

			return references;
		}
	}
}