namespace System.Diagnostics
{
	[Serializable]
	internal class LogSwitch
	{
		internal string strName;

		internal string strDescription;

		private LogSwitch ParentSwitch;

		private LogSwitch[] ChildSwitch;

		internal LoggingLevels iLevel;

		internal LoggingLevels iOldLevel;

		private int iNumChildren;

		private int iChildArraySize;

		public virtual string Name => strName;

		public virtual string Description => strDescription;

		public virtual LogSwitch Parent => ParentSwitch;

		public virtual LoggingLevels MinimumLevel
		{
			get
			{
				return iLevel;
			}
			set
			{
				iLevel = value;
				iOldLevel = value;
				string strParentName = ((ParentSwitch != null) ? ParentSwitch.Name : "");
				if (Debugger.IsAttached)
				{
					Log.ModifyLogSwitch((int)iLevel, strName, strParentName);
				}
				Log.InvokeLogSwitchLevelHandlers(this, iLevel);
			}
		}

		private LogSwitch()
		{
		}

		public LogSwitch(string name, string description, LogSwitch parent)
		{
			if (name != null && parent != null)
			{
				if (name.Length == 0)
				{
					throw new ArgumentOutOfRangeException("Name", Environment.GetResourceString("Argument_StringZeroLength"));
				}
				strName = name;
				strDescription = description;
				iLevel = LoggingLevels.ErrorLevel;
				iOldLevel = iLevel;
				parent.AddChildSwitch(this);
				ParentSwitch = parent;
				ChildSwitch = null;
				iNumChildren = 0;
				iChildArraySize = 0;
				Log.m_Hashtable.Add(strName, this);
				Log.AddLogSwitch(this);
				Log.iNumOfSwitches++;
				return;
			}
			throw new ArgumentNullException((name == null) ? "name" : "parent");
		}

		internal LogSwitch(string name, string description)
		{
			strName = name;
			strDescription = description;
			iLevel = LoggingLevels.ErrorLevel;
			iOldLevel = iLevel;
			ParentSwitch = null;
			ChildSwitch = null;
			iNumChildren = 0;
			iChildArraySize = 0;
			Log.m_Hashtable.Add(strName, this);
			Log.AddLogSwitch(this);
			Log.iNumOfSwitches++;
		}

		public virtual bool CheckLevel(LoggingLevels level)
		{
			if (iLevel > level)
			{
				if (ParentSwitch == null)
				{
					return false;
				}
				return ParentSwitch.CheckLevel(level);
			}
			return true;
		}

		public static LogSwitch GetSwitch(string name)
		{
			return (LogSwitch)Log.m_Hashtable[name];
		}

		private void AddChildSwitch(LogSwitch child)
		{
			if (iChildArraySize <= iNumChildren)
			{
				int num = ((iChildArraySize != 0) ? (iChildArraySize * 3 / 2) : 10);
				LogSwitch[] array = new LogSwitch[num];
				if (iNumChildren > 0)
				{
					Array.Copy(ChildSwitch, array, iNumChildren);
				}
				iChildArraySize = num;
				ChildSwitch = array;
			}
			ChildSwitch[iNumChildren++] = child;
		}
	}
}
