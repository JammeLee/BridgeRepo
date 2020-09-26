using System.Reflection;

namespace System.Runtime.InteropServices
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
	[ComVisible(true)]
	public sealed class StructLayoutAttribute : Attribute
	{
		private const int DEFAULT_PACKING_SIZE = 8;

		internal LayoutKind _val;

		public int Pack;

		public int Size;

		public CharSet CharSet;

		public LayoutKind Value => _val;

		internal static Attribute GetCustomAttribute(Type type)
		{
			if (!IsDefined(type))
			{
				return null;
			}
			int packSize = 0;
			int classSize = 0;
			LayoutKind layoutKind = LayoutKind.Auto;
			switch (type.Attributes & TypeAttributes.LayoutMask)
			{
			case TypeAttributes.ExplicitLayout:
				layoutKind = LayoutKind.Explicit;
				break;
			case TypeAttributes.NotPublic:
				layoutKind = LayoutKind.Auto;
				break;
			case TypeAttributes.SequentialLayout:
				layoutKind = LayoutKind.Sequential;
				break;
			}
			CharSet charSet = CharSet.None;
			switch (type.Attributes & TypeAttributes.StringFormatMask)
			{
			case TypeAttributes.NotPublic:
				charSet = CharSet.Ansi;
				break;
			case TypeAttributes.AutoClass:
				charSet = CharSet.Auto;
				break;
			case TypeAttributes.UnicodeClass:
				charSet = CharSet.Unicode;
				break;
			}
			type.Module.MetadataImport.GetClassLayout(type.MetadataToken, out packSize, out classSize);
			if (packSize == 0)
			{
				packSize = 8;
			}
			return new StructLayoutAttribute(layoutKind, packSize, classSize, charSet);
		}

		internal static bool IsDefined(Type type)
		{
			if (type.IsInterface || type.HasElementType || type.IsGenericParameter)
			{
				return false;
			}
			return true;
		}

		internal StructLayoutAttribute(LayoutKind layoutKind, int pack, int size, CharSet charSet)
		{
			_val = layoutKind;
			Pack = pack;
			Size = size;
			CharSet = charSet;
		}

		public StructLayoutAttribute(LayoutKind layoutKind)
		{
			_val = layoutKind;
		}

		public StructLayoutAttribute(short layoutKind)
		{
			_val = (LayoutKind)layoutKind;
		}
	}
}
