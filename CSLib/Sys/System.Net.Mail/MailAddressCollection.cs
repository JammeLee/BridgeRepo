using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Mime;
using System.Text;

namespace System.Net.Mail
{
	public class MailAddressCollection : Collection<MailAddress>
	{
		public void Add(string addresses)
		{
			if (addresses == null)
			{
				throw new ArgumentNullException("addresses");
			}
			if (addresses == string.Empty)
			{
				throw new ArgumentException(SR.GetString("net_emptystringcall", "addresses"), "addresses");
			}
			ParseValue(addresses);
		}

		protected override void SetItem(int index, MailAddress item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			base.SetItem(index, item);
		}

		protected override void InsertItem(int index, MailAddress item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			base.InsertItem(index, item);
		}

		internal void ParseValue(string addresses)
		{
			for (int i = 0; i < addresses.Length; i++)
			{
				MailAddress mailAddress = MailBnfHelper.ReadMailAddress(addresses, ref i);
				if (mailAddress == null)
				{
					break;
				}
				Add(mailAddress);
				if (!MailBnfHelper.SkipCFWS(addresses, ref i) || addresses[i] != ',')
				{
					break;
				}
			}
		}

		internal string ToEncodedString()
		{
			bool flag = true;
			StringBuilder stringBuilder = new StringBuilder();
			using (IEnumerator<MailAddress> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					MailAddress current = enumerator.Current;
					if (!flag)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(current.ToEncodedString());
					flag = false;
				}
			}
			return stringBuilder.ToString();
		}

		public override string ToString()
		{
			bool flag = true;
			StringBuilder stringBuilder = new StringBuilder();
			using (IEnumerator<MailAddress> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					MailAddress current = enumerator.Current;
					if (!flag)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(current.ToString());
					flag = false;
				}
			}
			return stringBuilder.ToString();
		}
	}
}
