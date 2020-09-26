using System.Collections;
using System.Reflection;
using System.Security.Permissions;

namespace System
{
	internal class AppDomainInitializerInfo
	{
		internal class ItemInfo
		{
			public string TargetTypeAssembly;

			public string TargetTypeName;

			public string MethodName;
		}

		internal ItemInfo[] Info;

		internal AppDomainInitializerInfo(AppDomainInitializer init)
		{
			Info = null;
			if (init == null)
			{
				return;
			}
			ArrayList arrayList = new ArrayList();
			ArrayList arrayList2 = new ArrayList
			{
				init
			};
			int num = 0;
			while (arrayList2.Count > num)
			{
				AppDomainInitializer appDomainInitializer = (AppDomainInitializer)arrayList2[num++];
				Delegate[] invocationList = appDomainInitializer.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					if (!invocationList[i].Method.IsStatic)
					{
						if (invocationList[i].Target != null)
						{
							AppDomainInitializer appDomainInitializer2 = invocationList[i].Target as AppDomainInitializer;
							if (appDomainInitializer2 == null)
							{
								throw new ArgumentException(Environment.GetResourceString("Arg_MustBeStatic"), invocationList[i].Method.ReflectedType.FullName + "::" + invocationList[i].Method.Name);
							}
							arrayList2.Add(appDomainInitializer2);
						}
					}
					else
					{
						arrayList.Add(new ItemInfo
						{
							TargetTypeAssembly = invocationList[i].Method.ReflectedType.Module.Assembly.FullName,
							TargetTypeName = invocationList[i].Method.ReflectedType.FullName,
							MethodName = invocationList[i].Method.Name
						});
					}
				}
			}
			Info = (ItemInfo[])arrayList.ToArray(typeof(ItemInfo));
		}

		internal AppDomainInitializer Unwrap()
		{
			if (Info == null)
			{
				return null;
			}
			AppDomainInitializer appDomainInitializer = null;
			new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Assert();
			for (int i = 0; i < Info.Length; i++)
			{
				Assembly assembly = Assembly.Load(Info[i].TargetTypeAssembly);
				AppDomainInitializer appDomainInitializer2 = (AppDomainInitializer)Delegate.CreateDelegate(typeof(AppDomainInitializer), assembly.GetType(Info[i].TargetTypeName), Info[i].MethodName);
				appDomainInitializer = ((appDomainInitializer != null) ? ((AppDomainInitializer)Delegate.Combine(appDomainInitializer, appDomainInitializer2)) : appDomainInitializer2);
			}
			return appDomainInitializer;
		}
	}
}
