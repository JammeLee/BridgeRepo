using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32;

namespace System.Diagnostics
{
	public static class CounterSampleCalculator
	{
		private static bool perfCounterDllLoaded;

		private static float GetElapsedTime(CounterSample oldSample, CounterSample newSample)
		{
			if (newSample.RawValue == 0)
			{
				return 0f;
			}
			float num = (ulong)oldSample.CounterFrequency;
			if (oldSample.UnsignedRawValue >= (ulong)newSample.CounterTimeStamp || num <= 0f)
			{
				return 0f;
			}
			float num2 = (ulong)newSample.CounterTimeStamp - oldSample.UnsignedRawValue;
			return num2 / num;
		}

		public static float ComputeCounterValue(CounterSample newSample)
		{
			return ComputeCounterValue(CounterSample.Empty, newSample);
		}

		public static float ComputeCounterValue(CounterSample oldSample, CounterSample newSample)
		{
			int counterType = (int)newSample.CounterType;
			if (oldSample.SystemFrequency == 0)
			{
				if (counterType != 537003008 && counterType != 65536 && counterType != 0 && counterType != 65792 && counterType != 256 && counterType != 1107494144)
				{
					return 0f;
				}
			}
			else if (oldSample.CounterType != newSample.CounterType)
			{
				throw new InvalidOperationException(SR.GetString("MismatchedCounterTypes"));
			}
			if (counterType == 807666944)
			{
				return GetElapsedTime(oldSample, newSample);
			}
			NativeMethods.PDH_RAW_COUNTER pDH_RAW_COUNTER = new NativeMethods.PDH_RAW_COUNTER();
			NativeMethods.PDH_RAW_COUNTER pDH_RAW_COUNTER2 = new NativeMethods.PDH_RAW_COUNTER();
			FillInValues(oldSample, newSample, pDH_RAW_COUNTER2, pDH_RAW_COUNTER);
			LoadPerfCounterDll();
			NativeMethods.PDH_FMT_COUNTERVALUE pDH_FMT_COUNTERVALUE = new NativeMethods.PDH_FMT_COUNTERVALUE();
			long pTimeBase = newSample.SystemFrequency;
			int num = Microsoft.Win32.SafeNativeMethods.FormatFromRawValue((uint)counterType, 37376u, ref pTimeBase, pDH_RAW_COUNTER, pDH_RAW_COUNTER2, pDH_FMT_COUNTERVALUE);
			switch (num)
			{
			case -2147481643:
			case -2147481642:
			case -2147481640:
				return 0f;
			default:
				throw new Win32Exception(num, SR.GetString("PerfCounterPdhError", num.ToString("x", CultureInfo.InvariantCulture)));
			case 0:
				return (float)pDH_FMT_COUNTERVALUE.data;
			}
		}

		private static void FillInValues(CounterSample oldSample, CounterSample newSample, NativeMethods.PDH_RAW_COUNTER oldPdhValue, NativeMethods.PDH_RAW_COUNTER newPdhValue)
		{
			int counterType = (int)newSample.CounterType;
			switch (counterType)
			{
			case 4260864:
			case 4523008:
			case 6620416:
			case 272696320:
			case 543229184:
				newPdhValue.FirstValue = newSample.RawValue;
				newPdhValue.SecondValue = newSample.TimeStamp;
				oldPdhValue.FirstValue = oldSample.RawValue;
				oldPdhValue.SecondValue = oldSample.TimeStamp;
				break;
			case 5571840:
				newPdhValue.FirstValue = newSample.RawValue;
				newPdhValue.SecondValue = newSample.TimeStamp100nSec;
				oldPdhValue.FirstValue = oldSample.RawValue;
				oldPdhValue.SecondValue = oldSample.TimeStamp100nSec;
				break;
			case 4523264:
			case 272696576:
			case 541132032:
			case 557909248:
			case 574686464:
			case 591463680:
				newPdhValue.FirstValue = newSample.RawValue;
				newPdhValue.SecondValue = newSample.TimeStamp;
				oldPdhValue.FirstValue = oldSample.RawValue;
				oldPdhValue.SecondValue = oldSample.TimeStamp;
				if (counterType == 574686464 || counterType == 591463680)
				{
					newPdhValue.FirstValue *= (uint)newSample.CounterFrequency;
					if (oldSample.CounterFrequency != 0)
					{
						oldPdhValue.FirstValue *= (uint)oldSample.CounterFrequency;
					}
				}
				if ((counterType & 0x2000000) == 33554432)
				{
					newPdhValue.MultiCount = (int)newSample.BaseValue;
					oldPdhValue.MultiCount = (int)oldSample.BaseValue;
				}
				break;
			case 0:
			case 256:
			case 65536:
			case 65792:
			case 4195328:
			case 4195584:
				newPdhValue.FirstValue = newSample.RawValue;
				newPdhValue.SecondValue = 0L;
				oldPdhValue.FirstValue = oldSample.RawValue;
				oldPdhValue.SecondValue = 0L;
				break;
			case 542180608:
			case 558957824:
			case 575735040:
			case 592512256:
				newPdhValue.FirstValue = newSample.RawValue;
				newPdhValue.SecondValue = newSample.TimeStamp100nSec;
				oldPdhValue.FirstValue = oldSample.RawValue;
				oldPdhValue.SecondValue = oldSample.TimeStamp100nSec;
				if ((counterType & 0x2000000) == 33554432)
				{
					newPdhValue.MultiCount = (int)newSample.BaseValue;
					oldPdhValue.MultiCount = (int)oldSample.BaseValue;
				}
				break;
			case 537003008:
			case 537003264:
			case 541525248:
			case 542573824:
			case 543622400:
			case 549585920:
			case 805438464:
			case 1073874176:
				newPdhValue.FirstValue = newSample.RawValue;
				newPdhValue.SecondValue = newSample.BaseValue;
				oldPdhValue.FirstValue = oldSample.RawValue;
				oldPdhValue.SecondValue = oldSample.BaseValue;
				break;
			default:
				newPdhValue.FirstValue = 0L;
				newPdhValue.SecondValue = 0L;
				oldPdhValue.FirstValue = 0L;
				oldPdhValue.SecondValue = 0L;
				break;
			}
		}

		private static void LoadPerfCounterDll()
		{
			if (perfCounterDllLoaded)
			{
				return;
			}
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
			IntPtr moduleHandle = NativeMethods.GetModuleHandle("mscorwks.dll");
			if (moduleHandle == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			int num = 132;
			HandleRef hModule = new HandleRef(null, moduleHandle);
			StringBuilder stringBuilder;
			do
			{
				num *= 2;
				stringBuilder = new StringBuilder(num);
				num = Microsoft.Win32.UnsafeNativeMethods.GetModuleFileName(hModule, stringBuilder, num);
				if (num == 0)
				{
					throw new Win32Exception();
				}
			}
			while (num == stringBuilder.Capacity);
			string directoryName = Path.GetDirectoryName(stringBuilder.ToString());
			string libFilename = Path.Combine(directoryName, "perfcounter.dll");
			if (Microsoft.Win32.SafeNativeMethods.LoadLibrary(libFilename) == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			perfCounterDllLoaded = true;
		}
	}
}
