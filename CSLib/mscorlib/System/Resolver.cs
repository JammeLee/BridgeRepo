using System.Reflection;
using System.Threading;

namespace System
{
	internal abstract class Resolver
	{
		internal struct CORINFO_EH_CLAUSE
		{
			internal int Flags;

			internal int TryOffset;

			internal int TryLength;

			internal int HandlerOffset;

			internal int HandlerLength;

			internal int ClassTokenOrFilterOffset;
		}

		internal const int COR_ILEXCEPTION_CLAUSE_CACHED_CLASS = 268435456;

		internal const int COR_ILEXCEPTION_CLAUSE_MUST_CACHE_CLASS = 536870912;

		internal const int TypeToken = 1;

		internal const int MethodToken = 2;

		internal const int FieldToken = 4;

		internal abstract void GetJitContext(ref int securityControlFlags, ref RuntimeTypeHandle typeOwner);

		internal abstract byte[] GetCodeInfo(ref int stackSize, ref int initLocals, ref int EHCount);

		internal abstract byte[] GetLocalsSignature();

		internal unsafe abstract void GetEHInfo(int EHNumber, void* exception);

		internal abstract byte[] GetRawEHInfo();

		internal abstract string GetStringLiteral(int token);

		internal unsafe abstract void* ResolveToken(int token);

		internal abstract int ParentToken(int token);

		internal abstract byte[] ResolveSignature(int token, int fromMethod);

		internal abstract int IsValidToken(int token);

		internal abstract MethodInfo GetDynamicMethod();

		internal abstract CompressedStack GetSecurityContext();
	}
}
