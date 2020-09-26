using System.Collections.Generic;
using CSLib.Utility;

namespace CSLib.Framework
{
	public class CFSMManager
	{
		private int m_ᜀ;

		private int m_ᜁ;

		private CDictionary<int, CFSMState> m_ᜂ = new CDictionary<int, CFSMState>();

		private object m_ᜃ;

		public object Owner
		{
			get
			{
				return this.m_ᜃ;
			}
			set
			{
				this.m_ᜃ = value;
			}
		}

		public int getPreviousStateID()
		{
			return this.m_ᜀ;
		}

		public int getCurrentStateID()
		{
			return this.m_ᜁ;
		}

		public void setCurrentStateID(int iStateID)
		{
			ᜃ(iStateID);
		}

		private void ᜃ(int A_0)
		{
			//Discarded unreachable code: IL_0062
			while (true)
			{
				CFSMState cFSMState = null;
				int num = 11;
				while (true)
				{
					switch (num)
					{
					case 11:
						if (this.m_ᜁ != 0)
						{
							num = 8;
							continue;
						}
						goto case 6;
					case 10:
						num = 3;
						continue;
					case 3:
						if (true)
						{
						}
						if (cFSMState.StateProcess != null)
						{
							num = 5;
							continue;
						}
						return;
					case 4:
						cFSMState.StateProcess.processOut();
						num = 6;
						continue;
					case 8:
						cFSMState = getState(this.m_ᜁ);
						num = 1;
						continue;
					case 1:
						if (cFSMState != null)
						{
							num = 9;
							continue;
						}
						goto case 6;
					case 5:
						cFSMState.StateProcess.processIn();
						num = 0;
						continue;
					case 0:
						return;
					case 9:
						num = 7;
						continue;
					case 7:
						if (cFSMState.StateProcess != null)
						{
							num = 4;
							continue;
						}
						goto case 6;
					case 6:
						this.m_ᜀ = this.m_ᜁ;
						this.m_ᜁ = A_0;
						cFSMState = getState(this.m_ᜁ);
						num = 2;
						continue;
					case 2:
						if (cFSMState != null)
						{
							num = 10;
							continue;
						}
						return;
					}
					break;
				}
			}
		}

		public CFSMState getState(int iStateID)
		{
			return ᜂ(iStateID);
		}

		private CFSMState ᜂ(int A_0)
		{
			return this.m_ᜂ.GetObject(A_0);
		}

		public void addState(CFSMState state)
		{
			ᜀ(state);
		}

		private void ᜀ(CFSMState A_0)
		{
			this.m_ᜂ.AddObject(A_0.StateID, A_0);
		}

		public void delState(int iStateID)
		{
			ᜁ(iStateID);
		}

		private void ᜁ(int A_0)
		{
			this.m_ᜂ.DelObject(A_0);
		}

		public int stateTransition(int iInputID)
		{
			return ᜀ(iInputID);
		}

		private int ᜀ(int A_0)
		{
			//Discarded unreachable code: IL_00a0
			int output = default(int);
			while (true)
			{
				CFSMState cFSMState = null;
				cFSMState = getState(this.m_ᜁ);
				int num = 7;
				while (true)
				{
					switch (num)
					{
					case 7:
						if (cFSMState == null)
						{
							num = 14;
							continue;
						}
						output = cFSMState.getOutput(A_0);
						num = 6;
						continue;
					case 11:
						this.m_ᜁ = output;
						cFSMState = getState(this.m_ᜁ);
						num = 9;
						continue;
					case 9:
						if (cFSMState != null)
						{
							num = 10;
							continue;
						}
						goto case 5;
					case 13:
						if (cFSMState.StateProcess != null)
						{
							num = 8;
							continue;
						}
						goto case 11;
					case 6:
						if (output != this.m_ᜁ)
						{
							if (true)
							{
							}
							this.m_ᜀ = this.m_ᜁ;
							num = 13;
						}
						else
						{
							num = 2;
						}
						continue;
					case 10:
						num = 1;
						continue;
					case 1:
						if (cFSMState.StateProcess != null)
						{
							num = 4;
							continue;
						}
						goto case 5;
					case 14:
						this.m_ᜁ = 0;
						return this.m_ᜁ;
					case 8:
						cFSMState.StateProcess.processOut();
						cFSMState.StateProcess.processInput(A_0);
						num = 11;
						continue;
					case 4:
						cFSMState.StateProcess.processIn();
						num = 5;
						continue;
					case 0:
						cFSMState.StateProcess.processInput(A_0);
						num = 12;
						continue;
					case 2:
						num = 3;
						continue;
					case 3:
						if (cFSMState.StateProcess != null)
						{
							num = 0;
							continue;
						}
						goto case 12;
					case 12:
						return this.m_ᜁ;
					case 5:
						return this.m_ᜁ;
					}
					break;
				}
			}
		}

		public void update(float fDeltaTime)
		{
			//Discarded unreachable code: IL_0068
			int num = 0;
			CFSMState @object = default(CFSMState);
			while (true)
			{
				switch (num)
				{
				default:
					if (this.m_ᜁ <= 0)
					{
						num = 4;
						break;
					}
					@object = this.m_ᜂ.GetObject(this.m_ᜁ);
					num = 1;
					break;
				case 4:
					return;
				case 3:
					@object.processState(fDeltaTime);
					num = 2;
					break;
				case 2:
					return;
				case 1:
					if (@object != null)
					{
						if (true)
						{
						}
						num = 3;
						break;
					}
					return;
				}
			}
		}

		public void release()
		{
			//Discarded unreachable code: IL_00b0
			using (Dictionary<int, CFSMState>.Enumerator enumerator = this.m_ᜂ.Objects.GetEnumerator())
			{
				int num = 5;
				KeyValuePair<int, CFSMState> current = default(KeyValuePair<int, CFSMState>);
				while (true)
				{
					switch (num)
					{
					case 1:
						if (current.Value != null)
						{
							num = 6;
							continue;
						}
						goto default;
					default:
						num = 4;
						continue;
					case 4:
						if (enumerator.MoveNext())
						{
							current = enumerator.Current;
							num = 1;
						}
						else
						{
							num = 0;
						}
						continue;
					case 6:
						current.Value.onDestory();
						num = 3;
						continue;
					case 0:
						num = 2;
						continue;
					case 2:
						break;
					}
					break;
				}
			}
			if (true)
			{
			}
			this.m_ᜂ.Objects.Clear();
		}
	}
}
