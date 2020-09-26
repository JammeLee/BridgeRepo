using System;
using System.ComponentModel;
using System.Reflection;

namespace CSLib.Utility
{
	public class CPropertyDescriptor : PropertyDescriptor
	{
		private object ᜀ;

		private object ᜁ;

		private object ᜂ;

		private string ᜃ;

		private Type ᜄ = Type.Missing.GetType();

		private AttributeCollection ᜅ = new AttributeCollection();

		public object Owner => ᜀ;

		public object Tag => ᜂ;

		public override Type ComponentType => ᜀ.GetType();

		public override bool IsReadOnly => false;

		public override Type PropertyType => ᜄ;

		public CPropertyDescriptor(object owner, object value, string name, Type type, object tag, params Attribute[] attributes)
			: base(name, attributes)
		{
			ᜀ = owner;
			ᜁ = value;
			ᜂ = tag;
			ᜃ = name;
			ᜄ = type;
		}

		public override object GetValue(object component)
		{
			return ᜁ;
		}

		public override void SetValue(object component, object value)
		{
			//Discarded unreachable code: IL_0017
			while (true)
			{
				if (true)
				{
				}
				ᜁ = value;
				PropertyInfo property = component.GetType().GetProperty(ᜃ);
				int num = 1;
				while (true)
				{
					switch (num)
					{
					case 1:
						if (property != null)
						{
							num = 0;
							continue;
						}
						return;
					case 0:
						property.SetValue(component, value, null);
						num = 2;
						continue;
					case 2:
						return;
					}
					break;
				}
			}
		}

		public override void ResetValue(object component)
		{
		}

		public override bool CanResetValue(object component)
		{
			return false;
		}

		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}
	}
}
