using System.Collections;
using System.Security.Permissions;

namespace System.ComponentModel
{
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	internal sealed class ExtendedPropertyDescriptor : PropertyDescriptor
	{
		private readonly ReflectPropertyDescriptor extenderInfo;

		private readonly IExtenderProvider provider;

		public override Type ComponentType => extenderInfo.ComponentType;

		public override bool IsReadOnly => Attributes[typeof(ReadOnlyAttribute)].Equals(ReadOnlyAttribute.Yes);

		public override Type PropertyType => extenderInfo.ExtenderGetType(provider);

		public override string DisplayName
		{
			get
			{
				string text = base.DisplayName;
				DisplayNameAttribute displayNameAttribute = Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
				if (displayNameAttribute == null || displayNameAttribute.IsDefaultAttribute())
				{
					ISite site = MemberDescriptor.GetSite(provider);
					if (site != null)
					{
						string name = site.Name;
						if (name != null && name.Length > 0)
						{
							text = SR.GetString("MetaExtenderName", text, name);
						}
					}
				}
				return text;
			}
		}

		public ExtendedPropertyDescriptor(ReflectPropertyDescriptor extenderInfo, Type receiverType, IExtenderProvider provider, Attribute[] attributes)
			: base(extenderInfo, attributes)
		{
			ArrayList arrayList = new ArrayList(AttributeArray)
			{
				ExtenderProvidedPropertyAttribute.Create(extenderInfo, receiverType, provider)
			};
			if (extenderInfo.IsReadOnly)
			{
				arrayList.Add(ReadOnlyAttribute.Yes);
			}
			Attribute[] array = new Attribute[arrayList.Count];
			arrayList.CopyTo(array, 0);
			AttributeArray = array;
			this.extenderInfo = extenderInfo;
			this.provider = provider;
		}

		public ExtendedPropertyDescriptor(PropertyDescriptor extender, Attribute[] attributes)
			: base(extender, attributes)
		{
			ExtenderProvidedPropertyAttribute extenderProvidedPropertyAttribute = extender.Attributes[typeof(ExtenderProvidedPropertyAttribute)] as ExtenderProvidedPropertyAttribute;
			ReflectPropertyDescriptor reflectPropertyDescriptor = (extenderInfo = extenderProvidedPropertyAttribute.ExtenderProperty as ReflectPropertyDescriptor);
			provider = extenderProvidedPropertyAttribute.Provider;
		}

		public override bool CanResetValue(object comp)
		{
			return extenderInfo.ExtenderCanResetValue(provider, comp);
		}

		public override object GetValue(object comp)
		{
			return extenderInfo.ExtenderGetValue(provider, comp);
		}

		public override void ResetValue(object comp)
		{
			extenderInfo.ExtenderResetValue(provider, comp, this);
		}

		public override void SetValue(object component, object value)
		{
			extenderInfo.ExtenderSetValue(provider, component, value, this);
		}

		public override bool ShouldSerializeValue(object comp)
		{
			return extenderInfo.ExtenderShouldSerializeValue(provider, comp);
		}
	}
}
