using System.Collections.Specialized;
using System.Net.Mail;
using System.Text;

namespace System.Net.Mime
{
	public class ContentType
	{
		private string mediaType;

		private string subType;

		private bool isChanged;

		private string type;

		private bool isPersisted;

		private TrackingStringDictionary parameters;

		internal static readonly string Default = "application/octet-stream";

		public string Boundary
		{
			get
			{
				return Parameters["boundary"];
			}
			set
			{
				if (value == null || value == string.Empty)
				{
					Parameters.Remove("boundary");
				}
				else
				{
					Parameters["boundary"] = value;
				}
			}
		}

		public string CharSet
		{
			get
			{
				return Parameters["charset"];
			}
			set
			{
				if (value == null || value == string.Empty)
				{
					Parameters.Remove("charset");
				}
				else
				{
					Parameters["charset"] = value;
				}
			}
		}

		public string MediaType
		{
			get
			{
				return mediaType + "/" + subType;
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
				int offset = 0;
				mediaType = MailBnfHelper.ReadToken(value, ref offset, null);
				if (mediaType.Length == 0 || offset >= value.Length || value[offset++] != '/')
				{
					throw new FormatException(SR.GetString("MediaTypeInvalid"));
				}
				subType = MailBnfHelper.ReadToken(value, ref offset, null);
				if (subType.Length == 0 || offset < value.Length)
				{
					throw new FormatException(SR.GetString("MediaTypeInvalid"));
				}
				isChanged = true;
				isPersisted = false;
			}
		}

		public string Name
		{
			get
			{
				string text = Parameters["name"];
				Encoding encoding = MimeBasePart.DecodeEncoding(text);
				if (encoding != null)
				{
					text = MimeBasePart.DecodeHeaderValue(text);
				}
				return text;
			}
			set
			{
				if (value == null || value == string.Empty)
				{
					Parameters.Remove("name");
					return;
				}
				if (MimeBasePart.IsAscii(value, permitCROrLF: false))
				{
					Parameters["name"] = value;
					return;
				}
				Encoding encoding = Encoding.GetEncoding("utf-8");
				Parameters["name"] = MimeBasePart.EncodeHeaderValue(value, encoding, MimeBasePart.ShouldUseBase64Encoding(encoding));
			}
		}

		public StringDictionary Parameters
		{
			get
			{
				if (parameters == null && type == null)
				{
					parameters = new TrackingStringDictionary();
				}
				return parameters;
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

		public ContentType()
			: this(Default)
		{
		}

		public ContentType(string contentType)
		{
			if (contentType == null)
			{
				throw new ArgumentNullException("contentType");
			}
			if (contentType == string.Empty)
			{
				throw new ArgumentException(SR.GetString("net_emptystringcall", "contentType"), "contentType");
			}
			isChanged = true;
			type = contentType;
			ParseValue();
		}

		internal void Set(string contentType, HeaderCollection headers)
		{
			type = contentType;
			ParseValue();
			headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentType), ToString());
			isPersisted = true;
		}

		internal void PersistIfNeeded(HeaderCollection headers, bool forcePersist)
		{
			if (IsChanged || !isPersisted || forcePersist)
			{
				headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentType), ToString());
				isPersisted = true;
			}
		}

		public override string ToString()
		{
			if (type == null || IsChanged)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(mediaType);
				stringBuilder.Append('/');
				stringBuilder.Append(subType);
				foreach (string key in Parameters.Keys)
				{
					stringBuilder.Append("; ");
					stringBuilder.Append(key);
					stringBuilder.Append('=');
					MailBnfHelper.GetTokenOrQuotedString(parameters[key], stringBuilder);
				}
				type = stringBuilder.ToString();
				isChanged = false;
				parameters.IsChanged = false;
				isPersisted = false;
			}
			return type;
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
			Exception ex = null;
			parameters = new TrackingStringDictionary();
			try
			{
				mediaType = MailBnfHelper.ReadToken(type, ref offset, null);
				if (mediaType == null || mediaType.Length == 0 || offset >= type.Length || type[offset++] != '/')
				{
					ex = new FormatException(SR.GetString("ContentTypeInvalid"));
				}
				if (ex == null)
				{
					subType = MailBnfHelper.ReadToken(type, ref offset, null);
					if (subType == null || subType.Length == 0)
					{
						ex = new FormatException(SR.GetString("ContentTypeInvalid"));
					}
				}
				if (ex == null)
				{
					while (MailBnfHelper.SkipCFWS(type, ref offset))
					{
						if (type[offset++] != ';')
						{
							ex = new FormatException(SR.GetString("ContentTypeInvalid"));
							break;
						}
						if (!MailBnfHelper.SkipCFWS(type, ref offset))
						{
							break;
						}
						string text = MailBnfHelper.ReadParameterAttribute(type, ref offset, null);
						if (text == null || text.Length == 0)
						{
							ex = new FormatException(SR.GetString("ContentTypeInvalid"));
							break;
						}
						if (offset >= type.Length || type[offset++] != '=')
						{
							ex = new FormatException(SR.GetString("ContentTypeInvalid"));
							break;
						}
						if (!MailBnfHelper.SkipCFWS(type, ref offset))
						{
							ex = new FormatException(SR.GetString("ContentTypeInvalid"));
							break;
						}
						string text2 = ((type[offset] != '"') ? MailBnfHelper.ReadToken(type, ref offset, null) : MailBnfHelper.ReadQuotedString(type, ref offset, null));
						if (text2 == null)
						{
							ex = new FormatException(SR.GetString("ContentTypeInvalid"));
							break;
						}
						parameters.Add(text, text2);
					}
				}
				parameters.IsChanged = false;
			}
			catch (FormatException)
			{
				throw new FormatException(SR.GetString("ContentTypeInvalid"));
			}
			if (ex != null)
			{
				throw new FormatException(SR.GetString("ContentTypeInvalid"));
			}
		}
	}
}
