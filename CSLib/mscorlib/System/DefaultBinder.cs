using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{
	[Serializable]
	internal class DefaultBinder : Binder
	{
		internal class BinderState
		{
			internal int[] m_argsMap;

			internal int m_originalSize;

			internal bool m_isParamArray;

			internal BinderState(int[] argsMap, int originalSize, bool isParamArray)
			{
				m_argsMap = argsMap;
				m_originalSize = originalSize;
				m_isParamArray = isParamArray;
			}
		}

		public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] canidates, ref object[] args, ParameterModifier[] modifiers, CultureInfo cultureInfo, string[] names, out object state)
		{
			state = null;
			if (canidates == null || canidates.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EmptyArray"), "canidates");
			}
			int[][] array = new int[canidates.Length][];
			for (int i = 0; i < canidates.Length; i++)
			{
				ParameterInfo[] parametersNoCopy = canidates[i].GetParametersNoCopy();
				array[i] = new int[(parametersNoCopy.Length > args.Length) ? parametersNoCopy.Length : args.Length];
				if (names == null)
				{
					for (int j = 0; j < args.Length; j++)
					{
						array[i][j] = j;
					}
				}
				else if (!CreateParamOrder(array[i], parametersNoCopy, names))
				{
					canidates[i] = null;
				}
			}
			Type[] array2 = new Type[canidates.Length];
			Type[] array3 = new Type[args.Length];
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] != null)
				{
					array3[i] = args[i].GetType();
				}
			}
			int num = 0;
			bool flag = (bindingAttr & BindingFlags.OptionalParamBinding) != 0;
			Type type = null;
			for (int i = 0; i < canidates.Length; i++)
			{
				type = null;
				if (canidates[i] == null)
				{
					continue;
				}
				ParameterInfo[] parametersNoCopy2 = canidates[i].GetParametersNoCopy();
				if (parametersNoCopy2.Length == 0)
				{
					if (args.Length == 0 || (canidates[i].CallingConvention & CallingConventions.VarArgs) != 0)
					{
						array[num] = array[i];
						canidates[num++] = canidates[i];
					}
					continue;
				}
				int j;
				if (parametersNoCopy2.Length > args.Length)
				{
					for (j = args.Length; j < parametersNoCopy2.Length - 1 && parametersNoCopy2[j].DefaultValue != DBNull.Value; j++)
					{
					}
					if (j != parametersNoCopy2.Length - 1)
					{
						continue;
					}
					if (parametersNoCopy2[j].DefaultValue == DBNull.Value)
					{
						if (!parametersNoCopy2[j].ParameterType.IsArray || !parametersNoCopy2[j].IsDefined(typeof(ParamArrayAttribute), inherit: true))
						{
							continue;
						}
						type = parametersNoCopy2[j].ParameterType.GetElementType();
					}
				}
				else if (parametersNoCopy2.Length < args.Length)
				{
					int num2 = parametersNoCopy2.Length - 1;
					if (!parametersNoCopy2[num2].ParameterType.IsArray || !parametersNoCopy2[num2].IsDefined(typeof(ParamArrayAttribute), inherit: true) || array[i][num2] != num2)
					{
						continue;
					}
					type = parametersNoCopy2[num2].ParameterType.GetElementType();
				}
				else
				{
					int num3 = parametersNoCopy2.Length - 1;
					if (parametersNoCopy2[num3].ParameterType.IsArray && parametersNoCopy2[num3].IsDefined(typeof(ParamArrayAttribute), inherit: true) && array[i][num3] == num3 && !parametersNoCopy2[num3].ParameterType.IsAssignableFrom(array3[num3]))
					{
						type = parametersNoCopy2[num3].ParameterType.GetElementType();
					}
				}
				Type type2 = null;
				int num4 = ((type != null) ? (parametersNoCopy2.Length - 1) : args.Length);
				for (j = 0; j < num4; j++)
				{
					type2 = parametersNoCopy2[j].ParameterType;
					if (type2.IsByRef)
					{
						type2 = type2.GetElementType();
					}
					if (type2 == array3[array[i][j]] || (flag && args[array[i][j]] == Type.Missing) || args[array[i][j]] == null || type2 == typeof(object))
					{
						continue;
					}
					if (type2.IsPrimitive)
					{
						if (array3[array[i][j]] == null || !CanConvertPrimitiveObjectToType(args[array[i][j]], (RuntimeType)type2))
						{
							break;
						}
					}
					else if (array3[array[i][j]] != null && !type2.IsAssignableFrom(array3[array[i][j]]) && (!array3[array[i][j]].IsCOMObject || !type2.IsInstanceOfType(args[array[i][j]])))
					{
						break;
					}
				}
				if (type != null && j == parametersNoCopy2.Length - 1)
				{
					for (; j < args.Length; j++)
					{
						if (type.IsPrimitive)
						{
							if (array3[j] == null || !CanConvertPrimitiveObjectToType(args[j], (RuntimeType)type))
							{
								break;
							}
						}
						else if (array3[j] != null && !type.IsAssignableFrom(array3[j]) && (!array3[j].IsCOMObject || !type.IsInstanceOfType(args[j])))
						{
							break;
						}
					}
				}
				if (j == args.Length)
				{
					array[num] = array[i];
					array2[num] = type;
					canidates[num++] = canidates[i];
				}
			}
			switch (num)
			{
			case 0:
				throw new MissingMethodException(Environment.GetResourceString("MissingMember"));
			case 1:
			{
				if (names != null)
				{
					state = new BinderState((int[])array[0].Clone(), args.Length, array2[0] != null);
					ReorderParams(array[0], args);
				}
				ParameterInfo[] parametersNoCopy4 = canidates[0].GetParametersNoCopy();
				if (parametersNoCopy4.Length == args.Length)
				{
					if (array2[0] != null)
					{
						object[] array7 = new object[parametersNoCopy4.Length];
						int num8 = parametersNoCopy4.Length - 1;
						Array.Copy(args, 0, array7, 0, num8);
						array7[num8] = Array.CreateInstance(array2[0], 1);
						((Array)array7[num8]).SetValue(args[num8], 0);
						args = array7;
					}
				}
				else if (parametersNoCopy4.Length > args.Length)
				{
					object[] array8 = new object[parametersNoCopy4.Length];
					int i;
					for (i = 0; i < args.Length; i++)
					{
						array8[i] = args[i];
					}
					for (; i < parametersNoCopy4.Length - 1; i++)
					{
						array8[i] = parametersNoCopy4[i].DefaultValue;
					}
					if (array2[0] != null)
					{
						array8[i] = Array.CreateInstance(array2[0], 0);
					}
					else
					{
						array8[i] = parametersNoCopy4[i].DefaultValue;
					}
					args = array8;
				}
				else if ((canidates[0].CallingConvention & CallingConventions.VarArgs) == 0)
				{
					object[] array9 = new object[parametersNoCopy4.Length];
					int num9 = parametersNoCopy4.Length - 1;
					Array.Copy(args, 0, array9, 0, num9);
					array9[num9] = Array.CreateInstance(array2[0], args.Length - num9);
					Array.Copy(args, num9, (Array)array9[num9], 0, args.Length - num9);
					args = array9;
				}
				return canidates[0];
			}
			default:
			{
				int num5 = 0;
				bool flag2 = false;
				int i;
				for (i = 1; i < num; i++)
				{
					switch (FindMostSpecificMethod(canidates[num5], array[num5], array2[num5], canidates[i], array[i], array2[i], array3, args))
					{
					case 0:
						flag2 = true;
						break;
					case 2:
						num5 = i;
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.Ambiguous"));
				}
				if (names != null)
				{
					state = new BinderState((int[])array[num5].Clone(), args.Length, array2[num5] != null);
					ReorderParams(array[num5], args);
				}
				ParameterInfo[] parametersNoCopy3 = canidates[num5].GetParametersNoCopy();
				if (parametersNoCopy3.Length == args.Length)
				{
					if (array2[num5] != null)
					{
						object[] array4 = new object[parametersNoCopy3.Length];
						int num6 = parametersNoCopy3.Length - 1;
						Array.Copy(args, 0, array4, 0, num6);
						array4[num6] = Array.CreateInstance(array2[num5], 1);
						((Array)array4[num6]).SetValue(args[num6], 0);
						args = array4;
					}
				}
				else if (parametersNoCopy3.Length > args.Length)
				{
					object[] array5 = new object[parametersNoCopy3.Length];
					for (i = 0; i < args.Length; i++)
					{
						array5[i] = args[i];
					}
					for (; i < parametersNoCopy3.Length - 1; i++)
					{
						array5[i] = parametersNoCopy3[i].DefaultValue;
					}
					if (array2[num5] != null)
					{
						array5[i] = Array.CreateInstance(array2[num5], 0);
					}
					else
					{
						array5[i] = parametersNoCopy3[i].DefaultValue;
					}
					args = array5;
				}
				else if ((canidates[num5].CallingConvention & CallingConventions.VarArgs) == 0)
				{
					object[] array6 = new object[parametersNoCopy3.Length];
					int num7 = parametersNoCopy3.Length - 1;
					Array.Copy(args, 0, array6, 0, num7);
					array6[i] = Array.CreateInstance(array2[num5], args.Length - num7);
					Array.Copy(args, num7, (Array)array6[i], 0, args.Length - num7);
					args = array6;
				}
				return canidates[num5];
			}
			}
		}

		public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo cultureInfo)
		{
			int num = 0;
			Type type = null;
			if ((bindingAttr & BindingFlags.SetField) != 0)
			{
				type = value.GetType();
				for (int i = 0; i < match.Length; i++)
				{
					Type fieldType = match[i].FieldType;
					if (fieldType == type)
					{
						match[num++] = match[i];
					}
					else if (value == Empty.Value && fieldType.IsClass)
					{
						match[num++] = match[i];
					}
					else if (fieldType == typeof(object))
					{
						match[num++] = match[i];
					}
					else if (fieldType.IsPrimitive)
					{
						if (CanConvertPrimitiveObjectToType(value, (RuntimeType)fieldType))
						{
							match[num++] = match[i];
						}
					}
					else if (fieldType.IsAssignableFrom(type))
					{
						match[num++] = match[i];
					}
				}
				switch (num)
				{
				case 0:
					throw new MissingFieldException(Environment.GetResourceString("MissingField"));
				case 1:
					return match[0];
				}
			}
			int num2 = 0;
			bool flag = false;
			for (int i = 1; i < num; i++)
			{
				switch (FindMostSpecificField(match[num2], match[i]))
				{
				case 0:
					flag = true;
					break;
				case 2:
					num2 = i;
					flag = false;
					break;
				}
			}
			if (flag)
			{
				throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.Ambiguous"));
			}
			return match[num2];
		}

		public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
		{
			Type[] array = new Type[types.Length];
			for (int i = 0; i < types.Length; i++)
			{
				array[i] = types[i].UnderlyingSystemType;
				if (!(array[i] is RuntimeType))
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "types");
				}
			}
			types = array;
			if (match == null || match.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EmptyArray"), "match");
			}
			int num = 0;
			for (int i = 0; i < match.Length; i++)
			{
				ParameterInfo[] parametersNoCopy = match[i].GetParametersNoCopy();
				if (parametersNoCopy.Length != types.Length)
				{
					continue;
				}
				int j;
				for (j = 0; j < types.Length; j++)
				{
					Type parameterType = parametersNoCopy[j].ParameterType;
					if (parameterType == types[j] || parameterType == typeof(object))
					{
						continue;
					}
					if (parameterType.IsPrimitive)
					{
						if (!(types[j].UnderlyingSystemType is RuntimeType) || !CanConvertPrimitive((RuntimeType)types[j].UnderlyingSystemType, (RuntimeType)parameterType.UnderlyingSystemType))
						{
							break;
						}
					}
					else if (!parameterType.IsAssignableFrom(types[j]))
					{
						break;
					}
				}
				if (j == types.Length)
				{
					match[num++] = match[i];
				}
			}
			switch (num)
			{
			case 0:
				return null;
			case 1:
				return match[0];
			default:
			{
				int num2 = 0;
				bool flag = false;
				int[] array2 = new int[types.Length];
				for (int i = 0; i < types.Length; i++)
				{
					array2[i] = i;
				}
				for (int i = 1; i < num; i++)
				{
					switch (FindMostSpecificMethod(match[num2], array2, null, match[i], array2, null, types, null))
					{
					case 0:
						flag = true;
						break;
					case 2:
						num2 = i;
						flag = false;
						num2 = i;
						break;
					}
				}
				if (flag)
				{
					throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.Ambiguous"));
				}
				return match[num2];
			}
			}
		}

		public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
		{
			int i = 0;
			if (match == null || match.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EmptyArray"), "match");
			}
			int num = 0;
			int num2 = ((indexes != null) ? indexes.Length : 0);
			for (int j = 0; j < match.Length; j++)
			{
				if (indexes != null)
				{
					ParameterInfo[] indexParameters = match[j].GetIndexParameters();
					if (indexParameters.Length != num2)
					{
						continue;
					}
					for (i = 0; i < num2; i++)
					{
						Type parameterType = indexParameters[i].ParameterType;
						if (parameterType == indexes[i] || parameterType == typeof(object))
						{
							continue;
						}
						if (parameterType.IsPrimitive)
						{
							if (!(indexes[i].UnderlyingSystemType is RuntimeType) || !CanConvertPrimitive((RuntimeType)indexes[i].UnderlyingSystemType, (RuntimeType)parameterType.UnderlyingSystemType))
							{
								break;
							}
						}
						else if (!parameterType.IsAssignableFrom(indexes[i]))
						{
							break;
						}
					}
				}
				if (i != num2)
				{
					continue;
				}
				if (returnType != null)
				{
					if (match[j].PropertyType.IsPrimitive)
					{
						if (!(returnType.UnderlyingSystemType is RuntimeType) || !CanConvertPrimitive((RuntimeType)returnType.UnderlyingSystemType, (RuntimeType)match[j].PropertyType.UnderlyingSystemType))
						{
							continue;
						}
					}
					else if (!match[j].PropertyType.IsAssignableFrom(returnType))
					{
						continue;
					}
				}
				match[num++] = match[j];
			}
			switch (num)
			{
			case 0:
				return null;
			case 1:
				return match[0];
			default:
			{
				int num3 = 0;
				bool flag = false;
				int[] array = new int[num2];
				for (int j = 0; j < num2; j++)
				{
					array[j] = j;
				}
				for (int j = 1; j < num; j++)
				{
					int num4 = FindMostSpecificType(match[num3].PropertyType, match[j].PropertyType, returnType);
					if (num4 == 0 && indexes != null)
					{
						num4 = FindMostSpecific(match[num3].GetIndexParameters(), array, null, match[j].GetIndexParameters(), array, null, indexes, null);
					}
					if (num4 == 0)
					{
						num4 = FindMostSpecificProperty(match[num3], match[j]);
						if (num4 == 0)
						{
							flag = true;
						}
					}
					if (num4 == 2)
					{
						flag = false;
						num3 = j;
					}
				}
				if (flag)
				{
					throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.Ambiguous"));
				}
				return match[num3];
			}
			}
		}

		public override object ChangeType(object value, Type type, CultureInfo cultureInfo)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_ChangeType"));
		}

		public override void ReorderArgumentArray(ref object[] args, object state)
		{
			BinderState binderState = (BinderState)state;
			ReorderParams(binderState.m_argsMap, args);
			if (binderState.m_isParamArray)
			{
				int num = args.Length - 1;
				if (args.Length == binderState.m_originalSize)
				{
					args[num] = ((object[])args[num])[0];
					return;
				}
				object[] array = new object[args.Length];
				Array.Copy(args, 0, array, 0, num);
				int num2 = num;
				int num3 = 0;
				while (num2 < array.Length)
				{
					array[num2] = ((object[])args[num])[num3];
					num2++;
					num3++;
				}
				args = array;
			}
			else if (args.Length > binderState.m_originalSize)
			{
				object[] array2 = new object[binderState.m_originalSize];
				Array.Copy(args, 0, array2, 0, binderState.m_originalSize);
				args = array2;
			}
		}

		public static MethodBase ExactBinding(MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
		{
			if (match == null)
			{
				throw new ArgumentNullException("match");
			}
			MethodBase[] array = new MethodBase[match.Length];
			int num = 0;
			for (int i = 0; i < match.Length; i++)
			{
				ParameterInfo[] parametersNoCopy = match[i].GetParametersNoCopy();
				if (parametersNoCopy.Length == 0)
				{
					continue;
				}
				int j;
				for (j = 0; j < types.Length; j++)
				{
					Type parameterType = parametersNoCopy[j].ParameterType;
					if (!parameterType.Equals(types[j]))
					{
						break;
					}
				}
				if (j >= types.Length)
				{
					array[num] = match[i];
					num++;
				}
			}
			return num switch
			{
				0 => null, 
				1 => array[0], 
				_ => FindMostDerivedNewSlotMeth(array, num), 
			};
		}

		public static PropertyInfo ExactPropertyBinding(PropertyInfo[] match, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			if (match == null)
			{
				throw new ArgumentNullException("match");
			}
			PropertyInfo propertyInfo = null;
			int num = ((types != null) ? types.Length : 0);
			for (int i = 0; i < match.Length; i++)
			{
				ParameterInfo[] indexParameters = match[i].GetIndexParameters();
				int j;
				for (j = 0; j < num; j++)
				{
					Type parameterType = indexParameters[j].ParameterType;
					if (parameterType != types[j])
					{
						break;
					}
				}
				if (j >= num && (returnType == null || returnType == match[i].PropertyType))
				{
					if (propertyInfo != null)
					{
						throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.Ambiguous"));
					}
					propertyInfo = match[i];
				}
			}
			return propertyInfo;
		}

		private static int FindMostSpecific(ParameterInfo[] p1, int[] paramOrder1, Type paramArrayType1, ParameterInfo[] p2, int[] paramOrder2, Type paramArrayType2, Type[] types, object[] args)
		{
			if (paramArrayType1 != null && paramArrayType2 == null)
			{
				return 2;
			}
			if (paramArrayType2 != null && paramArrayType1 == null)
			{
				return 1;
			}
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < types.Length; i++)
			{
				if (args != null && args[i] == Type.Missing)
				{
					continue;
				}
				Type type = p1[paramOrder1[i]].ParameterType;
				Type type2 = p2[paramOrder2[i]].ParameterType;
				if (i == p1.Length - 1 && paramOrder1[i] == p1.Length - 1 && paramArrayType1 != null)
				{
					type = paramArrayType1;
				}
				if (i == p2.Length - 1 && paramOrder2[i] == p2.Length - 1 && paramArrayType2 != null)
				{
					type2 = paramArrayType2;
				}
				if (type != type2)
				{
					switch (FindMostSpecificType(type, type2, types[i]))
					{
					case 0:
						return 0;
					case 1:
						flag = true;
						break;
					case 2:
						flag2 = true;
						break;
					}
				}
			}
			if (flag == flag2)
			{
				if (!flag && p1.Length != p2.Length && args != null)
				{
					if (p1.Length == args.Length)
					{
						return 1;
					}
					if (p2.Length == args.Length)
					{
						return 2;
					}
				}
				return 0;
			}
			if (!flag)
			{
				return 2;
			}
			return 1;
		}

		private static int FindMostSpecificType(Type c1, Type c2, Type t)
		{
			if (c1 == c2)
			{
				return 0;
			}
			if (c1 == t)
			{
				return 1;
			}
			if (c2 == t)
			{
				return 2;
			}
			if (c1.IsByRef || c2.IsByRef)
			{
				if (c1.IsByRef && c2.IsByRef)
				{
					c1 = c1.GetElementType();
					c2 = c2.GetElementType();
				}
				else if (c1.IsByRef)
				{
					if (c1.GetElementType() == c2)
					{
						return 2;
					}
					c1 = c1.GetElementType();
				}
				else
				{
					if (c2.GetElementType() == c1)
					{
						return 1;
					}
					c2 = c2.GetElementType();
				}
			}
			bool flag;
			bool flag2;
			if (c1.IsPrimitive && c2.IsPrimitive)
			{
				flag = CanConvertPrimitive((RuntimeType)c2, (RuntimeType)c1);
				flag2 = CanConvertPrimitive((RuntimeType)c1, (RuntimeType)c2);
			}
			else
			{
				flag = c1.IsAssignableFrom(c2);
				flag2 = c2.IsAssignableFrom(c1);
			}
			if (flag == flag2)
			{
				return 0;
			}
			if (flag)
			{
				return 2;
			}
			return 1;
		}

		private static int FindMostSpecificMethod(MethodBase m1, int[] paramOrder1, Type paramArrayType1, MethodBase m2, int[] paramOrder2, Type paramArrayType2, Type[] types, object[] args)
		{
			int num = FindMostSpecific(m1.GetParametersNoCopy(), paramOrder1, paramArrayType1, m2.GetParametersNoCopy(), paramOrder2, paramArrayType2, types, args);
			if (num != 0)
			{
				return num;
			}
			if (CompareMethodSigAndName(m1, m2))
			{
				int hierarchyDepth = GetHierarchyDepth(m1.DeclaringType);
				int hierarchyDepth2 = GetHierarchyDepth(m2.DeclaringType);
				if (hierarchyDepth == hierarchyDepth2)
				{
					return 0;
				}
				if (hierarchyDepth < hierarchyDepth2)
				{
					return 2;
				}
				return 1;
			}
			return 0;
		}

		private static int FindMostSpecificField(FieldInfo cur1, FieldInfo cur2)
		{
			if (cur1.Name == cur2.Name)
			{
				int hierarchyDepth = GetHierarchyDepth(cur1.DeclaringType);
				int hierarchyDepth2 = GetHierarchyDepth(cur2.DeclaringType);
				if (hierarchyDepth == hierarchyDepth2)
				{
					return 0;
				}
				if (hierarchyDepth < hierarchyDepth2)
				{
					return 2;
				}
				return 1;
			}
			return 0;
		}

		private static int FindMostSpecificProperty(PropertyInfo cur1, PropertyInfo cur2)
		{
			if (cur1.Name == cur2.Name)
			{
				int hierarchyDepth = GetHierarchyDepth(cur1.DeclaringType);
				int hierarchyDepth2 = GetHierarchyDepth(cur2.DeclaringType);
				if (hierarchyDepth == hierarchyDepth2)
				{
					return 0;
				}
				if (hierarchyDepth < hierarchyDepth2)
				{
					return 2;
				}
				return 1;
			}
			return 0;
		}

		internal static bool CompareMethodSigAndName(MethodBase m1, MethodBase m2)
		{
			ParameterInfo[] parametersNoCopy = m1.GetParametersNoCopy();
			ParameterInfo[] parametersNoCopy2 = m2.GetParametersNoCopy();
			if (parametersNoCopy.Length != parametersNoCopy2.Length)
			{
				return false;
			}
			int num = parametersNoCopy.Length;
			for (int i = 0; i < num; i++)
			{
				if (parametersNoCopy[i].ParameterType != parametersNoCopy2[i].ParameterType)
				{
					return false;
				}
			}
			return true;
		}

		internal static int GetHierarchyDepth(Type t)
		{
			int num = 0;
			Type type = t;
			do
			{
				num++;
				type = type.BaseType;
			}
			while (type != null);
			return num;
		}

		internal static MethodBase FindMostDerivedNewSlotMeth(MethodBase[] match, int cMatches)
		{
			int num = 0;
			MethodBase result = null;
			for (int i = 0; i < cMatches; i++)
			{
				int hierarchyDepth = GetHierarchyDepth(match[i].DeclaringType);
				if (hierarchyDepth == num)
				{
					throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.Ambiguous"));
				}
				if (hierarchyDepth > num)
				{
					num = hierarchyDepth;
					result = match[i];
				}
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool CanConvertPrimitive(RuntimeType source, RuntimeType target);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool CanConvertPrimitiveObjectToType(object source, RuntimeType type);

		private static void ReorderParams(int[] paramOrder, object[] vars)
		{
			for (int i = 0; i < vars.Length; i++)
			{
				while (paramOrder[i] != i)
				{
					int num = paramOrder[paramOrder[i]];
					object obj = vars[paramOrder[i]];
					paramOrder[paramOrder[i]] = paramOrder[i];
					vars[paramOrder[i]] = vars[i];
					paramOrder[i] = num;
					vars[i] = obj;
				}
			}
		}

		private static bool CreateParamOrder(int[] paramOrder, ParameterInfo[] pars, string[] names)
		{
			bool[] array = new bool[pars.Length];
			for (int i = 0; i < pars.Length; i++)
			{
				paramOrder[i] = -1;
			}
			for (int j = 0; j < names.Length; j++)
			{
				int k;
				for (k = 0; k < pars.Length; k++)
				{
					if (names[j].Equals(pars[k].Name))
					{
						paramOrder[k] = j;
						array[j] = true;
						break;
					}
				}
				if (k == pars.Length)
				{
					return false;
				}
			}
			int l = 0;
			for (int m = 0; m < pars.Length; m++)
			{
				if (paramOrder[m] != -1)
				{
					continue;
				}
				for (; l < pars.Length; l++)
				{
					if (!array[l])
					{
						paramOrder[m] = l;
						l++;
						break;
					}
				}
			}
			return true;
		}
	}
}
