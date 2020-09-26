using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Reflection
{
	[Serializable]
	[ComVisible(true)]
	public class StrongNameKeyPair : IDeserializationCallback, ISerializable
	{
		private bool _keyPairExported;

		private byte[] _keyPairArray;

		private string _keyPairContainer;

		private byte[] _publicKey;

		public byte[] PublicKey
		{
			get
			{
				if (_publicKey == null)
				{
					_publicKey = nGetPublicKey(_keyPairExported, _keyPairArray, _keyPairContainer);
				}
				byte[] array = new byte[_publicKey.Length];
				Array.Copy(_publicKey, array, _publicKey.Length);
				return array;
			}
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public StrongNameKeyPair(FileStream keyPairFile)
		{
			if (keyPairFile == null)
			{
				throw new ArgumentNullException("keyPairFile");
			}
			int num = (int)keyPairFile.Length;
			_keyPairArray = new byte[num];
			keyPairFile.Read(_keyPairArray, 0, num);
			_keyPairExported = true;
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public StrongNameKeyPair(byte[] keyPairArray)
		{
			if (keyPairArray == null)
			{
				throw new ArgumentNullException("keyPairArray");
			}
			_keyPairArray = new byte[keyPairArray.Length];
			Array.Copy(keyPairArray, _keyPairArray, keyPairArray.Length);
			_keyPairExported = true;
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public StrongNameKeyPair(string keyPairContainer)
		{
			if (keyPairContainer == null)
			{
				throw new ArgumentNullException("keyPairContainer");
			}
			_keyPairContainer = keyPairContainer;
			_keyPairExported = false;
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected StrongNameKeyPair(SerializationInfo info, StreamingContext context)
		{
			_keyPairExported = (bool)info.GetValue("_keyPairExported", typeof(bool));
			_keyPairArray = (byte[])info.GetValue("_keyPairArray", typeof(byte[]));
			_keyPairContainer = (string)info.GetValue("_keyPairContainer", typeof(string));
			_publicKey = (byte[])info.GetValue("_publicKey", typeof(byte[]));
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("_keyPairExported", _keyPairExported);
			info.AddValue("_keyPairArray", _keyPairArray);
			info.AddValue("_keyPairContainer", _keyPairContainer);
			info.AddValue("_publicKey", _publicKey);
		}

		void IDeserializationCallback.OnDeserialization(object sender)
		{
		}

		private bool GetKeyPair(out object arrayOrContainer)
		{
			arrayOrContainer = (_keyPairExported ? ((object)_keyPairArray) : ((object)_keyPairContainer));
			return _keyPairExported;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern byte[] nGetPublicKey(bool exported, byte[] array, string container);
	}
}
