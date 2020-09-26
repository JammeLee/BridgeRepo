using System.Security.Principal;

namespace System.Security.AccessControl
{
	public abstract class AccessRule : AuthorizationRule
	{
		private readonly AccessControlType _type;

		public AccessControlType AccessControlType => _type;

		protected AccessRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
			: base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags)
		{
			if (type != 0 && type != AccessControlType.Deny)
			{
				throw new ArgumentOutOfRangeException("type", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
			}
			if ((inheritanceFlags < InheritanceFlags.None) || inheritanceFlags > (InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit))
			{
				throw new ArgumentOutOfRangeException("inheritanceFlags", Environment.GetResourceString("Argument_InvalidEnumValue", inheritanceFlags, "InheritanceFlags"));
			}
			if ((propagationFlags < PropagationFlags.None) || propagationFlags > (PropagationFlags.NoPropagateInherit | PropagationFlags.InheritOnly))
			{
				throw new ArgumentOutOfRangeException("propagationFlags", Environment.GetResourceString("Argument_InvalidEnumValue", inheritanceFlags, "PropagationFlags"));
			}
			_type = type;
		}
	}
}
