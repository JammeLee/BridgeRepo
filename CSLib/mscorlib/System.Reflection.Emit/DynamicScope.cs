using System.Collections;
using System.Globalization;

namespace System.Reflection.Emit
{
	internal class DynamicScope
	{
		internal ArrayList m_tokens;

		internal object this[int token]
		{
			get
			{
				token &= 0xFFFFFF;
				if (token < 0 || token > m_tokens.Count)
				{
					return null;
				}
				return m_tokens[token];
			}
		}

		internal DynamicScope()
		{
			m_tokens = new ArrayList();
			m_tokens.Add(null);
		}

		internal int GetTokenFor(VarArgMethod varArgMethod)
		{
			return m_tokens.Add(varArgMethod) | 0xA000000;
		}

		internal string GetString(int token)
		{
			return this[token] as string;
		}

		internal byte[] ResolveSignature(int token, int fromMethod)
		{
			if (fromMethod == 0)
			{
				return (byte[])this[token];
			}
			return (this[token] as VarArgMethod)?.m_signature.GetSignature(appendEndOfSig: true);
		}

		public int GetTokenFor(RuntimeMethodHandle method)
		{
			MethodBase methodBase = RuntimeType.GetMethodBase(method);
			if (methodBase.DeclaringType != null && methodBase.DeclaringType.IsGenericType)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_MethodDeclaringTypeGenericLcg"), methodBase, methodBase.DeclaringType.GetGenericTypeDefinition()));
			}
			return m_tokens.Add(method) | 0x6000000;
		}

		public int GetTokenFor(RuntimeMethodHandle method, RuntimeTypeHandle typeContext)
		{
			return m_tokens.Add(new GenericMethodInfo(method, typeContext)) | 0x6000000;
		}

		public int GetTokenFor(DynamicMethod method)
		{
			return m_tokens.Add(method) | 0x6000000;
		}

		public int GetTokenFor(RuntimeFieldHandle field)
		{
			return m_tokens.Add(field) | 0x4000000;
		}

		public int GetTokenFor(RuntimeFieldHandle field, RuntimeTypeHandle typeContext)
		{
			return m_tokens.Add(new GenericFieldInfo(field, typeContext)) | 0x4000000;
		}

		public int GetTokenFor(RuntimeTypeHandle type)
		{
			return m_tokens.Add(type) | 0x2000000;
		}

		public int GetTokenFor(string literal)
		{
			return m_tokens.Add(literal) | 0x70000000;
		}

		public int GetTokenFor(byte[] signature)
		{
			return m_tokens.Add(signature) | 0x11000000;
		}
	}
}
