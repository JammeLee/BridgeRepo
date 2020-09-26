using System.IO;
using System.Net.Mime;
using System.Text;

namespace System.Net.Mail
{
	public class Attachment : AttachmentBase
	{
		private string name;

		private Encoding nameEncoding;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				Encoding encoding = MimeBasePart.DecodeEncoding(value);
				if (encoding != null)
				{
					nameEncoding = encoding;
					name = MimeBasePart.DecodeHeaderValue(value);
					base.MimePart.ContentType.Name = value;
				}
				else
				{
					name = value;
					SetContentTypeName();
				}
			}
		}

		public Encoding NameEncoding
		{
			get
			{
				return nameEncoding;
			}
			set
			{
				nameEncoding = value;
				if (name != null && name != string.Empty)
				{
					SetContentTypeName();
				}
			}
		}

		public ContentDisposition ContentDisposition
		{
			get
			{
				ContentDisposition contentDisposition = base.MimePart.ContentDisposition;
				if (contentDisposition == null)
				{
					contentDisposition = new ContentDisposition();
					base.MimePart.ContentDisposition = contentDisposition;
				}
				return contentDisposition;
			}
		}

		internal Attachment()
		{
		}

		public Attachment(string fileName)
			: base(fileName)
		{
			Name = AttachmentBase.ShortNameFromFile(fileName);
		}

		public Attachment(string fileName, string mediaType)
			: base(fileName, mediaType)
		{
			Name = AttachmentBase.ShortNameFromFile(fileName);
		}

		public Attachment(string fileName, ContentType contentType)
			: base(fileName, contentType)
		{
			if (contentType.Name == null || contentType.Name == string.Empty)
			{
				Name = AttachmentBase.ShortNameFromFile(fileName);
			}
			else
			{
				Name = contentType.Name;
			}
		}

		public Attachment(Stream contentStream, string name)
			: base(contentStream, null, null)
		{
			Name = name;
		}

		public Attachment(Stream contentStream, string name, string mediaType)
			: base(contentStream, null, mediaType)
		{
			Name = name;
		}

		public Attachment(Stream contentStream, ContentType contentType)
			: base(contentStream, contentType)
		{
			Name = contentType.Name;
		}

		internal void SetContentTypeName()
		{
			if (name != null && name.Length != 0 && !MimeBasePart.IsAscii(name, permitCROrLF: false))
			{
				Encoding encoding = NameEncoding;
				if (encoding == null)
				{
					encoding = Encoding.GetEncoding("utf-8");
				}
				base.MimePart.ContentType.Name = MimeBasePart.EncodeHeaderValue(name, encoding, MimeBasePart.ShouldUseBase64Encoding(encoding));
			}
			else
			{
				base.MimePart.ContentType.Name = name;
			}
		}

		internal override void PrepareForSending()
		{
			if (name != null && name != string.Empty)
			{
				SetContentTypeName();
			}
			base.PrepareForSending();
		}

		public static Attachment CreateAttachmentFromString(string content, string name)
		{
			Attachment attachment = new Attachment();
			attachment.SetContentFromString(content, null, string.Empty);
			attachment.Name = name;
			return attachment;
		}

		public static Attachment CreateAttachmentFromString(string content, string name, Encoding contentEncoding, string mediaType)
		{
			Attachment attachment = new Attachment();
			attachment.SetContentFromString(content, contentEncoding, mediaType);
			attachment.Name = name;
			return attachment;
		}

		public static Attachment CreateAttachmentFromString(string content, ContentType contentType)
		{
			Attachment attachment = new Attachment();
			attachment.SetContentFromString(content, contentType);
			attachment.Name = contentType.Name;
			return attachment;
		}
	}
}
