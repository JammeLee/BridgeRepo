using System.Collections;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public class MenuCommand
	{
		private const int ENABLED = 2;

		private const int INVISIBLE = 16;

		private const int CHECKED = 4;

		private const int SUPPORTED = 1;

		private EventHandler execHandler;

		private EventHandler statusHandler;

		private CommandID commandID;

		private int status;

		private IDictionary properties;

		public virtual bool Checked
		{
			get
			{
				return (status & 4) != 0;
			}
			set
			{
				SetStatus(4, value);
			}
		}

		public virtual bool Enabled
		{
			get
			{
				return (status & 2) != 0;
			}
			set
			{
				SetStatus(2, value);
			}
		}

		public virtual IDictionary Properties
		{
			get
			{
				if (properties == null)
				{
					properties = new HybridDictionary();
				}
				return properties;
			}
		}

		public virtual bool Supported
		{
			get
			{
				return (status & 1) != 0;
			}
			set
			{
				SetStatus(1, value);
			}
		}

		public virtual bool Visible
		{
			get
			{
				return (status & 0x10) == 0;
			}
			set
			{
				SetStatus(16, !value);
			}
		}

		public virtual CommandID CommandID => commandID;

		public virtual int OleStatus => status;

		public event EventHandler CommandChanged
		{
			add
			{
				statusHandler = (EventHandler)Delegate.Combine(statusHandler, value);
			}
			remove
			{
				statusHandler = (EventHandler)Delegate.Remove(statusHandler, value);
			}
		}

		public MenuCommand(EventHandler handler, CommandID command)
		{
			execHandler = handler;
			commandID = command;
			status = 3;
		}

		private void SetStatus(int mask, bool value)
		{
			int num = status;
			num = ((!value) ? (num & ~mask) : (num | mask));
			if (num != status)
			{
				status = num;
				OnCommandChanged(EventArgs.Empty);
			}
		}

		public virtual void Invoke()
		{
			if (execHandler == null)
			{
				return;
			}
			try
			{
				execHandler(this, EventArgs.Empty);
			}
			catch (CheckoutException ex)
			{
				if (ex == CheckoutException.Canceled)
				{
					return;
				}
				throw;
			}
		}

		public virtual void Invoke(object arg)
		{
			Invoke();
		}

		protected virtual void OnCommandChanged(EventArgs e)
		{
			if (statusHandler != null)
			{
				statusHandler(this, e);
			}
		}

		public override string ToString()
		{
			string text = commandID.ToString() + " : ";
			if (((uint)status & (true ? 1u : 0u)) != 0)
			{
				text += "Supported";
			}
			if (((uint)status & 2u) != 0)
			{
				text += "|Enabled";
			}
			if ((status & 0x10) == 0)
			{
				text += "|Visible";
			}
			if (((uint)status & 4u) != 0)
			{
				text += "|Checked";
			}
			return text;
		}
	}
}
