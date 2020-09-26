using System.Collections;
using System.Security.Permissions;

namespace System.ComponentModel
{
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	internal sealed class WeakHashtable : Hashtable
	{
		private class WeakKeyComparer : IEqualityComparer
		{
			bool IEqualityComparer.Equals(object x, object y)
			{
				if (object.ReferenceEquals(x, y))
				{
					return true;
				}
				if (x == null || y == null)
				{
					return false;
				}
				if (x.GetHashCode() == y.GetHashCode())
				{
					WeakReference weakReference = x as WeakReference;
					WeakReference weakReference2 = y as WeakReference;
					if (weakReference != null)
					{
						if (!weakReference.IsAlive)
						{
							return false;
						}
						x = weakReference.Target;
					}
					if (weakReference2 != null)
					{
						if (!weakReference2.IsAlive)
						{
							return false;
						}
						y = weakReference2.Target;
					}
					return object.ReferenceEquals(x, y);
				}
				return false;
			}

			int IEqualityComparer.GetHashCode(object obj)
			{
				return obj.GetHashCode();
			}
		}

		private sealed class EqualityWeakReference : WeakReference
		{
			private int _hashCode;

			internal EqualityWeakReference(object o)
				: base(o)
			{
				_hashCode = o.GetHashCode();
			}

			public override bool Equals(object o)
			{
				if (o == null)
				{
					return false;
				}
				if (o.GetHashCode() != _hashCode)
				{
					return false;
				}
				if (o == this || (IsAlive && object.ReferenceEquals(o, Target)))
				{
					return true;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return _hashCode;
			}
		}

		private static IEqualityComparer _comparer = new WeakKeyComparer();

		private long _lastGlobalMem;

		private int _lastHashCount;

		internal WeakHashtable()
			: base(_comparer)
		{
		}

		public void SetWeak(object key, object value)
		{
			ScavengeKeys();
			this[new EqualityWeakReference(key)] = value;
		}

		private void ScavengeKeys()
		{
			int count = Count;
			if (count == 0)
			{
				return;
			}
			if (_lastHashCount == 0)
			{
				_lastHashCount = count;
				return;
			}
			long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
			if (_lastGlobalMem == 0)
			{
				_lastGlobalMem = totalMemory;
				return;
			}
			float num = (float)(totalMemory - _lastGlobalMem) / (float)_lastGlobalMem;
			float num2 = (float)(count - _lastHashCount) / (float)_lastHashCount;
			if (num < 0f && num2 >= 0f)
			{
				ArrayList arrayList = null;
				foreach (object key in Keys)
				{
					WeakReference weakReference = key as WeakReference;
					if (weakReference != null && !weakReference.IsAlive)
					{
						if (arrayList == null)
						{
							arrayList = new ArrayList();
						}
						arrayList.Add(weakReference);
					}
				}
				if (arrayList != null)
				{
					foreach (object item in arrayList)
					{
						Remove(item);
					}
				}
			}
			_lastGlobalMem = totalMemory;
			_lastHashCount = count;
		}
	}
}
