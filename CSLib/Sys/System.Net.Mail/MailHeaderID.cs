namespace System.Net.Mail
{
	internal enum MailHeaderID
	{
		Bcc = 0,
		Cc = 1,
		Comments = 2,
		ContentDescription = 3,
		ContentDisposition = 4,
		ContentID = 5,
		ContentLocation = 6,
		ContentTransferEncoding = 7,
		ContentType = 8,
		Date = 9,
		From = 10,
		Importance = 11,
		InReplyTo = 12,
		Keywords = 13,
		Max = 14,
		MessageID = 0xF,
		MimeVersion = 0x10,
		Priority = 17,
		References = 18,
		ReplyTo = 19,
		ResentBcc = 20,
		ResentCc = 21,
		ResentDate = 22,
		ResentFrom = 23,
		ResentMessageID = 24,
		ResentSender = 25,
		ResentTo = 26,
		Sender = 27,
		Subject = 28,
		To = 29,
		XPriority = 30,
		XReceiver = 0x1F,
		XSender = 0x20,
		ZMaxEnumValue = 0x20,
		Unknown = -1
	}
}
