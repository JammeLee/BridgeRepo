using System.Security.Principal;

namespace System.Security.AccessControl
{
	public abstract class AuthorizationRule
	{
		private readonly IdentityReference _identity;

		private readonly int _accessMask;

		private readonly bool _isInherited;

		private readonly InheritanceFlags _inheritanceFlags;

		private readonly PropagationFlags _propagationFlags;

		public IdentityReference IdentityReference => _identity;

		protected internal int AccessMask => _accessMask;

		public bool IsInherited => _isInherited;

		public InheritanceFlags InheritanceFlags => _inheritanceFlags;

		public PropagationFlags PropagationFlags => _propagationFlags;

		protected internal AuthorizationRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
		{
			if (identity == null)
			{
				throw new ArgumentNullException("identity");
			}
			if (!identity.IsValidTargetType(typeof(SecurityIdentifier)))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeIdentityReferenceType"), "identity");
			}
			if (accessMask == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ArgumentZero"), "accessMask");
			}
			if ((inheritanceFlags < InheritanceFlags.None) || inheritanceFlags > (InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit))
			{
				throw new ArgumentOutOfRangeException("inheritanceFlags", Environment.GetResourceString("Argument_InvalidEnumValue", inheritanceFlags, "InheritanceFlags"));
			}
			if ((propagationFlags < PropagationFlags.None) || propagationFlags > (PropagationFlags.NoPropagateInherit | PropagationFlags.InheritOnly))
			{
				throw new ArgumentOutOfRangeException("propagationFlags", Environment.GetResourceString("Argument_InvalidEnumValue", inheritanceFlags, "PropagationFlags"));
			}
			_identity = identity;
			_accessMask = accessMask;
			_isInherited = isInherited;
			_inheritanceFlags = inheritanceFlags;
			if (inheritanceFlags != 0)
			{
				_propagationFlags = propagationFlags;
			}
			else
			{
				_propagationFlags = PropagationFlags.None;
			}
		}
	}
}
