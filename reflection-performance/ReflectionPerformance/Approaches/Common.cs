using System.Runtime.CompilerServices;

namespace ReflectionPerformance.Approaches
{
	public static class StaticMethod
	{
		private static readonly int _response = 1;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static int Parameters(int request) => _response;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void Void() {}
	}

	public sealed class NonVirtualMethod
	{
		private static readonly int _response = 1;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public int Parameters(int request) => _response;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void Void() {}
	}

	public interface IInterfaceMethod
	{
		int Parameters(int request);

		void Void();
	}

	public sealed class InterfaceMethod : IInterfaceMethod
	{
		private static readonly int _response = 1;

		int IInterfaceMethod.Parameters(int request) => _response;

		void IInterfaceMethod.Void() {}
	}
}
