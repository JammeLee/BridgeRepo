using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Resources
{
	[Serializable]
	[ComVisible(true)]
	public class ResourceSet : IDisposable, IEnumerable
	{
		[NonSerialized]
		protected IResourceReader Reader;

		protected Hashtable Table;

		private Hashtable _caseInsensitiveTable;

		protected ResourceSet()
		{
			Table = new Hashtable(0);
		}

		internal ResourceSet(bool junk)
		{
		}

		public ResourceSet(string fileName)
		{
			Reader = new ResourceReader(fileName);
			Table = new Hashtable();
			ReadResources();
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public ResourceSet(Stream stream)
		{
			Reader = new ResourceReader(stream);
			Table = new Hashtable();
			ReadResources();
		}

		public ResourceSet(IResourceReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			Reader = reader;
			Table = new Hashtable();
			ReadResources();
		}

		public virtual void Close()
		{
			Dispose(disposing: true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				IResourceReader reader = Reader;
				Reader = null;
				reader?.Close();
			}
			Reader = null;
			_caseInsensitiveTable = null;
			Table = null;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}

		public virtual Type GetDefaultReader()
		{
			return typeof(ResourceReader);
		}

		public virtual Type GetDefaultWriter()
		{
			return typeof(ResourceWriter);
		}

		[ComVisible(false)]
		public virtual IDictionaryEnumerator GetEnumerator()
		{
			return GetEnumeratorHelper();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumeratorHelper();
		}

		private IDictionaryEnumerator GetEnumeratorHelper()
		{
			Hashtable table = Table;
			if (table == null)
			{
				throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_ResourceSet"));
			}
			return table.GetEnumerator();
		}

		public virtual string GetString(string name)
		{
			Hashtable table = Table;
			if (table == null)
			{
				throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_ResourceSet"));
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			try
			{
				return (string)table[name];
			}
			catch (InvalidCastException)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceNotString_Name", name));
			}
		}

		public virtual string GetString(string name, bool ignoreCase)
		{
			Hashtable table = Table;
			if (table == null)
			{
				throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_ResourceSet"));
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			string text;
			try
			{
				text = (string)table[name];
			}
			catch (InvalidCastException)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceNotString_Name", name));
			}
			if (text != null || !ignoreCase)
			{
				return text;
			}
			Hashtable hashtable = _caseInsensitiveTable;
			if (hashtable == null)
			{
				hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
				IDictionaryEnumerator enumerator = table.GetEnumerator();
				while (enumerator.MoveNext())
				{
					hashtable.Add(enumerator.Key, enumerator.Value);
				}
				_caseInsensitiveTable = hashtable;
			}
			try
			{
				return (string)hashtable[name];
			}
			catch (InvalidCastException)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceNotString_Name", name));
			}
		}

		public virtual object GetObject(string name)
		{
			Hashtable table = Table;
			if (table == null)
			{
				throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_ResourceSet"));
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return table[name];
		}

		public virtual object GetObject(string name, bool ignoreCase)
		{
			Hashtable table = Table;
			if (table == null)
			{
				throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_ResourceSet"));
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			object obj = table[name];
			if (obj != null || !ignoreCase)
			{
				return obj;
			}
			Hashtable hashtable = _caseInsensitiveTable;
			if (hashtable == null)
			{
				hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
				IDictionaryEnumerator enumerator = table.GetEnumerator();
				while (enumerator.MoveNext())
				{
					hashtable.Add(enumerator.Key, enumerator.Value);
				}
				_caseInsensitiveTable = hashtable;
			}
			return hashtable[name];
		}

		protected virtual void ReadResources()
		{
			IDictionaryEnumerator enumerator = Reader.GetEnumerator();
			while (enumerator.MoveNext())
			{
				object value = enumerator.Value;
				Table.Add(enumerator.Key, value);
			}
		}
	}
}
