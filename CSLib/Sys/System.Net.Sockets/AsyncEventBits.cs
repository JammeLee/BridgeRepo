namespace System.Net.Sockets
{
	[Flags]
	internal enum AsyncEventBits
	{
		FdNone = 0x0,
		FdRead = 0x1,
		FdWrite = 0x2,
		FdOob = 0x4,
		FdAccept = 0x8,
		FdConnect = 0x10,
		FdClose = 0x20,
		FdQos = 0x40,
		FdGroupQos = 0x80,
		FdRoutingInterfaceChange = 0x100,
		FdAddressListChange = 0x200,
		FdAllEvents = 0x3FF
	}
}
