using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System
{
	public static class Console
	{
		[Flags]
		internal enum ControlKeyState
		{
			RightAltPressed = 0x1,
			LeftAltPressed = 0x2,
			RightCtrlPressed = 0x4,
			LeftCtrlPressed = 0x8,
			ShiftPressed = 0x10,
			NumLockOn = 0x20,
			ScrollLockOn = 0x40,
			CapsLockOn = 0x80,
			EnhancedKey = 0x100
		}

		internal sealed class ControlCHooker : CriticalFinalizerObject
		{
			private bool _hooked;

			private Win32Native.ConsoleCtrlHandlerRoutine _handler;

			internal ControlCHooker()
			{
				_handler = BreakEvent;
			}

			~ControlCHooker()
			{
				Unhook();
			}

			internal void Hook()
			{
				if (!_hooked)
				{
					if (!Win32Native.SetConsoleCtrlHandler(_handler, addOrRemove: true))
					{
						__Error.WinIOError();
					}
					_hooked = true;
				}
			}

			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			internal void Unhook()
			{
				if (_hooked)
				{
					if (!Win32Native.SetConsoleCtrlHandler(_handler, addOrRemove: false))
					{
						__Error.WinIOError();
					}
					_hooked = false;
				}
			}
		}

		private sealed class ControlCDelegateData
		{
			internal ConsoleSpecialKey ControlKey;

			internal bool Cancel;

			internal bool DelegateStarted;

			internal ManualResetEvent CompletionEvent;

			internal ConsoleCancelEventHandler CancelCallbacks;

			internal ControlCDelegateData(ConsoleSpecialKey controlKey, ConsoleCancelEventHandler cancelCallbacks)
			{
				ControlKey = controlKey;
				CancelCallbacks = cancelCallbacks;
				CompletionEvent = new ManualResetEvent(initialState: false);
			}
		}

		private const int _DefaultConsoleBufferSize = 256;

		private const int NumberLockVKCode = 144;

		private const int CapsLockVKCode = 20;

		private const int MinBeepFrequency = 37;

		private const int MaxBeepFrequency = 32767;

		private const int MaxConsoleTitleLength = 24500;

		private static TextReader _in;

		private static TextWriter _out;

		private static TextWriter _error;

		private static ConsoleCancelEventHandler _cancelCallbacks;

		private static ControlCHooker _hooker;

		private static Win32Native.InputRecord _cachedInputRecord;

		private static bool _haveReadDefaultColors;

		private static byte _defaultColors;

		private static bool _wasOutRedirected;

		private static bool _wasErrorRedirected;

		private static object s_InternalSyncObject;

		private static IntPtr _consoleInputHandle;

		private static IntPtr _consoleOutputHandle;

		private static object InternalSyncObject
		{
			get
			{
				if (s_InternalSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref s_InternalSyncObject, value, null);
				}
				return s_InternalSyncObject;
			}
		}

		private static IntPtr ConsoleInputHandle
		{
			get
			{
				if (_consoleInputHandle == IntPtr.Zero)
				{
					_consoleInputHandle = Win32Native.GetStdHandle(-10);
				}
				return _consoleInputHandle;
			}
		}

		private static IntPtr ConsoleOutputHandle
		{
			get
			{
				if (_consoleOutputHandle == IntPtr.Zero)
				{
					_consoleOutputHandle = Win32Native.GetStdHandle(-11);
				}
				return _consoleOutputHandle;
			}
		}

		public static TextWriter Error
		{
			[HostProtection(SecurityAction.LinkDemand, UI = true)]
			get
			{
				if (_error == null)
				{
					InitializeStdOutError(stdout: false);
				}
				return _error;
			}
		}

		public static TextReader In
		{
			[HostProtection(SecurityAction.LinkDemand, UI = true)]
			get
			{
				if (_in == null)
				{
					lock (InternalSyncObject)
					{
						if (_in == null)
						{
							Stream stream = OpenStandardInput(256);
							TextReader @in;
							if (stream == Stream.Null)
							{
								@in = StreamReader.Null;
							}
							else
							{
								Encoding encoding = Encoding.GetEncoding((int)Win32Native.GetConsoleCP());
								@in = TextReader.Synchronized(new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: false, 256, closable: false));
							}
							Thread.MemoryBarrier();
							_in = @in;
						}
					}
				}
				return _in;
			}
		}

		public static TextWriter Out
		{
			[HostProtection(SecurityAction.LinkDemand, UI = true)]
			get
			{
				if (_out == null)
				{
					InitializeStdOutError(stdout: true);
				}
				return _out;
			}
		}

		public static Encoding InputEncoding
		{
			get
			{
				uint consoleCP = Win32Native.GetConsoleCP();
				return Encoding.GetEncoding((int)consoleCP);
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				if (Environment.IsWin9X())
				{
					throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_Win9x"));
				}
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				uint codePage = (uint)value.CodePage;
				lock (InternalSyncObject)
				{
					if (!Win32Native.SetConsoleCP(codePage))
					{
						__Error.WinIOError();
					}
					_in = null;
				}
			}
		}

		public static Encoding OutputEncoding
		{
			get
			{
				uint consoleOutputCP = Win32Native.GetConsoleOutputCP();
				return Encoding.GetEncoding((int)consoleOutputCP);
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				if (Environment.IsWin9X())
				{
					throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_Win9x"));
				}
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				lock (InternalSyncObject)
				{
					if (_out != null && !_wasOutRedirected)
					{
						_out.Flush();
						_out = null;
					}
					if (_error != null && !_wasErrorRedirected)
					{
						_error.Flush();
						_error = null;
					}
					uint codePage = (uint)value.CodePage;
					if (!Win32Native.SetConsoleOutputCP(codePage))
					{
						__Error.WinIOError();
					}
				}
			}
		}

		public static ConsoleColor BackgroundColor
		{
			get
			{
				bool succeeded;
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(throwOnNoConsole: false, out succeeded);
				if (!succeeded)
				{
					return ConsoleColor.Black;
				}
				Win32Native.Color c = (Win32Native.Color)(bufferInfo.wAttributes & 0xF0);
				return ColorAttributeToConsoleColor(c);
			}
			set
			{
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				Win32Native.Color color = ConsoleColorToColorAttribute(value, isBackground: true);
				bool succeeded;
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(throwOnNoConsole: false, out succeeded);
				if (succeeded)
				{
					short wAttributes = bufferInfo.wAttributes;
					wAttributes = (short)(wAttributes & -241);
					wAttributes = (short)((ushort)wAttributes | (ushort)color);
					Win32Native.SetConsoleTextAttribute(ConsoleOutputHandle, wAttributes);
				}
			}
		}

		public static ConsoleColor ForegroundColor
		{
			get
			{
				bool succeeded;
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(throwOnNoConsole: false, out succeeded);
				if (!succeeded)
				{
					return ConsoleColor.Gray;
				}
				Win32Native.Color c = (Win32Native.Color)(bufferInfo.wAttributes & 0xF);
				return ColorAttributeToConsoleColor(c);
			}
			set
			{
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				Win32Native.Color color = ConsoleColorToColorAttribute(value, isBackground: false);
				bool succeeded;
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(throwOnNoConsole: false, out succeeded);
				if (succeeded)
				{
					short wAttributes = bufferInfo.wAttributes;
					wAttributes = (short)(wAttributes & -16);
					wAttributes = (short)((ushort)wAttributes | (ushort)color);
					Win32Native.SetConsoleTextAttribute(ConsoleOutputHandle, wAttributes);
				}
			}
		}

		public static int BufferHeight
		{
			get
			{
				return GetBufferInfo().dwSize.Y;
			}
			set
			{
				SetBufferSize(BufferWidth, value);
			}
		}

		public static int BufferWidth
		{
			get
			{
				return GetBufferInfo().dwSize.X;
			}
			set
			{
				SetBufferSize(value, BufferHeight);
			}
		}

		public static int WindowHeight
		{
			get
			{
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
				return bufferInfo.srWindow.Bottom - bufferInfo.srWindow.Top + 1;
			}
			set
			{
				SetWindowSize(WindowWidth, value);
			}
		}

		public static int WindowWidth
		{
			get
			{
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
				return bufferInfo.srWindow.Right - bufferInfo.srWindow.Left + 1;
			}
			set
			{
				SetWindowSize(value, WindowHeight);
			}
		}

		public static int LargestWindowWidth => Win32Native.GetLargestConsoleWindowSize(ConsoleOutputHandle).X;

		public static int LargestWindowHeight => Win32Native.GetLargestConsoleWindowSize(ConsoleOutputHandle).Y;

		public static int WindowLeft
		{
			get
			{
				return GetBufferInfo().srWindow.Left;
			}
			set
			{
				SetWindowPosition(value, WindowTop);
			}
		}

		public static int WindowTop
		{
			get
			{
				return GetBufferInfo().srWindow.Top;
			}
			set
			{
				SetWindowPosition(WindowLeft, value);
			}
		}

		public static int CursorLeft
		{
			get
			{
				return GetBufferInfo().dwCursorPosition.X;
			}
			set
			{
				SetCursorPosition(value, CursorTop);
			}
		}

		public static int CursorTop
		{
			get
			{
				return GetBufferInfo().dwCursorPosition.Y;
			}
			set
			{
				SetCursorPosition(CursorLeft, value);
			}
		}

		public static int CursorSize
		{
			get
			{
				IntPtr consoleOutputHandle = ConsoleOutputHandle;
				if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out var cci))
				{
					__Error.WinIOError();
				}
				return cci.dwSize;
			}
			set
			{
				if (value < 1 || value > 100)
				{
					throw new ArgumentOutOfRangeException("value", value, Environment.GetResourceString("ArgumentOutOfRange_CursorSize"));
				}
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				if (value == 100 && (Environment.OSInfo & Environment.OSName.Win9x) != 0)
				{
					value = 99;
				}
				IntPtr consoleOutputHandle = ConsoleOutputHandle;
				if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out var cci))
				{
					__Error.WinIOError();
				}
				cci.dwSize = value;
				if (!Win32Native.SetConsoleCursorInfo(consoleOutputHandle, ref cci))
				{
					__Error.WinIOError();
				}
			}
		}

		public static bool CursorVisible
		{
			get
			{
				IntPtr consoleOutputHandle = ConsoleOutputHandle;
				if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out var cci))
				{
					__Error.WinIOError();
				}
				return cci.bVisible;
			}
			set
			{
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				IntPtr consoleOutputHandle = ConsoleOutputHandle;
				if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out var cci))
				{
					__Error.WinIOError();
				}
				cci.bVisible = value;
				if (!Win32Native.SetConsoleCursorInfo(consoleOutputHandle, ref cci))
				{
					__Error.WinIOError();
				}
			}
		}

		public static string Title
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder(24501);
				Win32Native.SetLastError(0);
				int consoleTitle = Win32Native.GetConsoleTitle(stringBuilder, stringBuilder.Capacity);
				if (consoleTitle == 0)
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					if (lastWin32Error == 0)
					{
						stringBuilder.Length = 0;
					}
					else
					{
						__Error.WinIOError(lastWin32Error, string.Empty);
					}
				}
				else if (consoleTitle > 24500)
				{
					throw new InvalidOperationException(Environment.GetResourceString("ArgumentOutOfRange_ConsoleTitleTooLong"));
				}
				return stringBuilder.ToString();
			}
			set
			{
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				if (value.Length > 24500)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_ConsoleTitleTooLong"));
				}
				if (!Win32Native.SetConsoleTitle(value))
				{
					__Error.WinIOError();
				}
			}
		}

		public static bool KeyAvailable
		{
			[HostProtection(SecurityAction.LinkDemand, UI = true)]
			get
			{
				if (_cachedInputRecord.eventType == 1)
				{
					return true;
				}
				Win32Native.InputRecord buffer = default(Win32Native.InputRecord);
				int numEventsRead = 0;
				while (true)
				{
					if (!Win32Native.PeekConsoleInput(ConsoleInputHandle, out buffer, 1, out numEventsRead))
					{
						int lastWin32Error = Marshal.GetLastWin32Error();
						if (lastWin32Error == 6)
						{
							throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ConsoleKeyAvailableOnFile"));
						}
						__Error.WinIOError(lastWin32Error, "stdin");
					}
					if (numEventsRead == 0)
					{
						return false;
					}
					short virtualKeyCode = buffer.keyEvent.virtualKeyCode;
					if (IsKeyDownEvent(buffer) && !IsModKey(virtualKeyCode))
					{
						break;
					}
					if (!Win32Native.ReadConsoleInput(ConsoleInputHandle, out buffer, 1, out numEventsRead))
					{
						__Error.WinIOError();
					}
				}
				return true;
			}
		}

		public static bool NumberLock
		{
			get
			{
				short keyState = Win32Native.GetKeyState(144);
				return (keyState & 1) == 1;
			}
		}

		public static bool CapsLock
		{
			get
			{
				short keyState = Win32Native.GetKeyState(20);
				return (keyState & 1) == 1;
			}
		}

		public static bool TreatControlCAsInput
		{
			get
			{
				IntPtr consoleInputHandle = ConsoleInputHandle;
				if (consoleInputHandle == Win32Native.INVALID_HANDLE_VALUE)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
				}
				int mode = 0;
				if (!Win32Native.GetConsoleMode(consoleInputHandle, out mode))
				{
					__Error.WinIOError();
				}
				return (mode & 1) == 0;
			}
			set
			{
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				IntPtr consoleInputHandle = ConsoleInputHandle;
				if (consoleInputHandle == Win32Native.INVALID_HANDLE_VALUE)
				{
					throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
				}
				int mode = 0;
				bool consoleMode = Win32Native.GetConsoleMode(consoleInputHandle, out mode);
				mode = ((!value) ? (mode | 1) : (mode & -2));
				if (!Win32Native.SetConsoleMode(consoleInputHandle, mode))
				{
					__Error.WinIOError();
				}
			}
		}

		public static event ConsoleCancelEventHandler CancelKeyPress
		{
			add
			{
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				lock (InternalSyncObject)
				{
					_cancelCallbacks = (ConsoleCancelEventHandler)Delegate.Combine(_cancelCallbacks, value);
					if (_hooker == null)
					{
						_hooker = new ControlCHooker();
						_hooker.Hook();
					}
				}
			}
			remove
			{
				new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
				lock (InternalSyncObject)
				{
					_cancelCallbacks = (ConsoleCancelEventHandler)Delegate.Remove(_cancelCallbacks, value);
					if (_hooker != null && _cancelCallbacks == null)
					{
						_hooker.Unhook();
					}
				}
			}
		}

		private static void InitializeStdOutError(bool stdout)
		{
			lock (InternalSyncObject)
			{
				if ((!stdout || _out == null) && (stdout || _error == null))
				{
					TextWriter textWriter = null;
					Stream stream = ((!stdout) ? OpenStandardError(256) : OpenStandardOutput(256));
					if (stream == Stream.Null)
					{
						textWriter = TextWriter.Synchronized(StreamWriter.Null);
					}
					else
					{
						int consoleOutputCP = (int)Win32Native.GetConsoleOutputCP();
						Encoding encoding = Encoding.GetEncoding(consoleOutputCP);
						StreamWriter streamWriter = new StreamWriter(stream, encoding, 256, closeable: false);
						streamWriter.HaveWrittenPreamble = true;
						streamWriter.AutoFlush = true;
						textWriter = TextWriter.Synchronized(streamWriter);
					}
					if (stdout)
					{
						_out = textWriter;
					}
					else
					{
						_error = textWriter;
					}
				}
			}
		}

		private static Stream GetStandardFile(int stdHandleName, FileAccess access, int bufferSize)
		{
			IntPtr stdHandle = Win32Native.GetStdHandle(stdHandleName);
			SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, ownsHandle: false);
			if (safeFileHandle.IsInvalid)
			{
				safeFileHandle.SetHandleAsInvalid();
				return Stream.Null;
			}
			if (stdHandleName != -10 && !ConsoleHandleIsValid(safeFileHandle))
			{
				return Stream.Null;
			}
			return new __ConsoleStream(safeFileHandle, access);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Beep()
		{
			Beep(800, 200);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Beep(int frequency, int duration)
		{
			if (frequency < 37 || frequency > 32767)
			{
				throw new ArgumentOutOfRangeException("frequency", frequency, Environment.GetResourceString("ArgumentOutOfRange_BeepFrequency", 37, 32767));
			}
			if (duration <= 0)
			{
				throw new ArgumentOutOfRangeException("duration", duration, Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			Win32Native.Beep(frequency, duration);
		}

		public static void Clear()
		{
			Win32Native.COORD cOORD = default(Win32Native.COORD);
			IntPtr consoleOutputHandle = ConsoleOutputHandle;
			if (consoleOutputHandle == Win32Native.INVALID_HANDLE_VALUE)
			{
				throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
			}
			Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
			int num = bufferInfo.dwSize.X * bufferInfo.dwSize.Y;
			int pNumCharsWritten = 0;
			if (!Win32Native.FillConsoleOutputCharacter(consoleOutputHandle, ' ', num, cOORD, out pNumCharsWritten))
			{
				__Error.WinIOError();
			}
			pNumCharsWritten = 0;
			if (!Win32Native.FillConsoleOutputAttribute(consoleOutputHandle, bufferInfo.wAttributes, num, cOORD, out pNumCharsWritten))
			{
				__Error.WinIOError();
			}
			if (!Win32Native.SetConsoleCursorPosition(consoleOutputHandle, cOORD))
			{
				__Error.WinIOError();
			}
		}

		private static Win32Native.Color ConsoleColorToColorAttribute(ConsoleColor color, bool isBackground)
		{
			if (((uint)color & 0xFFFFFFF0u) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"));
			}
			Win32Native.Color color2 = (Win32Native.Color)color;
			if (isBackground)
			{
				color2 = (Win32Native.Color)((int)color2 << 4);
			}
			return color2;
		}

		private static ConsoleColor ColorAttributeToConsoleColor(Win32Native.Color c)
		{
			if ((c & Win32Native.Color.BackgroundMask) != 0)
			{
				c = (Win32Native.Color)((int)c >> 4);
			}
			return (ConsoleColor)c;
		}

		public static void ResetColor()
		{
			new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
			GetBufferInfo(throwOnNoConsole: false, out var succeeded);
			if (succeeded)
			{
				short attributes = _defaultColors;
				Win32Native.SetConsoleTextAttribute(ConsoleOutputHandle, attributes);
			}
		}

		public static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
		{
			MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop, ' ', ConsoleColor.Black, BackgroundColor);
		}

		public unsafe static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
		{
			if (sourceForeColor < ConsoleColor.Black || sourceForeColor > ConsoleColor.White)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"), "sourceForeColor");
			}
			if (sourceBackColor < ConsoleColor.Black || sourceBackColor > ConsoleColor.White)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"), "sourceBackColor");
			}
			Win32Native.COORD dwSize = GetBufferInfo().dwSize;
			if (sourceLeft < 0 || sourceLeft > dwSize.X)
			{
				throw new ArgumentOutOfRangeException("sourceLeft", sourceLeft, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (sourceTop < 0 || sourceTop > dwSize.Y)
			{
				throw new ArgumentOutOfRangeException("sourceTop", sourceTop, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (sourceWidth < 0 || sourceWidth > dwSize.X - sourceLeft)
			{
				throw new ArgumentOutOfRangeException("sourceWidth", sourceWidth, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (sourceHeight < 0 || sourceTop > dwSize.Y - sourceHeight)
			{
				throw new ArgumentOutOfRangeException("sourceHeight", sourceHeight, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (targetLeft < 0 || targetLeft > dwSize.X)
			{
				throw new ArgumentOutOfRangeException("targetLeft", targetLeft, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (targetTop < 0 || targetTop > dwSize.Y)
			{
				throw new ArgumentOutOfRangeException("targetTop", targetTop, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (sourceWidth == 0 || sourceHeight == 0)
			{
				return;
			}
			new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
			Win32Native.CHAR_INFO[] array = new Win32Native.CHAR_INFO[sourceWidth * sourceHeight];
			dwSize.X = (short)sourceWidth;
			dwSize.Y = (short)sourceHeight;
			Win32Native.COORD bufferCoord = default(Win32Native.COORD);
			Win32Native.SMALL_RECT readRegion = default(Win32Native.SMALL_RECT);
			readRegion.Left = (short)sourceLeft;
			readRegion.Right = (short)(sourceLeft + sourceWidth - 1);
			readRegion.Top = (short)sourceTop;
			readRegion.Bottom = (short)(sourceTop + sourceHeight - 1);
			bool flag;
			fixed (Win32Native.CHAR_INFO* pBuffer = array)
			{
				flag = Win32Native.ReadConsoleOutput(ConsoleOutputHandle, pBuffer, dwSize, bufferCoord, ref readRegion);
			}
			if (!flag)
			{
				__Error.WinIOError();
			}
			Win32Native.COORD cOORD = default(Win32Native.COORD);
			cOORD.X = (short)sourceLeft;
			Win32Native.Color color = ConsoleColorToColorAttribute(sourceBackColor, isBackground: true);
			color |= ConsoleColorToColorAttribute(sourceForeColor, isBackground: false);
			short wColorAttribute = (short)color;
			for (int i = sourceTop; i < sourceTop + sourceHeight; i++)
			{
				cOORD.Y = (short)i;
				if (!Win32Native.FillConsoleOutputCharacter(ConsoleOutputHandle, sourceChar, sourceWidth, cOORD, out var pNumCharsWritten))
				{
					__Error.WinIOError();
				}
				if (!Win32Native.FillConsoleOutputAttribute(ConsoleOutputHandle, wColorAttribute, sourceWidth, cOORD, out pNumCharsWritten))
				{
					__Error.WinIOError();
				}
			}
			Win32Native.SMALL_RECT writeRegion = default(Win32Native.SMALL_RECT);
			writeRegion.Left = (short)targetLeft;
			writeRegion.Right = (short)(targetLeft + sourceWidth);
			writeRegion.Top = (short)targetTop;
			writeRegion.Bottom = (short)(targetTop + sourceHeight);
			fixed (Win32Native.CHAR_INFO* buffer = array)
			{
				flag = Win32Native.WriteConsoleOutput(ConsoleOutputHandle, buffer, dwSize, bufferCoord, ref writeRegion);
			}
		}

		private static Win32Native.CONSOLE_SCREEN_BUFFER_INFO GetBufferInfo()
		{
			bool succeeded;
			return GetBufferInfo(throwOnNoConsole: true, out succeeded);
		}

		private static Win32Native.CONSOLE_SCREEN_BUFFER_INFO GetBufferInfo(bool throwOnNoConsole, out bool succeeded)
		{
			succeeded = false;
			IntPtr consoleOutputHandle = ConsoleOutputHandle;
			if (consoleOutputHandle == Win32Native.INVALID_HANDLE_VALUE)
			{
				if (!throwOnNoConsole)
				{
					return default(Win32Native.CONSOLE_SCREEN_BUFFER_INFO);
				}
				throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
			}
			if (!Win32Native.GetConsoleScreenBufferInfo(consoleOutputHandle, out var lpConsoleScreenBufferInfo))
			{
				bool consoleScreenBufferInfo = Win32Native.GetConsoleScreenBufferInfo(Win32Native.GetStdHandle(-12), out lpConsoleScreenBufferInfo);
				if (!consoleScreenBufferInfo)
				{
					consoleScreenBufferInfo = Win32Native.GetConsoleScreenBufferInfo(Win32Native.GetStdHandle(-10), out lpConsoleScreenBufferInfo);
				}
				if (!consoleScreenBufferInfo)
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					if (lastWin32Error == 6 && !throwOnNoConsole)
					{
						return default(Win32Native.CONSOLE_SCREEN_BUFFER_INFO);
					}
					__Error.WinIOError(lastWin32Error, null);
				}
			}
			if (!_haveReadDefaultColors)
			{
				_defaultColors = (byte)((uint)lpConsoleScreenBufferInfo.wAttributes & 0xFFu);
				_haveReadDefaultColors = true;
			}
			succeeded = true;
			return lpConsoleScreenBufferInfo;
		}

		public static void SetBufferSize(int width, int height)
		{
			new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
			Win32Native.SMALL_RECT srWindow = GetBufferInfo().srWindow;
			if (width < srWindow.Right + 1 || width >= 32767)
			{
				throw new ArgumentOutOfRangeException("width", width, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferLessThanWindowSize"));
			}
			if (height < srWindow.Bottom + 1 || height >= 32767)
			{
				throw new ArgumentOutOfRangeException("height", height, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferLessThanWindowSize"));
			}
			Win32Native.COORD size = default(Win32Native.COORD);
			size.X = (short)width;
			size.Y = (short)height;
			if (!Win32Native.SetConsoleScreenBufferSize(ConsoleOutputHandle, size))
			{
				__Error.WinIOError();
			}
		}

		public unsafe static void SetWindowSize(int width, int height)
		{
			new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
			Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
			if (width <= 0)
			{
				throw new ArgumentOutOfRangeException("width", width, Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			if (height <= 0)
			{
				throw new ArgumentOutOfRangeException("height", height, Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			bool flag = false;
			Win32Native.COORD size = default(Win32Native.COORD);
			size.X = bufferInfo.dwSize.X;
			size.Y = bufferInfo.dwSize.Y;
			if (bufferInfo.dwSize.X < bufferInfo.srWindow.Left + width)
			{
				if (bufferInfo.srWindow.Left >= 32767 - width)
				{
					throw new ArgumentOutOfRangeException("width", Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowBufferSize"));
				}
				size.X = (short)(bufferInfo.srWindow.Left + width);
				flag = true;
			}
			if (bufferInfo.dwSize.Y < bufferInfo.srWindow.Top + height)
			{
				if (bufferInfo.srWindow.Top >= 32767 - height)
				{
					throw new ArgumentOutOfRangeException("height", Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowBufferSize"));
				}
				size.Y = (short)(bufferInfo.srWindow.Top + height);
				flag = true;
			}
			if (flag && !Win32Native.SetConsoleScreenBufferSize(ConsoleOutputHandle, size))
			{
				__Error.WinIOError();
			}
			Win32Native.SMALL_RECT srWindow = bufferInfo.srWindow;
			srWindow.Bottom = (short)(srWindow.Top + height - 1);
			srWindow.Right = (short)(srWindow.Left + width - 1);
			if (!Win32Native.SetConsoleWindowInfo(ConsoleOutputHandle, absolute: true, &srWindow))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (flag)
				{
					Win32Native.SetConsoleScreenBufferSize(ConsoleOutputHandle, bufferInfo.dwSize);
				}
				Win32Native.COORD largestConsoleWindowSize = Win32Native.GetLargestConsoleWindowSize(ConsoleOutputHandle);
				if (width > largestConsoleWindowSize.X)
				{
					throw new ArgumentOutOfRangeException("width", width, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowSize_Size", largestConsoleWindowSize.X));
				}
				if (height > largestConsoleWindowSize.Y)
				{
					throw new ArgumentOutOfRangeException("height", height, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowSize_Size", largestConsoleWindowSize.Y));
				}
				__Error.WinIOError(lastWin32Error, string.Empty);
			}
		}

		public unsafe static void SetWindowPosition(int left, int top)
		{
			new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
			Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
			Win32Native.SMALL_RECT srWindow = bufferInfo.srWindow;
			int num = left + srWindow.Right - srWindow.Left + 1;
			if (left < 0 || num > bufferInfo.dwSize.X || num < 0)
			{
				throw new ArgumentOutOfRangeException("left", left, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowPos"));
			}
			int num2 = top + srWindow.Bottom - srWindow.Top + 1;
			if (top < 0 || num2 > bufferInfo.dwSize.Y || num2 < 0)
			{
				throw new ArgumentOutOfRangeException("top", top, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowPos"));
			}
			srWindow.Bottom -= (short)(srWindow.Top - top);
			srWindow.Right -= (short)(srWindow.Left - left);
			srWindow.Left = (short)left;
			srWindow.Top = (short)top;
			if (!Win32Native.SetConsoleWindowInfo(ConsoleOutputHandle, absolute: true, &srWindow))
			{
				__Error.WinIOError();
			}
		}

		public static void SetCursorPosition(int left, int top)
		{
			if (left < 0 || left >= 32767)
			{
				throw new ArgumentOutOfRangeException("left", left, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			if (top < 0 || top >= 32767)
			{
				throw new ArgumentOutOfRangeException("top", top, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
			}
			new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
			IntPtr consoleOutputHandle = ConsoleOutputHandle;
			Win32Native.COORD cursorPosition = default(Win32Native.COORD);
			cursorPosition.X = (short)left;
			cursorPosition.Y = (short)top;
			if (!Win32Native.SetConsoleCursorPosition(consoleOutputHandle, cursorPosition))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
				if (left < 0 || left >= bufferInfo.dwSize.X)
				{
					throw new ArgumentOutOfRangeException("left", left, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
				}
				if (top < 0 || top >= bufferInfo.dwSize.Y)
				{
					throw new ArgumentOutOfRangeException("top", top, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
				}
				__Error.WinIOError(lastWin32Error, string.Empty);
			}
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static ConsoleKeyInfo ReadKey()
		{
			return ReadKey(intercept: false);
		}

		private static bool IsKeyDownEvent(Win32Native.InputRecord ir)
		{
			if (ir.eventType == 1)
			{
				return ir.keyEvent.keyDown;
			}
			return false;
		}

		private static bool IsModKey(short keyCode)
		{
			if ((keyCode < 16 || keyCode > 18) && keyCode != 20 && keyCode != 144)
			{
				return keyCode == 145;
			}
			return true;
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static ConsoleKeyInfo ReadKey(bool intercept)
		{
			int numEventsRead = -1;
			Win32Native.InputRecord buffer;
			if (_cachedInputRecord.eventType == 1)
			{
				buffer = _cachedInputRecord;
				if (_cachedInputRecord.keyEvent.repeatCount == 0)
				{
					_cachedInputRecord.eventType = -1;
				}
				else
				{
					_cachedInputRecord.keyEvent.repeatCount--;
				}
			}
			else
			{
				while (true)
				{
					if (!Win32Native.ReadConsoleInput(ConsoleInputHandle, out buffer, 1, out numEventsRead) || numEventsRead == 0)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ConsoleReadKeyOnFile"));
					}
					if (IsKeyDownEvent(buffer))
					{
						if (buffer.keyEvent.uChar != 0)
						{
							break;
						}
						short virtualKeyCode = buffer.keyEvent.virtualKeyCode;
						if (!IsModKey(virtualKeyCode))
						{
							break;
						}
					}
				}
				if (buffer.keyEvent.repeatCount > 1)
				{
					buffer.keyEvent.repeatCount--;
					_cachedInputRecord = buffer;
				}
			}
			ControlKeyState controlKeyState = (ControlKeyState)buffer.keyEvent.controlKeyState;
			bool shift = (controlKeyState & ControlKeyState.ShiftPressed) != 0;
			bool alt = (controlKeyState & (ControlKeyState.RightAltPressed | ControlKeyState.LeftAltPressed)) != 0;
			bool control = (controlKeyState & (ControlKeyState.RightCtrlPressed | ControlKeyState.LeftCtrlPressed)) != 0;
			ConsoleKeyInfo result = new ConsoleKeyInfo(buffer.keyEvent.uChar, (ConsoleKey)buffer.keyEvent.virtualKeyCode, shift, alt, control);
			if (!intercept)
			{
				Write(buffer.keyEvent.uChar);
			}
			return result;
		}

		private static bool BreakEvent(int controlType)
		{
			if (controlType == 0 || controlType == 1)
			{
				ConsoleCancelEventHandler cancelCallbacks = _cancelCallbacks;
				if (cancelCallbacks == null)
				{
					return false;
				}
				ConsoleSpecialKey controlKey = ((controlType != 0) ? ConsoleSpecialKey.ControlBreak : ConsoleSpecialKey.ControlC);
				ControlCDelegateData controlCDelegateData = new ControlCDelegateData(controlKey, cancelCallbacks);
				WaitCallback callBack = ControlCDelegate;
				if (!ThreadPool.QueueUserWorkItem(callBack, controlCDelegateData))
				{
					return false;
				}
				TimeSpan timeout = new TimeSpan(0, 0, 30);
				controlCDelegateData.CompletionEvent.WaitOne(timeout, exitContext: false);
				if (!controlCDelegateData.DelegateStarted)
				{
					return false;
				}
				controlCDelegateData.CompletionEvent.WaitOne();
				controlCDelegateData.CompletionEvent.Close();
				return controlCDelegateData.Cancel;
			}
			return false;
		}

		private static void ControlCDelegate(object data)
		{
			ControlCDelegateData controlCDelegateData = (ControlCDelegateData)data;
			try
			{
				controlCDelegateData.DelegateStarted = true;
				ConsoleCancelEventArgs consoleCancelEventArgs = new ConsoleCancelEventArgs(controlCDelegateData.ControlKey);
				controlCDelegateData.CancelCallbacks(null, consoleCancelEventArgs);
				controlCDelegateData.Cancel = consoleCancelEventArgs.Cancel;
			}
			finally
			{
				controlCDelegateData.CompletionEvent.Set();
			}
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static Stream OpenStandardError()
		{
			return OpenStandardError(256);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static Stream OpenStandardError(int bufferSize)
		{
			if (bufferSize < 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			return GetStandardFile(-12, FileAccess.Write, bufferSize);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static Stream OpenStandardInput()
		{
			return OpenStandardInput(256);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static Stream OpenStandardInput(int bufferSize)
		{
			if (bufferSize < 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			return GetStandardFile(-10, FileAccess.Read, bufferSize);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static Stream OpenStandardOutput()
		{
			return OpenStandardOutput(256);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static Stream OpenStandardOutput(int bufferSize)
		{
			if (bufferSize < 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			return GetStandardFile(-11, FileAccess.Write, bufferSize);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void SetIn(TextReader newIn)
		{
			if (newIn == null)
			{
				throw new ArgumentNullException("newIn");
			}
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			newIn = TextReader.Synchronized(newIn);
			lock (InternalSyncObject)
			{
				_in = newIn;
			}
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void SetOut(TextWriter newOut)
		{
			if (newOut == null)
			{
				throw new ArgumentNullException("newOut");
			}
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			_wasOutRedirected = true;
			newOut = TextWriter.Synchronized(newOut);
			lock (InternalSyncObject)
			{
				_out = newOut;
			}
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void SetError(TextWriter newError)
		{
			if (newError == null)
			{
				throw new ArgumentNullException("newError");
			}
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			_wasErrorRedirected = true;
			newError = TextWriter.Synchronized(newError);
			lock (InternalSyncObject)
			{
				_error = newError;
			}
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static int Read()
		{
			return In.Read();
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static string ReadLine()
		{
			return In.ReadLine();
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine()
		{
			Out.WriteLine();
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(bool value)
		{
			Out.WriteLine(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(char value)
		{
			Out.WriteLine(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(char[] buffer)
		{
			Out.WriteLine(buffer);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(char[] buffer, int index, int count)
		{
			Out.WriteLine(buffer, index, count);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(decimal value)
		{
			Out.WriteLine(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(double value)
		{
			Out.WriteLine(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(float value)
		{
			Out.WriteLine(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(int value)
		{
			Out.WriteLine(value);
		}

		[CLSCompliant(false)]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(uint value)
		{
			Out.WriteLine(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(long value)
		{
			Out.WriteLine(value);
		}

		[CLSCompliant(false)]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(ulong value)
		{
			Out.WriteLine(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(object value)
		{
			Out.WriteLine(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(string value)
		{
			Out.WriteLine(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(string format, object arg0)
		{
			Out.WriteLine(format, arg0);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(string format, object arg0, object arg1)
		{
			Out.WriteLine(format, arg0, arg1);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(string format, object arg0, object arg1, object arg2)
		{
			Out.WriteLine(format, arg0, arg1, arg2);
		}

		[CLSCompliant(false)]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(string format, object arg0, object arg1, object arg2, object arg3, __arglist)
		{
			ArgIterator argIterator = new ArgIterator(__arglist);
			int num = argIterator.GetRemainingCount() + 4;
			object[] array = new object[num];
			array[0] = arg0;
			array[1] = arg1;
			array[2] = arg2;
			array[3] = arg3;
			for (int i = 4; i < num; i++)
			{
				array[i] = TypedReference.ToObject(argIterator.GetNextArg());
			}
			Out.WriteLine(format, array);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void WriteLine(string format, params object[] arg)
		{
			Out.WriteLine(format, arg);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(string format, object arg0)
		{
			Out.Write(format, arg0);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(string format, object arg0, object arg1)
		{
			Out.Write(format, arg0, arg1);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(string format, object arg0, object arg1, object arg2)
		{
			Out.Write(format, arg0, arg1, arg2);
		}

		[CLSCompliant(false)]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(string format, object arg0, object arg1, object arg2, object arg3, __arglist)
		{
			ArgIterator argIterator = new ArgIterator(__arglist);
			int num = argIterator.GetRemainingCount() + 4;
			object[] array = new object[num];
			array[0] = arg0;
			array[1] = arg1;
			array[2] = arg2;
			array[3] = arg3;
			for (int i = 4; i < num; i++)
			{
				array[i] = TypedReference.ToObject(argIterator.GetNextArg());
			}
			Out.Write(format, array);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(string format, params object[] arg)
		{
			Out.Write(format, arg);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(bool value)
		{
			Out.Write(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(char value)
		{
			Out.Write(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(char[] buffer)
		{
			Out.Write(buffer);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(char[] buffer, int index, int count)
		{
			Out.Write(buffer, index, count);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(double value)
		{
			Out.Write(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(decimal value)
		{
			Out.Write(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(float value)
		{
			Out.Write(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(int value)
		{
			Out.Write(value);
		}

		[CLSCompliant(false)]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(uint value)
		{
			Out.Write(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(long value)
		{
			Out.Write(value);
		}

		[CLSCompliant(false)]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(ulong value)
		{
			Out.Write(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(object value)
		{
			Out.Write(value);
		}

		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		public static void Write(string value)
		{
			Out.Write(value);
		}

		private unsafe static bool ConsoleHandleIsValid(SafeFileHandle handle)
		{
			if (handle.IsInvalid)
			{
				return false;
			}
			byte b = 65;
			int numBytesWritten;
			int num = __ConsoleStream.WriteFile(handle, &b, 0, out numBytesWritten, IntPtr.Zero);
			return num != 0;
		}
	}
}
