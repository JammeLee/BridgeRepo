using System.Reflection;

namespace System
{
	[Serializable]
	internal class __Filters
	{
		internal virtual bool FilterAttribute(MemberInfo m, object filterCriteria)
		{
			if (filterCriteria == null)
			{
				throw new InvalidFilterCriteriaException(Environment.GetResourceString("RFLCT.FltCritInt"));
			}
			switch (m.MemberType)
			{
			case MemberTypes.Constructor:
			case MemberTypes.Method:
			{
				MethodAttributes methodAttributes = MethodAttributes.PrivateScope;
				try
				{
					int num2 = (int)filterCriteria;
					methodAttributes = (MethodAttributes)num2;
				}
				catch
				{
					throw new InvalidFilterCriteriaException(Environment.GetResourceString("RFLCT.FltCritInt"));
				}
				MethodAttributes methodAttributes2 = ((m.MemberType != MemberTypes.Method) ? ((ConstructorInfo)m).Attributes : ((MethodInfo)m).Attributes);
				if ((methodAttributes & MethodAttributes.MemberAccessMask) != 0 && (methodAttributes2 & MethodAttributes.MemberAccessMask) != (methodAttributes & MethodAttributes.MemberAccessMask))
				{
					return false;
				}
				if ((methodAttributes & MethodAttributes.Static) != 0 && (methodAttributes2 & MethodAttributes.Static) == 0)
				{
					return false;
				}
				if ((methodAttributes & MethodAttributes.Final) != 0 && (methodAttributes2 & MethodAttributes.Final) == 0)
				{
					return false;
				}
				if ((methodAttributes & MethodAttributes.Virtual) != 0 && (methodAttributes2 & MethodAttributes.Virtual) == 0)
				{
					return false;
				}
				if ((methodAttributes & MethodAttributes.Abstract) != 0 && (methodAttributes2 & MethodAttributes.Abstract) == 0)
				{
					return false;
				}
				if ((methodAttributes & MethodAttributes.SpecialName) != 0 && (methodAttributes2 & MethodAttributes.SpecialName) == 0)
				{
					return false;
				}
				return true;
			}
			case MemberTypes.Field:
			{
				FieldAttributes fieldAttributes = FieldAttributes.PrivateScope;
				try
				{
					int num = (int)filterCriteria;
					fieldAttributes = (FieldAttributes)num;
				}
				catch
				{
					throw new InvalidFilterCriteriaException(Environment.GetResourceString("RFLCT.FltCritInt"));
				}
				FieldAttributes attributes = ((FieldInfo)m).Attributes;
				if ((fieldAttributes & FieldAttributes.FieldAccessMask) != 0 && (attributes & FieldAttributes.FieldAccessMask) != (fieldAttributes & FieldAttributes.FieldAccessMask))
				{
					return false;
				}
				if ((fieldAttributes & FieldAttributes.Static) != 0 && (attributes & FieldAttributes.Static) == 0)
				{
					return false;
				}
				if ((fieldAttributes & FieldAttributes.InitOnly) != 0 && (attributes & FieldAttributes.InitOnly) == 0)
				{
					return false;
				}
				if ((fieldAttributes & FieldAttributes.Literal) != 0 && (attributes & FieldAttributes.Literal) == 0)
				{
					return false;
				}
				if ((fieldAttributes & FieldAttributes.NotSerialized) != 0 && (attributes & FieldAttributes.NotSerialized) == 0)
				{
					return false;
				}
				if ((fieldAttributes & FieldAttributes.PinvokeImpl) != 0 && (attributes & FieldAttributes.PinvokeImpl) == 0)
				{
					return false;
				}
				return true;
			}
			default:
				return false;
			}
		}

		internal virtual bool FilterName(MemberInfo m, object filterCriteria)
		{
			if (filterCriteria == null || !(filterCriteria is string))
			{
				throw new InvalidFilterCriteriaException(Environment.GetResourceString("RFLCT.FltCritString"));
			}
			string text = (string)filterCriteria;
			text = text.Trim();
			string text2 = m.Name;
			if (m.MemberType == MemberTypes.NestedType)
			{
				text2 = text2.Substring(text2.LastIndexOf('+') + 1);
			}
			if (text.Length > 0 && text[text.Length - 1] == '*')
			{
				text = text.Substring(0, text.Length - 1);
				return text2.StartsWith(text, StringComparison.Ordinal);
			}
			return text2.Equals(text);
		}

		internal virtual bool FilterIgnoreCase(MemberInfo m, object filterCriteria)
		{
			if (filterCriteria == null || !(filterCriteria is string))
			{
				throw new InvalidFilterCriteriaException(Environment.GetResourceString("RFLCT.FltCritString"));
			}
			string text = (string)filterCriteria;
			text = text.Trim();
			string text2 = m.Name;
			if (m.MemberType == MemberTypes.NestedType)
			{
				text2 = text2.Substring(text2.LastIndexOf('+') + 1);
			}
			if (text.Length > 0 && text[text.Length - 1] == '*')
			{
				text = text.Substring(0, text.Length - 1);
				return string.Compare(text2, 0, text, 0, text.Length, StringComparison.OrdinalIgnoreCase) == 0;
			}
			return string.Compare(text, text2, StringComparison.OrdinalIgnoreCase) == 0;
		}
	}
}
