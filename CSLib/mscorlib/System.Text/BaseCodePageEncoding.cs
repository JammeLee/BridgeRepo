using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Text
{
	[Serializable]
	internal abstract class BaseCodePageEncoding : EncodingNLS, ISerializable
	{
		[StructLayout(LayoutKind.Explicit)]
		internal struct CodePageDataFileHeader
		{
			[FieldOffset(0)]
			internal char TableName;

			[FieldOffset(32)]
			internal ushort Version;

			[FieldOffset(40)]
			internal short CodePageCount;

			[FieldOffset(42)]
			internal short unused1;

			[FieldOffset(44)]
			internal CodePageIndex CodePages;
		}

		[StructLayout(LayoutKind.Explicit, Pack = 2)]
		internal struct CodePageIndex
		{
			[FieldOffset(0)]
			internal char CodePageName;

			[FieldOffset(32)]
			internal short CodePage;

			[FieldOffset(34)]
			internal short ByteCount;

			[FieldOffset(36)]
			internal int Offset;
		}

		[StructLayout(LayoutKind.Explicit)]
		internal struct CodePageHeader
		{
			[FieldOffset(0)]
			internal char CodePageName;

			[FieldOffset(32)]
			internal ushort VersionMajor;

			[FieldOffset(34)]
			internal ushort VersionMinor;

			[FieldOffset(36)]
			internal ushort VersionRevision;

			[FieldOffset(38)]
			internal ushort VersionBuild;

			[FieldOffset(40)]
			internal short CodePage;

			[FieldOffset(42)]
			internal short ByteCount;

			[FieldOffset(44)]
			internal char UnicodeReplace;

			[FieldOffset(46)]
			internal ushort ByteReplace;

			[FieldOffset(48)]
			internal short FirstDataWord;
		}

		internal const string CODE_PAGE_DATA_FILE_NAME = "codepages.nlp";

		[NonSerialized]
		protected int dataTableCodePage;

		[NonSerialized]
		protected bool bFlagDataTable = true;

		[NonSerialized]
		protected int iExtraBytes;

		[NonSerialized]
		protected char[] arrayUnicodeBestFit;

		[NonSerialized]
		protected char[] arrayBytesBestFit;

		[NonSerialized]
		protected bool m_bUseMlangTypeForSerialization;

		private unsafe static CodePageDataFileHeader* m_pCodePageFileHeader = (CodePageDataFileHeader*)GlobalizationAssembly.GetGlobalizationResourceBytePtr(typeof(CharUnicodeInfo).Assembly, "codepages.nlp");

		[NonSerialized]
		protected unsafe CodePageHeader* pCodePage = null;

		[NonSerialized]
		protected SafeViewOfFileHandle safeMemorySectionHandle;

		[NonSerialized]
		protected SafeFileMappingHandle safeFileMappingHandle;

		internal BaseCodePageEncoding(int codepage)
			: this(codepage, codepage)
		{
		}

		internal unsafe BaseCodePageEncoding(int codepage, int dataCodePage)
			: base((codepage == 0) ? Win32Native.GetACP() : codepage)
		{
			dataTableCodePage = dataCodePage;
			LoadCodePageTables();
		}

		internal unsafe BaseCodePageEncoding(SerializationInfo info, StreamingContext context)
			: base(0)
		{
			throw new ArgumentNullException("this");
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			SerializeEncoding(info, context);
			info.AddValue(m_bUseMlangTypeForSerialization ? "m_maxByteSize" : "maxCharSize", IsSingleByte ? 1 : 2);
			info.SetType(m_bUseMlangTypeForSerialization ? typeof(MLangCodePageEncoding) : typeof(CodePageEncoding));
		}

		private unsafe void LoadCodePageTables()
		{
			CodePageHeader* ptr = FindCodePage(dataTableCodePage);
			if (ptr == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_NoCodepageData", CodePage));
			}
			pCodePage = ptr;
			LoadManagedCodePage();
		}

		private unsafe static CodePageHeader* FindCodePage(int codePage)
		{
			for (int i = 0; i < m_pCodePageFileHeader->CodePageCount; i++)
			{
				CodePageIndex* ptr = &m_pCodePageFileHeader->CodePages + i;
				if (ptr->CodePage == codePage)
				{
					return (CodePageHeader*)((byte*)m_pCodePageFileHeader + ptr->Offset);
				}
			}
			return null;
		}

		internal unsafe static int GetCodePageByteSize(int codePage)
		{
			CodePageHeader* ptr = FindCodePage(codePage);
			if (ptr == null)
			{
				return 0;
			}
			return ptr->ByteCount;
		}

		protected abstract void LoadManagedCodePage();

		protected unsafe byte* GetSharedMemory(int iSize)
		{
			string memorySectionName = GetMemorySectionName();
			IntPtr mappedFileHandle;
			byte* ptr = EncodingTable.nativeCreateOpenFileMapping(memorySectionName, iSize, out mappedFileHandle);
			if (ptr == null)
			{
				throw new OutOfMemoryException(Environment.GetResourceString("Arg_OutOfMemoryException"));
			}
			if (mappedFileHandle != IntPtr.Zero)
			{
				safeMemorySectionHandle = new SafeViewOfFileHandle((IntPtr)ptr, ownsHandle: true);
				safeFileMappingHandle = new SafeFileMappingHandle(mappedFileHandle, ownsHandle: true);
			}
			return ptr;
		}

		protected unsafe virtual string GetMemorySectionName()
		{
			int num = (bFlagDataTable ? dataTableCodePage : CodePage);
			return string.Format(CultureInfo.InvariantCulture, "NLS_CodePage_{0}_{1}_{2}_{3}_{4}", num, pCodePage->VersionMajor, pCodePage->VersionMinor, pCodePage->VersionRevision, pCodePage->VersionBuild);
		}

		protected abstract void ReadBestFitTable();

		internal override char[] GetBestFitUnicodeToBytesData()
		{
			if (arrayUnicodeBestFit == null)
			{
				ReadBestFitTable();
			}
			return arrayUnicodeBestFit;
		}

		internal override char[] GetBestFitBytesToUnicodeData()
		{
			if (arrayUnicodeBestFit == null)
			{
				ReadBestFitTable();
			}
			return arrayBytesBestFit;
		}

		internal void CheckMemorySection()
		{
			if (safeMemorySectionHandle != null && safeMemorySectionHandle.DangerousGetHandle() == IntPtr.Zero)
			{
				LoadManagedCodePage();
			}
		}
	}
}
