using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace CSLib.Utility
{
	public class CShellHelper : CSingleton<CShellHelper>
	{
		private int m_ᜀ = 3600000;

		private Timer ᜁ;

		private Process ᜂ;

		public CShellHelper()
		{
			TimerCallback timerDelegate = ᜀ;
			ᜁ = CSingleton<CSimpleThreadPool>.Instance.NewThreadingTimer(timerDelegate, ᜂ, -1, -1, -1);
		}

		private void ᜀ(object A_0)
		{
			if (ᜂ != null)
			{
				ᜂ.Kill();
			}
		}

		public void ExecuteExe(string command, string arguments, string workingDir)
		{
			//Discarded unreachable code: IL_0043
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = command;
			processStartInfo.Arguments = arguments;
			processStartInfo.UseShellExecute = false;
			processStartInfo.RedirectStandardInput = false;
			processStartInfo.RedirectStandardOutput = false;
			processStartInfo.RedirectStandardError = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WorkingDirectory = workingDir;
			try
			{
				if (true)
				{
				}
				ᜂ = Process.Start(processStartInfo);
			}
			catch (Exception ex)
			{
				CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			}
		}

		public int ExecuteExe(string command, string args, out string consoleLog, out TimeSpan buildtime, bool unicodeOutputEncoding)
		{
			//Discarded unreachable code: IL_0190
			switch (0)
			{
			}
			FileInfo fileInfo = default(FileInfo);
			while (true)
			{
				int num = 0;
				buildtime = TimeSpan.Zero;
				ProcessStartInfo processStartInfo = new ProcessStartInfo();
				processStartInfo.FileName = command;
				processStartInfo.Arguments = args;
				processStartInfo.UseShellExecute = false;
				processStartInfo.RedirectStandardInput = true;
				processStartInfo.RedirectStandardOutput = true;
				processStartInfo.RedirectStandardError = true;
				processStartInfo.CreateNoWindow = true;
				int num2 = 4;
				while (true)
				{
					switch (num2)
					{
					case 4:
						if (unicodeOutputEncoding)
						{
							num2 = 5;
							continue;
						}
						goto case 3;
					case 6:
						try
						{
							ᜂ = Process.Start(processStartInfo);
							ᜁ.Change(this.m_ᜀ, 0);
							StreamReader standardOutput = ᜂ.StandardOutput;
							consoleLog = standardOutput.ReadToEnd();
							standardOutput.Close();
							ᜁ.Change(-1, -1);
							buildtime = ᜂ.ExitTime - ᜂ.StartTime;
							return ᜂ.ExitCode;
						}
						catch (Exception ex)
						{
							CSingleton<CLogInfoList>.Instance.WriteLine(ex);
							num = -1;
							consoleLog = ex.Message;
							return num;
						}
					case 5:
						processStartInfo.StandardOutputEncoding = Encoding.UTF8;
						num2 = 3;
						continue;
					case 3:
						fileInfo = new FileInfo(command);
						num2 = 0;
						continue;
					case 0:
						if (fileInfo.Exists)
						{
							num2 = 1;
							continue;
						}
						goto case 2;
					case 1:
						if (true)
						{
						}
						processStartInfo.WorkingDirectory = fileInfo.Directory.FullName;
						num2 = 2;
						continue;
					case 2:
						num2 = 6;
						continue;
					}
					break;
				}
			}
		}

		public int ExecuteCMD(string command, string args, out string consoleLog, out TimeSpan buildtime, bool unicodeOutputEncoding)
		{
			//Discarded unreachable code: IL_0185
			int a_ = 15;
			switch (0)
			{
			}
			while (true)
			{
				int num = 0;
				buildtime = TimeSpan.Zero;
				ProcessStartInfo processStartInfo = new ProcessStartInfo();
				processStartInfo.FileName = CSimpleThreadPool.b("⡊⁌⭎罐㙒ⵔ㉖", a_);
				processStartInfo.UseShellExecute = false;
				processStartInfo.RedirectStandardInput = true;
				processStartInfo.RedirectStandardOutput = true;
				processStartInfo.RedirectStandardError = true;
				processStartInfo.CreateNoWindow = true;
				int num2 = 0;
				while (true)
				{
					switch (num2)
					{
					case 0:
						if (unicodeOutputEncoding)
						{
							num2 = 3;
							continue;
						}
						goto case 1;
					case 2:
						try
						{
							ᜂ = Process.Start(processStartInfo);
							ᜂ.StandardInput.WriteLine(command + CSimpleThreadPool.b("歊", a_) + args);
							ᜂ.StandardInput.WriteLine(CSimpleThreadPool.b("\u2e4a㕌♎═", a_));
							ᜁ.Change(this.m_ᜀ, 0);
							StreamReader standardOutput = ᜂ.StandardOutput;
							consoleLog = standardOutput.ReadToEnd();
							ᜁ.Change(-1, -1);
							buildtime = ᜂ.ExitTime - ᜂ.StartTime;
							return ᜂ.ExitCode;
						}
						catch (Exception ex)
						{
							CSingleton<CLogInfoList>.Instance.WriteLine(ex);
							num = -1;
							consoleLog = ex.Message;
							return num;
						}
					case 1:
						num2 = 2;
						continue;
					case 3:
						processStartInfo.StandardOutputEncoding = Encoding.UTF8;
						if (true)
						{
						}
						num2 = 1;
						continue;
					}
					break;
				}
			}
		}
	}
}
