using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;

namespace System.Globalization
{
	internal sealed class GlobalizationAssembly
	{
		private static Hashtable m_assemblyHash = Hashtable.Synchronized(new Hashtable(4));

		internal static GlobalizationAssembly m_defaultInstance = GetGlobalizationAssembly(Assembly.GetAssembly(typeof(GlobalizationAssembly)));

		internal Hashtable compareInfoCache;

		internal unsafe void* pNativeGlobalizationAssembly;

		internal static GlobalizationAssembly DefaultInstance
		{
			get
			{
				if (m_defaultInstance == null)
				{
					throw new TypeLoadException("Failure has occurred while loading a type.");
				}
				return m_defaultInstance;
			}
		}

		internal static GlobalizationAssembly GetGlobalizationAssembly(Assembly assembly)
		{
			GlobalizationAssembly result;
			if ((result = (GlobalizationAssembly)m_assemblyHash[assembly]) == null)
			{
				RuntimeHelpers.TryCode code = CreateGlobalizationAssembly;
				RuntimeHelpers.ExecuteCodeWithLock(typeof(CultureTableRecord), code, assembly);
				result = (GlobalizationAssembly)m_assemblyHash[assembly];
			}
			return result;
		}

		[PrePrepareMethod]
		private unsafe static void CreateGlobalizationAssembly(object assem)
		{
			Assembly assembly = (Assembly)assem;
			GlobalizationAssembly globalizationAssembly;
			if ((globalizationAssembly = (GlobalizationAssembly)m_assemblyHash[assembly]) == null)
			{
				globalizationAssembly = new GlobalizationAssembly();
				globalizationAssembly.pNativeGlobalizationAssembly = nativeCreateGlobalizationAssembly(assembly);
				Thread.MemoryBarrier();
				m_assemblyHash[assembly] = globalizationAssembly;
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void* _nativeCreateGlobalizationAssembly(Assembly assembly);

		private unsafe static void* nativeCreateGlobalizationAssembly(Assembly assembly)
		{
			return _nativeCreateGlobalizationAssembly(assembly.InternalAssembly);
		}

		internal GlobalizationAssembly()
		{
			compareInfoCache = new Hashtable(4);
		}

		internal unsafe static byte* GetGlobalizationResourceBytePtr(Assembly assembly, string tableName)
		{
			Stream manifestResourceStream = assembly.GetManifestResourceStream(tableName);
			UnmanagedMemoryStream unmanagedMemoryStream = manifestResourceStream as UnmanagedMemoryStream;
			if (unmanagedMemoryStream != null)
			{
				byte* positionPointer = unmanagedMemoryStream.PositionPointer;
				if (positionPointer != null)
				{
					return positionPointer;
				}
			}
			throw new ExecutionEngineException();
		}
	}
}
