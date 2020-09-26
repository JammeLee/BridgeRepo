namespace System.Runtime.Remoting
{
	[Serializable]
	internal class DynamicTypeInfo : TypeInfo
	{
		internal DynamicTypeInfo(Type typeOfObj)
			: base(typeOfObj)
		{
		}

		public override bool CanCastTo(Type castType, object o)
		{
			return ((MarshalByRefObject)o).IsInstanceOfType(castType);
		}
	}
}
