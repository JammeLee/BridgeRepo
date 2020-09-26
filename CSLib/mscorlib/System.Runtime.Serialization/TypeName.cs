using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.Serialization
{
	internal class TypeName
	{
		[ComImport]
		[TypeLibType(256)]
		[Guid("B81FF171-20F3-11D2-8DCC-00A0C9B00522")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface ITypeName
		{
			uint GetNameCount();

			uint GetNames([In] uint count, IntPtr rgbszNamesArray);

			uint GetTypeArgumentCount();

			uint GetTypeArguments([In] uint count, IntPtr rgpArgumentsArray);

			uint GetModifierLength();

			uint GetModifiers([In] uint count, out uint rgModifiers);

			[return: MarshalAs(UnmanagedType.BStr)]
			string GetAssemblyName();
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[TypeLibType(256)]
		[Guid("B81FF171-20F3-11D2-8DCC-00A0C9B00521")]
		internal interface ITypeNameFactory
		{
			[return: MarshalAs(UnmanagedType.Interface)]
			ITypeName ParseTypeName([In][MarshalAs(UnmanagedType.LPWStr)] string szName, out int pError);
		}

		private TypeName()
		{
		}

		internal static Type GetType(Assembly initialAssembly, string fullTypeName)
		{
			Type typeFromCLSID = Type.GetTypeFromCLSID(new Guid(3089101169u, 8435, 4562, 141, 204, 0, 160, 201, 176, 5, 37));
			ITypeNameFactory typeNameFactory = (ITypeNameFactory)Activator.CreateInstance(typeFromCLSID);
			int pError;
			ITypeName typeNameInfo = typeNameFactory.ParseTypeName(fullTypeName, out pError);
			Type result = null;
			if (pError == -1)
			{
				result = LoadTypeWithPartialName(typeNameInfo, initialAssembly, fullTypeName);
			}
			return result;
		}

		private static Type LoadTypeWithPartialName(ITypeName typeNameInfo, Assembly initialAssembly, string fullTypeName)
		{
			uint nameCount = typeNameInfo.GetNameCount();
			uint typeArgumentCount = typeNameInfo.GetTypeArgumentCount();
			IntPtr[] array = new IntPtr[nameCount];
			IntPtr[] array2 = new IntPtr[typeArgumentCount];
			try
			{
				Type type = null;
				if (nameCount != 0)
				{
					GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
					nameCount = typeNameInfo.GetNames(nameCount, gCHandle.AddrOfPinnedObject());
					gCHandle.Free();
					string text = Marshal.PtrToStringBSTR(array[0]);
					string assemblyName = typeNameInfo.GetAssemblyName();
					if (string.IsNullOrEmpty(assemblyName))
					{
						type = ((initialAssembly == null) ? Type.GetType(text) : initialAssembly.GetType(text));
					}
					else
					{
						Assembly assembly = Assembly.LoadWithPartialName(assemblyName);
						if (assembly == null)
						{
							assembly = Assembly.LoadWithPartialName(new AssemblyName(assemblyName).Name);
						}
						type = assembly.GetType(text);
					}
					if (type == null)
					{
						throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadType"), fullTypeName));
					}
					for (int i = 1; i < nameCount; i++)
					{
						string name = Marshal.PtrToStringBSTR(array[i]);
						type = type.GetNestedType(name, BindingFlags.Public | BindingFlags.NonPublic);
						if (type == null)
						{
							throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadType"), fullTypeName));
						}
					}
					if (typeArgumentCount != 0)
					{
						GCHandle gCHandle2 = GCHandle.Alloc(array2, GCHandleType.Pinned);
						typeArgumentCount = typeNameInfo.GetTypeArguments(typeArgumentCount, gCHandle2.AddrOfPinnedObject());
						gCHandle2.Free();
						Type[] array3 = new Type[typeArgumentCount];
						for (int j = 0; j < typeArgumentCount; j++)
						{
							array3[j] = LoadTypeWithPartialName((ITypeName)Marshal.GetObjectForIUnknown(array2[j]), null, fullTypeName);
						}
						return type.MakeGenericType(array3);
					}
					return type;
				}
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadType"), fullTypeName));
			}
			finally
			{
				for (int k = 0; k < array.Length; k++)
				{
					_ = array[k];
					Marshal.FreeBSTR(array[k]);
				}
				for (int l = 0; l < array2.Length; l++)
				{
					_ = array2[l];
					Marshal.Release(array2[l]);
				}
			}
		}
	}
}
