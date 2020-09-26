using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Text
{
	internal class Normalization
	{
		private const int ERROR_SUCCESS = 0;

		private const int ERROR_NOT_ENOUGH_MEMORY = 8;

		private const int ERROR_INVALID_PARAMETER = 87;

		private const int ERROR_INSUFFICIENT_BUFFER = 122;

		private const int ERROR_NO_UNICODE_TRANSLATION = 1113;

		private static Normalization NFC;

		private static Normalization NFD;

		private static Normalization NFKC;

		private static Normalization NFKD;

		private static Normalization IDNA;

		private static Normalization NFCDisallowUnassigned;

		private static Normalization NFDDisallowUnassigned;

		private static Normalization NFKCDisallowUnassigned;

		private static Normalization NFKDDisallowUnassigned;

		private static Normalization IDNADisallowUnassigned;

		private NormalizationForm normalizationForm;

		internal unsafe Normalization(NormalizationForm form, string strDataFile)
		{
			normalizationForm = form;
			if (!nativeLoadNormalizationDLL())
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidNormalizationForm"));
			}
			byte* globalizationResourceBytePtr = GlobalizationAssembly.GetGlobalizationResourceBytePtr(typeof(Normalization).Assembly, strDataFile);
			if (globalizationResourceBytePtr == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidNormalizationForm"));
			}
			byte* ptr = nativeNormalizationInitNormalization(form, globalizationResourceBytePtr);
			if (ptr == null)
			{
				throw new OutOfMemoryException(Environment.GetResourceString("Arg_OutOfMemoryException"));
			}
		}

		internal static Normalization GetNormalization(NormalizationForm form)
		{
			return form switch
			{
				NormalizationForm.FormC => GetFormC(), 
				NormalizationForm.FormD => GetFormD(), 
				NormalizationForm.FormKC => GetFormKC(), 
				NormalizationForm.FormKD => GetFormKD(), 
				(NormalizationForm)13 => GetFormIDNA(), 
				(NormalizationForm)257 => GetFormCDisallowUnassigned(), 
				(NormalizationForm)258 => GetFormDDisallowUnassigned(), 
				(NormalizationForm)261 => GetFormKCDisallowUnassigned(), 
				(NormalizationForm)262 => GetFormKDDisallowUnassigned(), 
				(NormalizationForm)269 => GetFormIDNADisallowUnassigned(), 
				_ => throw new ArgumentException(Environment.GetResourceString("Argument_InvalidNormalizationForm")), 
			};
		}

		internal static Normalization GetFormC()
		{
			if (NFC != null)
			{
				return NFC;
			}
			NFC = new Normalization(NormalizationForm.FormC, "normnfc.nlp");
			return NFC;
		}

		internal static Normalization GetFormD()
		{
			if (NFD != null)
			{
				return NFD;
			}
			NFD = new Normalization(NormalizationForm.FormD, "normnfd.nlp");
			return NFD;
		}

		internal static Normalization GetFormKC()
		{
			if (NFKC != null)
			{
				return NFKC;
			}
			NFKC = new Normalization(NormalizationForm.FormKC, "normnfkc.nlp");
			return NFKC;
		}

		internal static Normalization GetFormKD()
		{
			if (NFKD != null)
			{
				return NFKD;
			}
			NFKD = new Normalization(NormalizationForm.FormKD, "normnfkd.nlp");
			return NFKD;
		}

		internal static Normalization GetFormIDNA()
		{
			if (IDNA != null)
			{
				return IDNA;
			}
			IDNA = new Normalization((NormalizationForm)13, "normidna.nlp");
			return IDNA;
		}

		internal static Normalization GetFormCDisallowUnassigned()
		{
			if (NFCDisallowUnassigned != null)
			{
				return NFCDisallowUnassigned;
			}
			NFCDisallowUnassigned = new Normalization((NormalizationForm)257, "normnfc.nlp");
			return NFCDisallowUnassigned;
		}

		internal static Normalization GetFormDDisallowUnassigned()
		{
			if (NFDDisallowUnassigned != null)
			{
				return NFDDisallowUnassigned;
			}
			NFDDisallowUnassigned = new Normalization((NormalizationForm)258, "normnfd.nlp");
			return NFDDisallowUnassigned;
		}

		internal static Normalization GetFormKCDisallowUnassigned()
		{
			if (NFKCDisallowUnassigned != null)
			{
				return NFKCDisallowUnassigned;
			}
			NFKCDisallowUnassigned = new Normalization((NormalizationForm)261, "normnfkc.nlp");
			return NFKCDisallowUnassigned;
		}

		internal static Normalization GetFormKDDisallowUnassigned()
		{
			if (NFKDDisallowUnassigned != null)
			{
				return NFKDDisallowUnassigned;
			}
			NFKDDisallowUnassigned = new Normalization((NormalizationForm)262, "normnfkd.nlp");
			return NFKDDisallowUnassigned;
		}

		internal static Normalization GetFormIDNADisallowUnassigned()
		{
			if (IDNADisallowUnassigned != null)
			{
				return IDNADisallowUnassigned;
			}
			IDNADisallowUnassigned = new Normalization((NormalizationForm)269, "normidna.nlp");
			return IDNADisallowUnassigned;
		}

		internal static bool IsNormalized(string strInput, NormalizationForm normForm)
		{
			return GetNormalization(normForm).IsNormalized(strInput);
		}

		private bool IsNormalized(string strInput)
		{
			if (strInput == null)
			{
				throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_String"), "strInput");
			}
			int iError = 0;
			int num = nativeNormalizationIsNormalizedString(normalizationForm, ref iError, strInput, strInput.Length);
			return iError switch
			{
				1113 => throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"), "strInput"), 
				8 => throw new OutOfMemoryException(Environment.GetResourceString("Arg_OutOfMemoryException")), 
				0 => (num & 1) == 1, 
				_ => throw new InvalidOperationException(Environment.GetResourceString("UnknownError_Num", iError)), 
			};
		}

		internal static string Normalize(string strInput, NormalizationForm normForm)
		{
			return GetNormalization(normForm).Normalize(strInput);
		}

		internal string Normalize(string strInput)
		{
			if (strInput == null)
			{
				throw new ArgumentNullException("strInput", Environment.GetResourceString("ArgumentNull_String"));
			}
			int num = GuessLength(strInput);
			if (num == 0)
			{
				return string.Empty;
			}
			char[] array = null;
			int iError = 122;
			while (iError == 122)
			{
				array = new char[num];
				num = nativeNormalizationNormalizeString(normalizationForm, ref iError, strInput, strInput.Length, array, array.Length);
				if (iError != 0)
				{
					switch (iError)
					{
					case 1113:
						throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequence", num), "strInput");
					case 8:
						throw new OutOfMemoryException(Environment.GetResourceString("Arg_OutOfMemoryException"));
					default:
						throw new InvalidOperationException(Environment.GetResourceString("UnknownError_Num", iError));
					case 122:
						break;
					}
				}
			}
			return new string(array, 0, num);
		}

		internal int GuessLength(string strInput)
		{
			if (strInput == null)
			{
				throw new ArgumentNullException("strInput", Environment.GetResourceString("ArgumentNull_String"));
			}
			int iError = 0;
			int num = nativeNormalizationNormalizeString(normalizationForm, ref iError, strInput, strInput.Length, null, 0);
			return iError switch
			{
				8 => throw new OutOfMemoryException(Environment.GetResourceString("Arg_OutOfMemoryException")), 
				0 => num, 
				_ => throw new InvalidOperationException(Environment.GetResourceString("UnknownError_Num", iError)), 
			};
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool nativeLoadNormalizationDLL();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int nativeNormalizationNormalizeString(NormalizationForm NormForm, ref int iError, string lpSrcString, int cwSrcLength, char[] lpDstString, int cwDstLength);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int nativeNormalizationIsNormalizedString(NormalizationForm NormForm, ref int iError, string lpString, int cwLength);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern byte* nativeNormalizationInitNormalization(NormalizationForm NormForm, byte* pTableData);
	}
}
