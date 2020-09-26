using System.Collections.Generic;

namespace System.Net.Mail
{
	internal static class MailHeaderInfo
	{
		private struct HeaderInfo
		{
			public readonly string NormalizedName;

			public readonly bool IsSingleton;

			public readonly MailHeaderID ID;

			public HeaderInfo(MailHeaderID id, string name, bool isSingleton)
			{
				ID = id;
				NormalizedName = name;
				IsSingleton = isSingleton;
			}
		}

		private static readonly HeaderInfo[] m_HeaderInfo;

		private static readonly Dictionary<string, int> m_HeaderDictionary;

		static MailHeaderInfo()
		{
			m_HeaderInfo = new HeaderInfo[33]
			{
				new HeaderInfo(MailHeaderID.Bcc, "Bcc", isSingleton: true),
				new HeaderInfo(MailHeaderID.Cc, "Cc", isSingleton: true),
				new HeaderInfo(MailHeaderID.Comments, "Comments", isSingleton: false),
				new HeaderInfo(MailHeaderID.ContentDescription, "Content-Description", isSingleton: true),
				new HeaderInfo(MailHeaderID.ContentDisposition, "Content-Disposition", isSingleton: true),
				new HeaderInfo(MailHeaderID.ContentID, "Content-ID", isSingleton: true),
				new HeaderInfo(MailHeaderID.ContentLocation, "Content-Location", isSingleton: true),
				new HeaderInfo(MailHeaderID.ContentTransferEncoding, "Content-Transfer-Encoding", isSingleton: true),
				new HeaderInfo(MailHeaderID.ContentType, "Content-Type", isSingleton: true),
				new HeaderInfo(MailHeaderID.Date, "Date", isSingleton: true),
				new HeaderInfo(MailHeaderID.From, "From", isSingleton: true),
				new HeaderInfo(MailHeaderID.Importance, "Importance", isSingleton: true),
				new HeaderInfo(MailHeaderID.InReplyTo, "In-Reply-To", isSingleton: true),
				new HeaderInfo(MailHeaderID.Keywords, "Keywords", isSingleton: false),
				new HeaderInfo(MailHeaderID.Max, "Max", isSingleton: false),
				new HeaderInfo(MailHeaderID.MessageID, "Message-ID", isSingleton: true),
				new HeaderInfo(MailHeaderID.MimeVersion, "MIME-Version", isSingleton: true),
				new HeaderInfo(MailHeaderID.Priority, "Priority", isSingleton: true),
				new HeaderInfo(MailHeaderID.References, "References", isSingleton: true),
				new HeaderInfo(MailHeaderID.ReplyTo, "Reply-To", isSingleton: true),
				new HeaderInfo(MailHeaderID.ResentBcc, "Resent-Bcc", isSingleton: false),
				new HeaderInfo(MailHeaderID.ResentCc, "Resent-Cc", isSingleton: false),
				new HeaderInfo(MailHeaderID.ResentDate, "Resent-Date", isSingleton: false),
				new HeaderInfo(MailHeaderID.ResentFrom, "Resent-From", isSingleton: false),
				new HeaderInfo(MailHeaderID.ResentMessageID, "Resent-Message-ID", isSingleton: false),
				new HeaderInfo(MailHeaderID.ResentSender, "Resent-Sender", isSingleton: false),
				new HeaderInfo(MailHeaderID.ResentTo, "Resent-To", isSingleton: false),
				new HeaderInfo(MailHeaderID.Sender, "Sender", isSingleton: true),
				new HeaderInfo(MailHeaderID.Subject, "Subject", isSingleton: true),
				new HeaderInfo(MailHeaderID.To, "To", isSingleton: true),
				new HeaderInfo(MailHeaderID.XPriority, "X-Priority", isSingleton: true),
				new HeaderInfo(MailHeaderID.XReceiver, "X-Receiver", isSingleton: false),
				new HeaderInfo(MailHeaderID.XSender, "X-Sender", isSingleton: true)
			};
			m_HeaderDictionary = new Dictionary<string, int>(33, StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < m_HeaderInfo.Length; i++)
			{
				m_HeaderDictionary.Add(m_HeaderInfo[i].NormalizedName, i);
			}
		}

		internal static string GetString(MailHeaderID id)
		{
			if (id == MailHeaderID.Unknown || id == (MailHeaderID)33)
			{
				return null;
			}
			return m_HeaderInfo[(int)id].NormalizedName;
		}

		internal static MailHeaderID GetID(string name)
		{
			if (m_HeaderDictionary.TryGetValue(name, out var value))
			{
				return (MailHeaderID)value;
			}
			return MailHeaderID.Unknown;
		}

		internal static bool IsWellKnown(string name)
		{
			int value;
			return m_HeaderDictionary.TryGetValue(name, out value);
		}

		internal static bool IsSingleton(string name)
		{
			if (m_HeaderDictionary.TryGetValue(name, out var value))
			{
				return m_HeaderInfo[value].IsSingleton;
			}
			return false;
		}

		internal static string NormalizeCase(string name)
		{
			if (m_HeaderDictionary.TryGetValue(name, out var value))
			{
				return m_HeaderInfo[value].NormalizedName;
			}
			return name;
		}

		internal static bool IsMatch(string name, MailHeaderID header)
		{
			if (m_HeaderDictionary.TryGetValue(name, out var value) && value == (int)header)
			{
				return true;
			}
			return false;
		}
	}
}
