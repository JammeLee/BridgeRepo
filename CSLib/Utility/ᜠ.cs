using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

internal class ᜠ : IComparer<PropertyDescriptor>
{
	public int Compare(PropertyDescriptor l, PropertyDescriptor r)
	{
		//Discarded unreachable code: IL_00d9
		switch (0)
		{
		default:
		{
			int num = 0;
			int num3 = default(int);
			int num2 = default(int);
			Attribute attribute2 = default(Attribute);
			IEnumerator enumerator = default(IEnumerator);
			Attribute attribute = default(Attribute);
			while (true)
			{
				switch (num)
				{
				default:
					if (l == null)
					{
						num = 10;
						break;
					}
					goto IL_00ef;
				case 8:
					if (num3 != -1)
					{
						num = 11;
						break;
					}
					goto case 1;
				case 7:
					return 1;
				case 5:
					if (num3 > num2)
					{
						if (true)
						{
						}
						num = 15;
					}
					else
					{
						num = 20;
					}
					break;
				case 17:
					if (r == null)
					{
						num = 19;
						break;
					}
					goto IL_0151;
				case 11:
					num = 21;
					break;
				case 21:
					num = ((num2 == -1) ? 1 : 5);
					break;
				case 20:
					if (num3 > num2)
					{
						num = 4;
						break;
					}
					return 0;
				case 16:
					if (l == null)
					{
						num = 12;
						break;
					}
					goto IL_01d4;
				case 15:
					return 1;
				case 4:
					return -1;
				case 2:
					return 0;
				case 10:
					num = 18;
					break;
				case 18:
					if (r != null)
					{
						num = 3;
						break;
					}
					goto IL_00ef;
				case 19:
					num = 13;
					break;
				case 13:
					if (l != null)
					{
						num = 7;
						break;
					}
					goto IL_0151;
				case 12:
					num = 14;
					break;
				case 14:
					if (r == null)
					{
						num = 2;
						break;
					}
					goto IL_01d4;
				case 1:
					return l.Name.CompareTo(r.Name);
				case 6:
					try
					{
						num = 4;
						while (true)
						{
							switch (num)
							{
							case 6:
								if (attribute2 is ᝆ)
								{
									num = 2;
									continue;
								}
								goto default;
							case 2:
								num3 = ((ᝆ)attribute2).ᜁ();
								num = 1;
								continue;
							case 1:
								break;
							default:
								num = 3;
								continue;
							case 3:
								if (enumerator.MoveNext())
								{
									attribute2 = (Attribute)enumerator.Current;
									num = 6;
								}
								else
								{
									num = 0;
								}
								continue;
							case 0:
								num = 5;
								continue;
							case 5:
								break;
							}
							break;
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
									goto end_IL_02a6;
								case 2:
									disposable.Dispose();
									num = 0;
									continue;
								case 0:
									goto end_IL_02a6;
								}
								break;
							}
						}
						end_IL_02a6:;
					}
					enumerator = r.Attributes.GetEnumerator();
					num = 9;
					break;
				case 3:
					return -1;
				case 9:
					{
						try
						{
							num = 1;
							while (true)
							{
								switch (num)
								{
								case 6:
									if (attribute is ᝆ)
									{
										num = 5;
										continue;
									}
									goto default;
								case 5:
									num2 = ((ᝆ)attribute).ᜁ();
									num = 2;
									continue;
								case 2:
									break;
								default:
									num = 0;
									continue;
								case 0:
									if (enumerator.MoveNext())
									{
										attribute = (Attribute)enumerator.Current;
										num = 6;
									}
									else
									{
										num = 3;
									}
									continue;
								case 3:
									num = 4;
									continue;
								case 4:
									break;
								}
								break;
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
										goto end_IL_0395;
									case 2:
										disposable.Dispose();
										num = 0;
										continue;
									case 0:
										goto end_IL_0395;
									}
									break;
								}
							}
							end_IL_0395:;
						}
						num = 8;
						break;
					}
					IL_0151:
					num = 16;
					break;
					IL_01d4:
					num3 = -1;
					num2 = -1;
					enumerator = l.Attributes.GetEnumerator();
					num = 6;
					break;
					IL_00ef:
					num = 17;
					break;
				}
			}
		}
		}
	}
}
