namespace System.Net.NetworkInformation
{
	public class PingOptions
	{
		private const int DontFragmentFlag = 2;

		private int ttl = 128;

		private bool dontFragment;

		public int Ttl
		{
			get
			{
				return ttl;
			}
			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				ttl = value;
			}
		}

		public bool DontFragment
		{
			get
			{
				return dontFragment;
			}
			set
			{
				dontFragment = value;
			}
		}

		internal PingOptions(IPOptions options)
		{
			ttl = options.ttl;
			dontFragment = (((options.flags & 2) > 0) ? true : false);
		}

		public PingOptions(int ttl, bool dontFragment)
		{
			if (ttl <= 0)
			{
				throw new ArgumentOutOfRangeException("ttl");
			}
			this.ttl = ttl;
			this.dontFragment = dontFragment;
		}

		public PingOptions()
		{
		}
	}
}
