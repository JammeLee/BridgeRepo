using System;
using System.Threading;

namespace CSLib.Utility
{
	[Serializable]
	public class CSingleton<T>
	{
		private static T m_instance = default(T);

		private static readonly object m_lockObj = new object();

		public static T Instance
		{
			get
			{
				//Discarded unreachable code: IL_001f
				int num = 1;
				object lockObj = default(object);
				while (true)
				{
					switch (num)
					{
					default:
						if (true)
						{
						}
						if (m_instance == null)
						{
							num = 2;
							continue;
						}
						break;
					case 0:
						try
						{
							num = 1;
							while (true)
							{
								switch (num)
								{
								default:
									if (m_instance == null)
									{
										num = 3;
										continue;
									}
									break;
								case 3:
									m_instance = (T)Activator.CreateInstance(typeof(T), nonPublic: true);
									num = 0;
									continue;
								case 0:
									break;
								case 2:
									goto end_IL_003d;
								}
								num = 2;
							}
							end_IL_003d:;
						}
						finally
						{
							Monitor.Exit(lockObj);
						}
						break;
					case 2:
						lockObj = m_lockObj;
						Monitor.Enter(lockObj);
						num = 0;
						continue;
					}
					break;
				}
				return m_instance;
			}
		}
	}
}
