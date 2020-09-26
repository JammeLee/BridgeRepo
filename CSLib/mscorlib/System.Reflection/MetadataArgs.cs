using System.Runtime.InteropServices;

namespace System.Reflection
{
	[Serializable]
	internal static class MetadataArgs
	{
		[Serializable]
		[StructLayout(LayoutKind.Auto)]
		[ComVisible(true)]
		public struct SkipAddresses
		{
			public string String;

			public int[] Int32Array;

			public byte[] ByteArray;

			public MetadataFieldOffset[] MetadataFieldOffsetArray;

			public int Int32;

			public TypeAttributes TypeAttributes;

			public MethodAttributes MethodAttributes;

			public PropertyAttributes PropertyAttributes;

			public MethodImplAttributes MethodImplAttributes;

			public ParameterAttributes ParameterAttributes;

			public FieldAttributes FieldAttributes;

			public EventAttributes EventAttributes;

			public MetadataColumnType MetadataColumnType;

			public PInvokeAttributes PInvokeAttributes;

			public MethodSemanticsAttributes MethodSemanticsAttributes;

			public DeclSecurityAttributes DeclSecurityAttributes;

			public CorElementType CorElementType;

			public ConstArray ConstArray;

			public Guid Guid;
		}

		public static SkipAddresses Skip = default(SkipAddresses);
	}
}
