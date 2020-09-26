using System;
using System.Runtime.CompilerServices;
using CSLib.Utility;

namespace CSLib.Framework
{
	public abstract class AECSSystem
	{
		[CompilerGenerated]
		private Type ᜀ;

		[CompilerGenerated]
		private string ᜁ;

		[CompilerGenerated]
		private string ᜂ;

		[CompilerGenerated]
		private float ᜃ;

		protected CECSEngine m_engine;

		protected AECSSystemProcess m_process;

		public Type type
		{
			[CompilerGenerated]
			get
			{
				return ᜀ;
			}
			[CompilerGenerated]
			private set
			{
				ᜀ = value;
			}
		}

		public string Name
		{
			[CompilerGenerated]
			get
			{
				return ᜁ;
			}
			[CompilerGenerated]
			private set
			{
				ᜁ = value;
			}
		}

		public string FullName
		{
			[CompilerGenerated]
			get
			{
				return ᜂ;
			}
			[CompilerGenerated]
			private set
			{
				ᜂ = value;
			}
		}

		public float deltaTime
		{
			[CompilerGenerated]
			get
			{
				return ᜃ;
			}
			[CompilerGenerated]
			set
			{
				ᜃ = value;
			}
		}

		public CECSEngine Engine
		{
			get
			{
				return m_engine;
			}
			set
			{
				m_engine = value;
			}
		}

		public AECSSystemProcess SystemProcess
		{
			get
			{
				return m_process;
			}
			set
			{
				m_process = value;
			}
		}

		public AECSSystem()
		{
			type = GetType();
			Name = type.Name;
			FullName = type.FullName;
			_SignalRegister();
		}

		protected virtual void _SignalRegister()
		{
		}

		protected virtual void _SignalUnregister()
		{
		}

		public virtual void CbAddEntity(CECSEntity entity)
		{
		}

		public virtual void CbDelEntity(CECSEntity entity)
		{
		}

		public virtual void Before()
		{
		}

		public virtual void Update(CECSEntity entity)
		{
		}

		public virtual void After()
		{
		}

		public virtual void Destroy()
		{
			//Discarded unreachable code: IL_000c
			int a_ = 9;
			if (true)
			{
			}
			CDebugOut.LogWarning(GetType().Name, CMessageLabel.b("\ueb90聆遬昡䄣唥尧堩䌫圭襊\ue75dᠳ䁫㵙㱍륪晦䇀", a_));
			_SignalUnregister();
			m_engine = null;
			m_process = null;
		}
	}
}
