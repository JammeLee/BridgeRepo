using System;
using System.Runtime.CompilerServices;
using CSLib.Utility;

namespace CSLib.Framework
{
	public abstract class AECSComponent
	{
		[CompilerGenerated]
		private string ᜀ;

		[CompilerGenerated]
		private readonly string ᜁ;

		[CompilerGenerated]
		private readonly Type ᜂ;

		public bool IsValied = true;

		private CECSEntity ᜃ;

		public string Name
		{
			[CompilerGenerated]
			get
			{
				return ᜀ;
			}
			[CompilerGenerated]
			private set
			{
				ᜀ = value;
			}
		}

		private string FullName
		{
			[CompilerGenerated]
			get
			{
				return ᜁ;
			}
		}

		private Type type
		{
			[CompilerGenerated]
			get
			{
				return ᜂ;
			}
		}

		public CECSEntity Entity
		{
			get
			{
				return ᜃ;
			}
			set
			{
				//Discarded unreachable code: IL_0023
				int num = 0;
				while (true)
				{
					switch (num)
					{
					default:
						if (true)
						{
						}
						if (ᜃ != null)
						{
							num = 1;
							continue;
						}
						break;
					case 2:
						return;
					case 1:
						num = 3;
						continue;
					case 3:
						if (value != null)
						{
							num = 2;
							continue;
						}
						break;
					}
					break;
				}
				ᜃ = value;
				_BindingEntity();
			}
		}

		public AECSComponent()
		{
			ᜂ = GetType();
			Name = type.Name;
			ᜁ = type.FullName;
		}

		public AECSComponent(string strName)
		{
			Name = strName;
		}

		public AECSComponent GetComponent(string componentType)
		{
			return ᜃ.GetComponent(componentType);
		}

		protected virtual void _BindingEntity()
		{
		}

		public virtual void Update(float deltaTime)
		{
		}

		public void Clear()
		{
			//Discarded unreachable code: IL_0014
			int a_ = 3;
			if (!IsValied)
			{
				if (true)
				{
				}
				CDebugOut.LogError(CMessageLabel.b("\udb84ᕎ\u1a8c\udd70崝伟伡吣䤥䘧伩䈫娭㇐䤱г䬵", a_), FullName);
			}
			else
			{
				Destroy();
				IsValied = false;
				ᜃ = null;
			}
		}

		protected abstract void Destroy();
	}
}
