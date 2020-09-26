using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;

namespace System.Globalization
{
	[Serializable]
	[ComVisible(true)]
	public sealed class NumberFormatInfo : ICloneable, IFormatProvider
	{
		private const NumberStyles InvalidNumberStyles = ~(NumberStyles.Any | NumberStyles.AllowHexSpecifier);

		private static NumberFormatInfo invariantInfo;

		internal int[] numberGroupSizes = new int[1]
		{
			3
		};

		internal int[] currencyGroupSizes = new int[1]
		{
			3
		};

		internal int[] percentGroupSizes = new int[1]
		{
			3
		};

		internal string positiveSign = "+";

		internal string negativeSign = "-";

		internal string numberDecimalSeparator = ".";

		internal string numberGroupSeparator = ",";

		internal string currencyGroupSeparator = ",";

		internal string currencyDecimalSeparator = ".";

		internal string currencySymbol = "¤";

		internal string ansiCurrencySymbol;

		internal string nanSymbol = "NaN";

		internal string positiveInfinitySymbol = "Infinity";

		internal string negativeInfinitySymbol = "-Infinity";

		internal string percentDecimalSeparator = ".";

		internal string percentGroupSeparator = ",";

		internal string percentSymbol = "%";

		internal string perMilleSymbol = "‰";

		[OptionalField(VersionAdded = 2)]
		internal string[] nativeDigits = new string[10]
		{
			"0",
			"1",
			"2",
			"3",
			"4",
			"5",
			"6",
			"7",
			"8",
			"9"
		};

		internal int m_dataItem;

		internal int numberDecimalDigits = 2;

		internal int currencyDecimalDigits = 2;

		internal int currencyPositivePattern;

		internal int currencyNegativePattern;

		internal int numberNegativePattern = 1;

		internal int percentPositivePattern;

		internal int percentNegativePattern;

		internal int percentDecimalDigits = 2;

		[OptionalField(VersionAdded = 2)]
		internal int digitSubstitution = 1;

		internal bool isReadOnly;

		internal bool m_useUserOverride;

		internal bool validForParseAsNumber = true;

		internal bool validForParseAsCurrency = true;

		public static NumberFormatInfo InvariantInfo
		{
			get
			{
				if (invariantInfo == null)
				{
					invariantInfo = ReadOnly(new NumberFormatInfo());
				}
				return invariantInfo;
			}
		}

		public int CurrencyDecimalDigits
		{
			get
			{
				return currencyDecimalDigits;
			}
			set
			{
				VerifyWritable();
				if (value < 0 || value > 99)
				{
					throw new ArgumentOutOfRangeException("CurrencyDecimalDigits", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 0, 99));
				}
				currencyDecimalDigits = value;
			}
		}

		public string CurrencyDecimalSeparator
		{
			get
			{
				return currencyDecimalSeparator;
			}
			set
			{
				VerifyWritable();
				VerifyDecimalSeparator(value, "CurrencyDecimalSeparator");
				currencyDecimalSeparator = value;
			}
		}

		public bool IsReadOnly => isReadOnly;

