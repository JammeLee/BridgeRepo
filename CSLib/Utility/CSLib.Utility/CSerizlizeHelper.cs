using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Xml.Serialization;

namespace CSLib.Utility
{
	public class CSerizlizeHelper : CSingleton<CSerizlizeHelper>, ISerializable
	{
		private SerializationInfo ᜀ;

		private FormatterConverter ᜁ;

		private BinaryFormatter ᜂ;

		public CSerizlizeHelper()
		{
			ᜁ = new FormatterConverter();
			ᜂ = new BinaryFormatter();
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info = ᜀ;
		}

		public bool Serizlize<Type>(string filename, Type serializObject)
		{
			//Discarded unreachable code: IL_008a
			try
			{
				FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
				try
				{
					ᜀ = new SerializationInfo(typeof(Type), ᜁ);
					ᜂ.Serialize(fileStream, serializObject);
				}
				finally
				{
					int num = 2;
					while (true)
					{
						switch (num)
						{
						default:
							if (fileStream != null)
							{
								num = 1;
								continue;
							}
							break;
						case 1:
							((IDisposable)fileStream).Dispose();
							num = 0;
							continue;
						case 0:
							break;
						}
						break;
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			}
			if (true)
			{
			}
			return false;
		}

		public Type Deserizlize<Type>(string filename)
		{
			//Discarded unreachable code: IL_0077
			Type result = default(Type);
			try
			{
				FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				try
				{
					result = (Type)ᜂ.Deserialize(fileStream);
				}
				finally
				{
					int num = 1;
					while (true)
					{
						switch (num)
						{
						default:
							if (fileStream != null)
							{
								num = 2;
								continue;
							}
							break;
						case 2:
							((IDisposable)fileStream).Dispose();
							num = 0;
							continue;
						case 0:
							break;
						}
						break;
					}
				}
			}
			catch (Exception ex)
			{
				CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			}
			if (true)
			{
			}
			return result;
		}

		public bool Serizlize(string filename, byte[] serializObject)
		{
			//Discarded unreachable code: IL_006a
			bool result;
			try
			{
				FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
				try
				{
					fileStream.Write(serializObject, 0, serializObject.Length);
				}
				finally
				{
					int num = 0;
					while (true)
					{
						switch (num)
						{
						default:
							if (fileStream != null)
							{
								num = 2;
								continue;
							}
							break;
						case 2:
							((IDisposable)fileStream).Dispose();
							num = 1;
							continue;
						case 1:
							break;
						}
						break;
					}
				}
				result = true;
			}
			catch (Exception ex)
			{
				CSingleton<CLogInfoList>.Instance.WriteLine(ex);
				goto IL_0065;
			}
			if (true)
			{
			}
			return result;
			IL_0065:
			return false;
		}

		public bool Serizlize(string filename, byte[] serializObject, int startIndex, int length)
		{
			//Discarded unreachable code: IL_0067
			try
			{
				FileStream fileStream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
				try
				{
					fileStream.Write(serializObject, startIndex, length);
				}
				finally
				{
					int num = 0;
					while (true)
					{
						switch (num)
						{
						default:
							if (fileStream != null)
							{
								num = 1;
								continue;
							}
							break;
						case 1:
							((IDisposable)fileStream).Dispose();
							num = 2;
							continue;
						case 2:
							break;
						}
						break;
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			}
			if (true)
			{
			}
			return false;
		}

		public bool Deserizlize(string filename, ref byte[] buffer)
		{
			//Discarded unreachable code: IL_008d
			try
			{
				switch (0)
				{
				default:
				{
					FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					try
					{
						long length = fileStream.Length;
						buffer = new byte[length];
						fileStream.Read(buffer, 0, buffer.Length);
					}
					finally
					{
						int num = 0;
						while (true)
						{
							switch (num)
							{
							default:
								if (fileStream != null)
								{
									num = 2;
									continue;
								}
								break;
							case 2:
								((IDisposable)fileStream).Dispose();
								num = 1;
								continue;
							case 1:
								break;
							}
							break;
						}
					}
					return true;
				}
				}
			}
			catch (Exception ex)
			{
				CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			}
			if (true)
			{
			}
			return false;
		}

		public bool Deserizlize(string filename, ref byte[] buffer, long startIndex)
		{
			//Discarded unreachable code: IL_014c
			try
			{
				switch (0)
				{
				default:
				{
					FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					try
					{
						long num3 = default(long);
						long num2 = default(long);
						while (true)
						{
							long length = fileStream.Length;
							int num = 5;
							while (true)
							{
								switch (num)
								{
								case 5:
									if (startIndex < length)
									{
										num = 2;
										continue;
									}
									goto case 7;
								case 2:
									num3 = CConstant.FILE_MAX_PART;
									num2 = length - startIndex;
									num = 1;
									continue;
								case 1:
									if (num2 > 0)
									{
										num = 3;
										continue;
									}
									goto case 0;
								case 0:
									buffer = new byte[num3];
									fileStream.Seek(startIndex, SeekOrigin.Begin);
									fileStream.Read(buffer, 0, buffer.Length);
									num = 7;
									continue;
								case 4:
									num3 = num2;
									num = 0;
									continue;
								case 3:
									num = 8;
									continue;
								case 8:
									if (num2 < num3)
									{
										num = 4;
										continue;
									}
									goto case 0;
								case 7:
									num = 6;
									continue;
								case 6:
									goto end_IL_001a;
								}
								break;
							}
						}
						end_IL_001a:;
					}
					finally
					{
						int num = 0;
						while (true)
						{
							switch (num)
							{
							default:
								if (fileStream != null)
								{
									num = 1;
									continue;
								}
								break;
							case 1:
								((IDisposable)fileStream).Dispose();
								num = 2;
								continue;
							case 2:
								break;
							}
							break;
						}
					}
					return true;
				}
				}
			}
			catch (Exception ex)
			{
				CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			}
			if (true)
			{
			}
			return false;
		}

		public MemoryStream Serizlize<Type>(Type serializObject)
		{
			//Discarded unreachable code: IL_000b
			MemoryStream memoryStream = new MemoryStream();
			try
			{
				ᜀ = new SerializationInfo(typeof(Type), ᜁ);
				ᜂ.Serialize(memoryStream, serializObject);
				return memoryStream;
			}
			catch (Exception ex)
			{
				memoryStream.Close();
				CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			}
			if (true)
			{
			}
			return memoryStream;
		}

		public Type Deserizlize<Type>(MemoryStream ms)
		{
			//Discarded unreachable code: IL_003d
			Type result = default(Type);
			try
			{
				ms.Position = 0L;
				result = (Type)ᜂ.Deserialize(ms);
			}
			catch (Exception ex)
			{
				ms.Close();
				CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			}
			if (true)
			{
			}
			return result;
		}

		public bool SerizlizeXml<Type>(string filename, Type serializObject)
		{
			//Discarded unreachable code: IL_0011
			try
			{
				switch (0)
				{
				default:
				{
					if (true)
					{
					}
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(Type));
					FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
					try
					{
						xmlSerializer.Serialize(fileStream, serializObject);
					}
					finally
					{
						int num = 0;
						while (true)
						{
							switch (num)
							{
							default:
								if (fileStream != null)
								{
									num = 2;
									continue;
								}
								break;
							case 2:
								((IDisposable)fileStream).Dispose();
								num = 1;
								continue;
							case 1:
								break;
							}
							break;
						}
					}
					return true;
				}
				}
			}
			catch (Exception ex)
			{
				CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			}
			return false;
		}

		public Type DeserizlizeXml<Type>(string filename)
		{
			//Discarded unreachable code: IL_0011
			switch (0)
			{
			default:
			{
				if (true)
				{
				}
				Type result = default(Type);
				try
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(Type));
					FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					try
					{
						result = (Type)xmlSerializer.Deserialize(fileStream);
						return result;
					}
					finally
					{
						int num = 2;
						while (true)
						{
							switch (num)
							{
							default:
								if (fileStream != null)
								{
									num = 0;
									continue;
								}
								break;
							case 0:
								((IDisposable)fileStream).Dispose();
								num = 1;
								continue;
							case 1:
								break;
							}
							break;
						}
					}
				}
				catch (Exception ex)
				{
					CSingleton<CLogInfoList>.Instance.WriteLine(ex);
					return result;
				}
			}
			}
		}
	}
}
