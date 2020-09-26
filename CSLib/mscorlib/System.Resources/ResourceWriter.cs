using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace System.Resources
{
	[ComVisible(true)]
	public sealed class ResourceWriter : IResourceWriter, IDisposable
	{
		private class PrecannedResource
		{
			internal string TypeName;

			internal byte[] Data;

			internal PrecannedResource(string typeName, byte[] data)
			{
				TypeName = typeName;
				Data = data;
			}
		}

		private const int _ExpectedNumberOfResources = 1000;

		private const int AverageNameSize = 40;

		private const int AverageValueSize = 40;

		private Hashtable _resourceList;

		private Stream _output;

		private Hashtable _caseInsensitiveDups;

		private Hashtable _preserializedData;

		public ResourceWriter(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			_output = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
			_resourceList = new Hashtable(1000, FastResourceComparer.Default);
			_caseInsensitiveDups = new Hashtable(StringComparer.OrdinalIgnoreCase);
		}

		public ResourceWriter(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanWrite)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_StreamNotWritable"));
			}
			_output = stream;
			_resourceList = new Hashtable(1000, FastResourceComparer.Default);
			_caseInsensitiveDups = new Hashtable(StringComparer.OrdinalIgnoreCase);
		}

		public void AddResource(string name, string value)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (_resourceList == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceWriterSaved"));
			}
			_caseInsensitiveDups.Add(name, null);
			_resourceList.Add(name, value);
		}

		public void AddResource(string name, object value)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (_resourceList == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceWriterSaved"));
			}
			_caseInsensitiveDups.Add(name, null);
			_resourceList.Add(name, value);
		}

		public void AddResource(string name, byte[] value)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (_resourceList == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceWriterSaved"));
			}
			_caseInsensitiveDups.Add(name, null);
			_resourceList.Add(name, value);
		}

		public void AddResourceData(string name, string typeName, byte[] serializedData)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (typeName == null)
			{
				throw new ArgumentNullException("typeName");
			}
			if (serializedData == null)
			{
				throw new ArgumentNullException("serializedData");
			}
			if (_resourceList == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceWriterSaved"));
			}
			_caseInsensitiveDups.Add(name, null);
			if (_preserializedData == null)
			{
				_preserializedData = new Hashtable(FastResourceComparer.Default);
			}
			_preserializedData.Add(name, new PrecannedResource(typeName, serializedData));
		}

		public void Close()
		{
			Dispose(disposing: true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_resourceList != null)
				{
					Generate();
				}
				if (_output != null)
				{
					_output.Close();
				}
			}
			_output = null;
			_caseInsensitiveDups = null;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}

		public void Generate()
		{
			if (_resourceList == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceWriterSaved"));
			}
			BinaryWriter binaryWriter = new BinaryWriter(_output, Encoding.UTF8);
			List<string> list = new List<string>();
			binaryWriter.Write(ResourceManager.MagicNumber);
			binaryWriter.Write(ResourceManager.HeaderVersionNumber);
			MemoryStream memoryStream = new MemoryStream(240);
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream);
			binaryWriter2.Write(typeof(ResourceReader).AssemblyQualifiedName);
			binaryWriter2.Write(ResourceManager.ResSetTypeName);
			binaryWriter2.Flush();
			binaryWriter.Write((int)memoryStream.Length);
			binaryWriter.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
			binaryWriter.Write(2);
			int num = _resourceList.Count;
			if (_preserializedData != null)
			{
				num += _preserializedData.Count;
			}
			binaryWriter.Write(num);
			int[] array = new int[num];
			int[] array2 = new int[num];
			int num2 = 0;
			MemoryStream memoryStream2 = new MemoryStream(num * 40);
			BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream2, Encoding.Unicode);
			MemoryStream memoryStream3 = new MemoryStream(num * 40);
			BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream3, Encoding.UTF8);
			IFormatter objFormatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File | StreamingContextStates.Persistence));
			SortedList sortedList = new SortedList(_resourceList, FastResourceComparer.Default);
			if (_preserializedData != null)
			{
				foreach (DictionaryEntry preserializedDatum in _preserializedData)
				{
					sortedList.Add(preserializedDatum.Key, preserializedDatum.Value);
				}
			}
			IDictionaryEnumerator enumerator = sortedList.GetEnumerator();
			while (enumerator.MoveNext())
			{
				array[num2] = FastResourceComparer.HashFunction((string)enumerator.Key);
				array2[num2++] = (int)binaryWriter3.Seek(0, SeekOrigin.Current);
				binaryWriter3.Write((string)enumerator.Key);
				binaryWriter3.Write((int)binaryWriter4.Seek(0, SeekOrigin.Current));
				object value = enumerator.Value;
				ResourceTypeCode resourceTypeCode = FindTypeCode(value, list);
				Write7BitEncodedInt(binaryWriter4, (int)resourceTypeCode);
				PrecannedResource precannedResource = value as PrecannedResource;
				if (precannedResource != null)
				{
					binaryWriter4.Write(precannedResource.Data);
				}
				else
				{
					WriteValue(resourceTypeCode, value, binaryWriter4, objFormatter);
				}
			}
			binaryWriter.Write(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				binaryWriter.Write(list[i]);
			}
			Array.Sort(array, array2);
			binaryWriter.Flush();
			int num3 = (int)binaryWriter.BaseStream.Position & 7;
			if (num3 > 0)
			{
				for (int j = 0; j < 8 - num3; j++)
				{
					binaryWriter.Write("PAD"[j % 3]);
				}
			}
			int[] array3 = array;
			foreach (int value2 in array3)
			{
				binaryWriter.Write(value2);
			}
			int[] array4 = array2;
			foreach (int value3 in array4)
			{
				binaryWriter.Write(value3);
			}
			binaryWriter.Flush();
			binaryWriter3.Flush();
			binaryWriter4.Flush();
			int num4 = (int)(binaryWriter.Seek(0, SeekOrigin.Current) + memoryStream2.Length);
			num4 += 4;
			binaryWriter.Write(num4);
			binaryWriter.Write(memoryStream2.GetBuffer(), 0, (int)memoryStream2.Length);
			binaryWriter3.Close();
			binaryWriter.Write(memoryStream3.GetBuffer(), 0, (int)memoryStream3.Length);
			binaryWriter4.Close();
			binaryWriter.Flush();
			_resourceList = null;
		}

		private ResourceTypeCode FindTypeCode(object value, List<string> types)
		{
			if (value == null)
			{
				return ResourceTypeCode.Null;
			}
			Type type = value.GetType();
			if (type == typeof(string))
			{
				return ResourceTypeCode.String;
			}
			if (type == typeof(int))
			{
				return ResourceTypeCode.Int32;
			}
			if (type == typeof(bool))
			{
				return ResourceTypeCode.Boolean;
			}
			if (type == typeof(char))
			{
				return ResourceTypeCode.Char;
			}
			if (type == typeof(byte))
			{
				return ResourceTypeCode.Byte;
			}
			if (type == typeof(sbyte))
			{
				return ResourceTypeCode.SByte;
			}
			if (type == typeof(short))
			{
				return ResourceTypeCode.Int16;
			}
			if (type == typeof(long))
			{
				return ResourceTypeCode.Int64;
			}
			if (type == typeof(ushort))
			{
				return ResourceTypeCode.UInt16;
			}
			if (type == typeof(uint))
			{
				return ResourceTypeCode.UInt32;
			}
			if (type == typeof(ulong))
			{
				return ResourceTypeCode.UInt64;
			}
			if (type == typeof(float))
			{
				return ResourceTypeCode.Single;
			}
			if (type == typeof(double))
			{
				return ResourceTypeCode.Double;
			}
			if (type == typeof(decimal))
			{
				return ResourceTypeCode.Decimal;
			}
			if (type == typeof(DateTime))
			{
				return ResourceTypeCode.DateTime;
			}
			if (type == typeof(TimeSpan))
			{
				return ResourceTypeCode.TimeSpan;
			}
			if (type == typeof(byte[]))
			{
				return ResourceTypeCode.ByteArray;
			}
			if (type == typeof(MemoryStream))
			{
				return ResourceTypeCode.Stream;
			}
			string text;
			if (type == typeof(PrecannedResource))
			{
				text = ((PrecannedResource)value).TypeName;
				if (text.StartsWith("ResourceTypeCode.", StringComparison.Ordinal))
				{
					text = text.Substring(17);
					return (ResourceTypeCode)Enum.Parse(typeof(ResourceTypeCode), text);
				}
			}
			else
			{
				text = type.AssemblyQualifiedName;
			}
			int num = types.IndexOf(text);
			if (num == -1)
			{
				num = types.Count;
				types.Add(text);
			}
			return (ResourceTypeCode)(num + 64);
		}

		private void WriteValue(ResourceTypeCode typeCode, object value, BinaryWriter writer, IFormatter objFormatter)
		{
			switch (typeCode)
			{
			case ResourceTypeCode.String:
				writer.Write((string)value);
				break;
			case ResourceTypeCode.Boolean:
				writer.Write((bool)value);
				break;
			case ResourceTypeCode.Char:
				writer.Write((ushort)(char)value);
				break;
			case ResourceTypeCode.Byte:
				writer.Write((byte)value);
				break;
			case ResourceTypeCode.SByte:
				writer.Write((sbyte)value);
				break;
			case ResourceTypeCode.Int16:
				writer.Write((short)value);
				break;
			case ResourceTypeCode.UInt16:
				writer.Write((ushort)value);
				break;
			case ResourceTypeCode.Int32:
				writer.Write((int)value);
				break;
			case ResourceTypeCode.UInt32:
				writer.Write((uint)value);
				break;
			case ResourceTypeCode.Int64:
				writer.Write((long)value);
				break;
			case ResourceTypeCode.UInt64:
				writer.Write((ulong)value);
				break;
			case ResourceTypeCode.Single:
				writer.Write((float)value);
				break;
			case ResourceTypeCode.Double:
				writer.Write((double)value);
				break;
			case ResourceTypeCode.Decimal:
				writer.Write((decimal)value);
				break;
			case ResourceTypeCode.DateTime:
			{
				long value2 = ((DateTime)value).ToBinary();
				writer.Write(value2);
				break;
			}
			case ResourceTypeCode.TimeSpan:
				writer.Write(((TimeSpan)value).Ticks);
				break;
			case ResourceTypeCode.ByteArray:
			{
				byte[] array = (byte[])value;
				writer.Write(array.Length);
				writer.Write(array, 0, array.Length);
				break;
			}
			case ResourceTypeCode.Stream:
			{
				MemoryStream memoryStream = (MemoryStream)value;
				if (memoryStream.Length > int.MaxValue)
				{
					throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_MemStreamLength"));
				}
				memoryStream.InternalGetOriginAndLength(out var origin, out var length);
				byte[] buffer = memoryStream.InternalGetBuffer();
				writer.Write(length);
				writer.Write(buffer, origin, length);
				break;
			}
			default:
				objFormatter.Serialize(writer.BaseStream, value);
				break;
			case ResourceTypeCode.Null:
				break;
			}
		}

		private static void Write7BitEncodedInt(BinaryWriter store, int value)
		{
			uint num;
			for (num = (uint)value; num >= 128; num >>= 7)
			{
				store.Write((byte)(num | 0x80u));
			}
			store.Write((byte)num);
		}
	}
}
