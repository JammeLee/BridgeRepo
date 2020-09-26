namespace System.Runtime.Remoting.Messaging
{
	internal class SmuggledObjRef
	{
		private ObjRef _objRef;

		public ObjRef ObjRef => _objRef;

		public SmuggledObjRef(ObjRef objRef)
		{
			_objRef = objRef;
		}
	}
}
