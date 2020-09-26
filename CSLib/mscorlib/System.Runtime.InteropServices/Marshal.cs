using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace System.Runtime.InteropServices
{
	[SuppressUnmanagedCodeSecurity]
	public static class Marshal
	{
		private const int LMEM_FIXED = 0;

		private const int LMEM_MOVEABLE = 2;

		private const string s_strConvertedTypeInfoAssemblyName = "InteropDynamicTypes";

		private const string s_strConvertedTypeInfoAssemblyTitle = "Interop Dynamic Types";

		private const string s_strConvertedTypeInfoAssemblyDesc = "Type dynamically generated from ITypeInfo's";

		private const string s_strConvertedTypeInfoNameSpace = "InteropDynamicTypes";

		private static readonly IntPtr HIWORDMASK = new IntPtr(-65536L);

		private static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

		public static readonly int SystemDefaultCharSize = 3 - Win32Native.lstrlen(new sbyte[4]
		{
			65,
			65,
			0,
			0
		});

		public static readonly int SystemMaxDBCSCharSize = GetSystemMaxDBCSCharSize();

		private static bool IsWin32Atom(IntPtr ptr)
		{
			long num = (long)ptr;
			return 0 == (num & (long)HIWORDMASK);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static bool IsNotWin32Atom(IntPtr ptr)
		{
			long num = (long)ptr;
			return 0 != (num & (long)HIWORDMASK);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int GetSystemMaxDBCSCharSize();

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static string PtrToStringAnsi(IntPtr ptr)
		{
			if (Win32Native.NULL == ptr)
			{
				return null;
			}
			if (IsWin32Atom(ptr))
			{
				return null;
			}
			int num = Win32Native.lstrlenA(ptr);
			if (num == 0)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder(num);
			Win32Native.CopyMemoryAnsi(stringBuilder, ptr, new IntPtr(1 + num));
			return stringBuilder.ToString();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern string PtrToStringAnsi(IntPtr ptr, int len);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern string PtrToStringUni(IntPtr ptr, int len);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static string PtrToStringAuto(IntPtr ptr, int len)
		{
			if (SystemDefaultCharSize != 1)
			{
				return PtrToStringUni(ptr, len);
			}
			return PtrToStringAnsi(ptr, len);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static string PtrToStringUni(IntPtr ptr)
		{
			if (Win32Native.NULL == ptr)
			{
				return null;
			}
			if (IsWin32Atom(ptr))
			{
				return null;
			}
			int num = Win32Native.lstrlenW(ptr);
			StringBuilder stringBuilder = new StringBuilder(num);
			Win32Native.CopyMemoryUni(stringBuilder, ptr, new IntPtr(2 * (1 + num)));
			return stringBuilder.ToString();
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static string PtrToStringAuto(IntPtr ptr)
		{
			if (Win32Native.NULL == ptr)
			{
				return null;
			}
			if (IsWin32Atom(ptr))
			{
				return null;
			}
			int capacity = Win32Native.lstrlen(ptr);
			StringBuilder stringBuilder = new StringBuilder(capacity);
			Win32Native.lstrcpy(stringBuilder, ptr);
			return stringBuilder.ToString();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ComVisible(true)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern int SizeOf(object structure);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern int SizeOf(Type t);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr OffsetOf(Type t, string fieldName)
		{
			if (t == null)
			{
				throw new ArgumentNullException("t");
			}
			FieldInfo field = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_OffsetOfFieldNotFound", t.FullName), "fieldName");
			}
			if (!(field is RuntimeFieldInfo))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeFieldInfo"), "fieldName");
			}
			return OffsetOfHelper(((RuntimeFieldInfo)field).GetFieldHandle().Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr OffsetOfHelper(IntPtr f);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern IntPtr UnsafeAddrOfPinnedArrayElement(Array arr, int index);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(int[] source, int startIndex, IntPtr destination, int length)
		{
			CopyToNative(source, startIndex, destination, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(char[] source, int startIndex, IntPtr destination, int length)
		{
			CopyToNative(source, startIndex, destination, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(short[] source, int startIndex, IntPtr destination, int length)
		{
			CopyToNative(source, startIndex, destination, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(long[] source, int startIndex, IntPtr destination, int length)
		{
			CopyToNative(source, startIndex, destination, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(float[] source, int startIndex, IntPtr destination, int length)
		{
			CopyToNative(source, startIndex, destination, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(double[] source, int startIndex, IntPtr destination, int length)
		{
			CopyToNative(source, startIndex, destination, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(byte[] source, int startIndex, IntPtr destination, int length)
		{
			CopyToNative(source, startIndex, destination, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(IntPtr[] source, int startIndex, IntPtr destination, int length)
		{
			CopyToNative(source, startIndex, destination, length);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void CopyToNative(object source, int startIndex, IntPtr destination, int length);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(IntPtr source, int[] destination, int startIndex, int length)
		{
			CopyToManaged(source, destination, startIndex, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(IntPtr source, char[] destination, int startIndex, int length)
		{
			CopyToManaged(source, destination, startIndex, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(IntPtr source, short[] destination, int startIndex, int length)
		{
			CopyToManaged(source, destination, startIndex, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(IntPtr source, long[] destination, int startIndex, int length)
		{
			CopyToManaged(source, destination, startIndex, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(IntPtr source, float[] destination, int startIndex, int length)
		{
			CopyToManaged(source, destination, startIndex, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(IntPtr source, double[] destination, int startIndex, int length)
		{
			CopyToManaged(source, destination, startIndex, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(IntPtr source, byte[] destination, int startIndex, int length)
		{
			CopyToManaged(source, destination, startIndex, length);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Copy(IntPtr source, IntPtr[] destination, int startIndex, int length)
		{
			CopyToManaged(source, destination, startIndex, length);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void CopyToManaged(IntPtr source, object destination, int startIndex, int length);

		[DllImport("mscoree.dll", EntryPoint = "ND_RU1")]
		public static extern byte ReadByte([In][MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs);

		[DllImport("mscoree.dll", EntryPoint = "ND_RU1")]
		public static extern byte ReadByte(IntPtr ptr, int ofs);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static byte ReadByte(IntPtr ptr)
		{
			return ReadByte(ptr, 0);
		}

		[DllImport("mscoree.dll", EntryPoint = "ND_RI2")]
		public static extern short ReadInt16([In][MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs);

		[DllImport("mscoree.dll", EntryPoint = "ND_RI2")]
		public static extern short ReadInt16(IntPtr ptr, int ofs);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static short ReadInt16(IntPtr ptr)
		{
			return ReadInt16(ptr, 0);
		}

		[DllImport("mscoree.dll", EntryPoint = "ND_RI4")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static extern int ReadInt32([In][MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs);

		[DllImport("mscoree.dll", EntryPoint = "ND_RI4")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static extern int ReadInt32(IntPtr ptr, int ofs);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static int ReadInt32(IntPtr ptr)
		{
			return ReadInt32(ptr, 0);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr ReadIntPtr([In][MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs)
		{
			return (IntPtr)ReadInt32(ptr, ofs);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr ReadIntPtr(IntPtr ptr, int ofs)
		{
			return (IntPtr)ReadInt32(ptr, ofs);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr ReadIntPtr(IntPtr ptr)
		{
			return (IntPtr)ReadInt32(ptr, 0);
		}

		[DllImport("mscoree.dll", EntryPoint = "ND_RI8")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static extern long ReadInt64([In][MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs);

		[DllImport("mscoree.dll", EntryPoint = "ND_RI8")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static extern long ReadInt64(IntPtr ptr, int ofs);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static long ReadInt64(IntPtr ptr)
		{
			return ReadInt64(ptr, 0);
		}

		[DllImport("mscoree.dll", EntryPoint = "ND_WU1")]
		public static extern void WriteByte(IntPtr ptr, int ofs, byte val);

		[DllImport("mscoree.dll", EntryPoint = "ND_WU1")]
		public static extern void WriteByte([In][Out][MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs, byte val);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void WriteByte(IntPtr ptr, byte val)
		{
			WriteByte(ptr, 0, val);
		}

		[DllImport("mscoree.dll", EntryPoint = "ND_WI2")]
		public static extern void WriteInt16(IntPtr ptr, int ofs, short val);

		[DllImport("mscoree.dll", EntryPoint = "ND_WI2")]
		public static extern void WriteInt16([In][Out][MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs, short val);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void WriteInt16(IntPtr ptr, short val)
		{
			WriteInt16(ptr, 0, val);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void WriteInt16(IntPtr ptr, int ofs, char val)
		{
			WriteInt16(ptr, ofs, (short)val);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void WriteInt16([In][Out] object ptr, int ofs, char val)
		{
			WriteInt16(ptr, ofs, (short)val);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void WriteInt16(IntPtr ptr, char val)
		{
			WriteInt16(ptr, 0, (short)val);
		}

		[DllImport("mscoree.dll", EntryPoint = "ND_WI4")]
		public static extern void WriteInt32(IntPtr ptr, int ofs, int val);

		[DllImport("mscoree.dll", EntryPoint = "ND_WI4")]
		public static extern void WriteInt32([In][Out][MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs, int val);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void WriteInt32(IntPtr ptr, int val)
		{
			WriteInt32(ptr, 0, val);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void WriteIntPtr(IntPtr ptr, int ofs, IntPtr val)
		{
			WriteInt32(ptr, ofs, (int)val);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void WriteIntPtr([In][Out][MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs, IntPtr val)
		{
			WriteInt32(ptr, ofs, (int)val);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void WriteIntPtr(IntPtr ptr, IntPtr val)
		{
			WriteInt32(ptr, 0, (int)val);
		}

		[DllImport("mscoree.dll", EntryPoint = "ND_WI8")]
		public static extern void WriteInt64(IntPtr ptr, int ofs, long val);

		[DllImport("mscoree.dll", EntryPoint = "ND_WI8")]
		public static extern void WriteInt64([In][Out][MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs, long val);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void WriteInt64(IntPtr ptr, long val)
		{
			WriteInt64(ptr, 0, val);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern int GetLastWin32Error();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern void SetLastWin32Error(int error);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static int GetHRForLastWin32Error()
		{
			int lastWin32Error = GetLastWin32Error();
			if ((lastWin32Error & 0x80000000u) == 2147483648u)
			{
				return lastWin32Error;
			}
			return (lastWin32Error & 0xFFFF) | -2147024896;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void Prelink(MethodInfo m)
		{
			if (m == null)
			{
				throw new ArgumentNullException("m");
			}
			if (!(m is RuntimeMethodInfo))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"));
			}
			InternalPrelink(m.MethodHandle.Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void InternalPrelink(IntPtr m);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void PrelinkAll(Type c)
		{
			if (c == null)
			{
				throw new ArgumentNullException("c");
			}
			MethodInfo[] methods = c.GetMethods();
			if (methods != null)
			{
				for (int i = 0; i < methods.Length; i++)
				{
					Prelink(methods[i]);
				}
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static int NumParamBytes(MethodInfo m)
		{
			if (m == null)
			{
				throw new ArgumentNullException("m");
			}
			if (!(m is RuntimeMethodInfo))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"));
			}
			return InternalNumParamBytes(m.GetMethodHandle().Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int InternalNumParamBytes(IntPtr m);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ComVisible(true)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern IntPtr GetExceptionPointers();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern int GetExceptionCode();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[ComVisible(true)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern void StructureToPtr(object structure, IntPtr ptr, bool fDeleteOld);

		[ComVisible(true)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void PtrToStructure(IntPtr ptr, object structure)
		{
			PtrToStructureHelper(ptr, structure, allowValueClasses: false);
		}

		[ComVisible(true)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static object PtrToStructure(IntPtr ptr, Type structureType)
		{
			if (ptr == Win32Native.NULL)
			{
				return null;
			}
			if (structureType == null)
			{
				throw new ArgumentNullException("structureType");
			}
			if (structureType.IsGenericType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "structureType");
			}
			object obj = Activator.InternalCreateInstanceWithNoMemberAccessCheck(structureType, nonPublic: true);
			PtrToStructureHelper(ptr, obj, allowValueClasses: true);
			return obj;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void PtrToStructureHelper(IntPtr ptr, object structure, bool allowValueClasses);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ComVisible(true)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern void DestroyStructure(IntPtr ptr, Type structuretype);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr GetHINSTANCE(Module m)
		{
			if (m == null)
			{
				throw new ArgumentNullException("m");
			}
			return m.GetHINSTANCE();
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void ThrowExceptionForHR(int errorCode)
		{
			if (errorCode < 0)
			{
				ThrowExceptionForHRInternal(errorCode, Win32Native.NULL);
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void ThrowExceptionForHR(int errorCode, IntPtr errorInfo)
		{
			if (errorCode < 0)
			{
				ThrowExceptionForHRInternal(errorCode, errorInfo);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void ThrowExceptionForHRInternal(int errorCode, IntPtr errorInfo);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Exception GetExceptionForHR(int errorCode)
		{
			if (errorCode < 0)
			{
				return GetExceptionForHRInternal(errorCode, Win32Native.NULL);
			}
			return null;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Exception GetExceptionForHR(int errorCode, IntPtr errorInfo)
		{
			if (errorCode < 0)
			{
				return GetExceptionForHRInternal(errorCode, errorInfo);
			}
			return null;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Exception GetExceptionForHRInternal(int errorCode, IntPtr errorInfo);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern int GetHRForException(Exception e);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[Obsolete("The GetUnmanagedThunkForManagedMethodPtr method has been deprecated and will be removed in a future release.", false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern IntPtr GetUnmanagedThunkForManagedMethodPtr(IntPtr pfnMethodToWrap, IntPtr pbSignature, int cbSignature);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[Obsolete("The GetManagedThunkForUnmanagedMethodPtr method has been deprecated and will be removed in a future release.", false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern IntPtr GetManagedThunkForUnmanagedMethodPtr(IntPtr pfnMethodToWrap, IntPtr pbSignature, int cbSignature);

		[Obsolete("The GetThreadFromFiberCookie method has been deprecated.  Use the hosting API to perform this operation.", false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Thread GetThreadFromFiberCookie(int cookie)
		{
			if (cookie == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ArgumentZero"), "cookie");
			}
			return InternalGetThreadFromFiberCookie(cookie);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Thread InternalGetThreadFromFiberCookie(int cookie);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr AllocHGlobal(IntPtr cb)
		{
			IntPtr intPtr = Win32Native.LocalAlloc_NoSafeHandle(0, cb);
			if (intPtr == Win32Native.NULL)
			{
				throw new OutOfMemoryException();
			}
			return intPtr;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr AllocHGlobal(int cb)
		{
			return AllocHGlobal((IntPtr)cb);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void FreeHGlobal(IntPtr hglobal)
		{
			if (IsNotWin32Atom(hglobal) && Win32Native.NULL != Win32Native.LocalFree(hglobal))
			{
				ThrowExceptionForHR(GetHRForLastWin32Error());
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr ReAllocHGlobal(IntPtr pv, IntPtr cb)
		{
			IntPtr intPtr = Win32Native.LocalReAlloc(pv, cb, 2);
			if (intPtr == Win32Native.NULL)
			{
				throw new OutOfMemoryException();
			}
			return intPtr;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr StringToHGlobalAnsi(string s)
		{
			if (s == null)
			{
				return Win32Native.NULL;
			}
			int num = (s.Length + 1) * SystemMaxDBCSCharSize;
			if (num < s.Length)
			{
				throw new ArgumentOutOfRangeException("s");
			}
			IntPtr intPtr = new IntPtr(num);
			IntPtr intPtr2 = Win32Native.LocalAlloc_NoSafeHandle(0, intPtr);
			if (intPtr2 == Win32Native.NULL)
			{
				throw new OutOfMemoryException();
			}
			Win32Native.CopyMemoryAnsi(intPtr2, s, intPtr);
			return intPtr2;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr StringToCoTaskMemAnsi(string s)
		{
			if (s == null)
			{
				return Win32Native.NULL;
			}
			int num = (s.Length + 1) * SystemMaxDBCSCharSize;
			if (num < s.Length)
			{
				throw new ArgumentOutOfRangeException("s");
			}
			IntPtr intPtr = Win32Native.CoTaskMemAlloc(num);
			if (intPtr == Win32Native.NULL)
			{
				throw new OutOfMemoryException();
			}
			Win32Native.CopyMemoryAnsi(intPtr, s, new IntPtr(num));
			return intPtr;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr StringToHGlobalUni(string s)
		{
			if (s == null)
			{
				return Win32Native.NULL;
			}
			int num = (s.Length + 1) * 2;
			if (num < s.Length)
			{
				throw new ArgumentOutOfRangeException("s");
			}
			IntPtr intPtr = new IntPtr(num);
			IntPtr intPtr2 = Win32Native.LocalAlloc_NoSafeHandle(0, intPtr);
			if (intPtr2 == Win32Native.NULL)
			{
				throw new OutOfMemoryException();
			}
			Win32Native.CopyMemoryUni(intPtr2, s, intPtr);
			return intPtr2;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr StringToHGlobalAuto(string s)
		{
			if (SystemDefaultCharSize != 1)
			{
				return StringToHGlobalUni(s);
			}
			return StringToHGlobalAnsi(s);
		}

		[Obsolete("Use System.Runtime.InteropServices.Marshal.GetTypeLibName(ITypeLib pTLB) instead. http://go.microsoft.com/fwlink/?linkid=14202&ID=0000011.", false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static string GetTypeLibName(UCOMITypeLib pTLB)
		{
			return GetTypeLibName((ITypeLib)pTLB);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static string GetTypeLibName(ITypeLib typelib)
		{
			string strName = null;
			string strDocString = null;
			int dwHelpContext = 0;
			string strHelpFile = null;
			if (typelib == null)
			{
				throw new ArgumentNullException("typelib");
			}
			typelib.GetDocumentation(-1, out strName, out strDocString, out dwHelpContext, out strHelpFile);
			return strName;
		}

		[Obsolete("Use System.Runtime.InteropServices.Marshal.GetTypeLibGuid(ITypeLib pTLB) instead. http://go.microsoft.com/fwlink/?linkid=14202&ID=0000011.", false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Guid GetTypeLibGuid(UCOMITypeLib pTLB)
		{
			return GetTypeLibGuid((ITypeLib)pTLB);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Guid GetTypeLibGuid(ITypeLib typelib)
		{
			Guid result = default(Guid);
			FCallGetTypeLibGuid(ref result, typelib);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallGetTypeLibGuid(ref Guid result, ITypeLib pTLB);

		[Obsolete("Use System.Runtime.InteropServices.Marshal.GetTypeLibLcid(ITypeLib pTLB) instead. http://go.microsoft.com/fwlink/?linkid=14202&ID=0000011.", false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static int GetTypeLibLcid(UCOMITypeLib pTLB)
		{
			return GetTypeLibLcid((ITypeLib)pTLB);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern int GetTypeLibLcid(ITypeLib typelib);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void GetTypeLibVersion(ITypeLib typeLibrary, out int major, out int minor);

		internal static Guid GetTypeInfoGuid(ITypeInfo typeInfo)
		{
			Guid result = default(Guid);
			FCallGetTypeInfoGuid(ref result, typeInfo);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallGetTypeInfoGuid(ref Guid result, ITypeInfo typeInfo);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Guid GetTypeLibGuidForAssembly(Assembly asm)
		{
			Guid result = default(Guid);
			FCallGetTypeLibGuidForAssembly(ref result, asm?.InternalAssembly);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallGetTypeLibGuidForAssembly(ref Guid result, Assembly asm);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetTypeLibVersionForAssembly(Assembly inputAssembly, out int majorVersion, out int minorVersion);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void GetTypeLibVersionForAssembly(Assembly inputAssembly, out int majorVersion, out int minorVersion)
		{
			_GetTypeLibVersionForAssembly(inputAssembly?.InternalAssembly, out majorVersion, out minorVersion);
		}

		[Obsolete("Use System.Runtime.InteropServices.Marshal.GetTypeInfoName(ITypeInfo pTLB) instead. http://go.microsoft.com/fwlink/?linkid=14202&ID=0000011.", false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static string GetTypeInfoName(UCOMITypeInfo pTI)
		{
			return GetTypeInfoName((ITypeInfo)pTI);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static string GetTypeInfoName(ITypeInfo typeInfo)
		{
			string strName = null;
			string strDocString = null;
			int dwHelpContext = 0;
			string strHelpFile = null;
			if (typeInfo == null)
			{
				throw new ArgumentNullException("typeInfo");
			}
			typeInfo.GetDocumentation(-1, out strName, out strDocString, out dwHelpContext, out strHelpFile);
			return strName;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Type GetLoadedTypeForGUID(ref Guid guid);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Type GetTypeForITypeInfo(IntPtr piTypeInfo)
		{
			ITypeInfo typeInfo = null;
			ITypeLib ppTLB = null;
			Type type = null;
			Assembly assembly = null;
			TypeLibConverter typeLibConverter = null;
			int pIndex = 0;
			if (piTypeInfo == Win32Native.NULL)
			{
				return null;
			}
			typeInfo = (ITypeInfo)GetObjectForIUnknown(piTypeInfo);
			Guid guid = GetTypeInfoGuid(typeInfo);
			type = GetLoadedTypeForGUID(ref guid);
			if (type != null)
			{
				return type;
			}
			try
			{
				typeInfo.GetContainingTypeLib(out ppTLB, out pIndex);
			}
			catch (COMException)
			{
				ppTLB = null;
			}
			if (ppTLB != null)
			{
				AssemblyName assemblyNameFromTypelib = TypeLibConverter.GetAssemblyNameFromTypelib(ppTLB, null, null, null, null, AssemblyNameFlags.None);
				string fullName = assemblyNameFromTypelib.FullName;
				Assembly[] assemblies = Thread.GetDomain().GetAssemblies();
				int num = assemblies.Length;
				for (int i = 0; i < num; i++)
				{
					if (string.Compare(assemblies[i].FullName, fullName, StringComparison.Ordinal) == 0)
					{
						assembly = assemblies[i];
					}
				}
				if (assembly == null)
				{
					typeLibConverter = new TypeLibConverter();
					assembly = typeLibConverter.ConvertTypeLibToAssembly(ppTLB, GetTypeLibName(ppTLB) + ".dll", TypeLibImporterFlags.None, new ImporterCallback(), null, null, null, null);
				}
				type = assembly.GetType(GetTypeLibName(ppTLB) + "." + GetTypeInfoName(typeInfo), throwOnError: true, ignoreCase: false);
				if (type != null && !type.IsVisible)
				{
					type = null;
				}
			}
			else
			{
				type = typeof(object);
			}
			return type;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern IntPtr GetITypeInfoForType(Type t);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr GetIUnknownForObject(object o)
		{
			return GetIUnknownForObjectNative(o, onlyInContext: false);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr GetIUnknownForObjectInContext(object o)
		{
			return GetIUnknownForObjectNative(o, onlyInContext: true);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr GetIUnknownForObjectNative(object o, bool onlyInContext);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr GetIDispatchForObject(object o)
		{
			return GetIDispatchForObjectNative(o, onlyInContext: false);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr GetIDispatchForObjectInContext(object o)
		{
			return GetIDispatchForObjectNative(o, onlyInContext: true);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr GetIDispatchForObjectNative(object o, bool onlyInContext);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr GetComInterfaceForObject(object o, Type T)
		{
			return GetComInterfaceForObjectNative(o, T, onlyInContext: false);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr GetComInterfaceForObjectInContext(object o, Type t)
		{
			return GetComInterfaceForObjectNative(o, t, onlyInContext: true);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr GetComInterfaceForObjectNative(object o, Type t, bool onlyInContext);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern object GetObjectForIUnknown(IntPtr pUnk);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern object GetUniqueObjectForIUnknown(IntPtr unknown);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern object GetTypedObjectForIUnknown(IntPtr pUnk, Type t);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern IntPtr CreateAggregatedObject(IntPtr pOuter, object o);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern bool IsComObject(object o);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static int ReleaseComObject(object o)
		{
			__ComObject _ComObject = null;
			try
			{
				_ComObject = (__ComObject)o;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "o");
			}
			return _ComObject.ReleaseSelf();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int InternalReleaseComObject(object o);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static int FinalReleaseComObject(object o)
		{
			__ComObject _ComObject = null;
			if (o == null)
			{
				throw new ArgumentNullException("o");
			}
			try
			{
				_ComObject = (__ComObject)o;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "o");
			}
			_ComObject.FinalReleaseSelf();
			return 0;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void InternalFinalReleaseComObject(object o);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static object GetComObjectData(object obj, object key)
		{
			__ComObject _ComObject = null;
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			try
			{
				_ComObject = (__ComObject)obj;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "obj");
			}
			return _ComObject.GetData(key);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static bool SetComObjectData(object obj, object key, object data)
		{
			__ComObject _ComObject = null;
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			try
			{
				_ComObject = (__ComObject)obj;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "obj");
			}
			return _ComObject.SetData(key, data);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static object CreateWrapperOfType(object o, Type t)
		{
			if (t == null)
			{
				throw new ArgumentNullException("t");
			}
			if (!t.IsCOMObject)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_TypeNotComObject"), "t");
			}
			if (t.IsGenericType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "t");
			}
			if (o == null)
			{
				return null;
			}
			if (!o.GetType().IsCOMObject)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "o");
			}
			if (o.GetType() == t)
			{
				return o;
			}
			object obj = GetComObjectData(o, t);
			if (obj == null)
			{
				obj = InternalCreateWrapperOfType(o, t);
				if (!SetComObjectData(o, t, obj))
				{
					obj = GetComObjectData(o, t);
				}
			}
			return obj;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		private static extern object InternalCreateWrapperOfType(object o, Type t);

		[Obsolete("This API did not perform any operation and will be removed in future versions of the CLR.", false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void ReleaseThreadCache()
		{
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern bool IsTypeVisibleFromCom(Type t);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern int QueryInterface(IntPtr pUnk, ref Guid iid, out IntPtr ppv);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern int AddRef(IntPtr pUnk);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern int Release(IntPtr pUnk);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr AllocCoTaskMem(int cb)
		{
			IntPtr intPtr = Win32Native.CoTaskMemAlloc(cb);
			if (intPtr == Win32Native.NULL)
			{
				throw new OutOfMemoryException();
			}
			return intPtr;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr ReAllocCoTaskMem(IntPtr pv, int cb)
		{
			IntPtr intPtr = Win32Native.CoTaskMemRealloc(pv, cb);
			if (intPtr == Win32Native.NULL && cb != 0)
			{
				throw new OutOfMemoryException();
			}
			return intPtr;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void FreeCoTaskMem(IntPtr ptr)
		{
			if (IsNotWin32Atom(ptr))
			{
				Win32Native.CoTaskMemFree(ptr);
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void FreeBSTR(IntPtr ptr)
		{
			if (IsNotWin32Atom(ptr))
			{
				Win32Native.SysFreeString(ptr);
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr StringToCoTaskMemUni(string s)
		{
			if (s == null)
			{
				return Win32Native.NULL;
			}
			int num = (s.Length + 1) * 2;
			if (num < s.Length)
			{
				throw new ArgumentOutOfRangeException("s");
			}
			IntPtr intPtr = Win32Native.CoTaskMemAlloc(num);
			if (intPtr == Win32Native.NULL)
			{
				throw new OutOfMemoryException();
			}
			Win32Native.CopyMemoryUni(intPtr, s, new IntPtr(num));
			return intPtr;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr StringToCoTaskMemAuto(string s)
		{
			if (s == null)
			{
				return Win32Native.NULL;
			}
			int num = (s.Length + 1) * SystemDefaultCharSize;
			if (num < s.Length)
			{
				throw new ArgumentOutOfRangeException("s");
			}
			IntPtr intPtr = Win32Native.CoTaskMemAlloc(num);
			if (intPtr == Win32Native.NULL)
			{
				throw new OutOfMemoryException();
			}
			Win32Native.lstrcpy(intPtr, s);
			return intPtr;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr StringToBSTR(string s)
		{
			if (s == null)
			{
				return Win32Native.NULL;
			}
			if (s.Length + 1 < s.Length)
			{
				throw new ArgumentOutOfRangeException("s");
			}
			IntPtr intPtr = Win32Native.SysAllocStringLen(s, s.Length);
			if (intPtr == Win32Native.NULL)
			{
				throw new OutOfMemoryException();
			}
			return intPtr;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static string PtrToStringBSTR(IntPtr ptr)
		{
			return PtrToStringUni(ptr, Win32Native.SysStringLen(ptr));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern void GetNativeVariantForObject(object obj, IntPtr pDstNativeVariant);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern object GetObjectForNativeVariant(IntPtr pSrcNativeVariant);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern object[] GetObjectsForNativeVariants(IntPtr aSrcNativeVariant, int cVars);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern int GetStartComSlot(Type t);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern int GetEndComSlot(Type t);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern MemberInfo GetMethodInfoForComSlot(Type t, int slot, ref ComMemberType memberType);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static int GetComSlotForMethodInfo(MemberInfo m)
		{
			if (m == null)
			{
				throw new ArgumentNullException("m");
			}
			if (!(m is RuntimeMethodInfo))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "m");
			}
			if (!m.DeclaringType.IsInterface)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeInterfaceMethod"), "m");
			}
			if (m.DeclaringType.IsGenericType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "m");
			}
			RuntimeMethodHandle methodHandle = ((RuntimeMethodInfo)m).GetMethodHandle();
			return InternalGetComSlotForMethodInfo(methodHandle);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int InternalGetComSlotForMethodInfo(RuntimeMethodHandle m);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Guid GenerateGuidForType(Type type)
		{
			Guid result = default(Guid);
			FCallGenerateGuidForType(ref result, type);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallGenerateGuidForType(ref Guid result, Type type);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static string GenerateProgIdForType(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!RegistrationServices.TypeRequiresRegistrationHelper(type))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_TypeMustBeComCreatable"), "type");
			}
			if (type.IsImport)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_TypeMustNotBeComImport"), "type");
			}
			if (type.IsGenericType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "type");
			}
			IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(type);
			for (int i = 0; i < customAttributes.Count; i++)
			{
				if (customAttributes[i].Constructor.DeclaringType == typeof(ProgIdAttribute))
				{
					IList<CustomAttributeTypedArgument> constructorArguments = customAttributes[i].ConstructorArguments;
					string text = (string)constructorArguments[0].Value;
					if (text == null)
					{
						text = string.Empty;
					}
					return text;
				}
			}
			return type.FullName;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static object BindToMoniker(string monikerName)
		{
			object ppvResult = null;
			IBindCtx ppbc = null;
			CreateBindCtx(0u, out ppbc);
			IMoniker ppmk = null;
			MkParseDisplayName(ppbc, monikerName, out var _, out ppmk);
			BindMoniker(ppmk, 0u, ref IID_IUnknown, out ppvResult);
			return ppvResult;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static object GetActiveObject(string progID)
		{
			object ppunk = null;
			Guid clsid;
			try
			{
				CLSIDFromProgIDEx(progID, out clsid);
			}
			catch (Exception)
			{
				CLSIDFromProgID(progID, out clsid);
			}
			GetActiveObject(ref clsid, IntPtr.Zero, out ppunk);
			return ppunk;
		}

		[DllImport("ole32.dll", PreserveSig = false)]
		private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

		[DllImport("ole32.dll", PreserveSig = false)]
		private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

		[DllImport("ole32.dll", PreserveSig = false)]
		private static extern void CreateBindCtx(uint reserved, out IBindCtx ppbc);

		[DllImport("ole32.dll", PreserveSig = false)]
		private static extern void MkParseDisplayName(IBindCtx pbc, [MarshalAs(UnmanagedType.LPWStr)] string szUserName, out uint pchEaten, out IMoniker ppmk);

		[DllImport("ole32.dll", PreserveSig = false)]
		private static extern void BindMoniker(IMoniker pmk, uint grfOpt, ref Guid iidResult, [MarshalAs(UnmanagedType.Interface)] out object ppvResult);

		[DllImport("oleaut32.dll", PreserveSig = false)]
		private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out object ppunk);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool InternalSwitchCCW(object oldtp, object newtp);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern object InternalWrapIUnknownWithComObject(IntPtr i);

		private static RuntimeTypeHandle LoadLicenseManager()
		{
			Assembly assembly = Assembly.Load("System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
			Type type = assembly.GetType("System.ComponentModel.LicenseManager");
			if (type == null || !type.IsVisible)
			{
				return RuntimeTypeHandle.EmptyHandle;
			}
			return type.TypeHandle;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static extern void ChangeWrapperHandleStrength(object otp, bool fIsWeak);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Delegate GetDelegateForFunctionPointer(IntPtr ptr, Type t)
		{
			if (ptr == IntPtr.Zero)
			{
				throw new ArgumentNullException("ptr");
			}
			if (t == null)
			{
				throw new ArgumentNullException("t");
			}
			if (!(t is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "t");
			}
			if (t.IsGenericType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "t");
			}
			Type baseType = t.BaseType;
			if (baseType == null || (baseType != typeof(Delegate) && baseType != typeof(MulticastDelegate)))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "t");
			}
			return GetDelegateForFunctionPointerInternal(ptr, t);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Delegate GetDelegateForFunctionPointerInternal(IntPtr ptr, Type t);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr GetFunctionPointerForDelegate(Delegate d)
		{
			if ((object)d == null)
			{
				throw new ArgumentNullException("d");
			}
			return GetFunctionPointerForDelegateInternal(d);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern IntPtr GetFunctionPointerForDelegateInternal(Delegate d);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr SecureStringToBSTR(SecureString s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			return s.ToBSTR();
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr SecureStringToCoTaskMemAnsi(SecureString s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			return s.ToAnsiStr(allocateFromHeap: false);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr SecureStringToGlobalAllocAnsi(SecureString s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			return s.ToAnsiStr(allocateFromHeap: true);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr SecureStringToCoTaskMemUnicode(SecureString s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			return s.ToUniStr(allocateFromHeap: false);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static IntPtr SecureStringToGlobalAllocUnicode(SecureString s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			return s.ToUniStr(allocateFromHeap: true);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void ZeroFreeBSTR(IntPtr s)
		{
			Win32Native.ZeroMemory(s, (uint)(Win32Native.SysStringLen(s) * 2));
			FreeBSTR(s);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void ZeroFreeCoTaskMemAnsi(IntPtr s)
		{
			Win32Native.ZeroMemory(s, (uint)Win32Native.lstrlenA(s));
			FreeCoTaskMem(s);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void ZeroFreeGlobalAllocAnsi(IntPtr s)
		{
			Win32Native.ZeroMemory(s, (uint)Win32Native.lstrlenA(s));
			FreeHGlobal(s);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void ZeroFreeCoTaskMemUnicode(IntPtr s)
		{
			Win32Native.ZeroMemory(s, (uint)(Win32Native.lstrlenW(s) * 2));
			FreeCoTaskMem(s);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void ZeroFreeGlobalAllocUnicode(IntPtr s)
		{
			Win32Native.ZeroMemory(s, (uint)(Win32Native.lstrlenW(s) * 2));
			FreeHGlobal(s);
		}
	}
}
