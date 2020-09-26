using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class CodeEventReferenceExpression : CodeExpression
	{
		private CodeExpression targetObject;

		private string eventName;

		public CodeExpression TargetObject
		{
			get
			{
				return targetObject;
			}
			set
			{
				targetObject = value;
			}
		}

		public string EventName
		{
			get
			{
				if (eventName != null)
				{
					return eventName;
				}
				return string.Empty;
			}
			set
			{
				eventName = value;
			}
		}

		public CodeEventReferenceExpression()
		{
		}

		public CodeEventReferenceExpression(CodeExpression targetObject, string eventName)
		{
			this.targetObject = targetObject;
			this.eventName = eventName;
		}
	}
}
