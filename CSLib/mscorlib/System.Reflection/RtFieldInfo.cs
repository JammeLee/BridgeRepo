using System.Diagnostics;
using System.Globalization;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	internal sealed class RtFieldInfo : RuntimeFieldInfo, ISerializable
	{
		private RuntimeFieldHandle m_fieldHandle;

		private FieldAttributes m_fieldAttributes;

		private string m_name;

		private RuntimeType m_fieldType;

		private uint m_invocationFlags;

		public override string Name
		{
			get
			{
				if (m_name == null)
				{
					m_name = m_fieldHandle.GetName();
				}
				return m_name;
			}
		}

		public override int MetadataToken => m_fieldHandle.GetToken();

		public override Module Module => m_fieldHandle.GetApproxDeclaringType().GetModuleHandle().GetModule();

		public override RuntimeFieldHandle FieldHandle
		{
			get
			{
				Type declaringType = DeclaringType;
				if ((declaringType == null && Module.Assembly.ReflectionOnly) || declaringType is ReflectionOnlyType)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAllowedInReflectionOnly"));
				}
				return m_fieldHandle;
			}
		}

		public override FieldAttributes Attributes => m_fieldAttributes;

		public override Type FieldType
		{
			get
			{
				if (m_fieldType == null)
				{
					m_fieldType = new Signature(m_fieldHandle, base.DeclaringTypeHandle).FieldTypeHandle.GetRuntimeType();
				}
				return m_fieldType;
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void PerformVisibilityCheckOnField(IntPtr field, object target, IntPtr declaringType, FieldAttributes attr, uint invocationFlags);

		internal RtFieldInfo()
		{
		}

		internal RtFieldInfo(RuntimeFieldHandle handle, RuntimeType declaringType, RuntimeType.RuntimeTypeCache reflectedTypeCache, BindingFlags bindingFlags)
			: base(reflectedTypeCache, declaringType, bindingFlags)
		{
			m_fieldHandle = handle;
			m_fieldAttributes = m_fieldHandle.GetAttributes();
		}

		private void GetOneTimeFlags()
		{
			Type declaringType = DeclaringType;
			uint num = 0u;
			if ((declaringType != null && declaringType.ContainsGenericParameters) || (declaringType == null && Module.Assembly.ReflectionOnly) || declaringType is ReflectionOnlyType)
			{
				num |= 2u;
			}
			else
			{
				AssemblyBuilderData assemblyData = Module.Assembly.m_assemblyData;
				if (assemblyData != null && (assemblyData.m_access & AssemblyBuilderAccess.Run) == 0)
				{
					num |= 2u;
				}
			}
			if (num == 0)
			{
				if ((m_fieldAttributes & FieldAttributes.InitOnly) != 0)
				{
					num |= 0x10u;
				}
				if ((m_fieldAttributes & FieldAttributes.HasFieldRVA) != 0)
				{
					num |= 0x10u;
				}
				if ((m_fieldAttributes & FieldAttributes.FieldAccessMask) != FieldAttributes.Public || (declaringType != null && !declaringType.IsVisible))
				{
					num |= 4u;
				}
				Type fieldType = FieldType;
				if (fieldType.IsPointer || fieldType.IsEnum || fieldType.IsPrimitive)
				{
					num |= 0x20u;
				}
			}
			num = (m_invocationFlags = num | 1u);
		}

		private void CheckConsistency(object target)
		{
			if ((m_fieldAttributes & FieldAttributes.Static) != FieldAttributes.Static && !m_declaringType.IsInstanceOfType(target))
			{
				if (target == null)
				{
					throw new TargetException(Environment.GetResourceString("RFLCT.Targ_StatFldReqTarg"));
				}
				throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Arg_FieldDeclTarget"), Name, m_declaringType, target.GetType()));
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal override bool CacheEquals(object o)
		{
			return (o as RtFieldInfo)?.m_fieldHandle.Equals(m_fieldHandle) ?? false;
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		internal void InternalSetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture, bool doVisibilityCheck)
		{
			InternalSetValue(obj, value, invokeAttr, binder, culture, doVisibilityCheck, doCheckConsistency: true);
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		internal void InternalSetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture, bool doVisibilityCheck, bool doCheckConsistency)
		{
			RuntimeType runtimeType = DeclaringType as RuntimeType;
			if ((m_invocationFlags & 1) == 0)
			{
				GetOneTimeFlags();
			}
			if ((m_invocationFlags & 2u) != 0)
			{
				if (runtimeType != null && runtimeType.ContainsGenericParameters)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Arg_UnboundGenField"));
				}
				if ((runtimeType == null && Module.Assembly.ReflectionOnly) || runtimeType is ReflectionOnlyType)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Arg_ReflectionOnlyField"));
				}
				throw new FieldAccessException();
			}
			if (doCheckConsistency)
			{
				CheckConsistency(obj);
			}
			value = ((RuntimeType)FieldType).CheckValue(value, binder, culture, invokeAttr);
			if (doVisibilityCheck && (m_invocationFlags & 0x14u) != 0)
			{
				PerformVisibilityCheckOnField(m_fieldHandle.Value, obj, m_declaringType.TypeHandle.Value, m_fieldAttributes, m_invocationFlags);
			}
			bool domainInitialized = false;
			if (runtimeType == null)
			{
				m_fieldHandle.SetValue(obj, value, FieldType.TypeHandle, m_fieldAttributes, RuntimeTypeHandle.EmptyHandle, ref domainInitialized);
				return;
			}
			domainInitialized = runtimeType.DomainInitialized;
			m_fieldHandle.SetValue(obj, value, FieldType.TypeHandle, m_fieldAttributes, DeclaringType.TypeHandle, ref domainInitialized);
			runtimeType.DomainInitialized = domainInitialized;
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		internal object InternalGetValue(object obj, bool doVisibilityCheck)
		{
			return InternalGetValue(obj, doVisibilityCheck, doCheckConsistency: true);
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		internal object InternalGetValue(object obj, bool doVisibilityCheck, bool doCheckConsistency)
		{
			RuntimeType runtimeType = DeclaringType as RuntimeType;
			if ((m_invocationFlags & 1) == 0)
			{
				GetOneTimeFlags();
			}
			if ((m_invocationFlags & 2u) != 0)
			{
				if (runtimeType != null && DeclaringType.ContainsGenericParameters)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Arg_UnboundGenField"));
				}
				if ((runtimeType == null && Module.Assembly.ReflectionOnly) || runtimeType is ReflectionOnlyType)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Arg_ReflectionOnlyField"));
				}
				throw new FieldAccessException();
			}
			if (doCheckConsistency)
			{
				CheckConsistency(obj);
			}
			RuntimeTypeHandle typeHandle = FieldType.TypeHandle;
			if (doVisibilityCheck && (m_invocationFlags & 4u) != 0)
			{
				PerformVisibilityCheckOnField(m_fieldHandle.Value, obj, m_declaringType.TypeHandle.Value, m_fieldAttributes, m_invocationFlags & 0xFFFFFFEFu);
			}
			bool domainInitialized = false;
			if (runtimeType == null)
			{
				return m_fieldHandle.GetValue(obj, typeHandle, RuntimeTypeHandle.EmptyHandle, ref domainInitialized);
			}
			domainInitialized = runtimeType.DomainInitialized;
			object value = m_fieldHandle.GetValue(obj, typeHandle, DeclaringType.TypeHandle, ref domainInitialized);
			runtimeType.DomainInitialized = domainInitialized;
			return value;
		}

		public override object GetValue(object obj)
		{
			return InternalGetValue(obj, doVisibilityCheck: true);
		}

		public override object GetRawConstantValue()
		{
			throw new InvalidOperationException();
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public override object GetValueDirect(TypedReference obj)
		{
			if (obj.IsNull)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_TypedReference_Null"));
			}
			return m_fieldHandle.GetValueDirect(FieldType.TypeHandle, obj, (DeclaringType == null) ? RuntimeTypeHandle.EmptyHandle : DeclaringType.TypeHandle);
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
		{
			InternalSetValue(obj, value, invokeAttr, binder, culture, doVisibilityCheck: true);
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public override void SetValueDirect(TypedReference obj, object value)
		{
			if (obj.IsNull)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_TypedReference_Null"));
			}
			m_fieldHandle.SetValueDirect(FieldType.TypeHandle, obj, value, (DeclaringType == null) ? RuntimeTypeHandle.EmptyHandle : DeclaringType.TypeHandle);
		}

		internal override RuntimeFieldHandle GetFieldHandle()
		{
			return m_fieldHandle;
		}

		public override Type[] GetRequiredCustomModifiers()
		{
			return new Signature(m_fieldHandle, base.DeclaringTypeHandle).GetCustomModifiers(1, required: true);
		}

		public override Type[] GetOptionalCustomModifiers()
		{
			return new Signature(m_fieldHandle, base.DeclaringTypeHandle).GetCustomModifiers(1, required: false);
		}
	}
}
