using System.Collections;

namespace System.Reflection
{
	internal static class Associates
	{
		[Flags]
		internal enum Attributes
		{
			ComposedOfAllVirtualMethods = 0x1,
			ComposedOfAllPrivateMethods = 0x2,
			ComposedOfNoPublicMembers = 0x4,
			ComposedOfNoStaticMembers = 0x8
		}

		internal static bool IncludeAccessor(MethodInfo associate, bool nonPublic)
		{
			if (associate == null)
			{
				return false;
			}
			if (nonPublic)
			{
				return true;
			}
			if (associate.IsPublic)
			{
				return true;
			}
			return false;
		}

		internal static RuntimeMethodInfo AssignAssociates(int tkMethod, RuntimeTypeHandle declaredTypeHandle, RuntimeTypeHandle reflectedTypeHandle)
		{
			if (MetadataToken.IsNullToken(tkMethod))
			{
				return null;
			}
			bool flag = !declaredTypeHandle.Equals(reflectedTypeHandle);
			RuntimeMethodHandle methodHandle = declaredTypeHandle.GetModuleHandle().ResolveMethodHandle(tkMethod, declaredTypeHandle.GetInstantiation(), new RuntimeTypeHandle[0]);
			MethodAttributes attributes = methodHandle.GetAttributes();
			bool flag2 = (attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private;
			bool flag3 = (attributes & MethodAttributes.Virtual) != 0;
			if (flag)
			{
				if (flag2)
				{
					return null;
				}
				if (flag3 && (declaredTypeHandle.GetAttributes() & TypeAttributes.ClassSemanticsMask) == 0)
				{
					int slot = methodHandle.GetSlot();
					methodHandle = reflectedTypeHandle.GetMethodAt(slot);
				}
			}
			MethodAttributes methodAttributes = attributes & MethodAttributes.MemberAccessMask;
			RuntimeMethodInfo runtimeMethodInfo = RuntimeType.GetMethodBase(reflectedTypeHandle, methodHandle) as RuntimeMethodInfo;
			if (runtimeMethodInfo == null)
			{
				runtimeMethodInfo = reflectedTypeHandle.GetRuntimeType().Module.ResolveMethod(tkMethod, null, null) as RuntimeMethodInfo;
			}
			return runtimeMethodInfo;
		}

		internal unsafe static void AssignAssociates(AssociateRecord* associates, int cAssociates, RuntimeTypeHandle declaringTypeHandle, RuntimeTypeHandle reflectedTypeHandle, out RuntimeMethodInfo addOn, out RuntimeMethodInfo removeOn, out RuntimeMethodInfo fireOn, out RuntimeMethodInfo getter, out RuntimeMethodInfo setter, out MethodInfo[] other, out bool composedOfAllPrivateMethods, out BindingFlags bindingFlags)
		{
			addOn = (removeOn = (fireOn = (getter = (setter = null))));
			other = null;
			Attributes attributes = Attributes.ComposedOfAllVirtualMethods | Attributes.ComposedOfAllPrivateMethods | Attributes.ComposedOfNoPublicMembers | Attributes.ComposedOfNoStaticMembers;
			while (reflectedTypeHandle.IsGenericVariable())
			{
				reflectedTypeHandle = reflectedTypeHandle.GetRuntimeType().BaseType.GetTypeHandleInternal();
			}
			bool isInherited = !declaringTypeHandle.Equals(reflectedTypeHandle);
			ArrayList arrayList = new ArrayList();
			for (int i = 0; i < cAssociates; i++)
			{
				RuntimeMethodInfo runtimeMethodInfo = AssignAssociates(associates[i].MethodDefToken, declaringTypeHandle, reflectedTypeHandle);
				if (runtimeMethodInfo != null)
				{
					MethodAttributes attributes2 = runtimeMethodInfo.Attributes;
					bool flag = (attributes2 & MethodAttributes.MemberAccessMask) == MethodAttributes.Private;
					bool flag2 = (attributes2 & MethodAttributes.Virtual) != 0;
					MethodAttributes methodAttributes = attributes2 & MethodAttributes.MemberAccessMask;
					bool flag3 = methodAttributes == MethodAttributes.Public;
					bool flag4 = (attributes2 & MethodAttributes.Static) != 0;
					if (flag3)
					{
						attributes &= ~Attributes.ComposedOfNoPublicMembers;
						attributes &= ~Attributes.ComposedOfAllPrivateMethods;
					}
					else if (!flag)
					{
						attributes &= ~Attributes.ComposedOfAllPrivateMethods;
					}
					if (flag4)
					{
						attributes &= ~Attributes.ComposedOfNoStaticMembers;
					}
					if (!flag2)
					{
						attributes &= ~Attributes.ComposedOfAllVirtualMethods;
					}
					if (associates[i].Semantics == MethodSemanticsAttributes.Setter)
					{
						setter = runtimeMethodInfo;
					}
					else if (associates[i].Semantics == MethodSemanticsAttributes.Getter)
					{
						getter = runtimeMethodInfo;
					}
					else if (associates[i].Semantics == MethodSemanticsAttributes.Fire)
					{
						fireOn = runtimeMethodInfo;
					}
					else if (associates[i].Semantics == MethodSemanticsAttributes.AddOn)
					{
						addOn = runtimeMethodInfo;
					}
					else if (associates[i].Semantics == MethodSemanticsAttributes.RemoveOn)
					{
						removeOn = runtimeMethodInfo;
					}
					else
					{
						arrayList.Add(runtimeMethodInfo);
					}
				}
			}
			bool isPublic = (attributes & Attributes.ComposedOfNoPublicMembers) == 0;
			bool isStatic = (attributes & Attributes.ComposedOfNoStaticMembers) == 0;
			bindingFlags = RuntimeType.FilterPreCalculate(isPublic, isInherited, isStatic);
			composedOfAllPrivateMethods = (attributes & Attributes.ComposedOfAllPrivateMethods) != 0;
			other = (MethodInfo[])arrayList.ToArray(typeof(MethodInfo));
		}
	}
}
