using System;
using System.ComponentModel;

namespace CSLib.Utility
{
	public class CTypeDescriptionProvider : TypeDescriptionProvider
	{
		private TypeDescriptionProvider ᜀ;

		public CTypeDescriptionProvider()
		{
		}

		public CTypeDescriptionProvider(TypeDescriptionProvider parent)
			: base(parent)
		{
			ᜀ = parent;
		}

		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
		{
			//Discarded unreachable code: IL_002f
			int num = 1;
			ICustomTypeDescriptor typeDescriptor = default(ICustomTypeDescriptor);
			while (true)
			{
				switch (num)
				{
				default:
					if (ᜀ == null)
					{
						if (true)
						{
						}
						num = 3;
						break;
					}
					goto case 2;
				case 0:
					return new ᜣ(typeDescriptor, instance);
				case 2:
					typeDescriptor = ᜀ.GetTypeDescriptor(objectType, instance);
					num = 4;
					break;
				case 4:
					if (instance != null)
					{
						num = 0;
						break;
					}
					return typeDescriptor;
				case 3:
					TypeDescriptor.RemoveProvider(this, objectType);
					ᜀ = TypeDescriptor.GetProvider(objectType);
					TypeDescriptor.AddProvider(this, objectType);
					num = 2;
					break;
				}
			}
		}
	}
}
