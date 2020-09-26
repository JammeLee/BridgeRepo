using System.Collections.Generic;
using CSLib.Utility;

namespace CSLib.Framework
{
	public class CMsgExecFuncFactory
	{
		private Dictionary<uint, DMsgExecFuncByTypeID> ᜀ = new Dictionary<uint, DMsgExecFuncByTypeID>();

		private Dictionary<uint, DMsgExecFuncByType> ᜁ = new Dictionary<uint, DMsgExecFuncByType>();

		private DMsgExecFuncByTypeID ᜂ;

		private DMsgExecFuncByType ᜃ;

		public CMsgExecFuncFactory()
		{
			ᜃ = _CreateMsgExecFun;
			ᜂ = _CreateMsgExecFun;
		}

		public bool Create(ushort type, ushort id, ref DMsgExecFunc msgExecut)
		{
			//Discarded unreachable code: IL_0043
			while (true)
			{
				uint key = CBitHelper.MergeUInt16(type, 16) + id;
				DMsgExecFuncByTypeID dMsgExecFuncByTypeID = ᜀ[key];
				int num = 3;
				while (true)
				{
					switch (num)
					{
					case 3:
						if (true)
						{
						}
						if (dMsgExecFuncByTypeID != null)
						{
							num = 2;
							continue;
						}
						goto IL_0055;
					case 5:
						if (ᜂ == null)
						{
							num = 1;
							continue;
						}
						return ᜂ(type, id, ref msgExecut);
					case 0:
						return true;
					case 2:
						num = 4;
						continue;
					case 4:
						if (dMsgExecFuncByTypeID(type, id, ref msgExecut))
						{
							num = 0;
							continue;
						}
						goto IL_0055;
					case 1:
						{
							return false;
						}
						IL_0055:
						num = 5;
						continue;
					}
					break;
				}
			}
		}

		public bool Create(ushort type, ref DMsgExecFunc msgExecut)
		{
			//Discarded unreachable code: IL_003b
			while (true)
			{
				DMsgExecFuncByType dMsgExecFuncByType = ᜁ[type];
				int num = 3;
				while (true)
				{
					switch (num)
					{
					case 3:
						if (dMsgExecFuncByType != null)
						{
							if (true)
							{
							}
							num = 4;
							continue;
						}
						goto IL_004a;
					case 1:
						if (ᜃ == null)
						{
							num = 0;
							continue;
						}
						return ᜃ(type, ref msgExecut);
					case 5:
						return true;
					case 4:
						num = 2;
						continue;
					case 2:
						if (dMsgExecFuncByType(type, ref msgExecut))
						{
							num = 5;
							continue;
						}
						goto IL_004a;
					case 0:
						{
							return false;
						}
						IL_004a:
						num = 1;
						continue;
					}
					break;
				}
			}
		}

		public bool SetMsgExecFunc(ushort type, ushort id, DMsgExecFuncByTypeID func)
		{
			//Discarded unreachable code: IL_0020
			uint key = CBitHelper.MergeUInt16(type, 16) + id;
			if (ᜀ.ContainsKey(key))
			{
				return false;
			}
			if (true)
			{
			}
			ᜀ[key] = func;
			return true;
		}

		public bool SetMsgExecFunc(ushort type, DMsgExecFuncByType func)
		{
			//Discarded unreachable code: IL_0015
			if (ᜁ.ContainsKey(type))
			{
				return false;
			}
			if (true)
			{
			}
			ᜁ[type] = func;
			return true;
		}

		public void SetMsgExecFunc(DMsgExecFuncByTypeID func)
		{
			ᜂ = func;
		}

		public void SetMsgExecFunc(DMsgExecFuncByType func)
		{
			ᜃ = func;
		}

		protected virtual bool _CreateMsgExecFun(ushort type, ushort id, ref DMsgExecFunc msgExecut)
		{
			return false;
		}

		protected virtual bool _CreateMsgExecFun(ushort type, ref DMsgExecFunc msgExecut)
		{
			return false;
		}
	}
}
