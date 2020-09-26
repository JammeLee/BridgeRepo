using System.Security.Principal;

namespace System.Security.AccessControl
{
	public abstract class AuditRule : AuthorizationRule
	{
		private readonly AuditFlags _flags;

		public AuditFlags AuditFlags => _flags;

		protected AuditRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags auditFlags)
			: base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags)
		{
			if (auditFlags == AuditFlags.None)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EnumAtLeastOneFlag"), "auditFlags");
			}
			if (((uint)auditFlags & 0xFFFFFFFCu) != 0)
			{
				throw new ArgumentOutOfRangeException("auditFlags", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
			}
			_flags = auditFlags;
		}
	}
}
