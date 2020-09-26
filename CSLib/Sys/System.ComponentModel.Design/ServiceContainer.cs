using System.Collections;
using System.Diagnostics;
using System.Security.Permissions;

namespace System.ComponentModel.Design
{
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public class ServiceContainer : IServiceContainer, IServiceProvider, IDisposable
	{
		private Hashtable services;

		private IServiceProvider parentProvider;

		private static Type[] _defaultServices = new Type[2]
		{
			typeof(IServiceContainer),
			typeof(ServiceContainer)
		};

		private static TraceSwitch TRACESERVICE = new TraceSwitch("TRACESERVICE", "ServiceProvider: Trace service provider requests.");

		private IServiceContainer Container
		{
			get
			{
				IServiceContainer result = null;
				if (parentProvider != null)
				{
					result = (IServiceContainer)parentProvider.GetService(typeof(IServiceContainer));
				}
				return result;
			}
		}

		protected virtual Type[] DefaultServices => _defaultServices;

		private Hashtable Services
		{
			get
			{
				if (services == null)
				{
					services = new Hashtable();
				}
				return services;
			}
		}

		public ServiceContainer()
		{
		}

		public ServiceContainer(IServiceProvider parentProvider)
		{
			this.parentProvider = parentProvider;
		}

		public void AddService(Type serviceType, object serviceInstance)
		{
			AddService(serviceType, serviceInstance, promote: false);
		}

		public virtual void AddService(Type serviceType, object serviceInstance, bool promote)
		{
			if (promote)
			{
				IServiceContainer container = Container;
				if (container != null)
				{
					container.AddService(serviceType, serviceInstance, promote);
					return;
				}
			}
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			if (serviceInstance == null)
			{
				throw new ArgumentNullException("serviceInstance");
			}
			if (!(serviceInstance is ServiceCreatorCallback) && !serviceInstance.GetType().IsCOMObject && !serviceType.IsAssignableFrom(serviceInstance.GetType()))
			{
				throw new ArgumentException(SR.GetString("ErrorInvalidServiceInstance", serviceType.FullName));
			}
			if (Services.ContainsKey(serviceType))
			{
				throw new ArgumentException(SR.GetString("ErrorServiceExists", serviceType.FullName), "serviceType");
			}
			Services[serviceType] = serviceInstance;
		}

		public void AddService(Type serviceType, ServiceCreatorCallback callback)
		{
			AddService(serviceType, callback, promote: false);
		}

		public virtual void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
		{
			if (promote)
			{
				IServiceContainer container = Container;
				if (container != null)
				{
					container.AddService(serviceType, callback, promote);
					return;
				}
			}
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}
			if (Services.ContainsKey(serviceType))
			{
				throw new ArgumentException(SR.GetString("ErrorServiceExists", serviceType.FullName), "serviceType");
			}
			Services[serviceType] = callback;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}
			Hashtable hashtable = services;
			services = null;
			if (hashtable == null)
			{
				return;
			}
			foreach (object value in hashtable.Values)
			{
				if (value is IDisposable)
				{
					((IDisposable)value).Dispose();
				}
			}
		}

		public virtual object GetService(Type serviceType)
		{
			object obj = null;
			Type[] defaultServices = DefaultServices;
			for (int i = 0; i < defaultServices.Length; i++)
			{
				if (serviceType == defaultServices[i])
				{
					obj = this;
					break;
				}
			}
			if (obj == null)
			{
				obj = Services[serviceType];
			}
			if (obj is ServiceCreatorCallback)
			{
				obj = ((ServiceCreatorCallback)obj)(this, serviceType);
				if (obj != null && !obj.GetType().IsCOMObject && !serviceType.IsAssignableFrom(obj.GetType()))
				{
					obj = null;
				}
				Services[serviceType] = obj;
			}
			if (obj == null && parentProvider != null)
			{
				obj = parentProvider.GetService(serviceType);
			}
			return obj;
		}

		public void RemoveService(Type serviceType)
		{
			RemoveService(serviceType, promote: false);
		}

		public virtual void RemoveService(Type serviceType, bool promote)
		{
			if (promote)
			{
				IServiceContainer container = Container;
				if (container != null)
				{
					container.RemoveService(serviceType, promote);
					return;
				}
			}
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			Services.Remove(serviceType);
		}
	}
}
