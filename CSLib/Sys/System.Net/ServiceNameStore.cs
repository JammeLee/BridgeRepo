using System.Collections.Generic;
using System.Net.Sockets;
using System.Security;
using System.Security.Authentication.ExtendedProtection;

namespace System.Net
{
	internal class ServiceNameStore
	{
		private List<string> serviceNames;

		private ServiceNameCollection serviceNameCollection;

		public ServiceNameCollection ServiceNames
		{
			get
			{
				if (serviceNameCollection == null)
				{
					serviceNameCollection = new ServiceNameCollection(serviceNames);
				}
				return serviceNameCollection;
			}
		}

		public ServiceNameStore()
		{
			serviceNames = new List<string>();
			serviceNameCollection = null;
		}

		private bool AddSingleServiceName(string spn)
		{
			if (Contains(spn))
			{
				return false;
			}
			serviceNames.Add(spn);
			return true;
		}

		public bool Add(string uriPrefix)
		{
			string[] array = BuildServiceNames(uriPrefix);
			bool flag = false;
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (AddSingleServiceName(text))
				{
					flag = true;
					if (Logging.On)
					{
						Logging.PrintInfo(Logging.HttpListener, "ServiceNameStore#" + ValidationHelper.HashString(this) + "::Add() adding default SPNs '" + text + "' from prefix '" + uriPrefix + "'");
					}
				}
			}
			if (flag)
			{
				serviceNameCollection = null;
			}
			else if (Logging.On)
			{
				Logging.PrintInfo(Logging.HttpListener, "ServiceNameStore#" + ValidationHelper.HashString(this) + "::Add() no default SPN added for prefix '" + uriPrefix + "'");
			}
			return flag;
		}

		public bool Remove(string uriPrefix)
		{
			string text = BuildSimpleServiceName(uriPrefix);
			bool flag = Contains(text);
			if (flag)
			{
				serviceNames.Remove(text);
				serviceNameCollection = null;
			}
			if (Logging.On)
			{
				if (flag)
				{
					Logging.PrintInfo(Logging.HttpListener, "ServiceNameStore#" + ValidationHelper.HashString(this) + "::Remove() removing default SPN '" + text + "' from prefix '" + uriPrefix + "'");
				}
				else
				{
					Logging.PrintInfo(Logging.HttpListener, "ServiceNameStore#" + ValidationHelper.HashString(this) + "::Remove() no default SPN removed for prefix '" + uriPrefix + "'");
				}
			}
			return flag;
		}

		private bool Contains(string newServiceName)
		{
			if (newServiceName == null)
			{
				return false;
			}
			bool result = false;
			foreach (string serviceName in serviceNames)
			{
				if (string.Compare(serviceName, newServiceName, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					return true;
				}
			}
			return result;
		}

		public void Clear()
		{
			serviceNames.Clear();
			serviceNameCollection = null;
		}

		private string ExtractHostname(string uriPrefix, bool allowInvalidUriStrings)
		{
			if (Uri.IsWellFormedUriString(uriPrefix, UriKind.Absolute))
			{
				Uri uri = new Uri(uriPrefix);
				return uri.Host;
			}
			if (allowInvalidUriStrings)
			{
				int num = uriPrefix.IndexOf("://") + 3;
				int i = num;
				for (bool flag = false; i < uriPrefix.Length && uriPrefix[i] != '/' && (uriPrefix[i] != ':' || flag); i++)
				{
					if (uriPrefix[i] == '[')
					{
						if (flag)
						{
							i = num;
							break;
						}
						flag = true;
					}
					if (flag && uriPrefix[i] == ']')
					{
						flag = false;
					}
				}
				return uriPrefix.Substring(num, i - num);
			}
			return null;
		}

		public string BuildSimpleServiceName(string uriPrefix)
		{
			string text = ExtractHostname(uriPrefix, allowInvalidUriStrings: false);
			if (text != null)
			{
				return "HTTP/" + text;
			}
			return null;
		}

		public string[] BuildServiceNames(string uriPrefix)
		{
			string text = ExtractHostname(uriPrefix, allowInvalidUriStrings: true);
			IPAddress address = null;
			if (string.Compare(text, "*", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(text, "+", StringComparison.InvariantCultureIgnoreCase) == 0 || IPAddress.TryParse(text, out address))
			{
				try
				{
					string hostName = Dns.GetHostEntry(string.Empty).HostName;
					return new string[1]
					{
						"HTTP/" + hostName
					};
				}
				catch (SocketException)
				{
					return new string[0];
				}
				catch (SecurityException)
				{
					return new string[0];
				}
			}
			if (!text.Contains("."))
			{
				try
				{
					string hostName2 = Dns.GetHostEntry(text).HostName;
					return new string[2]
					{
						"HTTP/" + text,
						"HTTP/" + hostName2
					};
				}
				catch (SocketException)
				{
					return new string[1]
					{
						"HTTP/" + text
					};
				}
				catch (SecurityException)
				{
					return new string[1]
					{
						"HTTP/" + text
					};
				}
			}
			return new string[1]
			{
				"HTTP/" + text
			};
		}
	}
}
