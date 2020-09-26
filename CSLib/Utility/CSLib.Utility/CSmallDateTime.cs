using System;

namespace CSLib.Utility
{
	public class CSmallDateTime : CBaseDateTime
	{
		public static DateTime Invalid
		{
			get
			{
				int a_ = 17;
				return Convert.ToDateTime(CSimpleThreadPool.b("籌癎慐捒硔晖瑘橚絜潞孠卢啤嵦奨孪", a_));
			}
		}

		public static DateTime MixValue
		{
			get
			{
				int a_ = 11;
				return Convert.ToDateTime(CSimpleThreadPool.b("癆灈筊経扎恐繒摔睖楘慚浜潞孠卢啤", a_));
			}
		}

		public static DateTime MaxValue
		{
			get
			{
				int a_ = 10;
				return Convert.ToDateTime(CSimpleThreadPool.b("瑅硇絉畋捍慏网敓癕桗恙汛湝婟剡呣", a_));
			}
		}
	}
}
