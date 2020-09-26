using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Reflection.Emit
{
	internal class TypeNameBuilder
	{
		internal enum Format
		{
			ToString,
			FullName,
			AssemblyQualifiedName
		}

		private IntPtr m_typeNameBuilder;

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr _CreateTypeNameBuilder();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _ReleaseTypeNameBuilder(IntPtr pAQN);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _OpenGenericArguments(IntPtr tnb);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _CloseGenericArguments(IntPtr tnb);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _OpenGenericArgument(IntPtr tnb);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _CloseGenericArgument(IntPtr tnb);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _AddName(IntPtr tnb, string name);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _AddPointer(IntPtr tnb);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _AddByRef(IntPtr tnb);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _AddSzArray(IntPtr tnb);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _AddArray(IntPtr tnb, int rank);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _AddAssemblySpec(IntPtr tnb, string assemblySpec);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string _ToString(IntPtr tnb);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _Clear(IntPtr tnb);

		internal static string ToString(Type type, Format format)
		{
			if ((format == Format.FullName || format == Format.AssemblyQualifiedName) && !type.IsGenericTypeDefinition && type.ContainsGenericParameters)
			{
				return null;
			}
			TypeNameBuilder typeNameBuilder = new TypeNameBuilder(_CreateTypeNameBuilder());
			typeNameBuilder.Clear();
			typeNameBuilder.ConstructAssemblyQualifiedNameWorker(type, format);
			string result = typeNameBuilder.ToString();
			typeNameBuilder.Dispose();
			return result;
		}

		private TypeNameBuilder(IntPtr typeNameBuilder)
		{
			m_typeNameBuilder = typeNameBuilder;
		}

		internal void Dispose()
		{
			_ReleaseTypeNameBuilder(m_typeNameBuilder);
		}

		private void AddElementType(Type elementType)
		{
			if (elementType.HasElementType)
			{
				AddElementType(elementType.GetElementType());
			}
			if (elementType.IsPointer)
			{
				AddPointer();
			}
			else if (elementType.IsByRef)
			{
				AddByRef();
			}
			else if (elementType.IsSzArray)
			{
				AddSzArray();
			}
			else if (elementType.IsArray)
			{
				AddArray(elementType.GetArrayRank());
			}
		}

		private void ConstructAssemblyQualifiedNameWorker(Type type, Format format)
		{
			Type type2 = type;
			while (type2.HasElementType)
			{
				type2 = type2.GetElementType();
			}
			List<Type> list = new List<Type>();
			for (Type type3 = type2; type3 != null; type3 = (type3.IsGenericParameter ? null : type3.DeclaringType))
			{
				list.Add(type3);
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				Type type4 = list[num];
				string text = type4.Name;
				if (num == list.Count - 1 && type4.Namespace != null && type4.Namespace.Length != 0)
				{
					text = type4.Namespace + "." + text;
				}
				AddName(text);
			}
			if (type2.IsGenericType && (!type2.IsGenericTypeDefinition || format == Format.ToString))
			{
				Type[] genericArguments = type2.GetGenericArguments();
				OpenGenericArguments();
				for (int i = 0; i < genericArguments.Length; i++)
				{
					Format format2 = ((format == Format.FullName) ? Format.AssemblyQualifiedName : format);
					OpenGenericArgument();
					ConstructAssemblyQualifiedNameWorker(genericArguments[i], format2);
					CloseGenericArgument();
				}
				CloseGenericArguments();
			}
			AddElementType(type);
			if (format == Format.AssemblyQualifiedName)
			{
				AddAssemblySpec(type.Module.Assembly.FullName);
			}
		}

		private void OpenGenericArguments()
		{
			_OpenGenericArguments(m_typeNameBuilder);
		}

		private void CloseGenericArguments()
		{
			_CloseGenericArguments(m_typeNameBuilder);
		}

		private void OpenGenericArgument()
		{
			_OpenGenericArgument(m_typeNameBuilder);
		}

		private void CloseGenericArgument()
		{
			_CloseGenericArgument(m_typeNameBuilder);
		}

		private void AddName(string name)
		{
			_AddName(m_typeNameBuilder, name);
		}

		private void AddPointer()
		{
			_AddPointer(m_typeNameBuilder);
		}

		private void AddByRef()
		{
			_AddByRef(m_typeNameBuilder);
		}

		private void AddSzArray()
		{
			_AddSzArray(m_typeNameBuilder);
		}

		private void AddArray(int rank)
		{
			_AddArray(m_typeNameBuilder, rank);
		}

		private void AddAssemblySpec(string assemblySpec)
		{
			_AddAssemblySpec(m_typeNameBuilder, assemblySpec);
		}

		public override string ToString()
		{
			return _ToString(m_typeNameBuilder);
		}

		private void Clear()
		{
			_Clear(m_typeNameBuilder);
		}
	}
}
