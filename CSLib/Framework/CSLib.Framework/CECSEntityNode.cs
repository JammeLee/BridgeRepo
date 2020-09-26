namespace CSLib.Framework
{
	public class CECSEntityNode
	{
		public CECSEntity Entity;

		public CECSEntityNode pre;

		public CECSEntityNode next;

		public void Destroy()
		{
			Entity.Destroy();
			Entity = null;
		}

		public void Dispose()
		{
			Entity = null;
		}
	}
}
