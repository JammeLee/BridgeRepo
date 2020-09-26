using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	[ComVisible(true)]
	public class OpCodes
	{
		public static readonly OpCode Nop = new OpCode("nop", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 0, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Break = new OpCode("break", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 1, FlowControl.Break, endsjmpblk: false, 0);

		public static readonly OpCode Ldarg_0 = new OpCode("ldarg.0", StackBehaviour.Pop0, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 2, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldarg_1 = new OpCode("ldarg.1", StackBehaviour.Pop0, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 3, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldarg_2 = new OpCode("ldarg.2", StackBehaviour.Pop0, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 4, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldarg_3 = new OpCode("ldarg.3", StackBehaviour.Pop0, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 5, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldloc_0 = new OpCode("ldloc.0", StackBehaviour.Pop0, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 6, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldloc_1 = new OpCode("ldloc.1", StackBehaviour.Pop0, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 7, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldloc_2 = new OpCode("ldloc.2", StackBehaviour.Pop0, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 8, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldloc_3 = new OpCode("ldloc.3", StackBehaviour.Pop0, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 9, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Stloc_0 = new OpCode("stloc.0", StackBehaviour.Pop1, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 10, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Stloc_1 = new OpCode("stloc.1", StackBehaviour.Pop1, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 11, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Stloc_2 = new OpCode("stloc.2", StackBehaviour.Pop1, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 12, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Stloc_3 = new OpCode("stloc.3", StackBehaviour.Pop1, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 13, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldarg_S = new OpCode("ldarg.s", StackBehaviour.Pop0, StackBehaviour.Push1, OperandType.ShortInlineVar, OpCodeType.Macro, 1, byte.MaxValue, 14, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldarga_S = new OpCode("ldarga.s", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.ShortInlineVar, OpCodeType.Macro, 1, byte.MaxValue, 15, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Starg_S = new OpCode("starg.s", StackBehaviour.Pop1, StackBehaviour.Push0, OperandType.ShortInlineVar, OpCodeType.Macro, 1, byte.MaxValue, 16, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldloc_S = new OpCode("ldloc.s", StackBehaviour.Pop0, StackBehaviour.Push1, OperandType.ShortInlineVar, OpCodeType.Macro, 1, byte.MaxValue, 17, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldloca_S = new OpCode("ldloca.s", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.ShortInlineVar, OpCodeType.Macro, 1, byte.MaxValue, 18, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Stloc_S = new OpCode("stloc.s", StackBehaviour.Pop1, StackBehaviour.Push0, OperandType.ShortInlineVar, OpCodeType.Macro, 1, byte.MaxValue, 19, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldnull = new OpCode("ldnull", StackBehaviour.Pop0, StackBehaviour.Pushref, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 20, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_I4_M1 = new OpCode("ldc.i4.m1", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 21, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_I4_0 = new OpCode("ldc.i4.0", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 22, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_I4_1 = new OpCode("ldc.i4.1", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 23, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_I4_2 = new OpCode("ldc.i4.2", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 24, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_I4_3 = new OpCode("ldc.i4.3", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 25, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_I4_4 = new OpCode("ldc.i4.4", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 26, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_I4_5 = new OpCode("ldc.i4.5", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 27, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_I4_6 = new OpCode("ldc.i4.6", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 28, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_I4_7 = new OpCode("ldc.i4.7", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 29, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_I4_8 = new OpCode("ldc.i4.8", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Macro, 1, byte.MaxValue, 30, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_I4_S = new OpCode("ldc.i4.s", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.ShortInlineI, OpCodeType.Macro, 1, byte.MaxValue, 31, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_I4 = new OpCode("ldc.i4", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineI, OpCodeType.Primitive, 1, byte.MaxValue, 32, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_I8 = new OpCode("ldc.i8", StackBehaviour.Pop0, StackBehaviour.Pushi8, OperandType.InlineI8, OpCodeType.Primitive, 1, byte.MaxValue, 33, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_R4 = new OpCode("ldc.r4", StackBehaviour.Pop0, StackBehaviour.Pushr4, OperandType.ShortInlineR, OpCodeType.Primitive, 1, byte.MaxValue, 34, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldc_R8 = new OpCode("ldc.r8", StackBehaviour.Pop0, StackBehaviour.Pushr8, OperandType.InlineR, OpCodeType.Primitive, 1, byte.MaxValue, 35, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Dup = new OpCode("dup", StackBehaviour.Pop1, StackBehaviour.Push1_push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 37, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Pop = new OpCode("pop", StackBehaviour.Pop1, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 38, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Jmp = new OpCode("jmp", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineMethod, OpCodeType.Primitive, 1, byte.MaxValue, 39, FlowControl.Call, endsjmpblk: true, 0);

		public static readonly OpCode Call = new OpCode("call", StackBehaviour.Varpop, StackBehaviour.Varpush, OperandType.InlineMethod, OpCodeType.Primitive, 1, byte.MaxValue, 40, FlowControl.Call, endsjmpblk: false, 0);

		public static readonly OpCode Calli = new OpCode("calli", StackBehaviour.Varpop, StackBehaviour.Varpush, OperandType.InlineSig, OpCodeType.Primitive, 1, byte.MaxValue, 41, FlowControl.Call, endsjmpblk: false, 0);

		public static readonly OpCode Ret = new OpCode("ret", StackBehaviour.Varpop, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 42, FlowControl.Return, endsjmpblk: true, 0);

		public static readonly OpCode Br_S = new OpCode("br.s", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 43, FlowControl.Branch, endsjmpblk: true, 0);

		public static readonly OpCode Brfalse_S = new OpCode("brfalse.s", StackBehaviour.Popi, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 44, FlowControl.Cond_Branch, endsjmpblk: false, -1);

		public static readonly OpCode Brtrue_S = new OpCode("brtrue.s", StackBehaviour.Popi, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 45, FlowControl.Cond_Branch, endsjmpblk: false, -1);

		public static readonly OpCode Beq_S = new OpCode("beq.s", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 46, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Bge_S = new OpCode("bge.s", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 47, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Bgt_S = new OpCode("bgt.s", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 48, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Ble_S = new OpCode("ble.s", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 49, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Blt_S = new OpCode("blt.s", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 50, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Bne_Un_S = new OpCode("bne.un.s", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 51, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Bge_Un_S = new OpCode("bge.un.s", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 52, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Bgt_Un_S = new OpCode("bgt.un.s", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 53, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Ble_Un_S = new OpCode("ble.un.s", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 54, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Blt_Un_S = new OpCode("blt.un.s", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 55, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Br = new OpCode("br", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, 1, byte.MaxValue, 56, FlowControl.Branch, endsjmpblk: true, 0);

		public static readonly OpCode Brfalse = new OpCode("brfalse", StackBehaviour.Popi, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, 1, byte.MaxValue, 57, FlowControl.Cond_Branch, endsjmpblk: false, -1);

		public static readonly OpCode Brtrue = new OpCode("brtrue", StackBehaviour.Popi, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, 1, byte.MaxValue, 58, FlowControl.Cond_Branch, endsjmpblk: false, -1);

		public static readonly OpCode Beq = new OpCode("beq", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 59, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Bge = new OpCode("bge", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 60, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Bgt = new OpCode("bgt", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 61, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Ble = new OpCode("ble", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 62, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Blt = new OpCode("blt", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 63, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Bne_Un = new OpCode("bne.un", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 64, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Bge_Un = new OpCode("bge.un", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 65, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Bgt_Un = new OpCode("bgt.un", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 66, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Ble_Un = new OpCode("ble.un", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 67, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Blt_Un = new OpCode("blt.un", StackBehaviour.Pop1_pop1, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Macro, 1, byte.MaxValue, 68, FlowControl.Cond_Branch, endsjmpblk: false, -2);

		public static readonly OpCode Switch = new OpCode("switch", StackBehaviour.Popi, StackBehaviour.Push0, OperandType.InlineSwitch, OpCodeType.Primitive, 1, byte.MaxValue, 69, FlowControl.Cond_Branch, endsjmpblk: false, -1);

		public static readonly OpCode Ldind_I1 = new OpCode("ldind.i1", StackBehaviour.Popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 70, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldind_U1 = new OpCode("ldind.u1", StackBehaviour.Popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 71, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldind_I2 = new OpCode("ldind.i2", StackBehaviour.Popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 72, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldind_U2 = new OpCode("ldind.u2", StackBehaviour.Popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 73, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldind_I4 = new OpCode("ldind.i4", StackBehaviour.Popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 74, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldind_U4 = new OpCode("ldind.u4", StackBehaviour.Popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 75, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldind_I8 = new OpCode("ldind.i8", StackBehaviour.Popi, StackBehaviour.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 76, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldind_I = new OpCode("ldind.i", StackBehaviour.Popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 77, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldind_R4 = new OpCode("ldind.r4", StackBehaviour.Popi, StackBehaviour.Pushr4, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 78, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldind_R8 = new OpCode("ldind.r8", StackBehaviour.Popi, StackBehaviour.Pushr8, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 79, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldind_Ref = new OpCode("ldind.ref", StackBehaviour.Popi, StackBehaviour.Pushref, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 80, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Stind_Ref = new OpCode("stind.ref", StackBehaviour.Popi_popi, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 81, FlowControl.Next, endsjmpblk: false, -2);

		public static readonly OpCode Stind_I1 = new OpCode("stind.i1", StackBehaviour.Popi_popi, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 82, FlowControl.Next, endsjmpblk: false, -2);

		public static readonly OpCode Stind_I2 = new OpCode("stind.i2", StackBehaviour.Popi_popi, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 83, FlowControl.Next, endsjmpblk: false, -2);

		public static readonly OpCode Stind_I4 = new OpCode("stind.i4", StackBehaviour.Popi_popi, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 84, FlowControl.Next, endsjmpblk: false, -2);

		public static readonly OpCode Stind_I8 = new OpCode("stind.i8", StackBehaviour.Popi_popi8, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 85, FlowControl.Next, endsjmpblk: false, -2);

		public static readonly OpCode Stind_R4 = new OpCode("stind.r4", StackBehaviour.Popi_popr4, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 86, FlowControl.Next, endsjmpblk: false, -2);

		public static readonly OpCode Stind_R8 = new OpCode("stind.r8", StackBehaviour.Popi_popr8, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 87, FlowControl.Next, endsjmpblk: false, -2);

		public static readonly OpCode Add = new OpCode("add", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 88, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Sub = new OpCode("sub", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 89, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Mul = new OpCode("mul", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 90, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Div = new OpCode("div", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 91, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Div_Un = new OpCode("div.un", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 92, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Rem = new OpCode("rem", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 93, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Rem_Un = new OpCode("rem.un", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 94, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode And = new OpCode("and", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 95, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Or = new OpCode("or", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 96, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Xor = new OpCode("xor", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 97, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Shl = new OpCode("shl", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 98, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Shr = new OpCode("shr", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 99, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Shr_Un = new OpCode("shr.un", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 100, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Neg = new OpCode("neg", StackBehaviour.Pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 101, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Not = new OpCode("not", StackBehaviour.Pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 102, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_I1 = new OpCode("conv.i1", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 103, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_I2 = new OpCode("conv.i2", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 104, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_I4 = new OpCode("conv.i4", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 105, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_I8 = new OpCode("conv.i8", StackBehaviour.Pop1, StackBehaviour.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 106, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_R4 = new OpCode("conv.r4", StackBehaviour.Pop1, StackBehaviour.Pushr4, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 107, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_R8 = new OpCode("conv.r8", StackBehaviour.Pop1, StackBehaviour.Pushr8, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 108, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_U4 = new OpCode("conv.u4", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 109, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_U8 = new OpCode("conv.u8", StackBehaviour.Pop1, StackBehaviour.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 110, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Callvirt = new OpCode("callvirt", StackBehaviour.Varpop, StackBehaviour.Varpush, OperandType.InlineMethod, OpCodeType.Objmodel, 1, byte.MaxValue, 111, FlowControl.Call, endsjmpblk: false, 0);

		public static readonly OpCode Cpobj = new OpCode("cpobj", StackBehaviour.Popi_popi, StackBehaviour.Push0, OperandType.InlineType, OpCodeType.Objmodel, 1, byte.MaxValue, 112, FlowControl.Next, endsjmpblk: false, -2);

		public static readonly OpCode Ldobj = new OpCode("ldobj", StackBehaviour.Popi, StackBehaviour.Push1, OperandType.InlineType, OpCodeType.Objmodel, 1, byte.MaxValue, 113, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldstr = new OpCode("ldstr", StackBehaviour.Pop0, StackBehaviour.Pushref, OperandType.InlineString, OpCodeType.Objmodel, 1, byte.MaxValue, 114, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Newobj = new OpCode("newobj", StackBehaviour.Varpop, StackBehaviour.Pushref, OperandType.InlineMethod, OpCodeType.Objmodel, 1, byte.MaxValue, 115, FlowControl.Call, endsjmpblk: false, 1);

		[ComVisible(true)]
		public static readonly OpCode Castclass = new OpCode("castclass", StackBehaviour.Popref, StackBehaviour.Pushref, OperandType.InlineType, OpCodeType.Objmodel, 1, byte.MaxValue, 116, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Isinst = new OpCode("isinst", StackBehaviour.Popref, StackBehaviour.Pushi, OperandType.InlineType, OpCodeType.Objmodel, 1, byte.MaxValue, 117, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_R_Un = new OpCode("conv.r.un", StackBehaviour.Pop1, StackBehaviour.Pushr8, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 118, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Unbox = new OpCode("unbox", StackBehaviour.Popref, StackBehaviour.Pushi, OperandType.InlineType, OpCodeType.Primitive, 1, byte.MaxValue, 121, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Throw = new OpCode("throw", StackBehaviour.Popref, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 122, FlowControl.Throw, endsjmpblk: true, -1);

		public static readonly OpCode Ldfld = new OpCode("ldfld", StackBehaviour.Popref, StackBehaviour.Push1, OperandType.InlineField, OpCodeType.Objmodel, 1, byte.MaxValue, 123, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldflda = new OpCode("ldflda", StackBehaviour.Popref, StackBehaviour.Pushi, OperandType.InlineField, OpCodeType.Objmodel, 1, byte.MaxValue, 124, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Stfld = new OpCode("stfld", StackBehaviour.Popref_pop1, StackBehaviour.Push0, OperandType.InlineField, OpCodeType.Objmodel, 1, byte.MaxValue, 125, FlowControl.Next, endsjmpblk: false, -2);

		public static readonly OpCode Ldsfld = new OpCode("ldsfld", StackBehaviour.Pop0, StackBehaviour.Push1, OperandType.InlineField, OpCodeType.Objmodel, 1, byte.MaxValue, 126, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldsflda = new OpCode("ldsflda", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineField, OpCodeType.Objmodel, 1, byte.MaxValue, 127, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Stsfld = new OpCode("stsfld", StackBehaviour.Pop1, StackBehaviour.Push0, OperandType.InlineField, OpCodeType.Objmodel, 1, byte.MaxValue, 128, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Stobj = new OpCode("stobj", StackBehaviour.Popi_pop1, StackBehaviour.Push0, OperandType.InlineType, OpCodeType.Primitive, 1, byte.MaxValue, 129, FlowControl.Next, endsjmpblk: false, -2);

		public static readonly OpCode Conv_Ovf_I1_Un = new OpCode("conv.ovf.i1.un", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 130, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_I2_Un = new OpCode("conv.ovf.i2.un", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 131, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_I4_Un = new OpCode("conv.ovf.i4.un", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 132, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_I8_Un = new OpCode("conv.ovf.i8.un", StackBehaviour.Pop1, StackBehaviour.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 133, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_U1_Un = new OpCode("conv.ovf.u1.un", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 134, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_U2_Un = new OpCode("conv.ovf.u2.un", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 135, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_U4_Un = new OpCode("conv.ovf.u4.un", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 136, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_U8_Un = new OpCode("conv.ovf.u8.un", StackBehaviour.Pop1, StackBehaviour.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 137, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_I_Un = new OpCode("conv.ovf.i.un", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 138, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_U_Un = new OpCode("conv.ovf.u.un", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 139, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Box = new OpCode("box", StackBehaviour.Pop1, StackBehaviour.Pushref, OperandType.InlineType, OpCodeType.Primitive, 1, byte.MaxValue, 140, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Newarr = new OpCode("newarr", StackBehaviour.Popi, StackBehaviour.Pushref, OperandType.InlineType, OpCodeType.Objmodel, 1, byte.MaxValue, 141, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldlen = new OpCode("ldlen", StackBehaviour.Popref, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 142, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldelema = new OpCode("ldelema", StackBehaviour.Popref_popi, StackBehaviour.Pushi, OperandType.InlineType, OpCodeType.Objmodel, 1, byte.MaxValue, 143, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldelem_I1 = new OpCode("ldelem.i1", StackBehaviour.Popref_popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 144, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldelem_U1 = new OpCode("ldelem.u1", StackBehaviour.Popref_popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 145, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldelem_I2 = new OpCode("ldelem.i2", StackBehaviour.Popref_popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 146, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldelem_U2 = new OpCode("ldelem.u2", StackBehaviour.Popref_popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 147, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldelem_I4 = new OpCode("ldelem.i4", StackBehaviour.Popref_popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 148, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldelem_U4 = new OpCode("ldelem.u4", StackBehaviour.Popref_popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 149, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldelem_I8 = new OpCode("ldelem.i8", StackBehaviour.Popref_popi, StackBehaviour.Pushi8, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 150, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldelem_I = new OpCode("ldelem.i", StackBehaviour.Popref_popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 151, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldelem_R4 = new OpCode("ldelem.r4", StackBehaviour.Popref_popi, StackBehaviour.Pushr4, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 152, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldelem_R8 = new OpCode("ldelem.r8", StackBehaviour.Popref_popi, StackBehaviour.Pushr8, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 153, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldelem_Ref = new OpCode("ldelem.ref", StackBehaviour.Popref_popi, StackBehaviour.Pushref, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 154, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Stelem_I = new OpCode("stelem.i", StackBehaviour.Popref_popi_popi, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 155, FlowControl.Next, endsjmpblk: false, -3);

		public static readonly OpCode Stelem_I1 = new OpCode("stelem.i1", StackBehaviour.Popref_popi_popi, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 156, FlowControl.Next, endsjmpblk: false, -3);

		public static readonly OpCode Stelem_I2 = new OpCode("stelem.i2", StackBehaviour.Popref_popi_popi, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 157, FlowControl.Next, endsjmpblk: false, -3);

		public static readonly OpCode Stelem_I4 = new OpCode("stelem.i4", StackBehaviour.Popref_popi_popi, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 158, FlowControl.Next, endsjmpblk: false, -3);

		public static readonly OpCode Stelem_I8 = new OpCode("stelem.i8", StackBehaviour.Popref_popi_popi8, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 159, FlowControl.Next, endsjmpblk: false, -3);

		public static readonly OpCode Stelem_R4 = new OpCode("stelem.r4", StackBehaviour.Popref_popi_popr4, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 160, FlowControl.Next, endsjmpblk: false, -3);

		public static readonly OpCode Stelem_R8 = new OpCode("stelem.r8", StackBehaviour.Popref_popi_popr8, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 161, FlowControl.Next, endsjmpblk: false, -3);

		public static readonly OpCode Stelem_Ref = new OpCode("stelem.ref", StackBehaviour.Popref_popi_popref, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Objmodel, 1, byte.MaxValue, 162, FlowControl.Next, endsjmpblk: false, -3);

		public static readonly OpCode Ldelem = new OpCode("ldelem", StackBehaviour.Popref_popi, StackBehaviour.Push1, OperandType.InlineType, OpCodeType.Objmodel, 1, byte.MaxValue, 163, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Stelem = new OpCode("stelem", StackBehaviour.Popref_popi_pop1, StackBehaviour.Push0, OperandType.InlineType, OpCodeType.Objmodel, 1, byte.MaxValue, 164, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Unbox_Any = new OpCode("unbox.any", StackBehaviour.Popref, StackBehaviour.Push1, OperandType.InlineType, OpCodeType.Objmodel, 1, byte.MaxValue, 165, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_I1 = new OpCode("conv.ovf.i1", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 179, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_U1 = new OpCode("conv.ovf.u1", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 180, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_I2 = new OpCode("conv.ovf.i2", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 181, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_U2 = new OpCode("conv.ovf.u2", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 182, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_I4 = new OpCode("conv.ovf.i4", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 183, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_U4 = new OpCode("conv.ovf.u4", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 184, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_I8 = new OpCode("conv.ovf.i8", StackBehaviour.Pop1, StackBehaviour.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 185, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_U8 = new OpCode("conv.ovf.u8", StackBehaviour.Pop1, StackBehaviour.Pushi8, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 186, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Refanyval = new OpCode("refanyval", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineType, OpCodeType.Primitive, 1, byte.MaxValue, 194, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ckfinite = new OpCode("ckfinite", StackBehaviour.Pop1, StackBehaviour.Pushr8, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 195, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Mkrefany = new OpCode("mkrefany", StackBehaviour.Popi, StackBehaviour.Push1, OperandType.InlineType, OpCodeType.Primitive, 1, byte.MaxValue, 198, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldtoken = new OpCode("ldtoken", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineTok, OpCodeType.Primitive, 1, byte.MaxValue, 208, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Conv_U2 = new OpCode("conv.u2", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 209, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_U1 = new OpCode("conv.u1", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 210, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_I = new OpCode("conv.i", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 211, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_I = new OpCode("conv.ovf.i", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 212, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Conv_Ovf_U = new OpCode("conv.ovf.u", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 213, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Add_Ovf = new OpCode("add.ovf", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 214, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Add_Ovf_Un = new OpCode("add.ovf.un", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 215, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Mul_Ovf = new OpCode("mul.ovf", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 216, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Mul_Ovf_Un = new OpCode("mul.ovf.un", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 217, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Sub_Ovf = new OpCode("sub.ovf", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 218, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Sub_Ovf_Un = new OpCode("sub.ovf.un", StackBehaviour.Pop1_pop1, StackBehaviour.Push1, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 219, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Endfinally = new OpCode("endfinally", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 220, FlowControl.Return, endsjmpblk: true, 0);

		public static readonly OpCode Leave = new OpCode("leave", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineBrTarget, OpCodeType.Primitive, 1, byte.MaxValue, 221, FlowControl.Branch, endsjmpblk: true, 0);

		public static readonly OpCode Leave_S = new OpCode("leave.s", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.ShortInlineBrTarget, OpCodeType.Primitive, 1, byte.MaxValue, 222, FlowControl.Branch, endsjmpblk: true, 0);

		public static readonly OpCode Stind_I = new OpCode("stind.i", StackBehaviour.Popi_popi, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 223, FlowControl.Next, endsjmpblk: false, -2);

		public static readonly OpCode Conv_U = new OpCode("conv.u", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 1, byte.MaxValue, 224, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Prefix7 = new OpCode("prefix7", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Nternal, 1, byte.MaxValue, 248, FlowControl.Meta, endsjmpblk: false, 0);

		public static readonly OpCode Prefix6 = new OpCode("prefix6", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Nternal, 1, byte.MaxValue, 249, FlowControl.Meta, endsjmpblk: false, 0);

		public static readonly OpCode Prefix5 = new OpCode("prefix5", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Nternal, 1, byte.MaxValue, 250, FlowControl.Meta, endsjmpblk: false, 0);

		public static readonly OpCode Prefix4 = new OpCode("prefix4", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Nternal, 1, byte.MaxValue, 251, FlowControl.Meta, endsjmpblk: false, 0);

		public static readonly OpCode Prefix3 = new OpCode("prefix3", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Nternal, 1, byte.MaxValue, 252, FlowControl.Meta, endsjmpblk: false, 0);

		public static readonly OpCode Prefix2 = new OpCode("prefix2", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Nternal, 1, byte.MaxValue, 253, FlowControl.Meta, endsjmpblk: false, 0);

		public static readonly OpCode Prefix1 = new OpCode("prefix1", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Nternal, 1, byte.MaxValue, 254, FlowControl.Meta, endsjmpblk: false, 0);

		public static readonly OpCode Prefixref = new OpCode("prefixref", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Nternal, 1, byte.MaxValue, byte.MaxValue, FlowControl.Meta, endsjmpblk: false, 0);

		public static readonly OpCode Arglist = new OpCode("arglist", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 2, 254, 0, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ceq = new OpCode("ceq", StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 2, 254, 1, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Cgt = new OpCode("cgt", StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 2, 254, 2, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Cgt_Un = new OpCode("cgt.un", StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 2, 254, 3, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Clt = new OpCode("clt", StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 2, 254, 4, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Clt_Un = new OpCode("clt.un", StackBehaviour.Pop1_pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 2, 254, 5, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldftn = new OpCode("ldftn", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineMethod, OpCodeType.Primitive, 2, 254, 6, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldvirtftn = new OpCode("ldvirtftn", StackBehaviour.Popref, StackBehaviour.Pushi, OperandType.InlineMethod, OpCodeType.Primitive, 2, 254, 7, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Ldarg = new OpCode("ldarg", StackBehaviour.Pop0, StackBehaviour.Push1, OperandType.InlineVar, OpCodeType.Primitive, 2, 254, 9, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldarga = new OpCode("ldarga", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineVar, OpCodeType.Primitive, 2, 254, 10, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Starg = new OpCode("starg", StackBehaviour.Pop1, StackBehaviour.Push0, OperandType.InlineVar, OpCodeType.Primitive, 2, 254, 11, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Ldloc = new OpCode("ldloc", StackBehaviour.Pop0, StackBehaviour.Push1, OperandType.InlineVar, OpCodeType.Primitive, 2, 254, 12, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Ldloca = new OpCode("ldloca", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineVar, OpCodeType.Primitive, 2, 254, 13, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Stloc = new OpCode("stloc", StackBehaviour.Pop1, StackBehaviour.Push0, OperandType.InlineVar, OpCodeType.Primitive, 2, 254, 14, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Localloc = new OpCode("localloc", StackBehaviour.Popi, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 2, 254, 15, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Endfilter = new OpCode("endfilter", StackBehaviour.Popi, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 2, 254, 17, FlowControl.Return, endsjmpblk: true, -1);

		public static readonly OpCode Unaligned = new OpCode("unaligned.", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.ShortInlineI, OpCodeType.Prefix, 2, 254, 18, FlowControl.Meta, endsjmpblk: false, 0);

		public static readonly OpCode Volatile = new OpCode("volatile.", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Prefix, 2, 254, 19, FlowControl.Meta, endsjmpblk: false, 0);

		public static readonly OpCode Tailcall = new OpCode("tail.", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Prefix, 2, 254, 20, FlowControl.Meta, endsjmpblk: false, 0);

		public static readonly OpCode Initobj = new OpCode("initobj", StackBehaviour.Popi, StackBehaviour.Push0, OperandType.InlineType, OpCodeType.Objmodel, 2, 254, 21, FlowControl.Next, endsjmpblk: false, -1);

		public static readonly OpCode Constrained = new OpCode("constrained.", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineType, OpCodeType.Prefix, 2, 254, 22, FlowControl.Meta, endsjmpblk: false, 0);

		public static readonly OpCode Cpblk = new OpCode("cpblk", StackBehaviour.Popi_popi_popi, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 2, 254, 23, FlowControl.Next, endsjmpblk: false, -3);

		public static readonly OpCode Initblk = new OpCode("initblk", StackBehaviour.Popi_popi_popi, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Primitive, 2, 254, 24, FlowControl.Next, endsjmpblk: false, -3);

		public static readonly OpCode Rethrow = new OpCode("rethrow", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Objmodel, 2, 254, 26, FlowControl.Throw, endsjmpblk: true, 0);

		public static readonly OpCode Sizeof = new OpCode("sizeof", StackBehaviour.Pop0, StackBehaviour.Pushi, OperandType.InlineType, OpCodeType.Primitive, 2, 254, 28, FlowControl.Next, endsjmpblk: false, 1);

		public static readonly OpCode Refanytype = new OpCode("refanytype", StackBehaviour.Pop1, StackBehaviour.Pushi, OperandType.InlineNone, OpCodeType.Primitive, 2, 254, 29, FlowControl.Next, endsjmpblk: false, 0);

		public static readonly OpCode Readonly = new OpCode("readonly.", StackBehaviour.Pop0, StackBehaviour.Push0, OperandType.InlineNone, OpCodeType.Prefix, 2, 254, 30, FlowControl.Meta, endsjmpblk: false, 0);

		private OpCodes()
		{
		}

		public static bool TakesSingleByteArgument(OpCode inst)
		{
			switch (inst.m_operand)
			{
			case OperandType.ShortInlineBrTarget:
			case OperandType.ShortInlineI:
			case OperandType.ShortInlineVar:
				return true;
			default:
				return false;
			}
		}
	}
}
