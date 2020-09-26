using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public abstract class MulticastDelegate : Delegate
	{
		private object _invocationList;

		private IntPtr _invocationCount;

		protected MulticastDelegate(object target, string method)
			: base(target, method)
		{
		}

		protected MulticastDelegate(Type target, string method)
			: base(target, method)
		{
		}

		internal bool IsUnmanagedFunctionPtr()
		{
			return _invocationCount == (IntPtr)(-1);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			int targetIndex = 0;
			object[] array = _invocationList as object[];
			if (array == null)
			{
				MethodInfo method = base.Method;
				if (method is DynamicMethod || method is DynamicMethod.RTDynamicMethod || IsUnmanagedFunctionPtr())
				{
					throw new SerializationException(Environment.GetResourceString("Serialization_InvalidDelegateType"));
				}
				if (_invocationList != null && !_invocationCount.IsNull())
				{
					throw new SerializationException(Environment.GetResourceString("Serialization_InvalidDelegateType"));
				}
				DelegateSerializationHolder.GetDelegateSerializationInfo(info, GetType(), base.Target, method, targetIndex);
				return;
			}
			DelegateSerializationHolder.DelegateEntry delegateEntry = null;
			int num = (int)_invocationCount;
			int num2 = num;
			while (--num2 >= 0)
			{
				MulticastDelegate multicastDelegate = (MulticastDelegate)array[num2];
				MethodInfo method2 = multicastDelegate.Method;
				if (!(method2 is DynamicMethod) && !(method2 is DynamicMethod.RTDynamicMethod) && !IsUnmanagedFunctionPtr() && (multicastDelegate._invocationList == null || multicastDelegate._invocationCount.IsNull()))
				{
					DelegateSerializationHolder.DelegateEntry delegateSerializationInfo = DelegateSerializationHolder.GetDelegateSerializationInfo(info, multicastDelegate.GetType(), multicastDelegate.Target, method2, targetIndex++);
					if (delegateEntry != null)
					{
						delegateEntry.Entry = delegateSerializationInfo;
					}
					delegateEntry = delegateSerializationInfo;
				}
			}
			if (delegateEntry != null)
			{
				return;
			}
			throw new SerializationException(Environment.GetResourceString("Serialization_InvalidDelegateType"));
		}

		public sealed override bool Equals(object obj)
		{
			if (obj == null || !Delegate.InternalEqualTypes(this, obj))
			{
				return false;
			}
			MulticastDelegate multicastDelegate = obj as MulticastDelegate;
			if ((object)multicastDelegate == null)
			{
				return false;
			}
			if (_invocationCount != (IntPtr)0)
			{
				if (_invocationList == null)
				{
					if (IsUnmanagedFunctionPtr())
					{
						if (!multicastDelegate.IsUnmanagedFunctionPtr())
						{
							return false;
						}
						if (_methodPtr != multicastDelegate._methodPtr)
						{
							return false;
						}
						if (GetUnmanagedCallSite() != multicastDelegate.GetUnmanagedCallSite())
						{
							return false;
						}
						return true;
					}
					return base.Equals(obj);
				}
				if (_invocationList is Delegate)
				{
					return _invocationList.Equals(obj);
				}
				return InvocationListEquals(multicastDelegate);
			}
			if (_invocationList != null)
			{
				if (!_invocationList.Equals(multicastDelegate._invocationList))
				{
					return false;
				}
				return base.Equals((object)multicastDelegate);
			}
			if (multicastDelegate._invocationList != null || multicastDelegate._invocationCount != (IntPtr)0)
			{
				if (multicastDelegate._invocationList is Delegate)
				{
					return (multicastDelegate._invocationList as Delegate).Equals(this);
				}
				return false;
			}
			return base.Equals((object)multicastDelegate);
		}

		private bool InvocationListEquals(MulticastDelegate d)
		{
			object[] array = _invocationList as object[];
			if (d._invocationCount != _invocationCount)
			{
				return false;
			}
			int num = (int)_invocationCount;
			for (int i = 0; i < num; i++)
			{
				Delegate @delegate = (Delegate)array[i];
				object[] array2 = d._invocationList as object[];
				if (!@delegate.Equals(array2[i]))
				{
					return false;
				}
			}
			return true;
		}

		private bool TrySetSlot(object[] a, int index, object o)
		{
			if (a[index] == null && Interlocked.CompareExchange(ref a[index], o, null) == null)
			{
				return true;
			}
			if (a[index] != null)
			{
				MulticastDelegate multicastDelegate = (MulticastDelegate)o;
				MulticastDelegate multicastDelegate2 = (MulticastDelegate)a[index];
				if (multicastDelegate2._methodPtr == multicastDelegate._methodPtr && multicastDelegate2._target == multicastDelegate._target && multicastDelegate2._methodPtrAux == multicastDelegate._methodPtrAux)
				{
					return true;
				}
			}
			return false;
		}

		internal MulticastDelegate NewMulticastDelegate(object[] invocationList, int invocationCount, bool thisIsMultiCastAlready)
		{
			MulticastDelegate multicastDelegate = Delegate.InternalAllocLike(this);
			if (thisIsMultiCastAlready)
			{
				multicastDelegate._methodPtr = _methodPtr;
				multicastDelegate._methodPtrAux = _methodPtrAux;
			}
			else
			{
				multicastDelegate._methodPtr = GetMulticastInvoke();
				multicastDelegate._methodPtrAux = GetInvokeMethod();
			}
			multicastDelegate._target = multicastDelegate;
			multicastDelegate._invocationList = invocationList;
			multicastDelegate._invocationCount = (IntPtr)invocationCount;
			return multicastDelegate;
		}

		internal MulticastDelegate NewMulticastDelegate(object[] invocationList, int invocationCount)
		{
			return NewMulticastDelegate(invocationList, invocationCount, thisIsMultiCastAlready: false);
		}

		internal void StoreDynamicMethod(MethodInfo dynamicMethod)
		{
			if (_invocationCount != (IntPtr)0)
			{
				MulticastDelegate multicastDelegate = (MulticastDelegate)_invocationList;
				multicastDelegate._methodBase = dynamicMethod;
			}
			else
			{
				_methodBase = dynamicMethod;
			}
		}

		protected sealed override Delegate CombineImpl(Delegate follow)
		{
			if ((object)follow == null)
			{
				return this;
			}
			if (!Delegate.InternalEqualTypes(this, follow))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTypeMis"));
			}
			MulticastDelegate multicastDelegate = (MulticastDelegate)follow;
			int num = 1;
			object[] array = multicastDelegate._invocationList as object[];
			if (array != null)
			{
				num = (int)multicastDelegate._invocationCount;
			}
			object[] array2 = _invocationList as object[];
			int num2;
			object[] array3;
			if (array2 == null)
			{
				num2 = 1 + num;
				array3 = new object[num2];
				array3[0] = this;
				if (array == null)
				{
					array3[1] = multicastDelegate;
				}
				else
				{
					for (int i = 0; i < num; i++)
					{
						array3[1 + i] = array[i];
					}
				}
				return NewMulticastDelegate(array3, num2);
			}
			int num3 = (int)_invocationCount;
			num2 = num3 + num;
			array3 = null;
			if (num2 <= array2.Length)
			{
				array3 = array2;
				if (array == null)
				{
					if (!TrySetSlot(array3, num3, multicastDelegate))
					{
						array3 = null;
					}
				}
				else
				{
					for (int j = 0; j < num; j++)
					{
						if (!TrySetSlot(array3, num3 + j, array[j]))
						{
							array3 = null;
							break;
						}
					}
				}
			}
			if (array3 == null)
			{
				int num4;
				for (num4 = array2.Length; num4 < num2; num4 *= 2)
				{
				}
				array3 = new object[num4];
				for (int k = 0; k < num3; k++)
				{
					array3[k] = array2[k];
				}
				if (array == null)
				{
					array3[num3] = multicastDelegate;
				}
				else
				{
					for (int l = 0; l < num; l++)
					{
						array3[num3 + l] = array[l];
					}
				}
			}
			return NewMulticastDelegate(array3, num2, thisIsMultiCastAlready: true);
		}

		private object[] DeleteFromInvocationList(object[] invocationList, int invocationCount, int deleteIndex, int deleteCount)
		{
			object[] array = _invocationList as object[];
			int num = array.Length;
			while (num / 2 >= invocationCount - deleteCount)
			{
				num /= 2;
			}
			object[] array2 = new object[num];
			for (int i = 0; i < deleteIndex; i++)
			{
				array2[i] = invocationList[i];
			}
			for (int j = deleteIndex + deleteCount; j < invocationCount; j++)
			{
				array2[j - deleteCount] = invocationList[j];
			}
			return array2;
		}

		private bool EqualInvocationLists(object[] a, object[] b, int start, int count)
		{
			for (int i = 0; i < count; i++)
			{
				if (!a[start + i].Equals(b[i]))
				{
					return false;
				}
			}
			return true;
		}

		protected sealed override Delegate RemoveImpl(Delegate value)
		{
			MulticastDelegate multicastDelegate = value as MulticastDelegate;
			if ((object)multicastDelegate == null)
			{
				return this;
			}
			if (!(multicastDelegate._invocationList is object[]))
			{
				object[] array = _invocationList as object[];
				if (array == null)
				{
					if (Equals(value))
					{
						return null;
					}
				}
				else
				{
					int num = (int)_invocationCount;
					int num2 = num;
					while (--num2 >= 0)
					{
						if (value.Equals(array[num2]))
						{
							if (num == 2)
							{
								return (Delegate)array[1 - num2];
							}
							object[] invocationList = DeleteFromInvocationList(array, num, num2, 1);
							return NewMulticastDelegate(invocationList, num - 1, thisIsMultiCastAlready: true);
						}
					}
				}
			}
			else
			{
				object[] array2 = _invocationList as object[];
				if (array2 != null)
				{
					int num3 = (int)_invocationCount;
					int num4 = (int)multicastDelegate._invocationCount;
					for (int num5 = num3 - num4; num5 >= 0; num5--)
					{
						if (EqualInvocationLists(array2, multicastDelegate._invocationList as object[], num5, num4))
						{
							if (num3 - num4 == 0)
							{
								return null;
							}
							if (num3 - num4 == 1)
							{
								return (Delegate)array2[(num5 == 0) ? (num3 - 1) : 0];
							}
							object[] invocationList2 = DeleteFromInvocationList(array2, num3, num5, num4);
							return NewMulticastDelegate(invocationList2, num3 - num4, thisIsMultiCastAlready: true);
						}
					}
				}
			}
			return this;
		}

		public sealed override Delegate[] GetInvocationList()
		{
			object[] array = _invocationList as object[];
			Delegate[] array2;
			if (array == null)
			{
				array2 = new Delegate[1]
				{
					this
				};
			}
			else
			{
				int num = (int)_invocationCount;
				array2 = new Delegate[num];
				for (int i = 0; i < num; i++)
				{
					array2[i] = (Delegate)array[i];
				}
			}
			return array2;
		}

		public static bool operator ==(MulticastDelegate d1, MulticastDelegate d2)
		{
			return d1?.Equals(d2) ?? ((object)d2 == null);
		}

		public static bool operator !=(MulticastDelegate d1, MulticastDelegate d2)
		{
			if ((object)d1 == null)
			{
				return (object)d2 != null;
			}
			return !d1.Equals(d2);
		}

		public sealed override int GetHashCode()
		{
			if (IsUnmanagedFunctionPtr())
			{
				return (int)(long)_methodPtr;
			}
			object[] array = _invocationList as object[];
			if (array == null)
			{
				return base.GetHashCode();
			}
			int num = 0;
			for (int i = 0; i < (int)_invocationCount; i++)
			{
				num = num * 33 + array[i].GetHashCode();
			}
			return num;
		}

		internal override object GetTarget()
		{
			if (_invocationCount != (IntPtr)0)
			{
				if (_invocationList == null)
				{
					return null;
				}
				object[] array = _invocationList as object[];
				if (array != null)
				{
					int num = (int)_invocationCount;
					return ((Delegate)array[num - 1]).GetTarget();
				}
				Delegate @delegate = _invocationList as Delegate;
				if ((object)@delegate != null)
				{
					return @delegate.GetTarget();
				}
			}
			return base.GetTarget();
		}

		protected override MethodInfo GetMethodImpl()
		{
			if (_invocationCount != (IntPtr)0 && _invocationList != null)
			{
				object[] array = _invocationList as object[];
				if (array != null)
				{
					int num = (int)_invocationCount - 1;
					return ((Delegate)array[num]).Method;
				}
				return ((MulticastDelegate)_invocationList).GetMethodImpl();
			}
			return base.GetMethodImpl();
		}

		[DebuggerNonUserCode]
		private void ThrowNullThisInDelegateToInstance()
		{
			throw new ArgumentException(Environment.GetResourceString("Arg_DlgtNullInst"));
		}

		[DebuggerNonUserCode]
		private void CtorClosed(object target, IntPtr methodPtr)
		{
			if (target == null)
			{
				ThrowNullThisInDelegateToInstance();
			}
			_target = target;
			_methodPtr = methodPtr;
		}

		[DebuggerNonUserCode]
		private void CtorClosedStatic(object target, IntPtr methodPtr)
		{
			_target = target;
			_methodPtr = methodPtr;
		}

		[DebuggerNonUserCode]
		private void CtorRTClosed(object target, IntPtr methodPtr)
		{
			_target = target;
			_methodPtr = AdjustTarget(target, methodPtr);
		}

		[DebuggerNonUserCode]
		private void CtorOpened(object target, IntPtr methodPtr, IntPtr shuffleThunk)
		{
			_target = this;
			_methodPtr = shuffleThunk;
			_methodPtrAux = methodPtr;
		}

		[DebuggerNonUserCode]
		private void CtorSecureClosed(object target, IntPtr methodPtr, IntPtr callThunk, IntPtr assembly)
		{
			MulticastDelegate multicastDelegate = Delegate.InternalAlloc(Type.GetTypeHandle(this));
			multicastDelegate.CtorClosed(target, methodPtr);
			_invocationList = multicastDelegate;
			_target = this;
			_methodPtr = callThunk;
			_methodPtrAux = assembly;
			_invocationCount = GetInvokeMethod();
		}

		[DebuggerNonUserCode]
		private void CtorSecureClosedStatic(object target, IntPtr methodPtr, IntPtr callThunk, IntPtr assembly)
		{
			MulticastDelegate multicastDelegate = Delegate.InternalAlloc(Type.GetTypeHandle(this));
			multicastDelegate.CtorClosedStatic(target, methodPtr);
			_invocationList = multicastDelegate;
			_target = this;
			_methodPtr = callThunk;
			_methodPtrAux = assembly;
			_invocationCount = GetInvokeMethod();
		}

		[DebuggerNonUserCode]
		private void CtorSecureRTClosed(object target, IntPtr methodPtr, IntPtr callThunk, IntPtr assembly)
		{
			MulticastDelegate multicastDelegate = Delegate.InternalAlloc(Type.GetTypeHandle(this));
			multicastDelegate.CtorRTClosed(target, methodPtr);
			_invocationList = multicastDelegate;
			_target = this;
			_methodPtr = callThunk;
			_methodPtrAux = assembly;
			_invocationCount = GetInvokeMethod();
		}

		[DebuggerNonUserCode]
		private void CtorSecureOpened(object target, IntPtr methodPtr, IntPtr shuffleThunk, IntPtr callThunk, IntPtr assembly)
		{
			MulticastDelegate multicastDelegate = Delegate.InternalAlloc(Type.GetTypeHandle(this));
			multicastDelegate.CtorOpened(target, methodPtr, shuffleThunk);
			_invocationList = multicastDelegate;
			_target = this;
			_methodPtr = callThunk;
			_methodPtrAux = assembly;
			_invocationCount = GetInvokeMethod();
		}

		[DebuggerNonUserCode]
		private void CtorVirtualDispatch(object target, IntPtr methodPtr, IntPtr shuffleThunk)
		{
			_target = this;
			_methodPtr = shuffleThunk;
			_methodPtrAux = GetCallStub(methodPtr);
		}

		[DebuggerNonUserCode]
		private void CtorSecureVirtualDispatch(object target, IntPtr methodPtr, IntPtr shuffleThunk, IntPtr callThunk, IntPtr assembly)
		{
			MulticastDelegate multicastDelegate = Delegate.InternalAlloc(Type.GetTypeHandle(this));
			multicastDelegate.CtorVirtualDispatch(target, methodPtr, shuffleThunk);
			_invocationList = multicastDelegate;
			_target = this;
			_methodPtr = callThunk;
			_methodPtrAux = assembly;
			_invocationCount = GetInvokeMethod();
		}
	}
}
