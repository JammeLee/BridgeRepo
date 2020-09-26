using System.Net.Mime;
using System.Text;

namespace System.Net.Mail
{
	public class MailAddress
	{
		private string displayName;

		private Encoding displayNameEncoding;

		private string encodedDisplayName;

		private string address;

		private string fullAddress;

		private string userName;

		private string host;

		public string DisplayName
		{
			get
			{
				if (displayName == null)
				{
					if (encodedDisplayName != null && encodedDisplayName.Length > 0)
					{
						displayName = MimeBasePart.DecodeHeaderValue(encodedDisplayName);
					}
					else
					{
						displayName = string.Empty;
					}
				}
				return displayName;
			}
		}

		public string User => userName;

		public string Host => host;

		public string Address
		{
			get
			{
				if (address == null)
				{
					CombineParts();
				}
				return address;
			}
		}

		internal string SmtpAddress
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append('<');
				stringBuilder.Append(Address);
				stringBuilder.Append('>');
				return stringBuilder.ToString();
			}
		}

		internal MailAddress(string address, string encodedDisplayName, uint bogusParam)
		{
			this.encodedDisplayName = encodedDisplayName;
			GetParts(address);
		}

		public MailAddress(string address)
			: this(address, null, null)
		{
		}

		public MailAddress(string address, string displayName)
			: this(address, displayName, null)
		{
		}

		public MailAddress(string address, string displayName, Encoding displayNameEncoding)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (address == string.Empty)
			{
				throw new ArgumentException(SR.GetString("net_emptystringcall", "address"), "address");
			}
			this.displayNameEncoding = displayNameEncoding;
			this.displayName = displayName;
			ParseValue(address);
			if (this.displayName != null && this.displayName != string.Empty)
			{
				if (this.displayName[0] == '"' && this.displayName[this.displayName.Length - 1] == '"')
				{
					this.displayName = this.displayName.Substring(1, this.displayName.Length - 2);
				}
				this.displayName = this.displayName.Trim();
			}
			if (this.displayName == null || this.displayName.Length <= 0)
			{
				return;
			}
			if (!MimeBasePart.IsAscii(this.displayName, permitCROrLF: false) || this.displayNameEncoding != null)
			{
				if (this.displayNameEncoding == null)
				{
					this.displayNameEncoding = Encoding.GetEncoding("utf-8");
				}
				encodedDisplayName = MimeBasePart.EncodeHeaderValue(this.displayName, this.displayNameEncoding, MimeBasePart.ShouldUseBase64Encoding(displayNameEncoding));
				StringBuilder stringBuilder = new StringBuilder();
				int offset = 0;
				MailBnfHelper.ReadUnQuotedString(encodedDisplayName, ref offset, stringBuilder);
				encodedDisplayName = stringBuilder.ToString();
			}
			else
			{
				encodedDisplayName = this.displayName;
			}
		}

		internal string ToEncodedString()
		{
			if (fullAddress == null)
			{
				if (encodedDisplayName != null && encodedDisplayName != string.Empty)
				{
					StringBuilder stringBuilder = new StringBuilder();
					MailBnfHelper.GetDotAtomOrQuotedString(encodedDisplayName, stringBuilder);
					stringBuilder.Append(" <");
					stringBuilder.Append(Address);
					stringBuilder.Append('>');
					fullAddress = stringBuilder.ToString();
				}
				else
				{
					fullAddress = Address;
				}
			}
			return fullAddress;
		}

		public override string ToString()
		{
			if (fullAddress == null)
			{
				if (encodedDisplayName != null && encodedDisplayName != string.Empty)
				{
					StringBuilder stringBuilder = new StringBuilder();
					if (DisplayName.StartsWith("\"") && DisplayName.EndsWith("\""))
					{
						stringBuilder.Append(DisplayName);
					}
					else
					{
						stringBuilder.Append('"');
						stringBuilder.Append(DisplayName);
						stringBuilder.Append('"');
					}
					stringBuilder.Append(" <");
					stringBuilder.Append(Address);
					stringBuilder.Append('>');
					fullAddress = stringBuilder.ToString();
				}
				else
				{
					fullAddress = Address;
				}
			}
			return fullAddress;
		}

		public override bool Equals(object value)
		{
			if (value == null)
			{
				return false;
			}
			return ToString().Equals(value.ToString(), StringComparison.InvariantCultureIgnoreCase);
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		private void GetParts(string address)
		{
			if (address != null)
			{
				int num = address.IndexOf('@');
				if (num < 0)
				{
					throw new FormatException(SR.GetString("MailAddressInvalidFormat"));
				}
				userName = address.Substring(0, num);
				host = address.Substring(num + 1);
			}
		}

		private void ParseValue(string address)
		{
			string text = null;
			int offset = 0;
			MailBnfHelper.SkipFWS(address, ref offset);
			int num = address.IndexOf('"', offset);
			if (num == offset)
			{
				num = address.IndexOf('"', num + 1);
				if (num > offset)
				{
					int offset2 = num + 1;
					MailBnfHelper.SkipFWS(address, ref offset2);
					if (address.Length > offset2 && address[offset2] != '@')
					{
						text = address.Substring(offset, num + 1 - offset);
						address = address.Substring(offset2);
					}
				}
			}
			if (text == null)
			{
				int num2 = address.IndexOf('<', offset);
				if (num2 >= offset)
				{
					text = address.Substring(offset, num2 - offset);
					address = address.Substring(num2);
				}
			}
			if (text == null)
			{
				num = address.IndexOf('"', offset);
				if (num > offset)
				{
					text = address.Substring(offset, num - offset);
					address = address.Substring(num);
				}
			}
			if (displayName == null)
			{
				displayName = text;
			}
			int offset3 = 0;
			address = MailBnfHelper.ReadMailAddress(address, ref offset3, out encodedDisplayName);
			GetParts(address);
		}

		private void CombineParts()
		{
			if (userName != null && host != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				MailBnfHelper.GetDotAtomOrQuotedString(User, stringBuilder);
				stringBuilder.Append('@');
				MailBnfHelper.GetDotAtomOrDomainLiteral(Host, stringBuilder);
				address = stringBuilder.ToString();
			}
		}
	}
}
