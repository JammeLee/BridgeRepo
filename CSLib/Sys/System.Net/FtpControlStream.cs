using System.Collections;
using System.Globalization;
using System.IO;
using System.Net.Cache;
using System.Net.Sockets;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace System.Net
{
	internal class FtpControlStream : CommandStream
	{
		private enum GetPathOption
		{
			Normal,
			AssumeFilename,
			AssumeNoFilename
		}

		private Socket m_DataSocket;

		private IPEndPoint m_PassiveEndPoint;

		private TlsStream m_TlsStream;

		private StringBuilder m_BannerMessage;

		private StringBuilder m_WelcomeMessage;

		private StringBuilder m_ExitMessage;

		private WeakReference m_Credentials;

		private string m_Alias;

		private bool m_IsRootPath;

		private long m_ContentLength = -1L;

		private DateTime m_LastModified;

		private bool m_DataHandshakeStarted;

		private string m_LoginDirectory;

		private string m_PreviousServerPath;

		private string m_NewServerPath;

		private Uri m_ResponseUri;

		private bool m_LastRequestWasUnknownMethod;

		private FtpLoginState m_LoginState;

		internal FtpStatusCode StatusCode;

		internal string StatusLine;

		private static readonly AsyncCallback m_AcceptCallbackDelegate = AcceptCallback;

		private static readonly AsyncCallback m_ConnectCallbackDelegate = ConnectCallback;

		private static readonly AsyncCallback m_SSLHandshakeCallback = SSLHandshakeCallback;

		internal NetworkCredential Credentials
		{
			get
			{
				if (m_Credentials != null && m_Credentials.IsAlive)
				{
					return (NetworkCredential)m_Credentials.Target;
				}
				return null;
			}
			set
			{
				if (m_Credentials == null)
				{
					m_Credentials = new WeakReference(null);
				}
				m_Credentials.Target = value;
			}
		}

		internal long ContentLength => m_ContentLength;

		internal DateTime LastModified => m_LastModified;

		internal Uri ResponseUri => m_ResponseUri;

		internal string BannerMessage
		{
			get
			{
				if (m_BannerMessage == null)
				{
					return null;
				}
				return m_BannerMessage.ToString();
			}
		}

		internal string WelcomeMessage
		{
			get
			{
				if (m_WelcomeMessage == null)
				{
					return null;
				}
				return m_WelcomeMessage.ToString();
			}
		}

		internal string ExitMessage
		{
			get
			{
				if (m_ExitMessage == null)
				{
					return null;
				}
				return m_ExitMessage.ToString();
			}
		}

		internal FtpControlStream(ConnectionPool connectionPool, TimeSpan lifetime, bool checkLifetime)
			: base(connectionPool, lifetime, checkLifetime)
		{
		}

		internal void AbortConnect()
		{
			Socket dataSocket = m_DataSocket;
			if (dataSocket != null)
			{
				try
				{
					dataSocket.Close();
				}
				catch (ObjectDisposedException)
				{
				}
			}
		}

		private static void AcceptCallback(IAsyncResult asyncResult)
		{
			FtpControlStream ftpControlStream = (FtpControlStream)asyncResult.AsyncState;
			LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
			Socket socket = (Socket)lazyAsyncResult.AsyncObject;
			try
			{
				ftpControlStream.m_DataSocket = socket.EndAccept(asyncResult);
				if (!ftpControlStream.ServerAddress.Equals(((IPEndPoint)ftpControlStream.m_DataSocket.RemoteEndPoint).Address))
				{
					ftpControlStream.m_DataSocket.Close();
					throw new WebException(SR.GetString("net_ftp_active_address_different"), WebExceptionStatus.ProtocolError);
				}
				ftpControlStream.ContinueCommandPipeline();
			}
			catch (Exception obj)
			{
				ftpControlStream.CloseSocket();
				ftpControlStream.InvokeRequestCallback(obj);
			}
			finally
			{
				socket.Close();
			}
		}

		private static void ConnectCallback(IAsyncResult asyncResult)
		{
			FtpControlStream ftpControlStream = (FtpControlStream)asyncResult.AsyncState;
			try
			{
				LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
				Socket socket = (Socket)lazyAsyncResult.AsyncObject;
				socket.EndConnect(asyncResult);
				ftpControlStream.ContinueCommandPipeline();
			}
			catch (Exception obj)
			{
				ftpControlStream.CloseSocket();
				ftpControlStream.InvokeRequestCallback(obj);
			}
		}

		private static void SSLHandshakeCallback(IAsyncResult asyncResult)
		{
			FtpControlStream ftpControlStream = (FtpControlStream)asyncResult.AsyncState;
			try
			{
				ftpControlStream.ContinueCommandPipeline();
			}
			catch (Exception obj)
			{
				ftpControlStream.CloseSocket();
				ftpControlStream.InvokeRequestCallback(obj);
			}
		}

		private PipelineInstruction QueueOrCreateFtpDataStream(ref Stream stream)
		{
			if (m_DataSocket == null)
			{
				throw new InternalException();
			}
			if (m_TlsStream != null)
			{
				stream = new FtpDataStream(m_TlsStream, (FtpWebRequest)m_Request, IsFtpDataStreamWriteable());
				m_TlsStream = null;
				return PipelineInstruction.GiveStream;
			}
			NetworkStream networkStream = new NetworkStream(m_DataSocket, ownsSocket: true);
			if (base.UsingSecureStream)
			{
				FtpWebRequest ftpWebRequest = (FtpWebRequest)m_Request;
				TlsStream tlsStream = new TlsStream(ftpWebRequest.RequestUri.Host, networkStream, ftpWebRequest.ClientCertificates, base.Pool.ServicePoint, ftpWebRequest, m_Async ? ftpWebRequest.GetWritingContext().ContextCopy : null);
				networkStream = tlsStream;
				if (m_Async)
				{
					m_TlsStream = tlsStream;
					LazyAsyncResult result = new LazyAsyncResult(null, this, m_SSLHandshakeCallback);
					tlsStream.ProcessAuthentication(result);
					return PipelineInstruction.Pause;
				}
				tlsStream.ProcessAuthentication(null);
			}
			stream = new FtpDataStream(networkStream, (FtpWebRequest)m_Request, IsFtpDataStreamWriteable());
			return PipelineInstruction.GiveStream;
		}

		protected override void ClearState()
		{
			m_ContentLength = -1L;
			m_LastModified = DateTime.MinValue;
			m_ResponseUri = null;
			m_DataHandshakeStarted = false;
			StatusCode = FtpStatusCode.Undefined;
			StatusLine = null;
			m_DataSocket = null;
			m_PassiveEndPoint = null;
			m_TlsStream = null;
			base.ClearState();
		}

		protected override PipelineInstruction PipelineCallback(PipelineEntry entry, ResponseDescription response, bool timeout, ref Stream stream)
		{
			if (response == null)
			{
				return PipelineInstruction.Abort;
			}
			FtpStatusCode status = (FtpStatusCode)response.Status;
			if (status != FtpStatusCode.ClosingControl)
			{
				StatusCode = status;
				StatusLine = response.StatusDescription;
			}
			if (response.InvalidStatusCode)
			{
				throw new WebException(SR.GetString("net_InvalidStatusCode"), WebExceptionStatus.ProtocolError);
			}
			if (m_Index == -1)
			{
				switch (status)
				{
				case FtpStatusCode.SendUserCommand:
					m_BannerMessage = new StringBuilder();
					m_BannerMessage.Append(StatusLine);
					return PipelineInstruction.Advance;
				case FtpStatusCode.ServiceTemporarilyNotAvailable:
					return PipelineInstruction.Reread;
				default:
					throw GenerateException(status, response.StatusDescription, null);
				}
			}
			if (entry.Command == "OPTS utf8 on\r\n")
			{
				if (response.PositiveCompletion)
				{
					base.Encoding = Encoding.UTF8;
				}
				else
				{
					base.Encoding = Encoding.Default;
				}
				return PipelineInstruction.Advance;
			}
			if (entry.Command.IndexOf("USER") != -1)
			{
				switch (status)
				{
				case FtpStatusCode.LoggedInProceed:
					m_LoginState = FtpLoginState.LoggedIn;
					m_Index++;
					break;
				case FtpStatusCode.NotLoggedIn:
					if (m_LoginState != 0)
					{
						m_LoginState = FtpLoginState.ReloginFailed;
						throw ExceptionHelper.IsolatedException;
					}
					break;
				}
			}
			if (response.TransientFailure || response.PermanentFailure)
			{
				if (status == FtpStatusCode.ServiceNotAvailable)
				{
					MarkAsRecoverableFailure();
				}
				throw GenerateException(status, response.StatusDescription, null);
			}
			if (m_LoginState != FtpLoginState.LoggedIn && entry.Command.IndexOf("PASS") != -1)
			{
				if (status != FtpStatusCode.NeedLoginAccount && status != FtpStatusCode.LoggedInProceed)
				{
					throw GenerateException(status, response.StatusDescription, null);
				}
				m_LoginState = FtpLoginState.LoggedIn;
			}
			if (entry.HasFlag(PipelineEntryFlags.CreateDataConnection) && (response.PositiveCompletion || response.PositiveIntermediate))
			{
				bool isSocketReady;
				PipelineInstruction result = QueueOrCreateDataConection(entry, response, timeout, ref stream, out isSocketReady);
				if (!isSocketReady)
				{
					return result;
				}
			}
			switch (status)
			{
			case FtpStatusCode.DataAlreadyOpen:
			case FtpStatusCode.OpeningData:
				if (m_DataSocket == null)
				{
					return PipelineInstruction.Abort;
				}
				if (!entry.HasFlag(PipelineEntryFlags.GiveDataStream))
				{
					m_AbortReason = SR.GetString("net_ftp_invalid_status_response", status, entry.Command);
					return PipelineInstruction.Abort;
				}
				TryUpdateContentLength(response.StatusDescription);
				if (status == FtpStatusCode.OpeningData)
				{
					FtpWebRequest ftpWebRequest = (FtpWebRequest)m_Request;
					if (ftpWebRequest.MethodInfo.ShouldParseForResponseUri)
					{
						TryUpdateResponseUri(response.StatusDescription, ftpWebRequest);
					}
				}
				return QueueOrCreateFtpDataStream(ref stream);
			case FtpStatusCode.LoggedInProceed:
				if (StatusLine.ToLower(CultureInfo.InvariantCulture).IndexOf("alias") > 0)
				{
					int num = StatusLine.IndexOf("230-", 3);
					if (num > 0)
					{
						for (num += 4; num < StatusLine.Length && StatusLine[num] == ' '; num++)
						{
						}
						if (num < StatusLine.Length)
						{
							int num2 = StatusLine.IndexOf(' ', num);
							if (num2 < 0)
							{
								num2 = StatusLine.Length;
							}
							m_Alias = StatusLine.Substring(num, num2 - num);
							if (!m_IsRootPath)
							{
								for (num = 0; num < m_Commands.Length; num++)
								{
									if (m_Commands[num].Command.IndexOf("CWD") == 0)
									{
										string parameter2 = m_Alias + m_NewServerPath;
										m_Commands[num] = new PipelineEntry(FormatFtpCommand("CWD", parameter2));
										break;
									}
								}
							}
						}
					}
				}
				m_WelcomeMessage.Append(StatusLine);
				break;
			case FtpStatusCode.ClosingControl:
				m_ExitMessage.Append(response.StatusDescription);
				CloseSocket();
				break;
			case FtpStatusCode.ServerWantsSecureSession:
			{
				FtpWebRequest ftpWebRequest2 = (FtpWebRequest)m_Request;
				TlsStream tlsStream = (TlsStream)(base.NetworkStream = new TlsStream(ftpWebRequest2.RequestUri.Host, base.NetworkStream, ftpWebRequest2.ClientCertificates, base.Pool.ServicePoint, ftpWebRequest2, m_Async ? ftpWebRequest2.GetWritingContext().ContextCopy : null));
				break;
			}
			case FtpStatusCode.FileStatus:
				_ = (FtpWebRequest)m_Request;
				if (entry.Command.StartsWith("SIZE "))
				{
					m_ContentLength = GetContentLengthFrom213Response(response.StatusDescription);
				}
				else if (entry.Command.StartsWith("MDTM "))
				{
					m_LastModified = GetLastModifiedFrom213Response(response.StatusDescription);
				}
				break;
			case FtpStatusCode.PathnameCreated:
			{
				if (!(entry.Command == "PWD\r\n") || entry.HasFlag(PipelineEntryFlags.UserCommand))
				{
					break;
				}
				m_LoginDirectory = GetLoginDirectory(response.StatusDescription);
				if (m_IsRootPath || !(m_LoginDirectory != "\\") || !(m_LoginDirectory != "/") || m_Alias != null)
				{
					break;
				}
				for (int i = 0; i < m_Commands.Length; i++)
				{
					if (m_Commands[i].Command.IndexOf("CWD") == 0)
					{
						string parameter = m_LoginDirectory + m_NewServerPath;
						m_Commands[i] = new PipelineEntry(FormatFtpCommand("CWD", parameter));
						break;
					}
				}
				break;
			}
			default:
				if (entry.Command.IndexOf("CWD") != -1)
				{
					m_PreviousServerPath = m_NewServerPath;
				}
				break;
			}
			if (response.PositiveIntermediate || (!base.UsingSecureStream && entry.Command == "AUTH TLS\r\n"))
			{
				return PipelineInstruction.Reread;
			}
			return PipelineInstruction.Advance;
		}

		protected override PipelineEntry[] BuildCommandsList(WebRequest req)
		{
			FtpWebRequest ftpWebRequest = (FtpWebRequest)req;
			m_ResponseUri = ftpWebRequest.RequestUri;
			ArrayList arrayList = new ArrayList();
			if ((m_LastRequestWasUnknownMethod && !ftpWebRequest.MethodInfo.IsUnknownMethod) || Credentials == null || !Credentials.IsEqualTo(ftpWebRequest.Credentials.GetCredential(ftpWebRequest.RequestUri, "basic")))
			{
				m_PreviousServerPath = null;
				m_NewServerPath = null;
				m_LoginDirectory = null;
				if (m_LoginState == FtpLoginState.LoggedIn)
				{
					m_LoginState = FtpLoginState.LoggedInButNeedsRelogin;
				}
			}
			m_LastRequestWasUnknownMethod = ftpWebRequest.MethodInfo.IsUnknownMethod;
			if (ftpWebRequest.EnableSsl && !base.UsingSecureStream)
			{
				arrayList.Add(new PipelineEntry(FormatFtpCommand("AUTH", "TLS")));
				arrayList.Add(new PipelineEntry(FormatFtpCommand("PBSZ", "0")));
				arrayList.Add(new PipelineEntry(FormatFtpCommand("PROT", "P")));
				if (m_LoginState == FtpLoginState.LoggedIn)
				{
					m_LoginState = FtpLoginState.LoggedInButNeedsRelogin;
				}
			}
			if (m_LoginState != FtpLoginState.LoggedIn)
			{
				Credentials = ftpWebRequest.Credentials.GetCredential(ftpWebRequest.RequestUri, "basic");
				m_WelcomeMessage = new StringBuilder();
				m_ExitMessage = new StringBuilder();
				string text = string.Empty;
				string text2 = string.Empty;
				if (Credentials != null)
				{
					text = Credentials.InternalGetDomainUserName();
					text2 = Credentials.InternalGetPassword();
				}
				if (text.Length == 0 && text2.Length == 0)
				{
					text = "anonymous";
					text2 = "anonymous@";
				}
				arrayList.Add(new PipelineEntry(FormatFtpCommand("USER", text)));
				arrayList.Add(new PipelineEntry(FormatFtpCommand("PASS", text2), PipelineEntryFlags.DontLogParameter));
				arrayList.Add(new PipelineEntry(FormatFtpCommand("OPTS", "utf8 on")));
				arrayList.Add(new PipelineEntry(FormatFtpCommand("PWD", null)));
			}
			GetPathOption pathOption = GetPathOption.Normal;
			if (ftpWebRequest.MethodInfo.HasFlag(FtpMethodFlags.DoesNotTakeParameter))
			{
				pathOption = GetPathOption.AssumeNoFilename;
			}
			else if (ftpWebRequest.MethodInfo.HasFlag(FtpMethodFlags.ParameterIsDirectory))
			{
				pathOption = GetPathOption.AssumeFilename;
			}
			string path = null;
			string filename = null;
			GetPathAndFilename(pathOption, ftpWebRequest.RequestUri, ref path, ref filename, ref m_IsRootPath);
			if (filename.Length == 0 && ftpWebRequest.MethodInfo.HasFlag(FtpMethodFlags.TakesParameter))
			{
				throw new WebException(SR.GetString("net_ftp_invalid_uri"));
			}
			string text3 = path;
			if (m_PreviousServerPath != text3)
			{
				if (!m_IsRootPath && m_LoginState == FtpLoginState.LoggedIn && m_LoginDirectory != null)
				{
					text3 = m_LoginDirectory + text3;
				}
				m_NewServerPath = text3;
				arrayList.Add(new PipelineEntry(FormatFtpCommand("CWD", text3), PipelineEntryFlags.UserCommand));
			}
			if (ftpWebRequest.CacheProtocol != null && ftpWebRequest.CacheProtocol.ProtocolStatus == CacheValidationStatus.DoNotTakeFromCache && ftpWebRequest.MethodInfo.Operation == FtpOperation.DownloadFile)
			{
				arrayList.Add(new PipelineEntry(FormatFtpCommand("MDTM", filename)));
			}
			if (!ftpWebRequest.MethodInfo.IsCommandOnly)
			{
				if (ftpWebRequest.CacheProtocol == null || ftpWebRequest.CacheProtocol.ProtocolStatus != CacheValidationStatus.Continue)
				{
					if (ftpWebRequest.UseBinary)
					{
						arrayList.Add(new PipelineEntry(FormatFtpCommand("TYPE", "I")));
					}
					else
					{
						arrayList.Add(new PipelineEntry(FormatFtpCommand("TYPE", "A")));
					}
					if (ftpWebRequest.UsePassive)
					{
						string command = ((base.ServerAddress.AddressFamily == AddressFamily.InterNetwork) ? "PASV" : "EPSV");
						arrayList.Add(new PipelineEntry(FormatFtpCommand(command, null), PipelineEntryFlags.CreateDataConnection));
					}
					else
					{
						string command2 = ((base.ServerAddress.AddressFamily == AddressFamily.InterNetwork) ? "PORT" : "EPRT");
						CreateFtpListenerSocket(ftpWebRequest);
						arrayList.Add(new PipelineEntry(FormatFtpCommand(command2, GetPortCommandLine(ftpWebRequest))));
					}
					if (ftpWebRequest.CacheProtocol != null && ftpWebRequest.CacheProtocol.ProtocolStatus == CacheValidationStatus.CombineCachedAndServerResponse)
					{
						if (ftpWebRequest.CacheProtocol.Validator.CacheEntry.StreamSize > 0)
						{
							arrayList.Add(new PipelineEntry(FormatFtpCommand("REST", ftpWebRequest.CacheProtocol.Validator.CacheEntry.StreamSize.ToString(CultureInfo.InvariantCulture))));
						}
					}
					else if (ftpWebRequest.ContentOffset > 0)
					{
						arrayList.Add(new PipelineEntry(FormatFtpCommand("REST", ftpWebRequest.ContentOffset.ToString(CultureInfo.InvariantCulture))));
					}
				}
				else
				{
					arrayList.Add(new PipelineEntry(FormatFtpCommand("SIZE", filename)));
					arrayList.Add(new PipelineEntry(FormatFtpCommand("MDTM", filename)));
				}
			}
			if (ftpWebRequest.CacheProtocol == null || ftpWebRequest.CacheProtocol.ProtocolStatus != CacheValidationStatus.Continue)
			{
				PipelineEntryFlags pipelineEntryFlags = PipelineEntryFlags.UserCommand;
				if (!ftpWebRequest.MethodInfo.IsCommandOnly)
				{
					pipelineEntryFlags |= PipelineEntryFlags.GiveDataStream;
					if (!ftpWebRequest.UsePassive)
					{
						pipelineEntryFlags |= PipelineEntryFlags.CreateDataConnection;
					}
				}
				if (ftpWebRequest.MethodInfo.Operation == FtpOperation.Rename)
				{
					arrayList.Add(new PipelineEntry(FormatFtpCommand("RNFR", filename), pipelineEntryFlags));
					arrayList.Add(new PipelineEntry(FormatFtpCommand("RNTO", ftpWebRequest.RenameTo), pipelineEntryFlags));
				}
				else
				{
					arrayList.Add(new PipelineEntry(FormatFtpCommand(ftpWebRequest.Method, filename), pipelineEntryFlags));
				}
				if (!ftpWebRequest.KeepAlive)
				{
					arrayList.Add(new PipelineEntry(FormatFtpCommand("QUIT", null)));
				}
			}
			return (PipelineEntry[])arrayList.ToArray(typeof(PipelineEntry));
		}

		private PipelineInstruction QueueOrCreateDataConection(PipelineEntry entry, ResponseDescription response, bool timeout, ref Stream stream, out bool isSocketReady)
		{
			isSocketReady = false;
			if (m_DataHandshakeStarted)
			{
				isSocketReady = true;
				return PipelineInstruction.Pause;
			}
			m_DataHandshakeStarted = true;
			bool flag = false;
			int port = -1;
			if (entry.Command == "PASV\r\n" || entry.Command == "EPSV\r\n")
			{
				if (!response.PositiveCompletion)
				{
					m_AbortReason = SR.GetString("net_ftp_server_failed_passive", response.Status);
					return PipelineInstruction.Abort;
				}
				if (entry.Command == "PASV\r\n")
				{
					IPAddress ipAddress = null;
					port = GetAddressAndPort(response.StatusDescription, ref ipAddress);
					if (!base.ServerAddress.Equals(ipAddress))
					{
						throw new WebException(SR.GetString("net_ftp_passive_address_different"));
					}
				}
				else
				{
					port = GetPortV6(response.StatusDescription);
				}
				flag = true;
			}
			new SocketPermission(PermissionState.Unrestricted).Assert();
			try
			{
				if (flag)
				{
					try
					{
						m_DataSocket = CreateFtpDataSocket((FtpWebRequest)m_Request, base.Socket);
					}
					catch (ObjectDisposedException)
					{
						throw ExceptionHelper.RequestAbortedException;
					}
					IPEndPoint localEP = new IPEndPoint(((IPEndPoint)base.Socket.LocalEndPoint).Address, 0);
					m_DataSocket.Bind(localEP);
					m_PassiveEndPoint = new IPEndPoint(base.ServerAddress, port);
				}
				if (m_PassiveEndPoint != null)
				{
					IPEndPoint passiveEndPoint = m_PassiveEndPoint;
					m_PassiveEndPoint = null;
					if (m_Async)
					{
						m_DataSocket.BeginConnect(passiveEndPoint, m_ConnectCallbackDelegate, this);
						return PipelineInstruction.Pause;
					}
					m_DataSocket.Connect(passiveEndPoint);
					return PipelineInstruction.Advance;
				}
				if (m_Async)
				{
					m_DataSocket.BeginAccept(m_AcceptCallbackDelegate, this);
					return PipelineInstruction.Pause;
				}
				Socket dataSocket = m_DataSocket;
				try
				{
					m_DataSocket = m_DataSocket.Accept();
					if (!base.ServerAddress.Equals(((IPEndPoint)m_DataSocket.RemoteEndPoint).Address))
					{
						m_DataSocket.Close();
						throw new WebException(SR.GetString("net_ftp_active_address_different"), WebExceptionStatus.ProtocolError);
					}
					isSocketReady = true;
					return PipelineInstruction.Pause;
				}
				finally
				{
					dataSocket.Close();
				}
			}
			finally
			{
				CodeAccessPermission.RevertAssert();
			}
		}

		internal void Quit()
		{
			CloseSocket();
		}

		private static void GetPathAndFilename(GetPathOption pathOption, Uri uri, ref string path, ref string filename, ref bool isRoot)
		{
			string text = uri.GetParts(UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.Unescaped);
			isRoot = false;
			if (text.StartsWith("//"))
			{
				isRoot = true;
				text = text.Substring(1, text.Length - 1);
			}
			int num = text.LastIndexOf('/');
			switch (pathOption)
			{
			case GetPathOption.AssumeFilename:
				if (num != -1 && num == text.Length - 1)
				{
					text = text.Substring(0, text.Length - 1);
					num = text.LastIndexOf('/');
				}
				path = text.Substring(0, num + 1);
				filename = text.Substring(num + 1, text.Length - (num + 1));
				break;
			case GetPathOption.AssumeNoFilename:
				path = text;
				filename = "";
				break;
			default:
				path = text.Substring(0, num + 1);
				filename = text.Substring(num + 1, text.Length - (num + 1));
				break;
			}
			if (path.Length == 0)
			{
				path = "/";
			}
		}

		private string FormatAddress(IPAddress address, int Port)
		{
			byte[] addressBytes = address.GetAddressBytes();
			StringBuilder stringBuilder = new StringBuilder(32);
			byte[] array = addressBytes;
			foreach (byte value in array)
			{
				stringBuilder.Append(value);
				stringBuilder.Append(',');
			}
			stringBuilder.Append(Port / 256);
			stringBuilder.Append(',');
			stringBuilder.Append(Port % 256);
			return stringBuilder.ToString();
		}

		private string FormatAddressV6(IPAddress address, int port)
		{
			StringBuilder stringBuilder = new StringBuilder(43);
			string value = address.ToString();
			stringBuilder.Append("|2|");
			stringBuilder.Append(value);
			stringBuilder.Append('|');
			stringBuilder.Append(port.ToString(NumberFormatInfo.InvariantInfo));
			stringBuilder.Append('|');
			return stringBuilder.ToString();
		}

		private long GetContentLengthFrom213Response(string responseString)
		{
			string[] array = responseString.Split(' ');
			if (array.Length < 2)
			{
				throw new FormatException(SR.GetString("net_ftp_response_invalid_format", responseString));
			}
			return Convert.ToInt64(array[1], NumberFormatInfo.InvariantInfo);
		}

		private DateTime GetLastModifiedFrom213Response(string str)
		{
			DateTime result = m_LastModified;
			string[] array = str.Split(' ', '.');
			if (array.Length < 2)
			{
				return result;
			}
			string text = array[1];
			if (text.Length < 14)
			{
				return result;
			}
			int year = Convert.ToInt32(text.Substring(0, 4), NumberFormatInfo.InvariantInfo);
			int month = Convert.ToInt16(text.Substring(4, 2), NumberFormatInfo.InvariantInfo);
			int day = Convert.ToInt16(text.Substring(6, 2), NumberFormatInfo.InvariantInfo);
			int hour = Convert.ToInt16(text.Substring(8, 2), NumberFormatInfo.InvariantInfo);
			int minute = Convert.ToInt16(text.Substring(10, 2), NumberFormatInfo.InvariantInfo);
			int second = Convert.ToInt16(text.Substring(12, 2), NumberFormatInfo.InvariantInfo);
			int millisecond = 0;
			if (array.Length > 2)
			{
				millisecond = Convert.ToInt16(array[2], NumberFormatInfo.InvariantInfo);
			}
			try
			{
				result = new DateTime(year, month, day, hour, minute, second, millisecond);
				result = result.ToLocalTime();
				return result;
			}
			catch (ArgumentOutOfRangeException)
			{
				return result;
			}
			catch (ArgumentException)
			{
				return result;
			}
		}

		private void TryUpdateResponseUri(string str, FtpWebRequest request)
		{
			Uri uri = request.RequestUri;
			int num = str.IndexOf("for ");
			if (num == -1)
			{
				return;
			}
			num += 4;
			int num2 = str.LastIndexOf('(');
			if (num2 == -1)
			{
				num2 = str.Length;
			}
			if (num2 > num)
			{
				string text = str.Substring(num, num2 - num);
				text = text.TrimEnd(' ', '.', '\r', '\n');
				string text2 = text.Replace("%", "%25");
				text2 = text2.Replace("#", "%23");
				string absolutePath = uri.AbsolutePath;
				if (absolutePath.Length > 0 && absolutePath[absolutePath.Length - 1] != '/')
				{
					UriBuilder uriBuilder = new UriBuilder(uri);
					uriBuilder.Path = absolutePath + "/";
					uri = uriBuilder.Uri;
				}
				if (!Uri.TryCreate(uri, text2, out var result))
				{
					throw new FormatException(SR.GetString("net_ftp_invalid_response_filename", text));
				}
				if (!uri.IsBaseOf(result) || uri.Segments.Length != result.Segments.Length - 1)
				{
					throw new FormatException(SR.GetString("net_ftp_invalid_response_filename", text));
				}
				m_ResponseUri = result;
			}
		}

		private void TryUpdateContentLength(string str)
		{
			int num = str.LastIndexOf("(");
			if (num == -1)
			{
				return;
			}
			int num2 = str.IndexOf(" bytes).");
			if (num2 != -1 && num2 > num)
			{
				num++;
				if (long.TryParse(str.Substring(num, num2 - num), NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo, out var result))
				{
					m_ContentLength = result;
				}
			}
		}

		private string GetLoginDirectory(string str)
		{
			int num = str.IndexOf('"');
			int num2 = str.LastIndexOf('"');
			if (num != -1 && num2 != -1 && num != num2)
			{
				return str.Substring(num + 1, num2 - num - 1);
			}
			return string.Empty;
		}

		private int GetAddressAndPort(string responseString, ref IPAddress ipAddress)
		{
			int num = 0;
			string[] array = responseString.Split('(', ',', ')');
			if (6 >= array.Length)
			{
				throw new FormatException(SR.GetString("net_ftp_response_invalid_format", responseString));
			}
			num = Convert.ToInt32(array[5], NumberFormatInfo.InvariantInfo) * 256;
			num += Convert.ToInt32(array[6], NumberFormatInfo.InvariantInfo);
			long num2 = 0L;
			try
			{
				for (int num3 = 4; num3 > 0; num3--)
				{
					num2 = (num2 << 8) + Convert.ToByte(array[num3], NumberFormatInfo.InvariantInfo);
				}
			}
			catch
			{
				throw new FormatException(SR.GetString("net_ftp_response_invalid_format", responseString));
			}
			ipAddress = new IPAddress(num2);
			return num;
		}

		private int GetPortV6(string responseString)
		{
			int num = responseString.LastIndexOf("(");
			int num2 = responseString.LastIndexOf(")");
			if (num == -1 || num2 <= num)
			{
				throw new FormatException(SR.GetString("net_ftp_response_invalid_format", responseString));
			}
			string text = responseString.Substring(num + 1, num2 - num - 1);
			string[] array = text.Split('|');
			if (array.Length < 4)
			{
				throw new FormatException(SR.GetString("net_ftp_response_invalid_format", responseString));
			}
			return Convert.ToInt32(array[3], NumberFormatInfo.InvariantInfo);
		}

		private void CreateFtpListenerSocket(FtpWebRequest request)
		{
			IPEndPoint localEP = new IPEndPoint(((IPEndPoint)base.Socket.LocalEndPoint).Address, 0);
			try
			{
				m_DataSocket = CreateFtpDataSocket(request, base.Socket);
			}
			catch (ObjectDisposedException)
			{
				throw ExceptionHelper.RequestAbortedException;
			}
			new SocketPermission(PermissionState.Unrestricted).Assert();
			try
			{
				m_DataSocket.Bind(localEP);
				m_DataSocket.Listen(1);
			}
			finally
			{
				CodeAccessPermission.RevertAssert();
			}
		}

		private string GetPortCommandLine(FtpWebRequest request)
		{
			try
			{
				IPEndPoint iPEndPoint = (IPEndPoint)m_DataSocket.LocalEndPoint;
				if (base.ServerAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					return FormatAddress(iPEndPoint.Address, iPEndPoint.Port);
				}
				if (base.ServerAddress.AddressFamily == AddressFamily.InterNetworkV6)
				{
					return FormatAddressV6(iPEndPoint.Address, iPEndPoint.Port);
				}
				throw new InternalException();
			}
			catch (Exception innerException)
			{
				throw GenerateException(WebExceptionStatus.ProtocolError, innerException);
			}
			catch
			{
				throw GenerateException(WebExceptionStatus.ProtocolError, new Exception(SR.GetString("net_nonClsCompliantException")));
			}
		}

		private string FormatFtpCommand(string command, string parameter)
		{
			StringBuilder stringBuilder = new StringBuilder(command.Length + (parameter?.Length ?? 0) + 3);
			stringBuilder.Append(command);
			if (!ValidationHelper.IsBlankString(parameter))
			{
				stringBuilder.Append(' ');
				stringBuilder.Append(parameter);
			}
			stringBuilder.Append("\r\n");
			return stringBuilder.ToString();
		}

		protected Socket CreateFtpDataSocket(FtpWebRequest request, Socket templateSocket)
		{
			return new Socket(templateSocket.AddressFamily, templateSocket.SocketType, templateSocket.ProtocolType);
		}

		protected override bool CheckValid(ResponseDescription response, ref int validThrough, ref int completeLength)
		{
			if (response.StatusBuffer.Length < 4)
			{
				return true;
			}
			string text = response.StatusBuffer.ToString();
			if (response.Status == -1)
			{
				if (!char.IsDigit(text[0]) || !char.IsDigit(text[1]) || !char.IsDigit(text[2]) || (text[3] != ' ' && text[3] != '-'))
				{
					return false;
				}
				response.StatusCodeString = text.Substring(0, 3);
				response.Status = Convert.ToInt16(response.StatusCodeString, NumberFormatInfo.InvariantInfo);
				if (text[3] == '-')
				{
					response.Multiline = true;
				}
			}
			int num = 0;
			while ((num = text.IndexOf("\r\n", validThrough)) != -1)
			{
				int num2 = validThrough;
				validThrough = num + 2;
				if (!response.Multiline)
				{
					completeLength = validThrough;
					return true;
				}
				if (text.Length > num2 + 4 && text.Substring(num2, 3) == response.StatusCodeString && text[num2 + 3] == ' ')
				{
					completeLength = validThrough;
					return true;
				}
			}
			return true;
		}

		private TriState IsFtpDataStreamWriteable()
		{
			FtpWebRequest ftpWebRequest = m_Request as FtpWebRequest;
			if (ftpWebRequest != null)
			{
				if (ftpWebRequest.MethodInfo.IsUpload)
				{
					return TriState.True;
				}
				if (ftpWebRequest.MethodInfo.IsDownload)
				{
					return TriState.False;
				}
			}
			return TriState.Unspecified;
		}
	}
}
