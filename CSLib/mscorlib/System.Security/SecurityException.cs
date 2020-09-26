using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Util;
using System.Text;
using System.Threading;

namespace System.Security
{
	[Serializable]
	[ComVisible(true)]
	public class SecurityException : SystemException
	{
		private const string ActionName = "Action";

		private const string FirstPermissionThatFailedName = "FirstPermissionThatFailed";

		private const string DemandedName = "Demanded";

		private const string GrantedSetName = "GrantedSet";

		private const string RefusedSetName = "RefusedSet";

		private const string DeniedName = "Denied";

		private const string PermitOnlyName = "PermitOnly";

		private const string Assembly_Name = "Assembly";

		private const string MethodName_Serialized = "Method";

		private const string MethodName_String = "Method_String";

		private const string ZoneName = "Zone";

		private const string UrlName = "Url";

		private string m_debugString;

		private SecurityAction m_action;

		[NonSerialized]
		private Type m_typeOfPermissionThatFailed;

		private string m_permissionThatFailed;

		private string m_demanded;

		private string m_granted;

		private string m_refused;

		private string m_denied;

		private string m_permitOnly;

		private AssemblyName m_assemblyName;

		private byte[] m_serializedMethodInfo;

		private string m_strMethodInfo;

		private SecurityZone m_zone;

		private string m_url;

		[ComVisible(false)]
		public SecurityAction Action
		{
			get
			{
				return m_action;
			}
			set
			{
				m_action = value;
			}
		}

		public Type PermissionType
		{
			get
			{
				if (m_typeOfPermissionThatFailed == null)
				{
					object obj = XMLUtil.XmlStringToSecurityObject(m_permissionThatFailed);
					if (obj == null)
					{
						obj = XMLUtil.XmlStringToSecurityObject(m_demanded);
					}
					if (obj != null)
					{
						m_typeOfPermissionThatFailed = obj.GetType();
					}
				}
				return m_typeOfPermissionThatFailed;
			}
			set
			{
				m_typeOfPermissionThatFailed = value;
			}
		}

