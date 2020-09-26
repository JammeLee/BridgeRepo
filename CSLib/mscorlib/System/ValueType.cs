using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public abstract class ValueType
	{
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			RuntimeType runtimeType = (RuntimeType)GetType();
			RuntimeType runtimeType2 = (RuntimeType)obj.GetType();
			if (runtimeType2 != runtimeType)
			{
				return false;
			}
			if (CanCompareBits(this))
			{
				return FastEqualsCheck(this, obj);
			}
			FieldInfo[] fields = runtimeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				object obj2 = ((RtFieldInfo)fields[i]).InternalGetValue(this, doVisibilityCheck: false);
				object obj3 = ((RtFieldInfo)fields[i]).InternalGetValue(obj, doVisibilityCheck: false);
				if (obj2 == null)
				{
					if (obj3 != null)
					{
						return false;
					}
				}
				else if (!obj2.Equals(obj3))
				{
					return false;
				}
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool CanCompareBits(object obj);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool FastEqualsCheck(object a, object b);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public override extern int GetHashCode();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int GetHashCodeOfPtr(IntPtr ptr);

		public override string ToString()
		{
			return GetType().ToString();
		}
	}
}