		public int[] CurrencyGroupSizes
		{
			get
			{
				return (int[])currencyGroupSizes.Clone();
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("CurrencyGroupSizes", Environment.GetResourceString("ArgumentNull_Obj"));
				}
				int[] groupSize = (int[])value.Clone();
				CheckGroupSize("CurrencyGroupSizes", groupSize);
				currencyGroupSizes = groupSize;
			}
		}

		public int[] NumberGroupSizes
		{
			get
			{
				return (int[])numberGroupSizes.Clone();
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("NumberGroupSizes", Environment.GetResourceString("ArgumentNull_Obj"));
				}
				int[] groupSize = (int[])value.Clone();
				CheckGroupSize("NumberGroupSizes", groupSize);
				numberGroupSizes = groupSize;
			}
		}

		public int[] PercentGroupSizes
		{
			get
			{
				return (int[])percentGroupSizes.Clone();
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("PercentGroupSizes", Environment.GetResourceString("ArgumentNull_Obj"));
				}
				int[] groupSize = (int[])value.Clone();
				CheckGroupSize("PercentGroupSizes", groupSize);
				percentGroupSizes = groupSize;
			}
		}

		public string CurrencyGroupSeparator
		{
			get
			{
				return currencyGroupSeparator;
			}
			set
			{
				VerifyWritable();
				VerifyGroupSeparator(value, "CurrencyGroupSeparator");
				currencyGroupSeparator = value;
			}
		}

		public string CurrencySymbol
		{
			get
			{
				return currencySymbol;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("CurrencySymbol", Environment.GetResourceString("ArgumentNull_String"));
				}
				currencySymbol = value;
			}
		}

		public static NumberFormatInfo CurrentInfo
		{
			get
			{
				CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
				if (!currentCulture.m_isInherited)
				{
					NumberFormatInfo numInfo = currentCulture.numInfo;
					if (numInfo != null)
					{
						return numInfo;
					}
				}
				return (NumberFormatInfo)currentCulture.GetFormat(typeof(NumberFormatInfo));
			}
		}

		public string NaNSymbol
		{
			get
			{
				return nanSymbol;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("NaNSymbol", Environment.GetResourceString("ArgumentNull_String"));
				}
				nanSymbol = value;
			}
		}

		public int CurrencyNegativePattern
		{
			get
			{
				return currencyNegativePattern;
			}
			set
			{
				VerifyWritable();
				if (value < 0 || value > 15)
				{
					throw new ArgumentOutOfRangeException("CurrencyNegativePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 0, 15));
				}
				currencyNegativePattern = value;
			}
		}

		public int NumberNegativePattern
		{
			get
			{
				return numberNegativePattern;
			}
			set
			{
				VerifyWritable();
				if (value < 0 || value > 4)
				{
					throw new ArgumentOutOfRangeException("NumberNegativePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 0, 4));
				}
				numberNegativePattern = value;
			}
		}

		public int PercentPositivePattern
		{
			get
			{
				return percentPositivePattern;
			}
			set
			{
				VerifyWritable();
				if (value < 0 || value > 3)
				{
					throw new ArgumentOutOfRangeException("PercentPositivePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 0, 3));
				}
				percentPositivePattern = value;
			}
		}

		public int PercentNegativePattern
		{
			get
			{
				return percentNegativePattern;
			}
			set
			{
				VerifyWritable();
				if (value < 0 || value > 11)
				{
					throw new ArgumentOutOfRangeException("PercentNegativePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 0, 11));
				}
				percentNegativePattern = value;
			}
		}

		public string NegativeInfinitySymbol
		{
			get
			{
				return negativeInfinitySymbol;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("NegativeInfinitySymbol", Environment.GetResourceString("ArgumentNull_String"));
				}
				negativeInfinitySymbol = value;
			}
		}

		public string NegativeSign
		{
			get
			{
				return negativeSign;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("NegativeSign", Environment.GetResourceString("ArgumentNull_String"));
				}
				negativeSign = value;
			}
		}

		public int NumberDecimalDigits
		{
			get
			{
				return numberDecimalDigits;
			}
			set
			{
				VerifyWritable();
				if (value < 0 || value > 99)
				{
					throw new ArgumentOutOfRangeException("NumberDecimalDigits", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 0, 99));
				}
				numberDecimalDigits = value;
			}
		}

		public string NumberDecimalSeparator
		{
			get
			{
				return numberDecimalSeparator;
			}
			set
			{
				VerifyWritable();
				VerifyDecimalSeparator(value, "NumberDecimalSeparator");
				numberDecimalSeparator = value;
			}
		}

		public string NumberGroupSeparator
		{
			get
			{
				return numberGroupSeparator;
			}
			set
			{
				VerifyWritable();
				VerifyGroupSeparator(value, "NumberGroupSeparator");
				numberGroupSeparator = value;
			}
		}

		public int CurrencyPositivePattern
		{
			get
			{
				return currencyPositivePattern;
			}
			set
			{
				VerifyWritable();
				if (value < 0 || value > 3)
				{
					throw new ArgumentOutOfRangeException("CurrencyPositivePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 0, 3));
				}
				currencyPositivePattern = value;
			}
		}

		public string PositiveInfinitySymbol
		{
			get
			{
				return positiveInfinitySymbol;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("PositiveInfinitySymbol", Environment.GetResourceString("ArgumentNull_String"));
				}
				positiveInfinitySymbol = value;
			}
		}

		public string PositiveSign
		{
			get
			{
				return positiveSign;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("PositiveSign", Environment.GetResourceString("ArgumentNull_String"));
				}
				positiveSign = value;
			}
		}

		public int PercentDecimalDigits
		{
			get
			{
				return percentDecimalDigits;
			}
			set
			{
				VerifyWritable();
				if (value < 0 || value > 99)
				{
					throw new ArgumentOutOfRangeException("PercentDecimalDigits", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), 0, 99));
				}
				percentDecimalDigits = value;
			}
		}

		public string PercentDecimalSeparator
		{
			get
			{
				return percentDecimalSeparator;
			}
			set
			{
				VerifyWritable();
				VerifyDecimalSeparator(value, "PercentDecimalSeparator");
				percentDecimalSeparator = value;
			}
		}

		public string PercentGroupSeparator
		{
			get
			{
				return percentGroupSeparator;
			}
			set
			{
				VerifyWritable();
				VerifyGroupSeparator(value, "PercentGroupSeparator");
				percentGroupSeparator = value;
			}
		}

		public string PercentSymbol
		{
			get
			{
				return percentSymbol;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("PercentSymbol", Environment.GetResourceString("ArgumentNull_String"));
				}
				percentSymbol = value;
			}
		}

		public string PerMilleSymbol
		{
			get
			{
				return perMilleSymbol;
			}
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("PerMilleSymbol", Environment.GetResourceString("ArgumentNull_String"));
				}
				perMilleSymbol = value;
			}
		}

		[ComVisible(false)]
		public string[] NativeDigits
		{
			get
			{
				return nativeDigits;
			}
			set
			{
				VerifyWritable();
				VerifyNativeDigits(value, "NativeDigits");
				nativeDigits = value;
			}
		}

		[ComVisible(false)]
		public DigitShapes DigitSubstitution
		{
			get
			{
				return (DigitShapes)digitSubstitution;
			}
			set
			{
				VerifyWritable();
				VerifyDigitSubstitution(value, "DigitSubstitution");
				digitSubstitution = (int)value;
			}
		}

		public NumberFormatInfo()
			: this(null)
		{
		}

		[OnSerializing]
		private void OnSerializing(StreamingContext ctx)
		{
			if (numberDecimalSeparator != numberGroupSeparator)
			{
				validForParseAsNumber = true;
			}
			else
			{
				validForParseAsNumber = false;
			}
			if (numberDecimalSeparator != numberGroupSeparator && numberDecimalSeparator != currencyGroupSeparator && currencyDecimalSeparator != numberGroupSeparator && currencyDecimalSeparator != currencyGroupSeparator)
			{
				validForParseAsCurrency = true;
			}
			else
			{
				validForParseAsCurrency = false;
			}
		}

		[OnDeserializing]
		private void OnDeserializing(StreamingContext ctx)
		{
			nativeDigits = null;
			digitSubstitution = -1;
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext ctx)
		{
			if (nativeDigits == null)
			{
				nativeDigits = new string[10]
				{
					"0",
					"1",
					"2",
					"3",
					"4",
					"5",
					"6",
					"7",
					"8",
					"9"
				};
			}
			if (digitSubstitution < 0)
			{
				digitSubstitution = 1;
			}
		}

		private void VerifyDecimalSeparator(string decSep, string propertyName)
		{
			if (decSep == null)
			{
				throw new ArgumentNullException(propertyName, Environment.GetResourceString("ArgumentNull_String"));
			}
			if (decSep.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyDecString"));
			}
		}

		private void VerifyGroupSeparator(string groupSep, string propertyName)
		{
			if (groupSep == null)
			{
				throw new ArgumentNullException(propertyName, Environment.GetResourceString("ArgumentNull_String"));
			}
		}

		private void VerifyNativeDigits(string[] nativeDig, string propertyName)
		{
			if (nativeDig == null)
			{
				throw new ArgumentNullException(propertyName, Environment.GetResourceString("ArgumentNull_Array"));
			}
			if (nativeDig.Length != 10)
			{
				throw new ArgumentException(propertyName, Environment.GetResourceString("Argument_InvalidNativeDigitCount"));
			}
			for (int i = 0; i < nativeDig.Length; i++)
			{
				if (nativeDig[i] == null)
				{
					throw new ArgumentNullException(propertyName, Environment.GetResourceString("ArgumentNull_ArrayValue"));
				}
				if (nativeDig[i].Length != 1)
				{
					if (nativeDig[i].Length != 2)
					{
						throw new ArgumentException(propertyName, Environment.GetResourceString("Argument_InvalidNativeDigitValue"));
					}
					if (!char.IsSurrogatePair(nativeDig[i][0], nativeDig[i][1]))
					{
						throw new ArgumentException(propertyName, Environment.GetResourceString("Argument_InvalidNativeDigitValue"));
					}
				}
				if (CharUnicodeInfo.GetDecimalDigitValue(nativeDig[i], 0) != i && CharUnicodeInfo.GetUnicodeCategory(nativeDig[i], 0) != UnicodeCategory.PrivateUse)
				{
					throw new ArgumentException(propertyName, Environment.GetResourceString("Argument_InvalidNativeDigitValue"));
				}
			}
		}

		private void VerifyDigitSubstitution(DigitShapes digitSub, string propertyName)
		{
			switch (digitSub)
			{
			case DigitShapes.Context:
			case DigitShapes.None:
			case DigitShapes.NativeNational:
				return;
			}
			throw new ArgumentException(propertyName, Environment.GetResourceString("Argument_InvalidDigitSubstitution"));
		}

		internal NumberFormatInfo(CultureTableRecord cultureTableRecord)
		{
			if (cultureTableRecord != null)
			{
				cultureTableRecord.GetNFIOverrideValues(this);
				if (932 == cultureTableRecord.IDEFAULTANSICODEPAGE || 949 == cultureTableRecord.IDEFAULTANSICODEPAGE)
				{
					ansiCurrencySymbol = "\\";
				}
				negativeInfinitySymbol = cultureTableRecord.SNEGINFINITY;
				positiveInfinitySymbol = cultureTableRecord.SPOSINFINITY;
				nanSymbol = cultureTableRecord.SNAN;
			}
		}

		private void VerifyWritable()
		{
			if (isReadOnly)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
			}
		}

		public static NumberFormatInfo GetInstance(IFormatProvider formatProvider)
		{
			CultureInfo cultureInfo = formatProvider as CultureInfo;
			NumberFormatInfo numInfo;
			if (cultureInfo != null && !cultureInfo.m_isInherited)
			{
				numInfo = cultureInfo.numInfo;
				if (numInfo != null)
				{
					return numInfo;
				}
				return cultureInfo.NumberFormat;
			}
			numInfo = formatProvider as NumberFormatInfo;
			if (numInfo != null)
			{
				return numInfo;
			}
			if (formatProvider != null)
			{
				numInfo = formatProvider.GetFormat(typeof(NumberFormatInfo)) as NumberFormatInfo;
				if (numInfo != null)
				{
					return numInfo;
				}
			}
			return CurrentInfo;
		}

		public object Clone()
		{
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo)MemberwiseClone();
			numberFormatInfo.isReadOnly = false;
			return numberFormatInfo;
		}

		internal void CheckGroupSize(string propName, int[] groupSize)
		{
			for (int i = 0; i < groupSize.Length; i++)
			{
				if (groupSize[i] < 1)
				{
					if (i == groupSize.Length - 1 && groupSize[i] == 0)
					{
						break;
					}
					throw new ArgumentException(propName, Environment.GetResourceString("Argument_InvalidGroupSize"));
				}
				if (groupSize[i] > 9)
				{
					throw new ArgumentException(propName, Environment.GetResourceString("Argument_InvalidGroupSize"));
				}
			}
		}

		public object GetFormat(Type formatType)
		{
			if (formatType != typeof(NumberFormatInfo))
			{
				return null;
			}
			return this;
		}

		public static NumberFormatInfo ReadOnly(NumberFormatInfo nfi)
		{
			if (nfi == null)
			{
				throw new ArgumentNullException("nfi");
			}
			if (nfi.IsReadOnly)
			{
				return nfi;
			}
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo)nfi.MemberwiseClone();
			numberFormatInfo.isReadOnly = true;
			return numberFormatInfo;
		}

		internal static void ValidateParseStyleInteger(NumberStyles style)
		{
			if (((uint)style & 0xFFFFFC00u) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidNumberStyles"), "style");
			}
			if ((style & NumberStyles.AllowHexSpecifier) != 0 && ((uint)style & 0xFFFFFDFCu) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidHexStyle"));
			}
		}

		internal static void ValidateParseStyleFloatingPoint(NumberStyles style)
		{
			if (((uint)style & 0xFFFFFC00u) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidNumberStyles"), "style");
			}
			if ((style & NumberStyles.AllowHexSpecifier) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_HexStyleNotSupported"));
			}
		}
	}
}
