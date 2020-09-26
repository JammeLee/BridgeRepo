using System.Collections.Specialized;
using System.Globalization;
using System.Net.Mail;
using System.Text;

namespace System.Net.Mime
{
	public class ContentDisposition
	{
		private string dispositionType;

		private TrackingStringDictionary parameters;

		private bool isChanged;

		private bool isPersisted;

		private string disposition;

		public string DispositionType
		{
			get
			{
				return dispositionType;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				if (value == string.Empty)
				{
					throw new ArgumentException(SR.GetString("net_emptystringset"), "value");
				}
				isChanged = true;
				dispositionType = value;
			}
		}

		public StringDictionary Parameters
		{
			get
			{
				if (parameters == null)
				{
					parameters = new TrackingStringDictionary();
				}
				return parameters;
			}
		}

		public string FileName
		{
			get
			{
				return Parameters["filename"];
			}
			set
			{
				if (value == null || value == string.Empty)
				{
					Parameters.Remove("filename");
				}
				else
				{
					Parameters["filename"] = value;
				}
			}
		}

		public DateTime CreationDate
		{
			get
			{
				string text = Parameters["creation-date"];
				if (text == null)
				{
					return DateTime.MinValue;
				}
				int offset = 0;
				return MailBnfHelper.ReadDateTime(text, ref offset);
			}
			set
			{
				Parameters["creation-date"] = MailBnfHelper.GetDateTimeString(value, null);
			}
		}

		public DateTime ModificationDate
		{
			get
			{
				string text = Parameters["modification-date"];
				if (text == null)
				{
					return DateTime.MinValue;
				}
				int offset = 0;
				return MailBnfHelper.ReadDateTime(text, ref offset);
			}
			set
			{
				Parameters["modification-date"] = MailBnfHelper.GetDateTimeString(value, null);
			}
		}

		public bool Inline
		{
			get
			{
				return dispositionType == "inline";
			}
			set
			{
				isChanged = true;
				if (value)
				{
					dispositionType = "inline";
				}
				else
				{
					dispositionType = "attachment";
				}
			}
		}

		public DateTime ReadDate
		{
			get
			{
				string text = Parameters["read-date"];
				if (text == null)
				{
					return DateTime.MinValue;
				}
				int offset = 0;
				return MailBnfHelper.ReadDateTime(text, ref offset);
			}
			set
			{
				Parameters["read-date"] = MailBnfHelper.GetDateTimeString(value, null);
			}
		}

		public long Size
		{
			get
			{
				string text = Parameters["size"];
				if (text == null)
				{
					return -1L;
				}
				return long.Parse(text, CultureInfo.InvariantCulture);
			}
			set
			{
				Parameters["size"] = value.ToString(CultureInfo.InvariantCulture);
			}
		}

		internal bool IsChanged
		{
			get
			{
				if (!isChanged)
				{
					if (parameters != null)
					{
						return parameters.IsChanged;
					}
					return false;
				}
				return true;
			}
		}

		public ContentDisposition()
		{
			isChanged = true;
			disposition = "attachment";
			ParseValue();
		}

		public ContentDisposition(string disposition)
		{
			if (disposition == null)
			{
				throw new ArgumentNullException("disposition");
			}
			isChanged = true;
			this.disposition = disposition;
			ParseValue();
		}

		internal void Set(string contentDisposition, HeaderCollection headers)
		{
			disposition = contentDisposition;
			ParseValue();
			headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentDisposition), ToString());
			isPersisted = true;
		}

		internal void PersistIfNeeded(HeaderCollection headers, bool forcePersist)
		{
			if (IsChanged || !isPersisted || forcePersist)
			{
				headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentDisposition), ToString());
				isPersisted = true;
			}
		}

		public override string ToString()
		{
			if (disposition == null || isChanged || (parameters != null && parameters.IsChanged))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(dispositionType);
				foreach (string key in Parameters.Keys)
				{
					stringBuilder.Append("; ");
					stringBuilder.Append(key);
					stringBuilder.Append('=');
					MailBnfHelper.GetTokenOrQuotedString(parameters[key], stringBuilder);
				}
				disposition = stringBuilder.ToString();
				isChanged = false;
				parameters.IsChanged = false;
				isPersisted = false;
			}
			return disposition;
		}

		public override bool Equals(object rparam)
		{
			if (rparam == null)
			{
				return false;
			}
			return string.Compare(ToString(), rparam.ToString(), StringComparison.OrdinalIgnoreCase) == 0;
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		private void ParseValue()
		{
			int offset = 0;
			parameters = new TrackingStringDictionary();
			Exception ex = null;
			try
			{
				dispositionType = MailBnfHelper.ReadToken(disposition, ref offset, null);
				if (dispositionType == null || dispositionType.Length == 0)
				{
					ex = new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
				}
				if (ex == null)
				{
					while (MailBnfHelper.SkipCFWS(disposition, ref offset))
					{
						if (disposition[offset++] != ';')
						{
							ex = new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
						}
						if (MailBnfHelper.SkipCFWS(disposition, ref offset))
						{
							string text = MailBnfHelper.ReadParameterAttribute(disposition, ref offset, null);
							if (disposition[offset++] != '=')
							{
								ex = new FormatException(SR.GetString("MailHeaderFieldMalformedHeader"));
								break;
							}
							string text2 = ((!MailBnfHelper.SkipCFWS(disposition, ref offset)) ? string.Empty : ((disposition[offset] != '"') ? MailBnfHelper.ReadToken(disposition, ref offset, null) : MailBnfHelper.ReadQuotedString(disposition, ref offset, null)));
							if (text == null || text2 == null || text.Length == 0 || text2.Length == 0)
							{
								ex = new FormatException(SR.GetString("ContentDispositionInvalid"));
								break;
							}
							if (string.Compare(text, "creation-date", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(text, "modification-date", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(text, "read-date", StringComparison.OrdinalIgnoreCase) == 0)
							{
								int offset2 = 0;
								MailBnfHelper.ReadDateTime(text2, ref offset2);
							}
							parameters.Add(text, text2);
							continue;
						}
						break;
					}
				}
			}
			catch (FormatException)
			{
				throw new FormatException(SR.GetString("ContentDispositionInvalid"));
			}
			if (ex != null)
			{
				throw ex;
			}
			parameters.IsChanged = false;
		}
	}
}
