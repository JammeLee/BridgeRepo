using System.Globalization;

namespace System.Net
{
	internal class FtpMethodInfo
	{
		internal string Method;

		internal FtpOperation Operation;

		internal FtpMethodFlags Flags;

		internal string HttpCommand;

		private static readonly FtpMethodInfo[] KnownMethodInfo = new FtpMethodInfo[13]
		{
			new FtpMethodInfo("RETR", FtpOperation.DownloadFile, FtpMethodFlags.IsDownload | FtpMethodFlags.TakesParameter | FtpMethodFlags.HasHttpCommand, "GET"),
			new FtpMethodInfo("NLST", FtpOperation.ListDirectory, FtpMethodFlags.IsDownload | FtpMethodFlags.MayTakeParameter | FtpMethodFlags.HasHttpCommand, "GET"),
			new FtpMethodInfo("LIST", FtpOperation.ListDirectoryDetails, FtpMethodFlags.IsDownload | FtpMethodFlags.MayTakeParameter | FtpMethodFlags.HasHttpCommand, "GET"),
			new FtpMethodInfo("STOR", FtpOperation.UploadFile, FtpMethodFlags.IsUpload | FtpMethodFlags.TakesParameter, null),
			new FtpMethodInfo("STOU", FtpOperation.UploadFileUnique, FtpMethodFlags.IsUpload | FtpMethodFlags.DoesNotTakeParameter | FtpMethodFlags.ShouldParseForResponseUri, null),
			new FtpMethodInfo("APPE", FtpOperation.AppendFile, FtpMethodFlags.IsUpload | FtpMethodFlags.TakesParameter, null),
			new FtpMethodInfo("DELE", FtpOperation.DeleteFile, FtpMethodFlags.TakesParameter, null),
			new FtpMethodInfo("MDTM", FtpOperation.GetDateTimestamp, FtpMethodFlags.TakesParameter, null),
			new FtpMethodInfo("SIZE", FtpOperation.GetFileSize, FtpMethodFlags.TakesParameter, null),
			new FtpMethodInfo("RENAME", FtpOperation.Rename, FtpMethodFlags.TakesParameter, null),
			new FtpMethodInfo("MKD", FtpOperation.MakeDirectory, FtpMethodFlags.TakesParameter | FtpMethodFlags.ParameterIsDirectory, null),
			new FtpMethodInfo("RMD", FtpOperation.RemoveDirectory, FtpMethodFlags.TakesParameter | FtpMethodFlags.ParameterIsDirectory, null),
			new FtpMethodInfo("PWD", FtpOperation.PrintWorkingDirectory, FtpMethodFlags.DoesNotTakeParameter, null)
		};

		internal bool IsCommandOnly => (Flags & (FtpMethodFlags.IsDownload | FtpMethodFlags.IsUpload)) == 0;

		internal bool IsUpload => (Flags & FtpMethodFlags.IsUpload) != 0;

		internal bool IsDownload => (Flags & FtpMethodFlags.IsDownload) != 0;

		internal bool HasHttpCommand => (Flags & FtpMethodFlags.HasHttpCommand) != 0;

		internal bool ShouldParseForResponseUri => (Flags & FtpMethodFlags.ShouldParseForResponseUri) != 0;

		internal bool IsUnknownMethod => Operation == FtpOperation.Other;

		internal FtpMethodInfo(string method, FtpOperation operation, FtpMethodFlags flags, string httpCommand)
		{
			Method = method;
			Operation = operation;
			Flags = flags;
			HttpCommand = httpCommand;
		}

		internal bool HasFlag(FtpMethodFlags flags)
		{
			return (Flags & flags) != 0;
		}

		internal static FtpMethodInfo GetMethodInfo(string method)
		{
			method = method.ToUpper(CultureInfo.InvariantCulture);
			FtpMethodInfo[] knownMethodInfo = KnownMethodInfo;
			foreach (FtpMethodInfo ftpMethodInfo in knownMethodInfo)
			{
				if (method == ftpMethodInfo.Method)
				{
					return ftpMethodInfo;
				}
			}
			throw new ArgumentException(SR.GetString("net_ftp_unsupported_method"), "method");
		}
	}
}
