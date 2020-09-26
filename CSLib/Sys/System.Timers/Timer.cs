using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Timers
{
	[DefaultEvent("Elapsed")]
	[DefaultProperty("Interval")]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public class Timer : Component, ISupportInitialize
	{
		internal struct FILE_TIME
		{
			internal int ftTimeLow;

			internal int ftTimeHigh;
		}

		private double interval;

		private bool enabled;

		private bool initializing;

		private bool delayedEnable;

		private ElapsedEventHandler onIntervalElapsed;

		private bool autoReset;

		private ISynchronizeInvoke synchronizingObject;

		private bool disposed;

		private System.Threading.Timer timer;

		private TimerCallback callback;

		private object cookie;

		[DefaultValue(true)]
		[TimersDescription("TimerAutoReset")]
		[Category("Behavior")]
		public bool AutoReset
		{
			get
			{
				return autoReset;
			}
			set
			{
				if (base.DesignMode)
				{
					autoReset = value;
				}
				else if (autoReset != value)
				{
					autoReset = value;
					if (timer != null)
					{
						UpdateTimer();
					}
				}
			}
		}

		[TimersDescription("TimerEnabled")]
		[Category("Behavior")]
		[DefaultValue(false)]
		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				if (base.DesignMode)
				{
					delayedEnable = value;
					enabled = value;
				}
				else if (initializing)
				{
					delayedEnable = value;
				}
				else
				{
					if (enabled == value)
					{
						return;
					}
					if (!value)
					{
						if (timer != null)
						{
							cookie = null;
							timer.Dispose();
							timer = null;
						}
						enabled = value;
						return;
					}
					enabled = value;
					if (timer == null)
					{
						if (disposed)
						{
							throw new ObjectDisposedException(GetType().Name);
						}
						int num = (int)Math.Ceiling(interval);
						cookie = new object();
						timer = new System.Threading.Timer(callback, cookie, num, autoReset ? num : (-1));
					}
					else
					{
						UpdateTimer();
					}
				}
			}
		}

		[RecommendedAsConfigurable(true)]
		[Category("Behavior")]
		[DefaultValue(100.0)]
		[TimersDescription("TimerInterval")]
		public double Interval
		{
			get
			{
				return interval;
			}
			set
			{
				if (value <= 0.0)
				{
					throw new ArgumentException(SR.GetString("TimerInvalidInterval", value, 0));
				}
				interval = value;
				if (timer != null)
				{
					UpdateTimer();
				}
			}
		}

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
				if (base.DesignMode)
				{
					enabled = true;
				}
			}
		}

		[TimersDescription("TimerSynchronizingObject")]
		[DefaultValue(null)]
		[Browsable(false)]
		public ISynchronizeInvoke SynchronizingObject
		{
			get
			{
				if (synchronizingObject == null && base.DesignMode)
				{
					IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
					if (designerHost != null)
					{
						object rootComponent = designerHost.RootComponent;
						if (rootComponent != null && rootComponent is ISynchronizeInvoke)
						{
							synchronizingObject = (ISynchronizeInvoke)rootComponent;
						}
					}
				}
				return synchronizingObject;
			}
			set
			{
				synchronizingObject = value;
			}
		}

		[Category("Behavior")]
		[TimersDescription("TimerIntervalElapsed")]
		public event ElapsedEventHandler Elapsed
		{
			add
			{
				onIntervalElapsed = (ElapsedEventHandler)Delegate.Combine(onIntervalElapsed, value);
			}
			remove
			{
				onIntervalElapsed = (ElapsedEventHandler)Delegate.Remove(onIntervalElapsed, value);
			}
		}

		public Timer()
		{
			interval = 100.0;
			enabled = false;
			autoReset = true;
			initializing = false;
			delayedEnable = false;
			callback = MyTimerCallback;
		}

		public Timer(double interval)
			: this()
		{
			if (interval <= 0.0)
			{
				throw new ArgumentException(SR.GetString("InvalidParameter", "interval", interval));
			}
			int num = (int)Math.Ceiling(interval);
			if (num < 0)
			{
				throw new ArgumentException(SR.GetString("InvalidParameter", "interval", interval));
			}
			this.interval = interval;
		}

		private void UpdateTimer()
		{
			int num = (int)Math.Ceiling(interval);
			timer.Change(num, autoReset ? num : (-1));
		}

		public void BeginInit()
		{
			Close();
			initializing = true;
		}

		public void Close()
		{
			initializing = false;
			delayedEnable = false;
			enabled = false;
			if (timer != null)
			{
				timer.Dispose();
				timer = null;
			}
		}

		protected override void Dispose(bool disposing)
		{
			Close();
			disposed = true;
			base.Dispose(disposing);
		}

		public void EndInit()
		{
			initializing = false;
			Enabled = delayedEnable;
		}

		public void Start()
		{
			Enabled = true;
		}

		public void Stop()
		{
			Enabled = false;
		}

		private void MyTimerCallback(object state)
		{
			if (state != cookie)
			{
				return;
			}
			if (!autoReset)
			{
				enabled = false;
			}
			FILE_TIME lpSystemTimeAsFileTime = default(FILE_TIME);
			GetSystemTimeAsFileTime(ref lpSystemTimeAsFileTime);
			ElapsedEventArgs elapsedEventArgs = new ElapsedEventArgs(lpSystemTimeAsFileTime.ftTimeLow, lpSystemTimeAsFileTime.ftTimeHigh);
			try
			{
				ElapsedEventHandler elapsedEventHandler = onIntervalElapsed;
				if (elapsedEventHandler != null)
				{
					if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
					{
						SynchronizingObject.BeginInvoke(elapsedEventHandler, new object[2]
						{
							this,
							elapsedEventArgs
						});
					}
					else
					{
						elapsedEventHandler(this, elapsedEventArgs);
					}
				}
			}
			catch
			{
			}
		}

		[DllImport("kernel32.dll")]
		[SuppressUnmanagedCodeSecurity]
		internal static extern void GetSystemTimeAsFileTime(ref FILE_TIME lpSystemTimeAsFileTime);
	}
}
