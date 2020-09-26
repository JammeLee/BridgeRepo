using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace System.Diagnostics
{
	[Serializable]
	internal class StackFrameHelper
	{
		[NonSerialized]
		private Thread targetThread;

		private int[] rgiOffset;

		private int[] rgiILOffset;

		private MethodBase[] rgMethodBase;

		private object dynamicMethods;

		[NonSerialized]
		private RuntimeMethodHandle[] rgMethodHandle;

		private string[] rgFilename;

		private int[] rgiLineNumber;

		private int[] rgiColumnNumber;

		private int iFrameCount;

		private bool fNeedFileInfo;

		public StackFrameHelper(bool fNeedFileLineColInfo, Thread target)
		{
			targetThread = target;
			rgMethodBase = null;
			rgMethodHandle = null;
			rgiOffset = null;
			rgiILOffset = null;
			rgFilename = null;
			rgiLineNumber = null;
			rgiColumnNumber = null;
			dynamicMethods = null;
			iFrameCount = 512;
			fNeedFileInfo = fNeedFileLineColInfo;
		}

		public virtual MethodBase GetMethodBase(int i)
		{
			RuntimeMethodHandle runtimeMethodHandle = rgMethodHandle[i];
			if (runtimeMethodHandle.IsNullHandle())
			{
				return null;
			}
			runtimeMethodHandle = runtimeMethodHandle.GetTypicalMethodDefinition();
			return RuntimeType.GetMethodBase(runtimeMethodHandle);
		}

		public virtual int GetOffset(int i)
		{
			return rgiOffset[i];
		}

		public virtual int GetILOffset(int i)
		{
			return rgiILOffset[i];
		}

		public virtual string GetFilename(int i)
		{
			return rgFilename[i];
		}

		public virtual int GetLineNumber(int i)
		{
			return rgiLineNumber[i];
		}

		public virtual int GetColumnNumber(int i)
		{
			return rgiColumnNumber[i];
		}

		public virtual int GetNumberOfFrames()
		{
			return iFrameCount;
		}

		public virtual void SetNumberOfFrames(int i)
		{
			iFrameCount = i;
		}

		[OnSerializing]
		private void OnSerializing(StreamingContext context)
		{
			rgMethodBase = ((rgMethodHandle == null) ? null : new MethodBase[rgMethodHandle.Length]);
			if (rgMethodHandle == null)
			{
				return;
			}
			for (int i = 0; i < rgMethodHandle.Length; i++)
			{
				if (!rgMethodHandle[i].IsNullHandle())
				{
					rgMethodBase[i] = RuntimeType.GetMethodBase(rgMethodHandle[i]);
				}
			}
		}

		[OnSerialized]
		private void OnSerialized(StreamingContext context)
		{
			rgMethodBase = null;
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			rgMethodHandle = ((rgMethodBase == null) ? null : new RuntimeMethodHandle[rgMethodBase.Length]);
			if (rgMethodBase != null)
			{
				for (int i = 0; i < rgMethodBase.Length; i++)
				{
					if (rgMethodBase[i] != null)
					{
						ref RuntimeMethodHandle reference = ref rgMethodHandle[i];
						reference = rgMethodBase[i].MethodHandle;
					}
				}
			}
			rgMethodBase = null;
		}
	}
}
