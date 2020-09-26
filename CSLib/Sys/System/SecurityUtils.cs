using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace System
{
	internal static class SecurityUtils
	{
		private static bool HasReflectionPermission
		{
			get
			{
				try
				{
					new ReflectionPermission(PermissionState.Unrestricted).Demand();
					return true;
				}
				catch (SecurityException)
				{
				}
				return false;
			}
		}

		internal static object SecureCreateInstance(Type type)
		{
			return SecureCreateInstance(type, null);
		}

		internal static object SecureCreateInstance(Type type, object[] args)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (type.Assembly == typeof(SecurityUtils).Assembly && !type.IsPublic && !type.IsNestedPublic)
			{
				new ReflectionPermission(PermissionState.Unrestricted).Demand();
			}
			return Activator.CreateInstance(type, args);
		}

		internal static object SecureCreateInstance(Type type, object[] args, bool allowNonPublic)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;
			if (type.Assembly == typeof(SecurityUtils).Assembly)
			{
				if (!type.IsPublic && !type.IsNestedPublic)
				{
					new ReflectionPermission(PermissionState.Unrestricted).Demand();
				}
				else if (allowNonPublic && !HasReflectionPermission)
				{
					allowNonPublic = false;
				}
			}
			if (allowNonPublic)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			return Activator.CreateInstance(type, bindingFlags, null, args, null);
		}

		internal static object SecureConstructorInvoke(Type type, Type[] argTypes, object[] args, bool allowNonPublic)
		{
			return SecureConstructorInvoke(type, argTypes, args, allowNonPublic, BindingFlags.Default);
		}

		internal static object SecureConstructorInvoke(Type type, Type[] argTypes, object[] args, bool allowNonPublic, BindingFlags extraFlags)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | extraFlags;
			if (type.Assembly == typeof(SecurityUtils).Assembly)
			{
				if (!type.IsPublic && !type.IsNestedPublic)
				{
					new ReflectionPermission(PermissionState.Unrestricted).Demand();
				}
				else if (allowNonPublic && !HasReflectionPermission)
				{
					allowNonPublic = false;
				}
			}
			if (allowNonPublic)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			return type.GetConstructor(bindingFlags, null, argTypes, null)?.Invoke(args);
		}
	}
}
