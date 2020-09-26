using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class Object
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public Object()
		{
		}

		public virtual string ToString()
		{
			return GetType().ToString();
		}

		public virtual bool Equals(object obj)
		{
			return InternalEquals(this, obj);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool InternalEquals(object objA, object objB);

		public static bool Equals(object objA, object objB)
		{
			if (objA == objB)
			{
				return true;
			}
			if (objA == null || objB == null)
			{
				return false;
			}
			return objA.Equals(objB);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static bool ReferenceEquals(object objA, object objB)
		{
			return objA == objB;
		}

		public virtual int GetHashCode()
		{
			return InternalGetHashCode(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int InternalGetHashCode(object obj);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern Type GetType();

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		~Object()
		{
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		protected extern object MemberwiseClone();

		private void FieldSetter(string typeName, string fieldName, object val)
		{
			FieldInfo fieldInfo = GetFieldInfo(typeName, fieldName);
			if (fieldInfo.IsInitOnly)
			{
				throw new FieldAccessException(Environment.GetResourceString("FieldAccess_InitOnly"));
			}
			Message.CoerceArg(val, fieldInfo.FieldType);
			fieldInfo.SetValue(this, val);
		}

		private void FieldGetter(string typeName, string fieldName, ref object val)
		{
			FieldInfo fieldInfo = GetFieldInfo(typeName, fieldName);
			val = fieldInfo.GetValue(this);
		}

		private FieldInfo GetFieldInfo(string typeName, string fieldName)
		{
			Type type = GetType();
			while (type != null && !type.FullName.Equals(typeName))
			{
				type = type.BaseType;
			}
			if (type == null)
			{
				throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadType"), typeName));
			}
			FieldInfo field = type.GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
			if (field == null)
			{
				throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadField"), fieldName, typeName));
			}
			return field;
		}
	}
}
