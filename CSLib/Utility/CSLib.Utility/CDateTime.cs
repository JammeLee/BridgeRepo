using System;

namespace CSLib.Utility
{
	public class CDateTime : CBaseDateTime
	{
		public static DateTime Invalid
		{
			get
			{
				int a_ = 16;
				return Convert.ToDateTime(CSimpleThreadPool.b("絋祍敏慑祓杕畗歙籛湝婟剡呣履塧婩", a_));
			}
		}

		public static DateTime MixValue
		{
			get
			{
				int a_ = 1;
				return Convert.ToDateTime(CSimpleThreadPool.b("\u0c3c࠾瑀灂桄癆摈穊浌罎歐捒敔浖楘歚", a_));
			}
		}

		public static DateTime MaxValue
		{
			get
			{
				int a_ = 6;
				return Convert.ToDateTime(CSimpleThreadPool.b("筁絃罅煇杉絋籍絏慑敓癕桗恙汛湝婟剡呣", a_));
			}
		}
	}
}
