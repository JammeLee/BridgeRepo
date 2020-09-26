using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	internal sealed class DelegateSerializationHolder : IObjectReference, ISerializable
	{
		[Serializable]
		internal class DelegateEntry
		{
			internal string type;

			internal string assembly;

			internal object target;

			internal string targetTypeAssembly;

			internal string targetTypeName;

			internal string methodName;

			internal DelegateEntry delegateEntry;

			internal DelegateEntry Entry
			{
				get
				{
					return delegateEntry;
				}
				set
				{
					delegateEntry = value;
				}
			}

			internal DelegateEntry(string type, string assembly, object target, string targetTypeAssembly, string targetTypeName, string methodName)
			{
				this.type = type;
				this.assembly = assembly;
				this.target = target;
				this.targetTypeAssembly = targetTypeAssembly;
				this.targetTypeName = targetTypeName;
				this.methodName = methodName;
			}
		}

		private DelegateEntry m_delegateEntry;

		private MethodInfo[] m_methods;

		internal static DelegateEntry GetDelegateSerializationInfo(SerializationInfo info, Type delegateType, object target, MethodInfo method, int targetIndex)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			if (!method.IsPublic || (method.DeclaringType != null && !method.DeclaringType.IsVisible))
			{
				new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
			}
			Type baseType = delegateType.BaseType;
			if (baseType == null || (baseType != typeof(Delegate) && baseType != typeof(MulticastDelegate)))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
			}
			if (method.DeclaringType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_GlobalMethodSerialization"));
			}
			DelegateEntry delegateEntry = new DelegateEntry(delegateType.FullName, delegateType.Module.Assembly.FullName, target, method.ReflectedType.Module.Assembly.FullName, method.ReflectedType.FullName, method.Name);
			if (info.MemberCount == 0)
			{
				info.SetType(typeof(DelegateSerializationHolder));
				info.AddValue("Delegate", delegateEntry, typeof(DelegateEntry));
			}
			if (target != null)
			{
				string text = "target" + targetIndex;
				info.AddValue(text, delegateEntry.target);
				delegateEntry.target = text;
			}
			string name = "method" + targetIndex;
			info.AddValue(name, method);
			return delegateEntry;
		}

		private DelegateSerializationHolder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			bool flag = true;
			try
			{
				m_delegateEntry = (DelegateEntry)info.GetValue("Delegate", typeof(DelegateEntry));
			}
			catch
			{
				m_delegateEntry = OldDelegateWireFormat(info, context);
				flag = false;
			}
			if (!flag)
			{
				return;
			}
			DelegateEntry delegateEntry = m_delegateEntry;
			int num = 0;
			while (delegateEntry != null)
			{
				if (delegateEntry.target != null)
				{
					string text = delegateEntry.target as string;
					if (text != null)
					{
						delegateEntry.target = info.GetValue(text, typeof(object));
					}
				}
				num++;
				delegateEntry = delegateEntry.delegateEntry;
			}
			MethodInfo[] array = new MethodInfo[num];
			int i;
			for (i = 0; i < num; i++)
			{
				string name = "method" + i;
				array[i] = (MethodInfo)info.GetValueNoThrow(name, typeof(MethodInfo));
				if (array[i] == null)
				{
					break;
				}
			}
			if (i == num)
			{
				m_methods = array;
			}
		}

		private void ThrowInsufficientState(string field)
		{
			throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_InsufficientDeserializationState"), field));
		}

		private DelegateEntry OldDelegateWireFormat(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			string @string = info.GetString("DelegateType");
			string string2 = info.GetString("DelegateAssembly");
			object value = info.GetValue("Target", typeof(object));
			string string3 = info.GetString("TargetTypeAssembly");
			string string4 = info.GetString("TargetTypeName");
			string string5 = info.GetString("MethodName");
			return new DelegateEntry(@string, string2, value, string3, string4, string5);
		}

		private Delegate GetDelegate(DelegateEntry de, int index)
		{
			try
			{
				if (de.methodName == null || de.methodName.Length == 0)
				{
					ThrowInsufficientState("MethodName");
				}
				if (de.assembly == null || de.assembly.Length == 0)
				{
					ThrowInsufficientState("DelegateAssembly");
				}
				if (de.targetTypeName == null || de.targetTypeName.Length == 0)
				{
					ThrowInsufficientState("TargetTypeName");
				}
				Type type = Assembly.Load(de.assembly).GetType(de.type, throwOnError: true, ignoreCase: false);
				Type type2 = Assembly.Load(de.targetTypeAssembly).GetType(de.targetTypeName, throwOnError: true, ignoreCase: false);
				Delegate @delegate;
				if (m_methods == null)
				{
					@delegate = ((de.target == null) ? Delegate.CreateDelegate(type, type2, de.methodName) : Delegate.CreateDelegate(type, RemotingServices.CheckCast(de.target, type2), de.methodName));
				}
				else
				{
					object firstArgument = ((de.target != null) ? RemotingServices.CheckCast(de.target, type2) : null);
					@delegate = Delegate.InternalCreateDelegate(type, firstArgument, m_methods[index]);
				}
				if (@delegate.Method == null || @delegate.Method.IsPublic)
				{
					if (@delegate.Method.DeclaringType == null)
					{
						return @delegate;
					}
					if (@delegate.Method.DeclaringType.IsVisible)
					{
						return @delegate;
					}
				}
				new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
				return @delegate;
			}
			catch (Exception ex)
			{
				if (ex is SerializationException)
				{
					throw ex;
				}
				throw new SerializationException(ex.Message, ex);
			}
			catch
			{
				throw new SerializationException();
			}
		}

		public object GetRealObject(StreamingContext context)
		{
			int num = 0;
			for (DelegateEntry delegateEntry = m_delegateEntry; delegateEntry != null; delegateEntry = delegateEntry.Entry)
			{
				num++;
			}
			int num2 = num - 1;
			if (num == 1)
			{
				return GetDelegate(m_delegateEntry, 0);
			}
			object[] array = new object[num];
			for (DelegateEntry delegateEntry2 = m_delegateEntry; delegateEntry2 != null; delegateEntry2 = delegateEntry2.Entry)
			{
				num--;
				array[num] = GetDelegate(delegateEntry2, num2 - num);
			}
			return ((MulticastDelegate)array[0]).NewMulticastDelegate(array, array.Length);
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DelegateSerHolderSerial"));
		}
	}
}
