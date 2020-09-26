using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CSLib.Utility
{
	[Serializable]
	public class CCollectionContainerListType<ValueType> : IEnumerable<ValueType>
	{
		[CompilerGenerated]
		private sealed class ᜀ : IEnumerator<ValueType>
		{
			private int m_ᜀ;

			private ValueType m_ᜁ;

			public CCollectionContainerListType<ValueType> ᜂ;

			private int m_ᜃ;

			[DebuggerHidden]
			public ᜀ(int A_0)
			{
				this.ᜀ = A_0;
			}

			[DebuggerHidden]
			private void ᜁ()
			{
			}

			void IDisposable.Dispose()
			{
				//ILSpy generated this explicit interface implementation from .override directive in ᜁ
				this.ᜁ();
			}

			private bool ᜅ()
			{
				//Discarded unreachable code: IL_0045
				while (true)
				{
					int num = this.ᜀ;
					int num2 = 3;
					while (true)
					{
						switch (num2)
						{
						case 3:
						{
							if (num != 0)
							{
								num2 = 0;
								continue;
							}
							this.ᜀ = -1;
							ValueType val = default(ValueType);
							this.ᜃ = 0;
							num2 = 7;
							continue;
						}
						case 0:
							if (true)
							{
							}
							num2 = 2;
							continue;
						case 2:
							if (num == 1)
							{
								this.ᜀ = -1;
								this.ᜃ++;
								num2 = 4;
							}
							else
							{
								num2 = 6;
							}
							continue;
						case 4:
						case 7:
							num2 = 1;
							continue;
						case 1:
						{
							if (this.ᜃ >= ᜂ.m_ObjectList.Count)
							{
								num2 = 5;
								continue;
							}
							ValueType val = (this.ᜁ = ᜂ.m_ObjectList[this.ᜃ]);
							this.ᜀ = 1;
							return true;
						}
						case 6:
							return false;
						case 5:
							return false;
						}
						break;
					}
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in ᜅ
				return this.ᜅ();
			}

			[DebuggerHidden]
			private ValueType ᜀ()
			{
				return this.ᜁ;
			}

			ValueType IEnumerator<ValueType>.get_Current()
			{
				//ILSpy generated this explicit interface implementation from .override directive in ᜀ
				return this.ᜀ();
			}

			[DebuggerHidden]
			private void ᜃ()
			{
				throw new NotSupportedException();
			}

			void IEnumerator.Reset()
			{
				//ILSpy generated this explicit interface implementation from .override directive in ᜃ
				this.ᜃ();
			}

			[DebuggerHidden]
			private object ᜄ()
			{
				return this.ᜁ;
			}

			object IEnumerator.get_Current()
			{
				//ILSpy generated this explicit interface implementation from .override directive in ᜄ
				return this.ᜄ();
			}
		}

		private List<ValueType> m_ObjectList;

		public int Count => m_ObjectList.Count;

		public ValueType this[int i]
		{
			get
			{
				//Discarded unreachable code: IL_0037
				while (true)
				{
					ValueType result = default(ValueType);
					int num = 5;
					while (true)
					{
						switch (num)
						{
						case 5:
							if (true)
							{
							}
							if (m_ObjectList != null)
							{
								num = 2;
								continue;
							}
							goto case 0;
						case 1:
							result = m_ObjectList[i];
							num = 0;
							continue;
						case 6:
							num = 3;
							continue;
						case 3:
							if (i >= 0)
							{
								num = 1;
								continue;
							}
							goto case 0;
						case 2:
							num = 4;
							continue;
						case 4:
							if (i < m_ObjectList.Count)
							{
								num = 6;
								continue;
							}
							goto case 0;
						case 0:
							return result;
						}
						break;
					}
				}
			}
			set
			{
				//Discarded unreachable code: IL_003f
				int num = 2;
				while (true)
				{
					switch (num)
					{
					default:
						if (m_ObjectList != null)
						{
							num = 5;
							break;
						}
						return;
					case 5:
						if (true)
						{
						}
						num = 1;
						break;
					case 0:
						_ = m_ObjectList[i];
						m_ObjectList[i] = value;
						num = 3;
						break;
					case 3:
						return;
					case 4:
						num = 6;
						break;
					case 6:
						if (i >= 0)
						{
							num = 0;
							break;
						}
						return;
					case 1:
						if (i >= m_ObjectList.Count)
						{
							num = 4;
							break;
						}
						goto case 0;
					}
				}
			}
		}

		public List<ValueType> ObjectList => m_ObjectList;

		public CCollectionContainerListType()
		{
			m_ObjectList = new List<ValueType>();
		}

		public void Init()
		{
			if (m_ObjectList == null)
			{
				m_ObjectList = new List<ValueType>();
			}
		}

		public IEnumerator<ValueType> GetEnumerator()
		{
			return new ᜀ(0)
			{
				ᜂ = this
			};
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(ValueType value)
		{
			if (m_ObjectList != null)
			{
				m_ObjectList.Add(value);
			}
		}

		public void AddRange(CCollectionContainerListType<ValueType> collection)
		{
			if (m_ObjectList != null)
			{
				m_ObjectList.AddRange(collection.m_ObjectList);
			}
		}

		public void Insert(int index, ValueType value)
		{
			if (m_ObjectList != null)
			{
				m_ObjectList.Insert(index, value);
			}
		}

		public void Remove(int index)
		{
			//Discarded unreachable code: IL_0075
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					if (m_ObjectList != null)
					{
						num = 5;
						break;
					}
					return;
				case 6:
					m_ObjectList.RemoveAt(index);
					num = 0;
					break;
				case 0:
					return;
				case 3:
					num = 2;
					break;
				case 2:
					if (index >= 0)
					{
						num = 6;
						break;
					}
					return;
				case 5:
					num = 4;
					break;
				case 4:
					if (true)
					{
					}
					if (index >= m_ObjectList.Count)
					{
						num = 3;
						break;
					}
					goto case 6;
				}
			}
		}

		public void Remove(ValueType value)
		{
			if (m_ObjectList != null)
			{
				m_ObjectList.Remove(value);
			}
		}

		public void Clear()
		{
			if (m_ObjectList != null)
			{
				m_ObjectList.Clear();
			}
		}

		public void Sort(Comparison<ValueType> comparison)
		{
			m_ObjectList.Sort(comparison);
		}

		public void Swaq(int first, int second)
		{
			//Discarded unreachable code: IL_0118
			int num = 11;
			while (true)
			{
				switch (num)
				{
				default:
					if (m_ObjectList != null)
					{
						num = 2;
						break;
					}
					return;
				case 3:
					num = 9;
					break;
				case 9:
					if (first != second)
					{
						num = 1;
						break;
					}
					return;
				case 10:
					num = 8;
					break;
				case 8:
					if (second >= 0)
					{
						num = 0;
						break;
					}
					return;
				case 2:
					num = 6;
					break;
				case 6:
					if (first >= 0)
					{
						num = 4;
						break;
					}
					return;
				case 1:
				{
					ValueType value = m_ObjectList[first];
					m_ObjectList[first] = m_ObjectList[second];
					m_ObjectList[second] = value;
					num = 12;
					break;
				}
				case 12:
					return;
				case 4:
					num = 5;
					break;
				case 5:
					if (first < m_ObjectList.Count)
					{
						num = 10;
						break;
					}
					return;
				case 0:
					if (true)
					{
					}
					num = 7;
					break;
				case 7:
					if (second < m_ObjectList.Count)
					{
						num = 3;
						break;
					}
					return;
				}
			}
		}
	}
}
