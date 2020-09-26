using System.Collections;
using System.Globalization;
using System.Security;

namespace System.Reflection.Emit
{
	internal class AssemblyBuilderData
	{
		internal const int m_iInitialSize = 16;

		internal const int m_tkAssembly = 536870913;

		internal ArrayList m_moduleBuilderList;

		internal ArrayList m_resWriterList;

		internal string m_strAssemblyName;

		internal AssemblyBuilderAccess m_access;

		internal Assembly m_assembly;

		internal Type[] m_publicComTypeList;

		internal int m_iPublicComTypeCount;

		internal ModuleBuilder m_entryPointModule;

		internal bool m_isSaved;

		internal string m_strDir;

		internal PermissionSet m_RequiredPset;

		internal PermissionSet m_OptionalPset;

		internal PermissionSet m_RefusedPset;

		internal bool m_isSynchronized;

		internal CustomAttributeBuilder[] m_CABuilders;

		internal int m_iCABuilder;

		internal byte[][] m_CABytes;

		internal ConstructorInfo[] m_CACons;

		internal int m_iCAs;

		internal PEFileKinds m_peFileKind;

		private ModuleBuilder m_InMemoryAssemblyModule;

		private ModuleBuilder m_OnDiskAssemblyModule;

		internal MethodInfo m_entryPointMethod;

		internal Assembly m_ISymWrapperAssembly;

		internal string m_strResourceFileName;

		internal byte[] m_resourceBytes;

		internal NativeVersionInfo m_nativeVersion;

		internal bool m_hasUnmanagedVersionInfo;

		internal bool m_OverrideUnmanagedVersionInfo;

		internal AssemblyBuilderData(Assembly assembly, string strAssemblyName, AssemblyBuilderAccess access, string dir)
		{
			m_assembly = assembly;
			m_strAssemblyName = strAssemblyName;
			m_access = access;
			m_moduleBuilderList = new ArrayList();
			m_resWriterList = new ArrayList();
			m_publicComTypeList = null;
			m_CABuilders = null;
			m_CABytes = null;
			m_CACons = null;
			m_iPublicComTypeCount = 0;
			m_iCABuilder = 0;
			m_iCAs = 0;
			m_entryPointModule = null;
			m_isSaved = false;
			if (dir == null && access != AssemblyBuilderAccess.Run)
			{
				m_strDir = Environment.CurrentDirectory;
			}
			else
			{
				m_strDir = dir;
			}
			m_RequiredPset = null;
			m_OptionalPset = null;
			m_RefusedPset = null;
			m_isSynchronized = true;
			m_hasUnmanagedVersionInfo = false;
			m_OverrideUnmanagedVersionInfo = false;
			m_InMemoryAssemblyModule = null;
			m_OnDiskAssemblyModule = null;
			m_peFileKind = PEFileKinds.Dll;
			m_strResourceFileName = null;
			m_resourceBytes = null;
			m_nativeVersion = null;
			m_entryPointMethod = null;
			m_ISymWrapperAssembly = null;
		}

		internal void AddModule(ModuleBuilder dynModule)
		{
			m_moduleBuilderList.Add(dynModule);
			if (m_assembly != null)
			{
				m_assembly.nAddFileToInMemoryFileList(dynModule.m_moduleData.m_strFileName, dynModule);
			}
		}

		internal void AddResWriter(ResWriterData resData)
		{
			m_resWriterList.Add(resData);
		}

		internal void AddCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			if (m_CABuilders == null)
			{
				m_CABuilders = new CustomAttributeBuilder[16];
			}
			if (m_iCABuilder == m_CABuilders.Length)
			{
				CustomAttributeBuilder[] array = new CustomAttributeBuilder[m_iCABuilder * 2];
				Array.Copy(m_CABuilders, array, m_iCABuilder);
				m_CABuilders = array;
			}
			m_CABuilders[m_iCABuilder] = customBuilder;
			m_iCABuilder++;
		}

