using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using CSLib.Utility;

internal class ᜣ : CustomTypeDescriptor
{
	private CPropertyDescriptorList m_ᜀ;

	private CPropertyDescriptorList ᜁ;

	private object ᜂ;

	private MethodInfo ᜃ;

	public ᜣ(ICustomTypeDescriptor A_0, object A_1)
	{
		int a_ = 9;
		this.m_ᜀ = new CPropertyDescriptorList();
		ᜁ = new CPropertyDescriptorList();
		base._002Ector(A_0);
		ᜂ = A_1;
		Type[] array = new Type[1];
		array.SetValue(typeof(CPropertyDescriptorList), 0);
		ᜃ = ᜂ.GetType().GetMethod(CSimpleThreadPool.b("Ʉ≆㵈ཊ㑌ⅎぐ㹒㱔㑖क़⥚㉜⽞Ѡᅢᅤ\u0e66౨ᡪ", a_), array);
	}

	public override PropertyDescriptorCollection GetProperties()
	{
		//Discarded unreachable code: IL_0033
		switch (0)
		{
		default:
		{
			int num = 1;
			IEnumerator enumerator = default(IEnumerator);
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (ᜃ != null)
					{
						num = 2;
						continue;
					}
					enumerator = base.GetProperties().GetEnumerator();
					num = 3;
					continue;
				case 3:
					try
					{
						num = 3;
						while (true)
						{
							switch (num)
							{
							default:
								num = 2;
								continue;
							case 2:
								if (enumerator.MoveNext())
								{
									PropertyDescriptor item = (PropertyDescriptor)enumerator.Current;
									ᜁ.Add(item);
									num = 0;
								}
								else
								{
									num = 4;
								}
								continue;
							case 4:
								num = 1;
								continue;
							case 1:
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
							num = 0;
							while (true)
							{
								switch (num)
								{
								case 0:
									if (disposable != null)
									{
										num = 1;
										continue;
									}
									goto end_IL_00c6;
								case 1:
									disposable.Dispose();
									num = 2;
									continue;
								case 2:
									goto end_IL_00c6;
								}
								break;
							}
						}
						end_IL_00c6:;
					}
					break;
				case 2:
				{
					object[] parameters = new object[1]
					{
						ᜁ
					};
					ᜃ.Invoke(ᜂ, parameters);
					ᜁ.SortUsingAttribute();
					num = 0;
					continue;
				}
				case 0:
					break;
				}
				break;
			}
			return new PropertyDescriptorCollection(ᜁ.ToArray());
		}
		}
	}

	public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		//Discarded unreachable code: IL_0033
		switch (0)
		{
		default:
		{
			int num = 0;
			IEnumerator enumerator = default(IEnumerator);
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (ᜃ != null)
					{
						num = 2;
						continue;
					}
					enumerator = base.GetProperties(attributes).GetEnumerator();
					num = 3;
					continue;
				case 3:
					try
					{
						num = 0;
						while (true)
						{
							switch (num)
							{
							default:
								num = 2;
								continue;
							case 2:
								if (enumerator.MoveNext())
								{
									PropertyDescriptor item = (PropertyDescriptor)enumerator.Current;
									this.m_ᜀ.Add(item);
									num = 4;
								}
								else
								{
									num = 3;
								}
								continue;
							case 3:
								num = 1;
								continue;
							case 1:
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
									goto end_IL_00c6;
								case 2:
									disposable.Dispose();
									num = 0;
									continue;
								case 0:
									goto end_IL_00c6;
								}
								break;
							}
						}
						end_IL_00c6:;
					}
					break;
				case 2:
				{
					object[] parameters = new object[1]
					{
						this.m_ᜀ
					};
					ᜃ.Invoke(ᜂ, parameters);
					this.m_ᜀ.SortUsingAttribute();
					num = 1;
					continue;
				}
				case 1:
					break;
				}
				break;
			}
			return new PropertyDescriptorCollection(this.m_ᜀ.ToArray());
		}
		}
	}

	public override object GetPropertyOwner(PropertyDescriptor pd)
	{
		return ᜂ;
	}

	private void ᜀ(PropertyDescriptorCollection A_0)
	{
		//Discarded unreachable code: IL_018c
		int a_ = 3;
		switch (0)
		{
		}
		CConsole.Write(CSimpleThreadPool.b("簾⹀㙂⭄㍆", a_) + A_0.Count);
		IEnumerator enumerator = A_0.GetEnumerator();
		try
		{
			int num = 6;
			PropertyDescriptor propertyDescriptor = default(PropertyDescriptor);
			while (true)
			{
				switch (num)
				{
				case 2:
					if (propertyDescriptor != null)
					{
						num = 0;
						continue;
					}
					CConsole.Write(CSimpleThreadPool.b("儾㑀⽂⥄", a_));
					num = 1;
					continue;
				default:
					num = 5;
					continue;
				case 5:
					if (enumerator.MoveNext())
					{
						propertyDescriptor = (PropertyDescriptor)enumerator.Current;
						CConsole.Write(CSimpleThreadPool.b("ጾ慀", a_));
						num = 2;
					}
					else
					{
						num = 3;
					}
					continue;
				case 0:
					CConsole.Write(A_0.IndexOf(propertyDescriptor) + CSimpleThreadPool.b("\u173e", a_) + propertyDescriptor.Name + CSimpleThreadPool.b("ᘾ", a_));
					num = 7;
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
				int num = 0;
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
						goto end_IL_0144;
					case 2:
						disposable.Dispose();
						num = 1;
						continue;
					case 1:
						goto end_IL_0144;
					}
					break;
				}
			}
			end_IL_0144:;
		}
		if (1 == 0)
		{
		}
	}
}
