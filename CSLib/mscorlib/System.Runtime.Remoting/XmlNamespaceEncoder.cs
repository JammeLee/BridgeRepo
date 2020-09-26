using System.Reflection;
using System.Text;

namespace System.Runtime.Remoting
{
	internal static class XmlNamespaceEncoder
	{
		internal static string GetXmlNamespaceForType(Type type, string dynamicUrl)
		{
			string fullName = type.FullName;
			Assembly assembly = type.Module.Assembly;
			StringBuilder stringBuilder = new StringBuilder(256);
			Assembly assembly2 = typeof(string).Module.Assembly;
			if (assembly == assembly2)
			{
				stringBuilder.Append(SoapServices.namespaceNS);
				stringBuilder.Append(fullName);
			}
			else
			{
				stringBuilder.Append(SoapServices.fullNS);
				stringBuilder.Append(fullName);
				stringBuilder.Append('/');
				stringBuilder.Append(assembly.nGetSimpleName());
			}
			return stringBuilder.ToString();
		}

		internal static string GetXmlNamespaceForTypeNamespace(Type type, string dynamicUrl)
		{
			string @namespace = type.Namespace;
			Assembly assembly = type.Module.Assembly;
			StringBuilder stringBuilder = new StringBuilder(256);
			Assembly assembly2 = typeof(string).Module.Assembly;
			if (assembly == assembly2)
			{
				stringBuilder.Append(SoapServices.namespaceNS);
				stringBuilder.Append(@namespace);
			}
			else
			{
				stringBuilder.Append(SoapServices.fullNS);
				stringBuilder.Append(@namespace);
				stringBuilder.Append('/');
				stringBuilder.Append(assembly.nGetSimpleName());
			}
			return stringBuilder.ToString();
		}

		internal static string GetTypeNameForSoapActionNamespace(string uri, out bool assemblyIncluded)
		{
			assemblyIncluded = false;
			string fullNS = SoapServices.fullNS;
			string namespaceNS = SoapServices.namespaceNS;
			if (uri.StartsWith(fullNS, StringComparison.Ordinal))
			{
				uri = uri.Substring(fullNS.Length);
				char[] separator = new char[1]
				{
					'/'
				};
				string[] array = uri.Split(separator);
				if (array.Length != 2)
				{
					return null;
				}
				assemblyIncluded = true;
				return array[0] + ", " + array[1];
			}
			if (uri.StartsWith(namespaceNS, StringComparison.Ordinal))
			{
				string str = typeof(string).Module.Assembly.nGetSimpleName();
				assemblyIncluded = true;
				return uri.Substring(namespaceNS.Length) + ", " + str;
			}
			return null;
		}
	}
}
