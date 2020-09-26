using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace CSLib.Utility
{
	public class CPropertyDescriptorList : List<PropertyDescriptor>
	{
		public PropertyDescriptor Find(string propertyName, bool ignoreCase)
		{
			//Discarded unreachable code: IL_000c
			using (Enumerator enumerator = GetEnumerator())
			{
				int num = 5;
				PropertyDescriptor current = default(PropertyDescriptor);
				PropertyDescriptor result = default(PropertyDescriptor);
				while (true)
				{
					switch (num)
					{
					default:
						num = 1;
						continue;
					case 1:
						if (!enumerator.MoveNext())
						{
							num = 0;
							continue;
						}
						current = enumerator.Current;
						num = 2;
						continue;
					case 2:
						if (string.Compare(current.Name, propertyName, ignoreCase) == 0)
						{
							num = 3;
							continue;
						}
						goto default;
					case 3:
						result = current;
						num = 6;
						continue;
					case 0:
						num = 4;
						continue;
					case 4:
						break;
					case 6:
						return result;
					}
					break;
				}
			}
			if (true)
			{
			}
			return null;
		}

		public void SortUsingAttribute()
		{
			if (ᜀ())
			{
				Sort(new ᜠ());
			}
		}

		private bool ᜀ()
		{
			//Discarded unreachable code: IL_0011
			switch (0)
			{
			default:
			{
				if (true)
				{
				}
				using (Enumerator enumerator2 = GetEnumerator())
				{
					int num = 1;
					IEnumerator enumerator = default(IEnumerator);
					bool result = default(bool);
					while (true)
					{
						switch (num)
						{
						default:
							num = 4;
							continue;
						case 4:
							if (!enumerator2.MoveNext())
							{
								num = 0;
								continue;
							}
							enumerator = enumerator2.Current.Attributes.GetEnumerator();
							num = 2;
							continue;
						case 2:
							try
							{
								num = 2;
								while (true)
								{
									switch (num)
									{
									case 4:
										result = true;
										num = 1;
										continue;
									case 5:
										if (((Attribute)enumerator.Current) is ᝆ)
										{
											num = 4;
											continue;
										}
										goto default;
									default:
										num = 0;
										continue;
									case 0:
										num = ((!enumerator.MoveNext()) ? 6 : 5);
										continue;
									case 6:
										num = 3;
										continue;
									case 3:
										break;
									case 1:
										return result;
									}
									break;
								}
							}
							finally
							{
								while (true)
								{
									IDisposable disposable = enumerator as IDisposable;
									num = 0;
									while (true)
									{
										switch (num)
										{
										case 0:
											if (disposable != null)
											{
												num = 2;
												continue;
											}
											goto end_IL_0116;
										case 2:
											disposable.Dispose();
											num = 1;
											continue;
										case 1:
											goto end_IL_0116;
										}
										break;
									}
								}
								end_IL_0116:;
							}
							goto default;
						case 0:
							num = 3;
							continue;
						case 3:
							break;
						}
						break;
					}
				}
				return false;
			}
			}
		}
	}
}
