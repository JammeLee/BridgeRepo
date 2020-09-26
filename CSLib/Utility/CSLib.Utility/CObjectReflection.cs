using System;
using System.Reflection;

namespace CSLib.Utility
{
	public class CObjectReflection : CSingleton<CObjectReflection>
	{
		public static string GetClassName(object obj)
		{
			//Discarded unreachable code: IL_0023
			int num = 0;
			Type type = default(Type);
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (obj != null)
					{
						num = 3;
						continue;
					}
					break;
				case 1:
					return type.FullName;
				case 3:
					type = obj.GetType();
					num = 2;
					continue;
				case 2:
					if (type != null)
					{
						num = 1;
						continue;
					}
					break;
				}
				break;
			}
			return "";
		}

		public static string GetNamespaceName(object obj)
		{
			//Discarded unreachable code: IL_0023
			int num = 1;
			Type type = default(Type);
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (obj != null)
					{
						num = 0;
						continue;
					}
					break;
				case 2:
					return type.Namespace;
				case 0:
					type = obj.GetType();
					num = 3;
					continue;
				case 3:
					if (type != null)
					{
						num = 2;
						continue;
					}
					break;
				}
				break;
			}
			return "";
		}

		public static void SetPropertyValue(object obj, string propertyName, object propertyValue)
		{
			//Discarded unreachable code: IL_007b
			int num = 5;
			PropertyInfo property = default(PropertyInfo);
			Type type = default(Type);
			while (true)
			{
				switch (num)
				{
				default:
					if (obj != null)
					{
						num = 1;
						break;
					}
					return;
				case 2:
					property.SetValue(obj, propertyValue, null);
					num = 0;
					break;
				case 0:
					return;
				case 6:
					property = type.GetProperty(propertyName);
					num = 3;
					break;
				case 3:
					if (property != null)
					{
						num = 2;
						break;
					}
					return;
				case 1:
					type = obj.GetType();
					num = 4;
					break;
				case 4:
					if (true)
					{
					}
					if (type != null)
					{
						num = 6;
						break;
					}
					return;
				}
			}
		}
	}
}
