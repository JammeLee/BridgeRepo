using System.Diagnostics;

namespace System
{
	internal sealed class ASSERT : Exception
	{
		private static bool AssertIsFriend(Type[] friends, StackTrace st)
		{
			Type declaringType = st.GetFrame(1).GetMethod().DeclaringType;
			Type declaringType2 = st.GetFrame(2).GetMethod().DeclaringType;
			bool flag = true;
			foreach (Type type in friends)
			{
				if (declaringType2 != type && declaringType2 != declaringType)
				{
					flag = false;
				}
			}
			if (flag)
			{
				Assert(condition: false, Environment.GetResourceString("RtType.InvalidCaller"), st.ToString());
			}
			return true;
		}

		[Conditional("_DEBUG")]
		internal static void FRIEND(Type[] friends)
		{
			StackTrace st = new StackTrace();
			AssertIsFriend(friends, st);
		}

		[Conditional("_DEBUG")]
		internal static void FRIEND(Type friend)
		{
			StackTrace st = new StackTrace();
			AssertIsFriend(new Type[1]
			{
				friend
			}, st);
		}

		[Conditional("_DEBUG")]
		internal static void FRIEND(string ns)
		{
			StackTrace stackTrace = new StackTrace();
			_ = stackTrace.GetFrame(1).GetMethod().DeclaringType.Namespace;
			string @namespace = stackTrace.GetFrame(2).GetMethod().DeclaringType.Namespace;
			Assert(@namespace.Equals(@namespace) || @namespace.Equals(ns), Environment.GetResourceString("RtType.InvalidCaller"), stackTrace.ToString());
		}

		[Conditional("_DEBUG")]
		internal static void PRECONDITION(bool condition)
		{
			Assert(condition);
		}

		[Conditional("_DEBUG")]
		internal static void PRECONDITION(bool condition, string message)
		{
			Assert(condition, message);
		}

		[Conditional("_DEBUG")]
		internal static void PRECONDITION(bool condition, string message, string detailedMessage)
		{
			Assert(condition, message, detailedMessage);
		}

		[Conditional("_DEBUG")]
		internal static void POSTCONDITION(bool condition)
		{
			Assert(condition);
		}

		[Conditional("_DEBUG")]
		internal static void POSTCONDITION(bool condition, string message)
		{
			Assert(condition, message);
		}

		[Conditional("_DEBUG")]
		internal static void POSTCONDITION(bool condition, string message, string detailedMessage)
		{
			Assert(condition, message, detailedMessage);
		}

		[Conditional("_DEBUG")]
		internal static void CONSISTENCY_CHECK(bool condition)
		{
			Assert(condition);
		}

		[Conditional("_DEBUG")]
		internal static void CONSISTENCY_CHECK(bool condition, string message)
		{
			Assert(condition, message);
		}

		[Conditional("_DEBUG")]
		internal static void CONSISTENCY_CHECK(bool condition, string message, string detailedMessage)
		{
			Assert(condition, message, detailedMessage);
		}

		[Conditional("_DEBUG")]
		internal static void SIMPLIFYING_ASSUMPTION(bool condition)
		{
			Assert(condition);
		}

		[Conditional("_DEBUG")]
		internal static void SIMPLIFYING_ASSUMPTION(bool condition, string message)
		{
			Assert(condition, message);
		}

		[Conditional("_DEBUG")]
		internal static void SIMPLIFYING_ASSUMPTION(bool condition, string message, string detailedMessage)
		{
			Assert(condition, message, detailedMessage);
		}

		[Conditional("_DEBUG")]
		internal static void UNREACHABLE()
		{
			Assert();
		}

		[Conditional("_DEBUG")]
		internal static void UNREACHABLE(string message)
		{
			Assert(message);
		}

		[Conditional("_DEBUG")]
		internal static void UNREACHABLE(string message, string detailedMessage)
		{
			Assert(message, detailedMessage);
		}

		[Conditional("_DEBUG")]
		internal static void NOT_IMPLEMENTED()
		{
			Assert();
		}

		[Conditional("_DEBUG")]
		internal static void NOT_IMPLEMENTED(string message)
		{
			Assert(message);
		}

		[Conditional("_DEBUG")]
		internal static void NOT_IMPLEMENTED(string message, string detailedMessage)
		{
			Assert(message, detailedMessage);
		}

		private static void Assert()
		{
			Assert(condition: false, null, null);
		}

		private static void Assert(string message)
		{
			Assert(condition: false, message, null);
		}

		private static void Assert(bool condition)
		{
			Assert(condition, null, null);
		}

		private static void Assert(bool condition, string message)
		{
			Assert(condition, message, null);
		}

		private static void Assert(string message, string detailedMessage)
		{
			Assert(condition: false, message, detailedMessage);
		}

		private static void Assert(bool condition, string message, string detailedMessage)
		{
		}
	}
}
