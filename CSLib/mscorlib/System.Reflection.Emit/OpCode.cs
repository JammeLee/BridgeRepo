using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	[ComVisible(true)]
	public struct OpCode
	{
		internal string m_stringname;

		internal StackBehaviour m_pop;

		internal StackBehaviour m_push;

		internal OperandType m_operand;

		internal OpCodeType m_type;

		internal int m_size;

		internal byte m_s1;

		internal byte m_s2;

		internal FlowControl m_ctrl;

		internal bool m_endsUncondJmpBlk;

		internal int m_stackChange;

		public OperandType OperandType => m_operand;

		public FlowControl FlowControl => m_ctrl;

		public OpCodeType OpCodeType => m_type;

		public StackBehaviour StackBehaviourPop => m_pop;

		public StackBehaviour StackBehaviourPush => m_push;

		public int Size => m_size;

		public short Value
		{
			get
			{
				if (m_size == 2)
				{
					return (short)((m_s1 << 8) | m_s2);
				}
				return m_s2;
			}
		}

		public string Name => m_stringname;

		internal OpCode(string stringname, StackBehaviour pop, StackBehaviour push, OperandType operand, OpCodeType type, int size, byte s1, byte s2, FlowControl ctrl, bool endsjmpblk, int stack)
		{
			m_stringname = stringname;
			m_pop = pop;
			m_push = push;
			m_operand = operand;
			m_type = type;
			m_size = size;
			m_s1 = s1;
			m_s2 = s2;
			m_ctrl = ctrl;
			m_endsUncondJmpBlk = endsjmpblk;
			m_stackChange = stack;
		}

		internal bool EndsUncondJmpBlk()
		{
			return m_endsUncondJmpBlk;
		}

		internal int StackChange()
		{
			return m_stackChange;
		}

		public override bool Equals(object obj)
		{
			if (obj is OpCode)
			{
				return Equals((OpCode)obj);
			}
			return false;
		}

		public bool Equals(OpCode obj)
		{
			if (obj.m_s1 == m_s1)
			{
				return obj.m_s2 == m_s2;
			}
			return false;
		}

		public static bool operator ==(OpCode a, OpCode b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(OpCode a, OpCode b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return m_stringname.GetHashCode();
		}

		public override string ToString()
		{
			return m_stringname;
		}
	}
}
