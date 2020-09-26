using System.Globalization;
using System.IO;

namespace System.Reflection.Emit
{
	[Serializable]
	internal class ModuleBuilderData
	{
		internal const string MULTI_BYTE_VALUE_CLASS = "$ArrayType$";

		internal string m_strModuleName;

		internal string m_strFileName;

		internal bool m_fGlobalBeenCreated;

		internal bool m_fHasGlobal;

		[NonSerialized]
		internal TypeBuilder m_globalTypeBuilder;

		[NonSerialized]
		internal ModuleBuilder m_module;

		internal int m_tkFile;

		internal bool m_isSaved;

		[NonSerialized]
		internal ResWriterData m_embeddedRes;

		internal bool m_isTransient;

		internal string m_strResourceFileName;

		internal byte[] m_resourceBytes;

		internal ModuleBuilderData(ModuleBuilder module, string strModuleName, string strFileName)
		{
			Init(module, strModuleName, strFileName);
		}

		internal virtual void Init(ModuleBuilder module, string strModuleName, string strFileName)
		{
			m_fGlobalBeenCreated = false;
			m_fHasGlobal = false;
			m_globalTypeBuilder = new TypeBuilder(module);
			m_module = module;
			m_strModuleName = strModuleName;
			m_tkFile = 0;
			m_isSaved = false;
			m_embeddedRes = null;
			m_strResourceFileName = null;
			m_resourceBytes = null;
			if (strFileName == null)
			{
				m_strFileName = strModuleName;
				m_isTransient = true;
			}
			else
			{
				string extension = Path.GetExtension(strFileName);
				if (extension == null || extension == string.Empty)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_NoModuleFileExtension"), strFileName));
				}
				m_strFileName = strFileName;
				m_isTransient = false;
			}
			m_module.InternalSetModuleProps(m_strModuleName);
		}

		internal virtual bool IsTransient()
		{
			return m_isTransient;
		}
	}
}
