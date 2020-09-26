using System.Collections;

namespace System.Security.Authentication.ExtendedProtection
{
	public class ServiceNameCollection : ReadOnlyCollectionBase
	{
		public ServiceNameCollection(ICollection items)
		{
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}
			base.InnerList.AddRange(items);
		}

		public ServiceNameCollection Merge(string serviceName)
		{
			ArrayList arrayList = new ArrayList();
			arrayList.AddRange(base.InnerList);
			AddIfNew(arrayList, serviceName);
			return new ServiceNameCollection(arrayList);
		}

		public ServiceNameCollection Merge(IEnumerable serviceNames)
		{
			ArrayList arrayList = new ArrayList();
			arrayList.AddRange(base.InnerList);
			foreach (object serviceName in serviceNames)
			{
				AddIfNew(arrayList, serviceName as string);
			}
			return new ServiceNameCollection(arrayList);
		}

		private void AddIfNew(ArrayList newServiceNames, string serviceName)
		{
			if (string.IsNullOrEmpty(serviceName))
			{
				throw new ArgumentException(SR.GetString("security_ServiceNameCollection_EmptyServiceName"));
			}
			if (!Contains(serviceName, newServiceNames))
			{
				newServiceNames.Add(serviceName);
			}
		}

		private bool Contains(string searchServiceName, ICollection serviceNames)
		{
			bool result = false;
			foreach (string serviceName in serviceNames)
			{
				if (string.Compare(serviceName, searchServiceName, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					return true;
				}
			}
			return result;
		}
	}
}
