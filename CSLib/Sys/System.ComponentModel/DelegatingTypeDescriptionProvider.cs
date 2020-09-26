using System.Collections;
using System.Security.Permissions;

namespace System.ComponentModel
{
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	internal sealed class DelegatingTypeDescriptionProvider : TypeDescriptionProvider
	{
		private Type _type;

		private TypeDescriptionProvider Provider => TypeDescriptor.GetProviderRecursive(_type);

		internal DelegatingTypeDescriptionProvider(Type type)
		{
			_type = type;
		}

		public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
		{
			return Provider.CreateInstance(provider, objectType, argTypes, args);
		}

		public override IDictionary GetCache(object instance)
		{
			return Provider.GetCache(instance);
		}

		public override string GetFullComponentName(object component)
		{
			return Provider.GetFullComponentName(component);
		}

		public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
		{
			return Provider.GetExtendedTypeDescriptor(instance);
		}

		public override Type GetReflectionType(Type objectType, object instance)
		{
			return Provider.GetReflectionType(objectType, instance);
		}

		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
		{
			return Provider.GetTypeDescriptor(objectType, instance);
		}
	}
}
