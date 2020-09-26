namespace System.Net
{
	internal enum IgnoreCertProblem
	{
		not_time_valid = 1,
		ctl_not_time_valid = 2,
		not_time_nested = 4,
		invalid_basic_constraints = 8,
		all_not_time_valid = 7,
		allow_unknown_ca = 0x10,
		wrong_usage = 0x20,
		invalid_name = 0x40,
		invalid_policy = 0x80,
		end_rev_unknown = 0x100,
		ctl_signer_rev_unknown = 0x200,
		ca_rev_unknown = 0x400,
		root_rev_unknown = 0x800,
		all_rev_unknown = 3840,
		none = 0xFFF
	}
}
