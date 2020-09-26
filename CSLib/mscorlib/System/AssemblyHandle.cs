using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{
	internal struct AssemblyHandle
	{
		private IntPtr m_ptr;

		internal unsafe void* Value => m_ptr.ToPointer();

		internal unsafe AssemblyHandle(void* pAssembly)
		{
			m_ptr = new IntPtr(pAssembly);
		}

		public override int GetHashCode()
		{
			return ValueType.GetHashCodeOfPtr(m_ptr);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is AssemblyHandle))
			{
				return false;
			}
			return ((AssemblyHandle)obj).m_ptr == m_ptr;
		}

		public bool Equals(AssemblyHandle handle)
		{
			return handle.m_ptr == m_ptr;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern Assembly GetAssembly();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _GetManifestModule();

		internal unsafe ModuleHandle GetManifestModule()
		{
			return new ModuleHandle(_GetManifestModule());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool _AptcaCheck(IntPtr sourceAssembly);

		internal unsafe bool AptcaCheck(AssemblyHandle sourceAssembly)
		{
			return _AptcaCheck((IntPtr)sourceAssembly.Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int GetToken();
	}
}
