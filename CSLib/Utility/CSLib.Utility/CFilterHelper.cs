using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CSLib.Utility
{
	public class CFilterHelper : CSingleton<CFilterHelper>
	{
		private CCollectionContainerDictionaryType<string, CRegularExpressions> ᜀ = new CCollectionContainerDictionaryType<string, CRegularExpressions>();

		private CRegularExpressions ᜁ;

		public Dictionary<string, CRegularExpressions>.KeyCollection Keys => ᜀ.Keys;

		public bool InFilterList(string value)
		{
			//Discarded unreachable code: IL_0011
			switch (0)
			{
			default:
			{
				if (true)
				{
				}
				bool result = false;
				using Dictionary<string, CRegularExpressions>.KeyCollection.Enumerator enumerator = ᜀ.Keys.GetEnumerator();
				int num = 4;
				CRegularExpressions cRegularExpressions = default(CRegularExpressions);
				while (true)
				{
					switch (num)
					{
					case 6:
						if (cRegularExpressions != null)
						{
							num = 0;
							break;
						}
						goto default;
					default:
						num = 7;
						break;
					case 7:
						if (enumerator.MoveNext())
						{
							string current = enumerator.Current;
							cRegularExpressions = ᜀ.Get(current);
							num = 6;
						}
						else
						{
							num = 5;
						}
						break;
					case 0:
						num = 3;
						break;
					case 3:
						if (cRegularExpressions.IsMatch(value))
						{
							num = 2;
							break;
						}
						goto default;
					case 2:
						result = true;
						num = 8;
						break;
					case 5:
						num = 1;
						break;
					case 1:
						return result;
					case 8:
						return result;
					}
				}
			}
			}
		}

		public void BeginFilter(string pattern)
		{
			ᜁ = new CRegularExpressions(pattern);
			ᜁ.Generate(RegexOptions.IgnoreCase);
		}

		public bool IsInFilter(string value)
		{
			//Discarded unreachable code: IL_0017
			while (true)
			{
				if (true)
				{
				}
				bool result = false;
				int num = 1;
				while (true)
				{
					switch (num)
					{
					case 1:
						if (ᜁ != null)
						{
							num = 2;
							continue;
						}
						goto case 0;
					case 2:
						result = ᜁ.IsMatch(value);
						num = 0;
						continue;
					case 0:
						return result;
					}
					break;
				}
			}
		}

		public List<string> GetResults(string value)
		{
			//Discarded unreachable code: IL_0117
			switch (0)
			{
			}
			IEnumerator enumerator = default(IEnumerator);
			while (true)
			{
				List<string> list = new List<string>();
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (ᜁ != null)
						{
							num = 1;
							continue;
						}
						return list;
					case 0:
						try
						{
							num = 1;
							while (true)
							{
								switch (num)
								{
								default:
									num = 3;
									break;
								case 3:
								{
									if (!enumerator.MoveNext())
									{
										num = 0;
										break;
									}
									Match match = (Match)enumerator.Current;
									list.Add(match.Value);
									num = 2;
									break;
								}
								case 0:
									num = 4;
									break;
								case 4:
									return list;
								}
							}
						}
						finally
						{
							while (true)
							{
								IDisposable disposable = enumerator as IDisposable;
								num = 1;
								while (true)
								{
									switch (num)
									{
									case 1:
										if (disposable != null)
										{
											num = 2;
											continue;
										}
										goto end_IL_00bd;
									case 2:
										disposable.Dispose();
										num = 0;
										continue;
									case 0:
										goto end_IL_00bd;
									}
									break;
								}
							}
							end_IL_00bd:;
						}
					case 1:
						enumerator = ᜁ.GetMatchs(value).GetEnumerator();
						if (true)
						{
						}
						num = 0;
						continue;
					}
					break;
				}
			}
		}

		public void EndFilter()
		{
			ᜁ = null;
		}

		public void AddPattern(string pattern)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			CRegularExpressions cRegularExpressions = new CRegularExpressions(pattern);
			cRegularExpressions.Generate(RegexOptions.None);
			ᜀ.Add(pattern, cRegularExpressions);
		}

		public void DelPattern(string pattern)
		{
			ᜀ.Remove(pattern);
		}

		public string GetPattern(string pattern)
		{
			if (ᜀ.Get(pattern) != null)
			{
				return pattern;
			}
			return null;
		}

		public string BuildDosWildcard(string wildcard)
		{
			//Discarded unreachable code: IL_0048
			int a_ = 11;
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					num = (string.IsNullOrEmpty(wildcard) ? 1 : 3);
					break;
				case 2:
					return null;
				case 3:
				{
					if (true)
					{
					}
					if (wildcard.IndexOfAny(new char[6]
					{
						'<',
						'>',
						'/',
						'"',
						'|',
						':'
					}) >= 0)
					{
						num = 2;
						break;
					}
					StringBuilder stringBuilder = new StringBuilder(wildcard);
					stringBuilder.Replace(CSimpleThreadPool.b("ᭆ", a_), CSimpleThreadPool.b("ᭆᕈ", a_));
					stringBuilder.Replace(CSimpleThreadPool.b("᥆", a_), CSimpleThreadPool.b("ᭆᝈ", a_));
					stringBuilder.Replace(CSimpleThreadPool.b("捆", a_), CSimpleThreadPool.b("ᭆ浈", a_));
					stringBuilder.Replace(CSimpleThreadPool.b("汆", a_), CSimpleThreadPool.b("ᭆ扈", a_));
					stringBuilder.Replace(CSimpleThreadPool.b("㱆", a_), CSimpleThreadPool.b("ᭆ㉈", a_));
					stringBuilder.Replace(CSimpleThreadPool.b("㩆", a_), CSimpleThreadPool.b("ᭆ㑈", a_));
					stringBuilder.Replace(CSimpleThreadPool.b("楆", a_), CSimpleThreadPool.b("ᭆ杈", a_));
					stringBuilder.Replace(CSimpleThreadPool.b("᱆", a_), CSimpleThreadPool.b("ᭆቈ", a_));
					stringBuilder.Replace(CSimpleThreadPool.b("ᩆ", a_), CSimpleThreadPool.b("ᭆᑈ", a_));
					stringBuilder.Replace(CSimpleThreadPool.b("潆", a_), CSimpleThreadPool.b("ᭆ慈", a_));
					stringBuilder.Replace(CSimpleThreadPool.b("湆", a_), CSimpleThreadPool.b("ᭆ恈", a_));
					stringBuilder.Replace(CSimpleThreadPool.b("浆", a_), CSimpleThreadPool.b("楆捈", a_));
					stringBuilder.Replace(CSimpleThreadPool.b("硆", a_), CSimpleThreadPool.b("楆癈扊", a_));
					stringBuilder.Append(CSimpleThreadPool.b("捆", a_));
					return stringBuilder.ToString();
				}
				case 1:
					return CSimpleThreadPool.b("楆捈ᝊ捌慎筐", a_);
				}
			}
		}
	}
}