		internal void AddCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			if (m_CABytes == null)
			{
				m_CABytes = new byte[16][];
				m_CACons = new ConstructorInfo[16];
			}
			if (m_iCAs == m_CABytes.Length)
			{
				byte[][] array = new byte[m_iCAs * 2][];
				ConstructorInfo[] array2 = new ConstructorInfo[m_iCAs * 2];
				for (int i = 0; i < m_iCAs; i++)
				{
					array[i] = m_CABytes[i];
					array2[i] = m_CACons[i];
				}
				m_CABytes = array;
				m_CACons = array2;
			}
			byte[] array3 = new byte[binaryAttribute.Length];
			Array.Copy(binaryAttribute, array3, binaryAttribute.Length);
			m_CABytes[m_iCAs] = array3;
			m_CACons[m_iCAs] = con;
			m_iCAs++;
		}

		internal void FillUnmanagedVersionInfo()
		{
			CultureInfo locale = m_assembly.GetLocale();
			if (locale != null)
			{
				m_nativeVersion.m_lcid = locale.LCID;
			}
			for (int i = 0; i < m_iCABuilder; i++)
			{
				Type declaringType = m_CABuilders[i].m_con.DeclaringType;
				if (m_CABuilders[i].m_constructorArgs.Length == 0 || m_CABuilders[i].m_constructorArgs[0] == null)
				{
					continue;
				}
				if (declaringType.Equals(typeof(AssemblyCopyrightAttribute)))
				{
					if (m_CABuilders[i].m_constructorArgs.Length != 1)
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_BadCAForUnmngRSC"), m_CABuilders[i].m_con.ReflectedType.Name));
					}
					if (!m_OverrideUnmanagedVersionInfo)
					{
						m_nativeVersion.m_strCopyright = m_CABuilders[i].m_constructorArgs[0].ToString();
					}
				}
				else if (declaringType.Equals(typeof(AssemblyTrademarkAttribute)))
				{
					if (m_CABuilders[i].m_constructorArgs.Length != 1)
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_BadCAForUnmngRSC"), m_CABuilders[i].m_con.ReflectedType.Name));
					}
					if (!m_OverrideUnmanagedVersionInfo)
					{
						m_nativeVersion.m_strTrademark = m_CABuilders[i].m_constructorArgs[0].ToString();
					}
				}
				else if (declaringType.Equals(typeof(AssemblyProductAttribute)))
				{
					if (!m_OverrideUnmanagedVersionInfo)
					{
						m_nativeVersion.m_strProduct = m_CABuilders[i].m_constructorArgs[0].ToString();
					}
				}
				else if (declaringType.Equals(typeof(AssemblyCompanyAttribute)))
				{
					if (m_CABuilders[i].m_constructorArgs.Length != 1)
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_BadCAForUnmngRSC"), m_CABuilders[i].m_con.ReflectedType.Name));
					}
					if (!m_OverrideUnmanagedVersionInfo)
					{
						m_nativeVersion.m_strCompany = m_CABuilders[i].m_constructorArgs[0].ToString();
					}
				}
				else if (declaringType.Equals(typeof(AssemblyDescriptionAttribute)))
				{
					if (m_CABuilders[i].m_constructorArgs.Length != 1)
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_BadCAForUnmngRSC"), m_CABuilders[i].m_con.ReflectedType.Name));
					}
					m_nativeVersion.m_strDescription = m_CABuilders[i].m_constructorArgs[0].ToString();
				}
				else if (declaringType.Equals(typeof(AssemblyTitleAttribute)))
				{
					if (m_CABuilders[i].m_constructorArgs.Length != 1)
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_BadCAForUnmngRSC"), m_CABuilders[i].m_con.ReflectedType.Name));
					}
					m_nativeVersion.m_strTitle = m_CABuilders[i].m_constructorArgs[0].ToString();
				}
				else if (declaringType.Equals(typeof(AssemblyInformationalVersionAttribute)))
				{
					if (m_CABuilders[i].m_constructorArgs.Length != 1)
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_BadCAForUnmngRSC"), m_CABuilders[i].m_con.ReflectedType.Name));
					}
					if (!m_OverrideUnmanagedVersionInfo)
					{
						m_nativeVersion.m_strProductVersion = m_CABuilders[i].m_constructorArgs[0].ToString();
					}
				}
				else if (declaringType.Equals(typeof(AssemblyCultureAttribute)))
				{
					if (m_CABuilders[i].m_constructorArgs.Length != 1)
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_BadCAForUnmngRSC"), m_CABuilders[i].m_con.ReflectedType.Name));
					}
					CultureInfo cultureInfo = new CultureInfo(m_CABuilders[i].m_constructorArgs[0].ToString());
					m_nativeVersion.m_lcid = cultureInfo.LCID;
				}
				else if (declaringType.Equals(typeof(AssemblyFileVersionAttribute)))
				{
					if (m_CABuilders[i].m_constructorArgs.Length != 1)
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_BadCAForUnmngRSC"), m_CABuilders[i].m_con.ReflectedType.Name));
					}
					if (!m_OverrideUnmanagedVersionInfo)
					{
						m_nativeVersion.m_strFileVersion = m_CABuilders[i].m_constructorArgs[0].ToString();
					}
				}
			}
		}

		internal void CheckResNameConflict(string strNewResName)
		{
			int count = m_resWriterList.Count;
			for (int i = 0; i < count; i++)
			{
				ResWriterData resWriterData = (ResWriterData)m_resWriterList[i];
				if (resWriterData.m_strName.Equals(strNewResName))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_DuplicateResourceName"));
				}
			}
		}

		internal void CheckNameConflict(string strNewModuleName)
		{
			int count = m_moduleBuilderList.Count;
			for (int i = 0; i < count; i++)
			{
				ModuleBuilder moduleBuilder = (ModuleBuilder)m_moduleBuilderList[i];
				if (moduleBuilder.m_moduleData.m_strModuleName.Equals(strNewModuleName))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_DuplicateModuleName"));
				}
			}
			if (!(m_assembly is AssemblyBuilder) && m_assembly.GetModule(strNewModuleName) != null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_DuplicateModuleName"));
			}
		}

		internal void CheckTypeNameConflict(string strTypeName, TypeBuilder enclosingType)
		{
			for (int i = 0; i < m_moduleBuilderList.Count; i++)
			{
				ModuleBuilder moduleBuilder = (ModuleBuilder)m_moduleBuilderList[i];
				for (int j = 0; j < moduleBuilder.m_TypeBuilderList.Count; j++)
				{
					Type type = (Type)moduleBuilder.m_TypeBuilderList[j];
					if (type.FullName.Equals(strTypeName) && type.DeclaringType == enclosingType)
					{
						throw new ArgumentException(Environment.GetResourceString("Argument_DuplicateTypeName"));
					}
				}
			}
			if (enclosingType == null && !(m_assembly is AssemblyBuilder) && m_assembly.GetType(strTypeName, throwOnError: false, ignoreCase: false) != null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_DuplicateTypeName"));
			}
		}

		internal void CheckFileNameConflict(string strFileName)
		{
			int count = m_moduleBuilderList.Count;
			for (int i = 0; i < count; i++)
			{
				ModuleBuilder moduleBuilder = (ModuleBuilder)m_moduleBuilderList[i];
				if (moduleBuilder.m_moduleData.m_strFileName != null && string.Compare(moduleBuilder.m_moduleData.m_strFileName, strFileName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_DuplicatedFileName"));
				}
			}
			count = m_resWriterList.Count;
			for (int i = 0; i < count; i++)
			{
				ResWriterData resWriterData = (ResWriterData)m_resWriterList[i];
				if (resWriterData.m_strFileName != null && string.Compare(resWriterData.m_strFileName, strFileName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_DuplicatedFileName"));
				}
			}
		}

		internal ModuleBuilder FindModuleWithFileName(string strFileName)
		{
			int count = m_moduleBuilderList.Count;
			for (int i = 0; i < count; i++)
			{
				ModuleBuilder moduleBuilder = (ModuleBuilder)m_moduleBuilderList[i];
				if (moduleBuilder.m_moduleData.m_strFileName != null && string.Compare(moduleBuilder.m_moduleData.m_strFileName, strFileName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return moduleBuilder;
				}
			}
			return null;
		}

		internal ModuleBuilder FindModuleWithName(string strName)
		{
			int count = m_moduleBuilderList.Count;
			for (int i = 0; i < count; i++)
			{
				ModuleBuilder moduleBuilder = (ModuleBuilder)m_moduleBuilderList[i];
				if (moduleBuilder.m_moduleData.m_strModuleName != null && string.Compare(moduleBuilder.m_moduleData.m_strModuleName, strName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return moduleBuilder;
				}
			}
			return null;
		}

		internal void AddPublicComType(Type type)
		{
			if (m_isSaved)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotAlterAssembly"));
			}
			EnsurePublicComTypeCapacity();
			m_publicComTypeList[m_iPublicComTypeCount] = type;
			m_iPublicComTypeCount++;
		}

		internal void AddPermissionRequests(PermissionSet required, PermissionSet optional, PermissionSet refused)
		{
			if (m_isSaved)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotAlterAssembly"));
			}
			m_RequiredPset = required;
			m_OptionalPset = optional;
			m_RefusedPset = refused;
		}

		internal void EnsurePublicComTypeCapacity()
		{
			if (m_publicComTypeList == null)
			{
				m_publicComTypeList = new Type[16];
			}
			if (m_iPublicComTypeCount == m_publicComTypeList.Length)
			{
				Type[] array = new Type[m_iPublicComTypeCount * 2];
				Array.Copy(m_publicComTypeList, array, m_iPublicComTypeCount);
				m_publicComTypeList = array;
			}
		}

		internal ModuleBuilder GetInMemoryAssemblyModule()
		{
			if (m_InMemoryAssemblyModule == null)
			{
				ModuleBuilder moduleBuilder = (ModuleBuilder)m_assembly.nGetInMemoryAssemblyModule();
				moduleBuilder.Init("RefEmit_InMemoryManifestModule", null, null);
				m_InMemoryAssemblyModule = moduleBuilder;
			}
			return m_InMemoryAssemblyModule;
		}

		internal ModuleBuilder GetOnDiskAssemblyModule()
		{
			if (m_OnDiskAssemblyModule == null)
			{
				ModuleBuilder moduleBuilder = (ModuleBuilder)m_assembly.nGetOnDiskAssemblyModule();
				moduleBuilder.Init("RefEmit_OnDiskManifestModule", null, null);
				m_OnDiskAssemblyModule = moduleBuilder;
			}
			return m_OnDiskAssemblyModule;
		}

		internal void SetOnDiskAssemblyModule(ModuleBuilder modBuilder)
		{
			m_OnDiskAssemblyModule = modBuilder;
		}
	}
}