		public IPermission FirstPermissionThatFailed
		{
			[SecurityPermission(SecurityAction.Demand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
			get
			{
				return (IPermission)XMLUtil.XmlStringToSecurityObject(m_permissionThatFailed);
			}
			set
			{
				m_permissionThatFailed = XMLUtil.SecurityObjectToXmlString(value);
			}
		}

		public string PermissionState
		{
			[SecurityPermission(SecurityAction.Demand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
			get
			{
				return m_demanded;
			}
			set
			{
				m_demanded = value;
			}
		}

		[ComVisible(false)]
		public object Demanded
		{
			[SecurityPermission(SecurityAction.Demand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
			get
			{
				return XMLUtil.XmlStringToSecurityObject(m_demanded);
			}
			set
			{
				m_demanded = XMLUtil.SecurityObjectToXmlString(value);
			}
		}

		public string GrantedSet
		{
			[SecurityPermission(SecurityAction.Demand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
			get
			{
				return m_granted;
			}
			set
			{
				m_granted = value;
			}
		}

		public string RefusedSet
		{
			[SecurityPermission(SecurityAction.Demand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
			get
			{
				return m_refused;
			}
			set
			{
				m_refused = value;
			}
		}

		[ComVisible(false)]
		public object DenySetInstance
		{
			[SecurityPermission(SecurityAction.Demand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
			get
			{
				return XMLUtil.XmlStringToSecurityObject(m_denied);
			}
			set
			{
				m_denied = XMLUtil.SecurityObjectToXmlString(value);
			}
		}

		[ComVisible(false)]
		public object PermitOnlySetInstance
		{
			[SecurityPermission(SecurityAction.Demand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
			get
			{
				return XMLUtil.XmlStringToSecurityObject(m_permitOnly);
			}
			set
			{
				m_permitOnly = XMLUtil.SecurityObjectToXmlString(value);
			}
		}

		[ComVisible(false)]
		public AssemblyName FailedAssemblyInfo
		{
			[SecurityPermission(SecurityAction.Demand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
			get
			{
				return m_assemblyName;
			}
			set
			{
				m_assemblyName = value;
			}
		}

		[ComVisible(false)]
		public MethodInfo Method
		{
			[SecurityPermission(SecurityAction.Demand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
			get
			{
				return getMethod();
			}
			set
			{
				RuntimeMethodInfo runtimeMethodInfo = value as RuntimeMethodInfo;
				m_serializedMethodInfo = ObjectToByteArray(runtimeMethodInfo);
				if (runtimeMethodInfo != null)
				{
					m_strMethodInfo = runtimeMethodInfo.ToString();
				}
			}
		}

		public SecurityZone Zone
		{
			get
			{
				return m_zone;
			}
			set
			{
				m_zone = value;
			}
		}

		public string Url
		{
			[SecurityPermission(SecurityAction.Demand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
			get
			{
				return m_url;
			}
			set
			{
				m_url = value;
			}
		}

		internal static string GetResString(string sResourceName)
		{
			PermissionSet.s_fullTrust.Assert();
			return Environment.GetResourceString(sResourceName);
		}

		internal static Exception MakeSecurityException(AssemblyName asmName, Evidence asmEvidence, PermissionSet granted, PermissionSet refused, RuntimeMethodHandle rmh, SecurityAction action, object demand, IPermission permThatFailed)
		{
			HostProtectionPermission hostProtectionPermission = permThatFailed as HostProtectionPermission;
			if (hostProtectionPermission != null)
			{
				return new HostProtectionException(GetResString("HostProtection_HostProtection"), HostProtectionPermission.protectedResources, hostProtectionPermission.Resources);
			}
			string message = "";
			MethodInfo method = null;
			try
			{
				message = ((granted == null && refused == null && demand == null) ? GetResString("Security_NoAPTCA") : ((demand != null && demand is IPermission) ? string.Format(CultureInfo.InvariantCulture, GetResString("Security_Generic"), demand.GetType().AssemblyQualifiedName) : ((permThatFailed == null) ? GetResString("Security_GenericNoType") : string.Format(CultureInfo.InvariantCulture, GetResString("Security_Generic"), permThatFailed.GetType().AssemblyQualifiedName))));
				method = SecurityRuntime.GetMethodInfo(rmh);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException)
				{
					throw;
				}
			}
			return new SecurityException(message, asmName, granted, refused, method, action, demand, permThatFailed, asmEvidence);
		}

		private static byte[] ObjectToByteArray(object obj)
		{
			if (obj == null)
			{
				return null;
			}
			MemoryStream memoryStream = new MemoryStream();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			try
			{
				binaryFormatter.Serialize(memoryStream, obj);
				return memoryStream.ToArray();
			}
			catch (NotSupportedException)
			{
				return null;
			}
		}

		private static object ByteArrayToObject(byte[] array)
		{
			if (array == null || array.Length == 0)
			{
				return null;
			}
			MemoryStream serializationStream = new MemoryStream(array);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			return binaryFormatter.Deserialize(serializationStream);
		}

		public SecurityException()
			: base(GetResString("Arg_SecurityException"))
		{
			SetErrorCode(-2146233078);
		}

		public SecurityException(string message)
			: base(message)
		{
			SetErrorCode(-2146233078);
		}

		public SecurityException(string message, Type type)
			: base(message)
		{
			PermissionSet.s_fullTrust.Assert();
			SetErrorCode(-2146233078);
			m_typeOfPermissionThatFailed = type;
		}

		public SecurityException(string message, Type type, string state)
			: base(message)
		{
			PermissionSet.s_fullTrust.Assert();
			SetErrorCode(-2146233078);
			m_typeOfPermissionThatFailed = type;
			m_demanded = state;
		}

		public SecurityException(string message, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2146233078);
		}

		internal SecurityException(PermissionSet grantedSetObj, PermissionSet refusedSetObj)
			: base(GetResString("Arg_SecurityException"))
		{
			PermissionSet.s_fullTrust.Assert();
			SetErrorCode(-2146233078);
			if (grantedSetObj != null)
			{
				m_granted = grantedSetObj.ToXml().ToString();
			}
			if (refusedSetObj != null)
			{
				m_refused = refusedSetObj.ToXml().ToString();
			}
		}

		internal SecurityException(string message, PermissionSet grantedSetObj, PermissionSet refusedSetObj)
			: base(message)
		{
			PermissionSet.s_fullTrust.Assert();
			SetErrorCode(-2146233078);
			if (grantedSetObj != null)
			{
				m_granted = grantedSetObj.ToXml().ToString();
			}
			if (refusedSetObj != null)
			{
				m_refused = refusedSetObj.ToXml().ToString();
			}
		}

		protected SecurityException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			try
			{
				m_action = (SecurityAction)info.GetValue("Action", typeof(SecurityAction));
				m_permissionThatFailed = (string)info.GetValueNoThrow("FirstPermissionThatFailed", typeof(string));
				m_demanded = (string)info.GetValueNoThrow("Demanded", typeof(string));
				m_granted = (string)info.GetValueNoThrow("GrantedSet", typeof(string));
				m_refused = (string)info.GetValueNoThrow("RefusedSet", typeof(string));
				m_denied = (string)info.GetValueNoThrow("Denied", typeof(string));
				m_permitOnly = (string)info.GetValueNoThrow("PermitOnly", typeof(string));
				m_assemblyName = (AssemblyName)info.GetValueNoThrow("Assembly", typeof(AssemblyName));
				m_serializedMethodInfo = (byte[])info.GetValueNoThrow("Method", typeof(byte[]));
				m_strMethodInfo = (string)info.GetValueNoThrow("Method_String", typeof(string));
				m_zone = (SecurityZone)info.GetValue("Zone", typeof(SecurityZone));
				m_url = (string)info.GetValueNoThrow("Url", typeof(string));
			}
			catch
			{
				m_action = (SecurityAction)0;
				m_permissionThatFailed = "";
				m_demanded = "";
				m_granted = "";
				m_refused = "";
				m_denied = "";
				m_permitOnly = "";
				m_assemblyName = null;
				m_serializedMethodInfo = null;
				m_strMethodInfo = null;
				m_zone = SecurityZone.NoZone;
				m_url = "";
			}
		}

		public SecurityException(string message, AssemblyName assemblyName, PermissionSet grant, PermissionSet refused, MethodInfo method, SecurityAction action, object demanded, IPermission permThatFailed, Evidence evidence)
			: base(message)
		{
			PermissionSet.s_fullTrust.Assert();
			SetErrorCode(-2146233078);
			Action = action;
			if (permThatFailed != null)
			{
				m_typeOfPermissionThatFailed = permThatFailed.GetType();
			}
			FirstPermissionThatFailed = permThatFailed;
			Demanded = demanded;
			m_granted = ((grant == null) ? "" : grant.ToXml().ToString());
			m_refused = ((refused == null) ? "" : refused.ToXml().ToString());
			m_denied = "";
			m_permitOnly = "";
			m_assemblyName = assemblyName;
			Method = method;
			m_url = "";
			m_zone = SecurityZone.NoZone;
			if (evidence != null)
			{
				Url url = (Url)evidence.FindType(typeof(Url));
				if (url != null)
				{
					m_url = url.GetURLString().ToString();
				}
				Zone zone = (Zone)evidence.FindType(typeof(Zone));
				if (zone != null)
				{
					m_zone = zone.SecurityZone;
				}
			}
			m_debugString = ToString(includeSensitiveInfo: true, includeBaseInfo: false);
		}

		public SecurityException(string message, object deny, object permitOnly, MethodInfo method, object demanded, IPermission permThatFailed)
			: base(message)
		{
			PermissionSet.s_fullTrust.Assert();
			SetErrorCode(-2146233078);
			Action = SecurityAction.Demand;
			if (permThatFailed != null)
			{
				m_typeOfPermissionThatFailed = permThatFailed.GetType();
			}
			FirstPermissionThatFailed = permThatFailed;
			Demanded = demanded;
			m_granted = "";
			m_refused = "";
			DenySetInstance = deny;
			PermitOnlySetInstance = permitOnly;
			m_assemblyName = null;
			Method = method;
			m_zone = SecurityZone.NoZone;
			m_url = "";
			m_debugString = ToString(includeSensitiveInfo: true, includeBaseInfo: false);
		}

		private MethodInfo getMethod()
		{
			return (MethodInfo)ByteArrayToObject(m_serializedMethodInfo);
		}

		private void ToStringHelper(StringBuilder sb, string resourceString, object attr)
		{
			if (attr != null)
			{
				string text = attr as string;
				if (text == null)
				{
					text = attr.ToString();
				}
				if (text.Length != 0)
				{
					sb.Append(Environment.NewLine);
					sb.Append(GetResString(resourceString));
					sb.Append(Environment.NewLine);
					sb.Append(text);
				}
			}
		}

		private string ToString(bool includeSensitiveInfo, bool includeBaseInfo)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (includeBaseInfo)
			{
				try
				{
					stringBuilder.Append(base.ToString());
				}
				catch (SecurityException)
				{
				}
			}
			PermissionSet.s_fullTrust.Assert();
			if (Action > (SecurityAction)0)
			{
				ToStringHelper(stringBuilder, "Security_Action", Action);
			}
			ToStringHelper(stringBuilder, "Security_TypeFirstPermThatFailed", PermissionType);
			if (includeSensitiveInfo)
			{
				ToStringHelper(stringBuilder, "Security_FirstPermThatFailed", m_permissionThatFailed);
				ToStringHelper(stringBuilder, "Security_Demanded", m_demanded);
				ToStringHelper(stringBuilder, "Security_GrantedSet", m_granted);
				ToStringHelper(stringBuilder, "Security_RefusedSet", m_refused);
				ToStringHelper(stringBuilder, "Security_Denied", m_denied);
				ToStringHelper(stringBuilder, "Security_PermitOnly", m_permitOnly);
				ToStringHelper(stringBuilder, "Security_Assembly", m_assemblyName);
				ToStringHelper(stringBuilder, "Security_Method", m_strMethodInfo);
			}
			if (m_zone != SecurityZone.NoZone)
			{
				ToStringHelper(stringBuilder, "Security_Zone", m_zone);
			}
			if (includeSensitiveInfo)
			{
				ToStringHelper(stringBuilder, "Security_Url", m_url);
			}
			return stringBuilder.ToString();
		}

		private bool CanAccessSensitiveInfo()
		{
			bool result = false;
			try
			{
				new SecurityPermission(SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy).Demand();
				result = true;
				return result;
			}
			catch (SecurityException)
			{
				return result;
			}
		}

		public override string ToString()
		{
			return ToString(CanAccessSensitiveInfo(), includeBaseInfo: true);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			base.GetObjectData(info, context);
			info.AddValue("Action", m_action, typeof(SecurityAction));
			info.AddValue("FirstPermissionThatFailed", m_permissionThatFailed, typeof(string));
			info.AddValue("Demanded", m_demanded, typeof(string));
			info.AddValue("GrantedSet", m_granted, typeof(string));
			info.AddValue("RefusedSet", m_refused, typeof(string));
			info.AddValue("Denied", m_denied, typeof(string));
			info.AddValue("PermitOnly", m_permitOnly, typeof(string));
			info.AddValue("Assembly", m_assemblyName, typeof(AssemblyName));
			info.AddValue("Method", m_serializedMethodInfo, typeof(byte[]));
			info.AddValue("Method_String", m_strMethodInfo, typeof(string));
			info.AddValue("Zone", m_zone, typeof(SecurityZone));
			info.AddValue("Url", m_url, typeof(string));
		}
	}
}
