using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Win32
{
	internal sealed class OAVariantLib
	{
		public const int NoValueProp = 1;

		public const int AlphaBool = 2;

		public const int NoUserOverride = 4;

		public const int CalendarHijri = 8;

		public const int LocalBool = 16;

		private const int CV_OBJECT = 18;

		internal static readonly Type[] ClassTypes = new Type[23]
		{
			typeof(Empty),
			typeof(void),
			typeof(bool),
			typeof(char),
			typeof(sbyte),
			typeof(byte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(float),
			typeof(double),
			typeof(string),
			typeof(void),
			typeof(DateTime),
			typeof(TimeSpan),
			typeof(object),
			typeof(decimal),
			null,
			typeof(Missing),
			typeof(DBNull)
		};

		private OAVariantLib()
		{
		}

		internal static Variant ChangeType(Variant source, Type targetClass, short options, CultureInfo culture)
		{
			if (targetClass == null)
			{
				throw new ArgumentNullException("targetClass");
			}
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			Variant result = default(Variant);
			ChangeTypeEx(ref result, source, culture.LCID, GetCVTypeFromClass(targetClass), options);
			return result;
		}

		private static int GetCVTypeFromClass(Type ctype)
		{
			int num = -1;
			for (int i = 0; i < ClassTypes.Length; i++)
			{
				if (ctype.Equals(ClassTypes[i]))
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				num = 18;
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void ChangeTypeEx(ref Variant result, Variant source, int lcid, int cvType, short flags);
	}
}
