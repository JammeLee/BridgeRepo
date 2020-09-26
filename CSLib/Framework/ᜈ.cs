using CSLib.Framework;
using CSLib.Utility;

internal class ᜈ
{
	internal _1715 ᜀ;

	public ᜈ()
	{
		ᜀ = new _1715();
		CSingleton<_1734>.Instance.ᜁ(this);
	}

	public bool ᜁ(CMessage A_0)
	{
		//Discarded unreachable code: IL_0023
		int num = 2;
		while (true)
		{
			switch (num)
			{
			default:
				if (true)
				{
				}
				if (A_0 != null)
				{
					num = 0;
					continue;
				}
				break;
			case 3:
			{
				CMessageBlock cMessageBlock = new CMessageBlock();
				cMessageBlock.Msg = A_0;
				ᜀ.ᜃ(cMessageBlock);
				return true;
			}
			case 0:
				num = 1;
				continue;
			case 1:
				if (!CSingleton<_1734>.Instance.ᜅ())
				{
					num = 3;
					continue;
				}
				break;
			}
			break;
		}
		return false;
	}

	public bool ᜁ(CMessage A_0, CMessageLabel A_1)
	{
		//Discarded unreachable code: IL_0023
		int num = 0;
		while (true)
		{
			switch (num)
			{
			default:
				if (true)
				{
				}
				if (A_0 != null)
				{
					num = 1;
					continue;
				}
				break;
			case 3:
			{
				CMessageBlock cMessageBlock = new CMessageBlock();
				cMessageBlock.Msg = A_0;
				cMessageBlock.Label = A_1;
				ᜀ.ᜃ(cMessageBlock);
				return true;
			}
			case 1:
				num = 2;
				continue;
			case 2:
				if (!CSingleton<_1734>.Instance.ᜅ())
				{
					num = 3;
					continue;
				}
				break;
			}
			break;
		}
		return false;
	}

	public bool ᜁ()
	{
		return ᜀ.ᜋ();
	}

	public void ᜁ(bool A_0)
	{
		ᜀ.ᜃ(A_0);
	}

	internal _1715 ᜂ()
	{
		return ᜀ;
	}
}
