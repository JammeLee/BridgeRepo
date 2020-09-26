using System.Reflection;

namespace System.Runtime.Remoting.Metadata
{
	internal class RemotingTypeCachedData : RemotingCachedData
	{
		private class LastCalledMethodClass
		{
			public string methodName;

			public MethodBase MB;
		}

		private LastCalledMethodClass _lastMethodCalled;

		private TypeInfo _typeInfo;

		private string _qualifiedTypeName;

		private string _assemblyName;

		private string _simpleAssemblyName;

		internal TypeInfo TypeInfo
		{
			get
			{
				if (_typeInfo == null)
				{
					_typeInfo = new TypeInfo((Type)RI);
				}
				return _typeInfo;
			}
		}

		internal string QualifiedTypeName
		{
			get
			{
				if (_qualifiedTypeName == null)
				{
					_qualifiedTypeName = RemotingServices.DetermineDefaultQualifiedTypeName((Type)RI);
				}
				return _qualifiedTypeName;
			}
		}

		internal string AssemblyName
		{
			get
			{
				if (_assemblyName == null)
				{
					_assemblyName = ((Type)RI).Module.Assembly.FullName;
				}
				return _assemblyName;
			}
		}

		internal string SimpleAssemblyName
		{
			get
			{
				if (_simpleAssemblyName == null)
				{
					_simpleAssemblyName = ((Type)RI).Module.Assembly.nGetSimpleName();
				}
				return _simpleAssemblyName;
			}
		}

		internal RemotingTypeCachedData(object ri)
			: base(ri)
		{
			_lastMethodCalled = null;
		}

		internal MethodBase GetLastCalledMethod(string newMeth)
		{
			LastCalledMethodClass lastMethodCalled = _lastMethodCalled;
			if (lastMethodCalled == null)
			{
				return null;
			}
			string methodName = lastMethodCalled.methodName;
			MethodBase mB = lastMethodCalled.MB;
			if (mB == null || methodName == null)
			{
				return null;
			}
			if (methodName.Equals(newMeth))
			{
				return mB;
			}
			return null;
		}

		internal void SetLastCalledMethod(string newMethName, MethodBase newMB)
		{
			LastCalledMethodClass lastCalledMethodClass = new LastCalledMethodClass();
			lastCalledMethodClass.methodName = newMethName;
			lastCalledMethodClass.MB = newMB;
			_lastMethodCalled = lastCalledMethodClass;
		}
	}
}
