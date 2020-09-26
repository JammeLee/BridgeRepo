using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Security.Permissions;
using System.Threading;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public abstract class MarshalByRefObject
	{
		private object __identity;

		private object Identity
		{
			get
			{
				return __identity;
			}
			set
			{
				__identity = value;
			}
		}

		internal IntPtr GetComIUnknown(bool fIsBeingMarshalled)
		{
			if (RemotingServices.IsTransparentProxy(this))
			{
				return RemotingServices.GetRealProxy(this).GetCOMIUnknown(fIsBeingMarshalled);
			}
			return Marshal.GetIUnknownForObject(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern IntPtr GetComIUnknown(MarshalByRefObject o);

		internal bool IsInstanceOfType(Type T)
		{
			return T.IsInstanceOfType(this);
		}

		internal object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
		{
			Type type = GetType();
			if (!type.IsCOMObject)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_InvokeMember"));
			}
			return type.InvokeMember(name, invokeAttr, binder, this, args, modifiers, culture, namedParameters);
		}

		protected MarshalByRefObject MemberwiseClone(bool cloneIdentity)
		{
			MarshalByRefObject marshalByRefObject = (MarshalByRefObject)MemberwiseClone();
			if (!cloneIdentity)
			{
				marshalByRefObject.Identity = null;
			}
			return marshalByRefObject;
		}

		internal static Identity GetIdentity(MarshalByRefObject obj, out bool fServer)
		{
			fServer = true;
			Identity result = null;
			if (obj != null)
			{
				if (!RemotingServices.IsTransparentProxy(obj))
				{
					result = (Identity)obj.Identity;
				}
				else
				{
					fServer = false;
					result = RemotingServices.GetRealProxy(obj).IdentityObject;
				}
			}
			return result;
		}

		internal static Identity GetIdentity(MarshalByRefObject obj)
		{
			bool fServer;
			return GetIdentity(obj, out fServer);
		}

		internal ServerIdentity __RaceSetServerIdentity(ServerIdentity id)
		{
			if (__identity == null)
			{
				if (!id.IsContextBound)
				{
					id.RaceSetTransparentProxy(this);
				}
				Interlocked.CompareExchange(ref __identity, id, null);
			}
			return (ServerIdentity)__identity;
		}

		internal void __ResetServerIdentity()
		{
			__identity = null;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public object GetLifetimeService()
		{
			return LifetimeServices.GetLease(this);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public virtual object InitializeLifetimeService()
		{
			return LifetimeServices.GetLeaseInitial(this);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public virtual ObjRef CreateObjRef(Type requestedType)
		{
			if (__identity == null)
			{
				throw new RemotingException(Environment.GetResourceString("Remoting_NoIdentityEntry"));
			}
			return new ObjRef(this, requestedType);
		}

		internal bool CanCastToXmlType(string xmlTypeName, string xmlTypeNamespace)
		{
			Type type = SoapServices.GetInteropTypeFromXmlType(xmlTypeName, xmlTypeNamespace);
			if (type == null)
			{
				if (!SoapServices.DecodeXmlNamespaceForClrTypeNamespace(xmlTypeNamespace, out var typeNamespace, out var assemblyName))
				{
					return false;
				}
				string name = ((typeNamespace == null || typeNamespace.Length <= 0) ? xmlTypeName : (typeNamespace + "." + xmlTypeName));
				try
				{
					Assembly assembly = Assembly.Load(assemblyName);
					type = assembly.GetType(name, throwOnError: false, ignoreCase: false);
				}
				catch
				{
					return false;
				}
			}
			return type?.IsAssignableFrom(GetType()) ?? false;
		}

		internal static bool CanCastToXmlTypeHelper(Type castType, MarshalByRefObject o)
		{
			if (castType == null)
			{
				throw new ArgumentNullException("castType");
			}
			if (!castType.IsInterface && !castType.IsMarshalByRef)
			{
				return false;
			}
			string xmlType = null;
			string xmlTypeNamespace = null;
			if (!SoapServices.GetXmlTypeForInteropType(castType, out xmlType, out xmlTypeNamespace))
			{
				xmlType = castType.Name;
				xmlTypeNamespace = SoapServices.CodeXmlNamespaceForClrTypeNamespace(castType.Namespace, castType.Module.Assembly.nGetSimpleName());
			}
			return o.CanCastToXmlType(xmlType, xmlTypeNamespace);
		}
	}
}
