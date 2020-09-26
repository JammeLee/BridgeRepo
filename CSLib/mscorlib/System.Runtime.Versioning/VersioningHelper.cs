using Microsoft.Win32;

namespace System.Runtime.Versioning
{
	public static class VersioningHelper
	{
		private static ResourceScope ResTypeMask = ResourceScope.Machine | ResourceScope.Process | ResourceScope.AppDomain | ResourceScope.Library;

		private static ResourceScope VisibilityMask = ResourceScope.Private | ResourceScope.Assembly;

		public static string MakeVersionSafeName(string name, ResourceScope from, ResourceScope to)
		{
			return MakeVersionSafeName(name, from, to, null);
		}

		public static string MakeVersionSafeName(string name, ResourceScope from, ResourceScope to, Type type)
		{
			ResourceScope resourceScope = from & ResTypeMask;
			ResourceScope resourceScope2 = to & ResTypeMask;
			if (resourceScope > resourceScope2)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ResourceScopeWrongDirection", resourceScope, resourceScope2), "from");
			}
			SxSRequirements requirements = GetRequirements(to, from);
			if ((requirements & (SxSRequirements.AssemblyName | SxSRequirements.TypeName)) != 0 && type == null)
			{
				throw new ArgumentNullException("type", Environment.GetResourceString("ArgumentNull_TypeRequiredByResourceScope"));
			}
			string text = "";
			if ((requirements & SxSRequirements.ProcessID) != 0)
			{
				text = text + "_" + Win32Native.GetCurrentProcessId();
			}
			if ((requirements & SxSRequirements.AppDomainID) != 0)
			{
				text = text + "_" + AppDomain.CurrentDomain.GetAppDomainId();
			}
			if ((requirements & SxSRequirements.TypeName) != 0)
			{
				text = text + "_" + type.Name;
			}
			if ((requirements & SxSRequirements.AssemblyName) != 0)
			{
				text = text + "_" + type.Assembly.FullName;
			}
			return name + text;
		}

		private static SxSRequirements GetRequirements(ResourceScope consumeAsScope, ResourceScope calleeScope)
		{
			SxSRequirements sxSRequirements = SxSRequirements.None;
			switch (calleeScope & ResTypeMask)
			{
			case ResourceScope.Machine:
				switch (consumeAsScope & ResTypeMask)
				{
				case ResourceScope.Process:
					sxSRequirements |= SxSRequirements.ProcessID;
					break;
				case ResourceScope.AppDomain:
					sxSRequirements |= SxSRequirements.AppDomainID | SxSRequirements.ProcessID;
					break;
				default:
					throw new ArgumentException(Environment.GetResourceString("Argument_BadResourceScopeTypeBits", consumeAsScope), "consumeAsScope");
				case ResourceScope.Machine:
					break;
				}
				break;
			case ResourceScope.Process:
				if ((consumeAsScope & ResourceScope.AppDomain) != 0)
				{
					sxSRequirements |= SxSRequirements.AppDomainID;
				}
				break;
			default:
				throw new ArgumentException(Environment.GetResourceString("Argument_BadResourceScopeTypeBits", calleeScope), "calleeScope");
			case ResourceScope.AppDomain:
				break;
			}
			switch (calleeScope & VisibilityMask)
			{
			case ResourceScope.None:
				switch (consumeAsScope & VisibilityMask)
				{
				case ResourceScope.Assembly:
					sxSRequirements |= SxSRequirements.AssemblyName;
					break;
				case ResourceScope.Private:
					sxSRequirements |= SxSRequirements.AssemblyName | SxSRequirements.TypeName;
					break;
				default:
					throw new ArgumentException(Environment.GetResourceString("Argument_BadResourceScopeVisibilityBits", consumeAsScope), "consumeAsScope");
				case ResourceScope.None:
					break;
				}
				break;
			case ResourceScope.Assembly:
				if ((consumeAsScope & ResourceScope.Private) != 0)
				{
					sxSRequirements |= SxSRequirements.TypeName;
				}
				break;
			default:
				throw new ArgumentException(Environment.GetResourceString("Argument_BadResourceScopeVisibilityBits", calleeScope), "calleeScope");
			case ResourceScope.Private:
				break;
			}
			return sxSRequirements;
		}
	}
}
