using System.Security.Permissions;
using Microsoft.Win32;

namespace System.Runtime.InteropServices
{
	[ComVisible(true)]
	public class StandardOleMarshalObject : MarshalByRefObject, Microsoft.Win32.UnsafeNativeMethods.IMarshal
	{
		protected StandardOleMarshalObject()
		{
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		private IntPtr GetStdMarshaller(ref Guid riid, int dwDestContext, int mshlflags)
		{
			IntPtr ppMarshal = IntPtr.Zero;
			IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(this);
			if (iUnknownForObject != IntPtr.Zero)
			{
				try
				{
					if (Microsoft.Win32.UnsafeNativeMethods.CoGetStandardMarshal(ref riid, iUnknownForObject, dwDestContext, IntPtr.Zero, mshlflags, out ppMarshal) == 0)
					{
						return ppMarshal;
					}
				}
				finally
				{
					Marshal.Release(iUnknownForObject);
				}
			}
			throw new InvalidOperationException(SR.GetString("StandardOleMarshalObjectGetMarshalerFailed", riid.ToString()));
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		int Microsoft.Win32.UnsafeNativeMethods.IMarshal.GetUnmarshalClass(ref Guid riid, IntPtr pv, int dwDestContext, IntPtr pvDestContext, int mshlflags, out Guid pCid)
		{
			pCid = typeof(Microsoft.Win32.UnsafeNativeMethods.IStdMarshal).GUID;
			return 0;
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		int Microsoft.Win32.UnsafeNativeMethods.IMarshal.GetMarshalSizeMax(ref Guid riid, IntPtr pv, int dwDestContext, IntPtr pvDestContext, int mshlflags, out int pSize)
		{
			Guid riid2 = riid;
			IntPtr stdMarshaller = GetStdMarshaller(ref riid2, dwDestContext, mshlflags);
			try
			{
				return Microsoft.Win32.UnsafeNativeMethods.CoGetMarshalSizeMax(out pSize, ref riid2, stdMarshaller, dwDestContext, pvDestContext, mshlflags);
			}
			finally
			{
				Marshal.Release(stdMarshaller);
			}
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		int Microsoft.Win32.UnsafeNativeMethods.IMarshal.MarshalInterface(object pStm, ref Guid riid, IntPtr pv, int dwDestContext, IntPtr pvDestContext, int mshlflags)
		{
			Guid riid2 = riid;
			IntPtr stdMarshaller = GetStdMarshaller(ref riid2, dwDestContext, mshlflags);
			try
			{
				return Microsoft.Win32.UnsafeNativeMethods.CoMarshalInterface(pStm, ref riid2, stdMarshaller, dwDestContext, pvDestContext, mshlflags);
			}
			finally
			{
				Marshal.Release(stdMarshaller);
				if (pStm != null)
				{
					Marshal.ReleaseComObject(pStm);
				}
			}
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		int Microsoft.Win32.UnsafeNativeMethods.IMarshal.UnmarshalInterface(object pStm, ref Guid riid, out IntPtr ppv)
		{
			ppv = IntPtr.Zero;
			if (pStm != null)
			{
				Marshal.ReleaseComObject(pStm);
			}
			return -2147467263;
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		int Microsoft.Win32.UnsafeNativeMethods.IMarshal.ReleaseMarshalData(object pStm)
		{
			if (pStm != null)
			{
				Marshal.ReleaseComObject(pStm);
			}
			return -2147467263;
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		int Microsoft.Win32.UnsafeNativeMethods.IMarshal.DisconnectObject(int dwReserved)
		{
			return -2147467263;
		}
	}
}
