#define TRACE
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Permissions;

namespace System.Diagnostics
{
	public class TraceSource
	{
		private static List<WeakReference> tracesources = new List<WeakReference>();

		private readonly TraceEventCache manager = new TraceEventCache();

		private SourceSwitch internalSwitch;

		private TraceListenerCollection listeners;

		private StringDictionary attributes;

		private SourceLevels switchLevel;

		private string sourceName;

		internal bool _initCalled;

		public StringDictionary Attributes
		{
			get
			{
				Initialize();
				if (attributes == null)
				{
					attributes = new StringDictionary();
				}
				return attributes;
			}
		}

		public string Name => sourceName;

		public TraceListenerCollection Listeners
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				Initialize();
				return listeners;
			}
		}

		public SourceSwitch Switch
		{
			get
			{
				Initialize();
				return internalSwitch;
			}
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Switch");
				}
				Initialize();
				internalSwitch = value;
			}
		}

		public TraceSource(string name)
			: this(name, SourceLevels.Off)
		{
		}

		public TraceSource(string name, SourceLevels defaultLevel)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException("name");
			}
			sourceName = name;
			switchLevel = defaultLevel;
			lock (tracesources)
			{
				tracesources.Add(new WeakReference(this));
			}
		}

		private void Initialize()
		{
			if (_initCalled)
			{
				return;
			}
			lock (this)
			{
				if (_initCalled)
				{
					return;
				}
				SourceElementsCollection sources = DiagnosticsConfiguration.Sources;
				if (sources != null)
				{
					SourceElement sourceElement = sources[sourceName];
					if (sourceElement != null)
					{
						if (!string.IsNullOrEmpty(sourceElement.SwitchName))
						{
							CreateSwitch(sourceElement.SwitchType, sourceElement.SwitchName);
						}
						else
						{
							CreateSwitch(sourceElement.SwitchType, sourceName);
							if (!string.IsNullOrEmpty(sourceElement.SwitchValue))
							{
								internalSwitch.Level = (SourceLevels)Enum.Parse(typeof(SourceLevels), sourceElement.SwitchValue);
							}
						}
						listeners = sourceElement.Listeners.GetRuntimeObject();
						attributes = new StringDictionary();
						TraceUtils.VerifyAttributes(sourceElement.Attributes, GetSupportedAttributes(), this);
						attributes.contents = sourceElement.Attributes;
					}
					else
					{
						NoConfigInit();
					}
				}
				else
				{
					NoConfigInit();
				}
				_initCalled = true;
			}
		}

		private void NoConfigInit()
		{
			internalSwitch = new SourceSwitch(sourceName, switchLevel.ToString());
			listeners = new TraceListenerCollection();
			listeners.Add(new DefaultTraceListener());
			attributes = null;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public void Close()
		{
			if (listeners == null)
			{
				return;
			}
			lock (TraceInternal.critSec)
			{
				foreach (TraceListener listener in listeners)
				{
					listener.Close();
				}
			}
		}

		public void Flush()
		{
			if (listeners == null)
			{
				return;
			}
			if (TraceInternal.UseGlobalLock)
			{
				lock (TraceInternal.critSec)
				{
					foreach (TraceListener listener in listeners)
					{
						listener.Flush();
					}
				}
				return;
			}
			foreach (TraceListener listener2 in listeners)
			{
				if (!listener2.IsThreadSafe)
				{
					lock (listener2)
					{
						listener2.Flush();
					}
				}
				else
				{
					listener2.Flush();
				}
			}
		}

		protected internal virtual string[] GetSupportedAttributes()
		{
			return null;
		}

		internal static void RefreshAll()
		{
			lock (tracesources)
			{
				for (int i = 0; i < tracesources.Count; i++)
				{
					((TraceSource)tracesources[i].Target)?.Refresh();
				}
			}
		}

		internal void Refresh()
		{
			if (!_initCalled)
			{
				Initialize();
				return;
			}
			SourceElementsCollection sources = DiagnosticsConfiguration.Sources;
			if (sources == null)
			{
				return;
			}
			SourceElement sourceElement = sources[Name];
			if (sourceElement != null)
			{
				if ((string.IsNullOrEmpty(sourceElement.SwitchType) && internalSwitch.GetType() != typeof(SourceSwitch)) || sourceElement.SwitchType != internalSwitch.GetType().AssemblyQualifiedName)
				{
					if (!string.IsNullOrEmpty(sourceElement.SwitchName))
					{
						CreateSwitch(sourceElement.SwitchType, sourceElement.SwitchName);
					}
					else
					{
						CreateSwitch(sourceElement.SwitchType, Name);
						if (!string.IsNullOrEmpty(sourceElement.SwitchValue))
						{
							internalSwitch.Level = (SourceLevels)Enum.Parse(typeof(SourceLevels), sourceElement.SwitchValue);
						}
					}
				}
				else if (!string.IsNullOrEmpty(sourceElement.SwitchName))
				{
					if (sourceElement.SwitchName != internalSwitch.DisplayName)
					{
						CreateSwitch(sourceElement.SwitchType, sourceElement.SwitchName);
					}
					else
					{
						internalSwitch.Refresh();
					}
				}
				else if (!string.IsNullOrEmpty(sourceElement.SwitchValue))
				{
					internalSwitch.Level = (SourceLevels)Enum.Parse(typeof(SourceLevels), sourceElement.SwitchValue);
				}
				else
				{
					internalSwitch.Level = SourceLevels.Off;
				}
				TraceListenerCollection traceListenerCollection = new TraceListenerCollection();
				foreach (ListenerElement listener in sourceElement.Listeners)
				{
					TraceListener traceListener = listeners[listener.Name];
					if (traceListener != null)
					{
						traceListenerCollection.Add(listener.RefreshRuntimeObject(traceListener));
					}
					else
					{
						traceListenerCollection.Add(listener.GetRuntimeObject());
					}
				}
				TraceUtils.VerifyAttributes(sourceElement.Attributes, GetSupportedAttributes(), this);
				attributes = new StringDictionary();
				attributes.contents = sourceElement.Attributes;
				listeners = traceListenerCollection;
			}
			else
			{
				internalSwitch.Level = switchLevel;
				listeners.Clear();
				attributes = null;
			}
		}

		[Conditional("TRACE")]
		public void TraceEvent(TraceEventType eventType, int id)
		{
			Initialize();
			if (!internalSwitch.ShouldTrace(eventType) || listeners == null)
			{
				return;
			}
			if (TraceInternal.UseGlobalLock)
			{
				lock (TraceInternal.critSec)
				{
					for (int i = 0; i < listeners.Count; i++)
					{
						TraceListener traceListener = listeners[i];
						traceListener.TraceEvent(manager, Name, eventType, id);
						if (Trace.AutoFlush)
						{
							traceListener.Flush();
						}
					}
				}
			}
			else
			{
				for (int j = 0; j < listeners.Count; j++)
				{
					TraceListener traceListener2 = listeners[j];
					if (!traceListener2.IsThreadSafe)
					{
						lock (traceListener2)
						{
							traceListener2.TraceEvent(manager, Name, eventType, id);
							if (Trace.AutoFlush)
							{
								traceListener2.Flush();
							}
						}
					}
					else
					{
						traceListener2.TraceEvent(manager, Name, eventType, id);
						if (Trace.AutoFlush)
						{
							traceListener2.Flush();
						}
					}
				}
			}
			manager.Clear();
		}

		[Conditional("TRACE")]
		public void TraceEvent(TraceEventType eventType, int id, string message)
		{
			Initialize();
			if (!internalSwitch.ShouldTrace(eventType) || listeners == null)
			{
				return;
			}
			if (TraceInternal.UseGlobalLock)
			{
				lock (TraceInternal.critSec)
				{
					for (int i = 0; i < listeners.Count; i++)
					{
						TraceListener traceListener = listeners[i];
						traceListener.TraceEvent(manager, Name, eventType, id, message);
						if (Trace.AutoFlush)
						{
							traceListener.Flush();
						}
					}
				}
			}
			else
			{
				for (int j = 0; j < listeners.Count; j++)
				{
					TraceListener traceListener2 = listeners[j];
					if (!traceListener2.IsThreadSafe)
					{
						lock (traceListener2)
						{
							traceListener2.TraceEvent(manager, Name, eventType, id, message);
							if (Trace.AutoFlush)
							{
								traceListener2.Flush();
							}
						}
					}
					else
					{
						traceListener2.TraceEvent(manager, Name, eventType, id, message);
						if (Trace.AutoFlush)
						{
							traceListener2.Flush();
						}
					}
				}
			}
			manager.Clear();
		}

		[Conditional("TRACE")]
		public void TraceEvent(TraceEventType eventType, int id, string format, params object[] args)
		{
			Initialize();
			if (!internalSwitch.ShouldTrace(eventType) || listeners == null)
			{
				return;
			}
			if (TraceInternal.UseGlobalLock)
			{
				lock (TraceInternal.critSec)
				{
					for (int i = 0; i < listeners.Count; i++)
					{
						TraceListener traceListener = listeners[i];
						traceListener.TraceEvent(manager, Name, eventType, id, format, args);
						if (Trace.AutoFlush)
						{
							traceListener.Flush();
						}
					}
				}
			}
			else
			{
				for (int j = 0; j < listeners.Count; j++)
				{
					TraceListener traceListener2 = listeners[j];
					if (!traceListener2.IsThreadSafe)
					{
						lock (traceListener2)
						{
							traceListener2.TraceEvent(manager, Name, eventType, id, format, args);
							if (Trace.AutoFlush)
							{
								traceListener2.Flush();
							}
						}
					}
					else
					{
						traceListener2.TraceEvent(manager, Name, eventType, id, format, args);
						if (Trace.AutoFlush)
						{
							traceListener2.Flush();
						}
					}
				}
			}
			manager.Clear();
		}

		[Conditional("TRACE")]
		public void TraceData(TraceEventType eventType, int id, object data)
		{
			Initialize();
			if (!internalSwitch.ShouldTrace(eventType) || listeners == null)
			{
				return;
			}
			if (TraceInternal.UseGlobalLock)
			{
				lock (TraceInternal.critSec)
				{
					for (int i = 0; i < listeners.Count; i++)
					{
						TraceListener traceListener = listeners[i];
						traceListener.TraceData(manager, Name, eventType, id, data);
						if (Trace.AutoFlush)
						{
							traceListener.Flush();
						}
					}
				}
			}
			else
			{
				for (int j = 0; j < listeners.Count; j++)
				{
					TraceListener traceListener2 = listeners[j];
					if (!traceListener2.IsThreadSafe)
					{
						lock (traceListener2)
						{
							traceListener2.TraceData(manager, Name, eventType, id, data);
							if (Trace.AutoFlush)
							{
								traceListener2.Flush();
							}
						}
					}
					else
					{
						traceListener2.TraceData(manager, Name, eventType, id, data);
						if (Trace.AutoFlush)
						{
							traceListener2.Flush();
						}
					}
				}
			}
			manager.Clear();
		}

		[Conditional("TRACE")]
		public void TraceData(TraceEventType eventType, int id, params object[] data)
		{
			Initialize();
			if (!internalSwitch.ShouldTrace(eventType) || listeners == null)
			{
				return;
			}
			if (TraceInternal.UseGlobalLock)
			{
				lock (TraceInternal.critSec)
				{
					for (int i = 0; i < listeners.Count; i++)
					{
						TraceListener traceListener = listeners[i];
						traceListener.TraceData(manager, Name, eventType, id, data);
						if (Trace.AutoFlush)
						{
							traceListener.Flush();
						}
					}
				}
			}
			else
			{
				for (int j = 0; j < listeners.Count; j++)
				{
					TraceListener traceListener2 = listeners[j];
					if (!traceListener2.IsThreadSafe)
					{
						lock (traceListener2)
						{
							traceListener2.TraceData(manager, Name, eventType, id, data);
							if (Trace.AutoFlush)
							{
								traceListener2.Flush();
							}
						}
					}
					else
					{
						traceListener2.TraceData(manager, Name, eventType, id, data);
						if (Trace.AutoFlush)
						{
							traceListener2.Flush();
						}
					}
				}
			}
			manager.Clear();
		}

		[Conditional("TRACE")]
		public void TraceInformation(string message)
		{
			TraceEvent(TraceEventType.Information, 0, message, null);
		}

		[Conditional("TRACE")]
		public void TraceInformation(string format, params object[] args)
		{
			TraceEvent(TraceEventType.Information, 0, format, args);
		}

		[Conditional("TRACE")]
		public void TraceTransfer(int id, string message, Guid relatedActivityId)
		{
			Initialize();
			if (!internalSwitch.ShouldTrace(TraceEventType.Transfer) || listeners == null)
			{
				return;
			}
			if (TraceInternal.UseGlobalLock)
			{
				lock (TraceInternal.critSec)
				{
					for (int i = 0; i < listeners.Count; i++)
					{
						TraceListener traceListener = listeners[i];
						traceListener.TraceTransfer(manager, Name, id, message, relatedActivityId);
						if (Trace.AutoFlush)
						{
							traceListener.Flush();
						}
					}
				}
			}
			else
			{
				for (int j = 0; j < listeners.Count; j++)
				{
					TraceListener traceListener2 = listeners[j];
					if (!traceListener2.IsThreadSafe)
					{
						lock (traceListener2)
						{
							traceListener2.TraceTransfer(manager, Name, id, message, relatedActivityId);
							if (Trace.AutoFlush)
							{
								traceListener2.Flush();
							}
						}
					}
					else
					{
						traceListener2.TraceTransfer(manager, Name, id, message, relatedActivityId);
						if (Trace.AutoFlush)
						{
							traceListener2.Flush();
						}
					}
				}
			}
			manager.Clear();
		}

		private void CreateSwitch(string typename, string name)
		{
			if (!string.IsNullOrEmpty(typename))
			{
				internalSwitch = (SourceSwitch)TraceUtils.GetRuntimeObject(typename, typeof(SourceSwitch), name);
			}
			else
			{
				internalSwitch = new SourceSwitch(name, switchLevel.ToString());
			}
		}
	}
}
