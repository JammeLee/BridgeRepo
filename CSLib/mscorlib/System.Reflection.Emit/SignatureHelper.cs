using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Reflection.Emit
{
	[ClassInterface(ClassInterfaceType.None)]
	[ComDefaultInterface(typeof(_SignatureHelper))]
	[ComVisible(true)]
	public sealed class SignatureHelper : _SignatureHelper
	{
		internal const int mdtTypeRef = 16777216;

		internal const int mdtTypeDef = 33554432;

		internal const int mdtTypeSpec = 553648128;

		internal const byte ELEMENT_TYPE_END = 0;

		internal const byte ELEMENT_TYPE_VOID = 1;

		internal const byte ELEMENT_TYPE_BOOLEAN = 2;

		internal const byte ELEMENT_TYPE_CHAR = 3;

		internal const byte ELEMENT_TYPE_I1 = 4;

		internal const byte ELEMENT_TYPE_U1 = 5;

		internal const byte ELEMENT_TYPE_I2 = 6;

		internal const byte ELEMENT_TYPE_U2 = 7;

		internal const byte ELEMENT_TYPE_I4 = 8;

		internal const byte ELEMENT_TYPE_U4 = 9;

		internal const byte ELEMENT_TYPE_I8 = 10;

		internal const byte ELEMENT_TYPE_U8 = 11;

		internal const byte ELEMENT_TYPE_R4 = 12;

		internal const byte ELEMENT_TYPE_R8 = 13;

		internal const byte ELEMENT_TYPE_STRING = 14;

		internal const byte ELEMENT_TYPE_PTR = 15;

		internal const byte ELEMENT_TYPE_BYREF = 16;

		internal const byte ELEMENT_TYPE_VALUETYPE = 17;

		internal const byte ELEMENT_TYPE_CLASS = 18;

		internal const byte ELEMENT_TYPE_VAR = 19;

		internal const byte ELEMENT_TYPE_ARRAY = 20;

		internal const byte ELEMENT_TYPE_GENERICINST = 21;

		internal const byte ELEMENT_TYPE_TYPEDBYREF = 22;

		internal const byte ELEMENT_TYPE_I = 24;

		internal const byte ELEMENT_TYPE_U = 25;

		internal const byte ELEMENT_TYPE_FNPTR = 27;

		internal const byte ELEMENT_TYPE_OBJECT = 28;

		internal const byte ELEMENT_TYPE_SZARRAY = 29;

		internal const byte ELEMENT_TYPE_MVAR = 30;

		internal const byte ELEMENT_TYPE_CMOD_REQD = 31;

		internal const byte ELEMENT_TYPE_CMOD_OPT = 32;

		internal const byte ELEMENT_TYPE_INTERNAL = 33;

		internal const byte ELEMENT_TYPE_MAX = 34;

		internal const byte ELEMENT_TYPE_SENTINEL = 65;

		internal const byte ELEMENT_TYPE_PINNED = 69;

		internal const int IMAGE_CEE_UNMANAGED_CALLCONV_C = 1;

		internal const int IMAGE_CEE_UNMANAGED_CALLCONV_STDCALL = 2;

		internal const int IMAGE_CEE_UNMANAGED_CALLCONV_THISCALL = 3;

		internal const int IMAGE_CEE_UNMANAGED_CALLCONV_FASTCALL = 4;

		internal const int IMAGE_CEE_CS_CALLCONV_DEFAULT = 0;

		internal const int IMAGE_CEE_CS_CALLCONV_VARARG = 5;

		internal const int IMAGE_CEE_CS_CALLCONV_FIELD = 6;

		internal const int IMAGE_CEE_CS_CALLCONV_LOCAL_SIG = 7;

		internal const int IMAGE_CEE_CS_CALLCONV_PROPERTY = 8;

		internal const int IMAGE_CEE_CS_CALLCONV_UNMGD = 9;

		internal const int IMAGE_CEE_CS_CALLCONV_GENERICINST = 10;

		internal const int IMAGE_CEE_CS_CALLCONV_MAX = 11;

		internal const int IMAGE_CEE_CS_CALLCONV_MASK = 15;

		internal const int IMAGE_CEE_CS_CALLCONV_GENERIC = 16;

		internal const int IMAGE_CEE_CS_CALLCONV_HASTHIS = 32;

		internal const int IMAGE_CEE_CS_CALLCONV_RETPARAM = 64;

		internal const int NO_SIZE_IN_SIG = -1;

		private byte[] m_signature;

		private int m_currSig;

		private int m_sizeLoc;

		private ModuleBuilder m_module;

		private bool m_sigDone;

		private int m_argCount;

		internal int ArgumentCount => m_argCount;

		public static SignatureHelper GetMethodSigHelper(Module mod, Type returnType, Type[] parameterTypes)
		{
			return GetMethodSigHelper(mod, CallingConventions.Standard, returnType, null, null, parameterTypes, null, null);
		}

		internal static SignatureHelper GetMethodSigHelper(Module mod, CallingConventions callingConvention, Type returnType, int cGenericParam)
		{
			return GetMethodSigHelper(mod, callingConvention, cGenericParam, returnType, null, null, null, null, null);
		}

		public static SignatureHelper GetMethodSigHelper(Module mod, CallingConventions callingConvention, Type returnType)
		{
			return GetMethodSigHelper(mod, callingConvention, returnType, null, null, null, null, null);
		}

		internal static SignatureHelper GetMethodSpecSigHelper(Module scope, Type[] inst)
		{
			SignatureHelper signatureHelper = new SignatureHelper(scope, 10);
			signatureHelper.AddData(inst.Length);
			foreach (Type clsArgument in inst)
			{
				signatureHelper.AddArgument(clsArgument);
			}
			return signatureHelper;
		}

		internal static SignatureHelper GetMethodSigHelper(Module scope, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			return GetMethodSigHelper(scope, callingConvention, 0, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
		}

		internal static SignatureHelper GetMethodSigHelper(Module scope, CallingConventions callingConvention, int cGenericParam, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			if (returnType == null)
			{
				returnType = typeof(void);
			}
			int num = 0;
			if ((callingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
			{
				num = 5;
			}
			if (cGenericParam > 0)
			{
				num |= 0x10;
			}
			if ((callingConvention & CallingConventions.HasThis) == CallingConventions.HasThis)
			{
				num |= 0x20;
			}
			SignatureHelper signatureHelper = new SignatureHelper(scope, num, cGenericParam, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers);
			signatureHelper.AddArguments(parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
			return signatureHelper;
		}

		public static SignatureHelper GetMethodSigHelper(Module mod, CallingConvention unmanagedCallConv, Type returnType)
		{
			if (returnType == null)
			{
				returnType = typeof(void);
			}
			int callingConvention;
			switch (unmanagedCallConv)
			{
			case CallingConvention.Cdecl:
				callingConvention = 1;
				break;
			case CallingConvention.Winapi:
			case CallingConvention.StdCall:
				callingConvention = 2;
				break;
			case CallingConvention.ThisCall:
				callingConvention = 3;
				break;
			case CallingConvention.FastCall:
				callingConvention = 4;
				break;
			default:
				throw new ArgumentException(Environment.GetResourceString("Argument_UnknownUnmanagedCallConv"), "unmanagedCallConv");
			}
			return new SignatureHelper(mod, callingConvention, returnType, null, null);
		}

		public static SignatureHelper GetLocalVarSigHelper()
		{
			return GetLocalVarSigHelper(null);
		}

		public static SignatureHelper GetMethodSigHelper(CallingConventions callingConvention, Type returnType)
		{
			return GetMethodSigHelper(null, callingConvention, returnType);
		}

		public static SignatureHelper GetMethodSigHelper(CallingConvention unmanagedCallingConvention, Type returnType)
		{
			return GetMethodSigHelper(null, unmanagedCallingConvention, returnType);
		}

		public static SignatureHelper GetLocalVarSigHelper(Module mod)
		{
			return new SignatureHelper(mod, 7);
		}

		public static SignatureHelper GetFieldSigHelper(Module mod)
		{
			return new SignatureHelper(mod, 6);
		}

		public static SignatureHelper GetPropertySigHelper(Module mod, Type returnType, Type[] parameterTypes)
		{
			return GetPropertySigHelper(mod, returnType, null, null, parameterTypes, null, null);
		}

		public static SignatureHelper GetPropertySigHelper(Module mod, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			return GetPropertySigHelper(mod, (CallingConventions)0, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
		}

		public static SignatureHelper GetPropertySigHelper(Module mod, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			if (returnType == null)
			{
				returnType = typeof(void);
			}
			int num = 8;
			if ((callingConvention & CallingConventions.HasThis) == CallingConventions.HasThis)
			{
				num |= 0x20;
			}
			SignatureHelper signatureHelper = new SignatureHelper(mod, num, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers);
			signatureHelper.AddArguments(parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
			return signatureHelper;
		}

		internal static SignatureHelper GetTypeSigToken(Module mod, Type type)
		{
			if (mod == null)
			{
				throw new ArgumentNullException("module");
			}
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return new SignatureHelper(mod, type);
		}

		private SignatureHelper(Module mod, int callingConvention)
		{
			Init(mod, callingConvention);
		}

		private SignatureHelper(Module mod, int callingConvention, int cGenericParameters, Type returnType, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			Init(mod, callingConvention, cGenericParameters);
			if (callingConvention == 6)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadFieldSig"));
			}
			AddOneArgTypeHelper(returnType, requiredCustomModifiers, optionalCustomModifiers);
		}

		private SignatureHelper(Module mod, int callingConvention, Type returnType, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
			: this(mod, callingConvention, 0, returnType, requiredCustomModifiers, optionalCustomModifiers)
		{
		}

		private SignatureHelper(Module mod, Type type)
		{
			Init(mod);
			AddOneArgTypeHelper(type);
		}

		private void Init(Module mod)
		{
			m_signature = new byte[32];
			m_currSig = 0;
			m_module = mod as ModuleBuilder;
			m_argCount = 0;
			m_sigDone = false;
			m_sizeLoc = -1;
			if (m_module == null && mod != null)
			{
				throw new ArgumentException(Environment.GetResourceString("NotSupported_MustBeModuleBuilder"));
			}
		}

		private void Init(Module mod, int callingConvention)
		{
			Init(mod, callingConvention, 0);
		}

		private void Init(Module mod, int callingConvention, int cGenericParam)
		{
			Init(mod);
			AddData(callingConvention);
			if (callingConvention == 6 || callingConvention == 10)
			{
				m_sizeLoc = -1;
				return;
			}
			if (cGenericParam > 0)
			{
				AddData(cGenericParam);
			}
			m_sizeLoc = m_currSig++;
		}

		private void AddOneArgTypeHelper(Type argument, bool pinned)
		{
			if (pinned)
			{
				AddElementType(69);
			}
			AddOneArgTypeHelper(argument);
		}

		private void AddOneArgTypeHelper(Type clsArgument, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			if (optionalCustomModifiers != null)
			{
				for (int i = 0; i < optionalCustomModifiers.Length; i++)
				{
					AddElementType(32);
					AddToken(m_module.GetTypeToken(optionalCustomModifiers[i]).Token);
				}
			}
			if (requiredCustomModifiers != null)
			{
				for (int j = 0; j < requiredCustomModifiers.Length; j++)
				{
					AddElementType(31);
					AddToken(m_module.GetTypeToken(requiredCustomModifiers[j]).Token);
				}
			}
			AddOneArgTypeHelper(clsArgument);
		}

		private void AddOneArgTypeHelper(Type clsArgument)
		{
			AddOneArgTypeHelperWorker(clsArgument, lastWasGenericInst: false);
		}

		private void AddOneArgTypeHelperWorker(Type clsArgument, bool lastWasGenericInst)
		{
			if (clsArgument.IsGenericParameter)
			{
				if (clsArgument.DeclaringMethod != null)
				{
					AddData(30);
				}
				else
				{
					AddData(19);
				}
				AddData(clsArgument.GenericParameterPosition);
				return;
			}
			if (clsArgument.IsGenericType && (!clsArgument.IsGenericTypeDefinition || !lastWasGenericInst))
			{
				AddElementType(21);
				AddOneArgTypeHelperWorker(clsArgument.GetGenericTypeDefinition(), lastWasGenericInst: true);
				Type[] genericArguments = clsArgument.GetGenericArguments();
				AddData(genericArguments.Length);
				Type[] array = genericArguments;
				foreach (Type clsArgument2 in array)
				{
					AddOneArgTypeHelper(clsArgument2);
				}
				return;
			}
			if (clsArgument is TypeBuilder)
			{
				TypeBuilder typeBuilder = (TypeBuilder)clsArgument;
				TypeToken clsToken = ((!typeBuilder.Module.Equals(m_module)) ? m_module.GetTypeToken(clsArgument) : typeBuilder.TypeToken);
				if (clsArgument.IsValueType)
				{
					InternalAddTypeToken(clsToken, 17);
				}
				else
				{
					InternalAddTypeToken(clsToken, 18);
				}
				return;
			}
			if (clsArgument is EnumBuilder)
			{
				TypeBuilder typeBuilder2 = ((EnumBuilder)clsArgument).m_typeBuilder;
				TypeToken clsToken2 = ((!typeBuilder2.Module.Equals(m_module)) ? m_module.GetTypeToken(clsArgument) : typeBuilder2.TypeToken);
				if (clsArgument.IsValueType)
				{
					InternalAddTypeToken(clsToken2, 17);
				}
				else
				{
					InternalAddTypeToken(clsToken2, 18);
				}
				return;
			}
			if (clsArgument.IsByRef)
			{
				AddElementType(16);
				clsArgument = clsArgument.GetElementType();
				AddOneArgTypeHelper(clsArgument);
				return;
			}
			if (clsArgument.IsPointer)
			{
				AddElementType(15);
				AddOneArgTypeHelper(clsArgument.GetElementType());
				return;
			}
			if (clsArgument.IsArray)
			{
				if (clsArgument.IsSzArray)
				{
					AddElementType(29);
					AddOneArgTypeHelper(clsArgument.GetElementType());
					return;
				}
				AddElementType(20);
				AddOneArgTypeHelper(clsArgument.GetElementType());
				AddData(clsArgument.GetArrayRank());
				AddData(0);
				AddData(0);
				return;
			}
			RuntimeType runtimeType = clsArgument as RuntimeType;
			int num = ((runtimeType != null) ? GetCorElementTypeFromClass(runtimeType) : 34);
			if (IsSimpleType(num))
			{
				AddElementType(num);
			}
			else if (clsArgument == typeof(object))
			{
				AddElementType(28);
			}
			else if (clsArgument == typeof(string))
			{
				AddElementType(14);
			}
			else if (m_module == null)
			{
				InternalAddRuntimeType(runtimeType);
			}
			else if (clsArgument.IsValueType)
			{
				InternalAddTypeToken(m_module.GetTypeToken(clsArgument), 17);
			}
			else
			{
				InternalAddTypeToken(m_module.GetTypeToken(clsArgument), 18);
			}
		}

		private void AddData(int data)
		{
			if (m_currSig + 4 >= m_signature.Length)
			{
				m_signature = ExpandArray(m_signature);
			}
			if (data <= 127)
			{
				m_signature[m_currSig++] = (byte)((uint)data & 0xFFu);
				return;
			}
			if (data <= 16383)
			{
				m_signature[m_currSig++] = (byte)((uint)(data >> 8) | 0x80u);
				m_signature[m_currSig++] = (byte)((uint)data & 0xFFu);
				return;
			}
			if (data <= 536870911)
			{
				m_signature[m_currSig++] = (byte)((uint)(data >> 24) | 0xC0u);
				m_signature[m_currSig++] = (byte)((uint)(data >> 16) & 0xFFu);
				m_signature[m_currSig++] = (byte)((uint)(data >> 8) & 0xFFu);
				m_signature[m_currSig++] = (byte)((uint)data & 0xFFu);
				return;
			}
			throw new ArgumentException(Environment.GetResourceString("Argument_LargeInteger"));
		}

		private void AddData(uint data)
		{
			if (m_currSig + 4 >= m_signature.Length)
			{
				m_signature = ExpandArray(m_signature);
			}
			m_signature[m_currSig++] = (byte)(data & 0xFFu);
			m_signature[m_currSig++] = (byte)((data >> 8) & 0xFFu);
			m_signature[m_currSig++] = (byte)((data >> 16) & 0xFFu);
			m_signature[m_currSig++] = (byte)((data >> 24) & 0xFFu);
		}

		private void AddData(ulong data)
		{
			if (m_currSig + 8 >= m_signature.Length)
			{
				m_signature = ExpandArray(m_signature);
			}
			m_signature[m_currSig++] = (byte)(data & 0xFF);
			m_signature[m_currSig++] = (byte)((data >> 8) & 0xFF);
			m_signature[m_currSig++] = (byte)((data >> 16) & 0xFF);
			m_signature[m_currSig++] = (byte)((data >> 24) & 0xFF);
			m_signature[m_currSig++] = (byte)((data >> 32) & 0xFF);
			m_signature[m_currSig++] = (byte)((data >> 40) & 0xFF);
			m_signature[m_currSig++] = (byte)((data >> 48) & 0xFF);
			m_signature[m_currSig++] = (byte)((data >> 56) & 0xFF);
		}

		private void AddElementType(int cvt)
		{
			if (m_currSig + 1 >= m_signature.Length)
			{
				m_signature = ExpandArray(m_signature);
			}
			m_signature[m_currSig++] = (byte)cvt;
		}

		private void AddToken(int token)
		{
			int num = token & 0xFFFFFF;
			int num2 = token & -16777216;
			if (num > 67108863)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_LargeInteger"));
			}
			num <<= 2;
			switch (num2)
			{
			case 16777216:
				num |= 1;
				break;
			case 553648128:
				num |= 2;
				break;
			}
			AddData(num);
		}

		private void InternalAddTypeToken(TypeToken clsToken, int CorType)
		{
			AddElementType(CorType);
			AddToken(clsToken.Token);
		}

		private unsafe void InternalAddRuntimeType(Type type)
		{
			AddElementType(33);
			void* ptr = (void*)type.GetTypeHandleInternal().Value;
			if (sizeof(void*) == 4)
			{
				AddData((uint)ptr);
			}
			else
			{
				AddData((ulong)ptr);
			}
		}

		private byte[] ExpandArray(byte[] inArray)
		{
			return ExpandArray(inArray, inArray.Length * 2);
		}

		private byte[] ExpandArray(byte[] inArray, int requiredLength)
		{
			if (requiredLength < inArray.Length)
			{
				requiredLength = inArray.Length * 2;
			}
			byte[] array = new byte[requiredLength];
			Array.Copy(inArray, array, inArray.Length);
			return array;
		}

		private void IncrementArgCounts()
		{
			if (m_sizeLoc != -1)
			{
				m_argCount++;
			}
		}

		private void SetNumberOfSignatureElements(bool forceCopy)
		{
			int currSig = m_currSig;
			if (m_sizeLoc != -1)
			{
				if (m_argCount < 128 && !forceCopy)
				{
					m_signature[m_sizeLoc] = (byte)m_argCount;
					return;
				}
				int num = ((m_argCount < 127) ? 1 : ((m_argCount >= 16383) ? 4 : 2));
				byte[] array = new byte[m_currSig + num - 1];
				array[0] = m_signature[0];
				Array.Copy(m_signature, m_sizeLoc + 1, array, m_sizeLoc + num, currSig - (m_sizeLoc + 1));
				m_signature = array;
				m_currSig = m_sizeLoc;
				AddData(m_argCount);
				m_currSig = currSig + (num - 1);
			}
		}

		internal static bool IsSimpleType(int type)
		{
			if (type <= 14)
			{
				return true;
			}
			if (type == 22 || type == 24 || type == 25 || type == 28)
			{
				return true;
			}
			return false;
		}

		internal byte[] InternalGetSignature(out int length)
		{
			if (!m_sigDone)
			{
				m_sigDone = true;
				SetNumberOfSignatureElements(forceCopy: false);
			}
			length = m_currSig;
			return m_signature;
		}

		internal byte[] InternalGetSignatureArray()
		{
			int argCount = m_argCount;
			int currSig = m_currSig;
			int num = currSig;
			num = ((argCount < 127) ? (num + 1) : ((argCount >= 16383) ? (num + 4) : (num + 2)));
			byte[] array = new byte[num];
			int destinationIndex = 0;
			array[destinationIndex++] = m_signature[0];
			if (argCount <= 127)
			{
				array[destinationIndex++] = (byte)((uint)argCount & 0xFFu);
			}
			else if (argCount <= 16383)
			{
				array[destinationIndex++] = (byte)((uint)(argCount >> 8) | 0x80u);
				array[destinationIndex++] = (byte)((uint)argCount & 0xFFu);
			}
			else
			{
				if (argCount > 536870911)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_LargeInteger"));
				}
				array[destinationIndex++] = (byte)((uint)(argCount >> 24) | 0xC0u);
				array[destinationIndex++] = (byte)((uint)(argCount >> 16) & 0xFFu);
				array[destinationIndex++] = (byte)((uint)(argCount >> 8) & 0xFFu);
				array[destinationIndex++] = (byte)((uint)argCount & 0xFFu);
			}
			Array.Copy(m_signature, 2, array, destinationIndex, currSig - 2);
			array[num - 1] = 0;
			return array;
		}

		public void AddArgument(Type clsArgument)
		{
			AddArgument(clsArgument, null, null);
		}

		public void AddArgument(Type argument, bool pinned)
		{
			if (argument == null)
			{
				throw new ArgumentNullException("argument");
			}
			IncrementArgCounts();
			AddOneArgTypeHelper(argument, pinned);
		}

		public void AddArguments(Type[] arguments, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			if (requiredCustomModifiers != null && (arguments == null || requiredCustomModifiers.Length != arguments.Length))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MismatchedArrays", "requiredCustomModifiers", "arguments"));
			}
			if (optionalCustomModifiers != null && (arguments == null || optionalCustomModifiers.Length != arguments.Length))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MismatchedArrays", "optionalCustomModifiers", "arguments"));
			}
			if (arguments != null)
			{
				for (int i = 0; i < arguments.Length; i++)
				{
					AddArgument(arguments[i], (requiredCustomModifiers == null) ? null : requiredCustomModifiers[i], (optionalCustomModifiers == null) ? null : optionalCustomModifiers[i]);
				}
			}
		}

		public void AddArgument(Type argument, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			if (m_sigDone)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_SigIsFinalized"));
			}
			if (argument == null)
			{
				throw new ArgumentNullException("argument");
			}
			if (requiredCustomModifiers != null)
			{
				foreach (Type type in requiredCustomModifiers)
				{
					if (type == null)
					{
						throw new ArgumentNullException("requiredCustomModifiers");
					}
					if (type.HasElementType)
					{
						throw new ArgumentException(Environment.GetResourceString("Argument_ArraysInvalid"), "requiredCustomModifiers");
					}
					if (type.ContainsGenericParameters)
					{
						throw new ArgumentException(Environment.GetResourceString("Argument_GenericsInvalid"), "requiredCustomModifiers");
					}
				}
			}
			if (optionalCustomModifiers != null)
			{
				foreach (Type type2 in optionalCustomModifiers)
				{
					if (type2 == null)
					{
						throw new ArgumentNullException("optionalCustomModifiers");
					}
					if (type2.HasElementType)
					{
						throw new ArgumentException(Environment.GetResourceString("Argument_ArraysInvalid"), "optionalCustomModifiers");
					}
					if (type2.ContainsGenericParameters)
					{
						throw new ArgumentException(Environment.GetResourceString("Argument_GenericsInvalid"), "optionalCustomModifiers");
					}
				}
			}
			IncrementArgCounts();
			AddOneArgTypeHelper(argument, requiredCustomModifiers, optionalCustomModifiers);
		}

		public void AddSentinel()
		{
			AddElementType(65);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is SignatureHelper))
			{
				return false;
			}
			SignatureHelper signatureHelper = (SignatureHelper)obj;
			if (!signatureHelper.m_module.Equals(m_module) || signatureHelper.m_currSig != m_currSig || signatureHelper.m_sizeLoc != m_sizeLoc || signatureHelper.m_sigDone != m_sigDone)
			{
				return false;
			}
			for (int i = 0; i < m_currSig; i++)
			{
				if (m_signature[i] != signatureHelper.m_signature[i])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			int num = m_module.GetHashCode() + m_currSig + m_sizeLoc;
			if (m_sigDone)
			{
				num++;
			}
			for (int i = 0; i < m_currSig; i++)
			{
				num += m_signature[i].GetHashCode();
			}
			return num;
		}

		public byte[] GetSignature()
		{
			return GetSignature(appendEndOfSig: false);
		}

		internal byte[] GetSignature(bool appendEndOfSig)
		{
			if (!m_sigDone)
			{
				if (appendEndOfSig)
				{
					AddElementType(0);
				}
				SetNumberOfSignatureElements(forceCopy: true);
				m_sigDone = true;
			}
			if (m_signature.Length > m_currSig)
			{
				byte[] array = new byte[m_currSig];
				Array.Copy(m_signature, array, m_currSig);
				m_signature = array;
			}
			return m_signature;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Length: " + m_currSig + Environment.NewLine);
			if (m_sizeLoc != -1)
			{
				stringBuilder.Append("Arguments: " + m_signature[m_sizeLoc] + Environment.NewLine);
			}
			else
			{
				stringBuilder.Append("Field Signature" + Environment.NewLine);
			}
			stringBuilder.Append("Signature: " + Environment.NewLine);
			for (int i = 0; i <= m_currSig; i++)
			{
				stringBuilder.Append(m_signature[i] + "  ");
			}
			stringBuilder.Append(Environment.NewLine);
			return stringBuilder.ToString();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int GetCorElementTypeFromClass(RuntimeType cls);

		void _SignatureHelper.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _SignatureHelper.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _SignatureHelper.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _SignatureHelper.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
