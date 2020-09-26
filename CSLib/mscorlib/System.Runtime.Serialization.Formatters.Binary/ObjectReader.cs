using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace System.Runtime.Serialization.Formatters.Binary
{
	internal sealed class ObjectReader
	{
		internal class TypeNAssembly
		{
			public Type type;

			public string assemblyName;
		}

		private const int THRESHOLD_FOR_VALUETYPE_IDS = int.MaxValue;

		internal Stream m_stream;

		internal ISurrogateSelector m_surrogates;

		internal StreamingContext m_context;

		internal ObjectManager m_objectManager;

		internal InternalFE formatterEnums;

		internal SerializationBinder m_binder;

		internal long topId;

		internal bool bSimpleAssembly;

		internal object handlerObject;

		internal object m_topObject;

		internal Header[] headers;

		internal HeaderHandler handler;

		internal SerObjectInfoInit serObjectInfoInit;

		internal IFormatterConverter m_formatterConverter;

		internal SerStack stack;

		private SerStack valueFixupStack;

		internal object[] crossAppDomainArray;

		private bool bMethodCall;

		private bool bMethodReturn;

		private bool bFullDeserialization;

		private BinaryMethodCall binaryMethodCall;

		private BinaryMethodReturn binaryMethodReturn;

		private bool bIsCrossAppDomain;

		private static FileIOPermission sfileIOPermission = new FileIOPermission(PermissionState.Unrestricted);

		private bool bOldFormatDetected;

		private IntSizedArray valTypeObjectIdTable;

		private NameCache typeCache = new NameCache();

		private string previousAssemblyString;

		private string previousName;

		private Type previousType;

		private SerStack ValueFixupStack
		{
			get
			{
				if (valueFixupStack == null)
				{
					valueFixupStack = new SerStack("ValueType Fixup Stack");
				}
				return valueFixupStack;
			}
		}

		internal object TopObject
		{
			get
			{
				return m_topObject;
			}
			set
			{
				m_topObject = value;
				if (m_objectManager != null)
				{
					m_objectManager.TopObject = value;
				}
			}
		}

		private bool IsRemoting
		{
			get
			{
				if (!bMethodCall)
				{
					return bMethodReturn;
				}
				return true;
			}
		}

		internal void SetMethodCall(BinaryMethodCall binaryMethodCall)
		{
			bMethodCall = true;
			this.binaryMethodCall = binaryMethodCall;
		}

		internal void SetMethodReturn(BinaryMethodReturn binaryMethodReturn)
		{
			bMethodReturn = true;
			this.binaryMethodReturn = binaryMethodReturn;
		}

		internal ObjectReader(Stream stream, ISurrogateSelector selector, StreamingContext context, InternalFE formatterEnums, SerializationBinder binder)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream", Environment.GetResourceString("ArgumentNull_Stream"));
			}
			m_stream = stream;
			m_surrogates = selector;
			m_context = context;
			m_binder = binder;
			if (m_binder != null)
			{
				ResourceReader.TypeLimitingDeserializationBinder typeLimitingDeserializationBinder = m_binder as ResourceReader.TypeLimitingDeserializationBinder;
				if (typeLimitingDeserializationBinder != null)
				{
					typeLimitingDeserializationBinder.ObjectReader = this;
				}
			}
			this.formatterEnums = formatterEnums;
		}

		internal object Deserialize(HeaderHandler handler, __BinaryParser serParser, bool fCheck, bool isCrossAppDomain, IMethodCallMessage methodCallMessage)
		{
			bFullDeserialization = false;
			bMethodCall = false;
			bMethodReturn = false;
			TopObject = null;
			topId = 0L;
			bIsCrossAppDomain = isCrossAppDomain;
			bSimpleAssembly = formatterEnums.FEassemblyFormat == FormatterAssemblyStyle.Simple;
			if (serParser == null)
			{
				throw new ArgumentNullException("serParser", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentNull_WithParamName"), serParser));
			}
			if (fCheck)
			{
				CodeAccessPermission.DemandInternal(PermissionType.SecuritySerialization);
			}
			this.handler = handler;
			if (bFullDeserialization)
			{
				m_objectManager = new ObjectManager(m_surrogates, m_context, checkSecurity: false, bIsCrossAppDomain);
				serObjectInfoInit = new SerObjectInfoInit();
			}
			serParser.Run();
			if (bFullDeserialization)
			{
				m_objectManager.DoFixups();
			}
			if (!bMethodCall && !bMethodReturn)
			{
				if (TopObject == null)
				{
					throw new SerializationException(Environment.GetResourceString("Serialization_TopObject"));
				}
				if (HasSurrogate(TopObject.GetType()) && topId != 0)
				{
					TopObject = m_objectManager.GetObject(topId);
				}
				if (TopObject is IObjectReference)
				{
					TopObject = ((IObjectReference)TopObject).GetRealObject(m_context);
				}
			}
			if (bFullDeserialization)
			{
				m_objectManager.RaiseDeserializationEvent();
			}
			if (handler != null)
			{
				handlerObject = handler(headers);
			}
			if (bMethodCall)
			{
				object[] callA = TopObject as object[];
				TopObject = binaryMethodCall.ReadArray(callA, handlerObject);
			}
			else if (bMethodReturn)
			{
				object[] returnA = TopObject as object[];
				TopObject = binaryMethodReturn.ReadArray(returnA, methodCallMessage, handlerObject);
			}
			return TopObject;
		}

		private bool HasSurrogate(Type t)
		{
			if (m_surrogates == null)
			{
				return false;
			}
			ISurrogateSelector selector;
			return m_surrogates.GetSurrogate(t, m_context, out selector) != null;
		}

		private void CheckSerializable(Type t)
		{
			if (!t.IsSerializable && !HasSurrogate(t))
			{
				throw new SerializationException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Serialization_NonSerType"), t.FullName, t.Assembly.FullName));
			}
		}

		private void InitFullDeserialization()
		{
			bFullDeserialization = true;
			stack = new SerStack("ObjectReader Object Stack");
			m_objectManager = new ObjectManager(m_surrogates, m_context, checkSecurity: false, bIsCrossAppDomain);
			if (m_formatterConverter == null)
			{
				m_formatterConverter = new FormatterConverter();
			}
		}

		internal object CrossAppDomainArray(int index)
		{
			return crossAppDomainArray[index];
		}

		internal ReadObjectInfo CreateReadObjectInfo(Type objectType)
		{
			return ReadObjectInfo.Create(objectType, m_surrogates, m_context, m_objectManager, serObjectInfoInit, m_formatterConverter, bSimpleAssembly);
		}

		internal ReadObjectInfo CreateReadObjectInfo(Type objectType, string[] memberNames, Type[] memberTypes)
		{
			return ReadObjectInfo.Create(objectType, memberNames, memberTypes, m_surrogates, m_context, m_objectManager, serObjectInfoInit, m_formatterConverter, bSimpleAssembly);
		}

		internal void Parse(ParseRecord pr)
		{
			switch (pr.PRparseTypeEnum)
			{
			case InternalParseTypeE.SerializedStreamHeader:
				ParseSerializedStreamHeader(pr);
				break;
			case InternalParseTypeE.SerializedStreamHeaderEnd:
				ParseSerializedStreamHeaderEnd(pr);
				break;
			case InternalParseTypeE.Object:
				ParseObject(pr);
				break;
			case InternalParseTypeE.ObjectEnd:
				ParseObjectEnd(pr);
				break;
			case InternalParseTypeE.Member:
				ParseMember(pr);
				break;
			case InternalParseTypeE.MemberEnd:
				ParseMemberEnd(pr);
				break;
			default:
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_XMLElement"), pr.PRname));
			case InternalParseTypeE.Envelope:
			case InternalParseTypeE.EnvelopeEnd:
			case InternalParseTypeE.Body:
			case InternalParseTypeE.BodyEnd:
				break;
			}
		}

		private void ParseError(ParseRecord processing, ParseRecord onStack)
		{
			throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_ParseError"), string.Concat(onStack.PRname, " ", onStack.PRparseTypeEnum, " ", processing.PRname, " ", processing.PRparseTypeEnum)));
		}

		private void ParseSerializedStreamHeader(ParseRecord pr)
		{
			stack.Push(pr);
		}

		private void ParseSerializedStreamHeaderEnd(ParseRecord pr)
		{
			stack.Pop();
		}

		internal void CheckSecurity(ParseRecord pr)
		{
			Type pRdtType = pr.PRdtType;
			if (pRdtType != null && IsRemoting)
			{
				if (typeof(MarshalByRefObject).IsAssignableFrom(pRdtType))
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_MBRAsMBV"), pRdtType.FullName));
				}
				FormatterServices.CheckTypeSecurity(pRdtType, formatterEnums.FEsecurityLevel);
			}
		}

		private void ParseObject(ParseRecord pr)
		{
			if (!bFullDeserialization)
			{
				InitFullDeserialization();
			}
			if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
			{
				topId = pr.PRobjectId;
			}
			if (pr.PRparseTypeEnum == InternalParseTypeE.Object)
			{
				stack.Push(pr);
			}
			if (pr.PRobjectTypeEnum == InternalObjectTypeE.Array)
			{
				ParseArray(pr);
				return;
			}
			if (pr.PRdtType == null)
			{
				pr.PRnewObj = new TypeLoadExceptionHolder(pr.PRkeyDt);
				return;
			}
			if (pr.PRdtType == Converter.typeofString)
			{
				if (pr.PRvalue != null)
				{
					pr.PRnewObj = pr.PRvalue;
					if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
					{
						TopObject = pr.PRnewObj;
						return;
					}
					stack.Pop();
					RegisterObject(pr.PRnewObj, pr, (ParseRecord)stack.Peek());
				}
				return;
			}
			CheckSerializable(pr.PRdtType);
			if (IsRemoting && formatterEnums.FEsecurityLevel != TypeFilterLevel.Full)
			{
				pr.PRnewObj = FormatterServices.GetSafeUninitializedObject(pr.PRdtType);
			}
			else
			{
				pr.PRnewObj = FormatterServices.GetUninitializedObject(pr.PRdtType);
			}
			m_objectManager.RaiseOnDeserializingEvent(pr.PRnewObj);
			if (pr.PRnewObj == null)
			{
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_TopObjectInstantiate"), pr.PRdtType));
			}
			if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
			{
				TopObject = pr.PRnewObj;
			}
			if (pr.PRobjectInfo == null)
			{
				pr.PRobjectInfo = ReadObjectInfo.Create(pr.PRdtType, m_surrogates, m_context, m_objectManager, serObjectInfoInit, m_formatterConverter, bSimpleAssembly);
			}
			CheckSecurity(pr);
		}

		private void ParseObjectEnd(ParseRecord pr)
		{
			ParseRecord parseRecord = (ParseRecord)stack.Peek();
			if (parseRecord == null)
			{
				parseRecord = pr;
			}
			if (parseRecord.PRobjectPositionEnum == InternalObjectPositionE.Top && parseRecord.PRdtType == Converter.typeofString)
			{
				parseRecord.PRnewObj = parseRecord.PRvalue;
				TopObject = parseRecord.PRnewObj;
				return;
			}
			stack.Pop();
			ParseRecord parseRecord2 = (ParseRecord)stack.Peek();
			if (parseRecord.PRnewObj == null)
			{
				return;
			}
			if (parseRecord.PRobjectTypeEnum == InternalObjectTypeE.Array)
			{
				if (parseRecord.PRobjectPositionEnum == InternalObjectPositionE.Top)
				{
					TopObject = parseRecord.PRnewObj;
				}
				RegisterObject(parseRecord.PRnewObj, parseRecord, parseRecord2);
				return;
			}
			parseRecord.PRobjectInfo.PopulateObjectMembers(parseRecord.PRnewObj, parseRecord.PRmemberData);
			if (!parseRecord.PRisRegistered && parseRecord.PRobjectId > 0)
			{
				RegisterObject(parseRecord.PRnewObj, parseRecord, parseRecord2);
			}
			if (parseRecord.PRisValueTypeFixup)
			{
				ValueFixup valueFixup = (ValueFixup)ValueFixupStack.Pop();
				valueFixup.Fixup(parseRecord, parseRecord2);
			}
			if (parseRecord.PRobjectPositionEnum == InternalObjectPositionE.Top)
			{
				TopObject = parseRecord.PRnewObj;
			}
			parseRecord.PRobjectInfo.ObjectEnd();
		}

		private void ParseArray(ParseRecord pr)
		{
			_ = pr.PRobjectId;
			if (pr.PRarrayTypeEnum == InternalArrayTypeE.Base64)
			{
				if (pr.PRvalue.Length > 0)
				{
					pr.PRnewObj = Convert.FromBase64String(pr.PRvalue);
				}
				else
				{
					pr.PRnewObj = new byte[0];
				}
				if (stack.Peek() == pr)
				{
					stack.Pop();
				}
				if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
				{
					TopObject = pr.PRnewObj;
				}
				ParseRecord objectPr = (ParseRecord)stack.Peek();
				RegisterObject(pr.PRnewObj, pr, objectPr);
			}
			else if (pr.PRnewObj != null && Converter.IsWriteAsByteArray(pr.PRarrayElementTypeCode))
			{
				if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
				{
					TopObject = pr.PRnewObj;
				}
				ParseRecord objectPr2 = (ParseRecord)stack.Peek();
				RegisterObject(pr.PRnewObj, pr, objectPr2);
			}
			else if (pr.PRarrayTypeEnum == InternalArrayTypeE.Jagged || pr.PRarrayTypeEnum == InternalArrayTypeE.Single)
			{
				bool flag = true;
				if (pr.PRlowerBoundA == null || pr.PRlowerBoundA[0] == 0)
				{
					if (pr.PRarrayElementType == Converter.typeofString)
					{
						pr.PRobjectA = new string[pr.PRlengthA[0]];
						pr.PRnewObj = pr.PRobjectA;
						flag = false;
					}
					else if (pr.PRarrayElementType == Converter.typeofObject)
					{
						pr.PRobjectA = new object[pr.PRlengthA[0]];
						pr.PRnewObj = pr.PRobjectA;
						flag = false;
					}
					else if (pr.PRarrayElementType != null)
					{
						pr.PRnewObj = Array.CreateInstance(pr.PRarrayElementType, pr.PRlengthA[0]);
					}
					pr.PRisLowerBound = false;
				}
				else
				{
					if (pr.PRarrayElementType != null)
					{
						pr.PRnewObj = Array.CreateInstance(pr.PRarrayElementType, pr.PRlengthA, pr.PRlowerBoundA);
					}
					pr.PRisLowerBound = true;
				}
				if (pr.PRarrayTypeEnum == InternalArrayTypeE.Single)
				{
					if (!pr.PRisLowerBound && Converter.IsWriteAsByteArray(pr.PRarrayElementTypeCode))
					{
						pr.PRprimitiveArray = new PrimitiveArray(pr.PRarrayElementTypeCode, (Array)pr.PRnewObj);
					}
					else if (flag && pr.PRarrayElementType != null && !pr.PRarrayElementType.IsValueType && !pr.PRisLowerBound)
					{
						pr.PRobjectA = (object[])pr.PRnewObj;
					}
				}
				if (pr.PRobjectPositionEnum == InternalObjectPositionE.Headers)
				{
					headers = (Header[])pr.PRnewObj;
				}
				pr.PRindexMap = new int[1];
			}
			else
			{
				if (pr.PRarrayTypeEnum != InternalArrayTypeE.Rectangular)
				{
					throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_ArrayType"), pr.PRarrayTypeEnum));
				}
				pr.PRisLowerBound = false;
				if (pr.PRlowerBoundA != null)
				{
					for (int i = 0; i < pr.PRrank; i++)
					{
						if (pr.PRlowerBoundA[i] != 0)
						{
							pr.PRisLowerBound = true;
						}
					}
				}
				if (pr.PRarrayElementType != null)
				{
					if (!pr.PRisLowerBound)
					{
						pr.PRnewObj = Array.CreateInstance(pr.PRarrayElementType, pr.PRlengthA);
					}
					else
					{
						pr.PRnewObj = Array.CreateInstance(pr.PRarrayElementType, pr.PRlengthA, pr.PRlowerBoundA);
					}
				}
				int num = 1;
				for (int j = 0; j < pr.PRrank; j++)
				{
					num *= pr.PRlengthA[j];
				}
				pr.PRindexMap = new int[pr.PRrank];
				pr.PRrectangularMap = new int[pr.PRrank];
				pr.PRlinearlength = num;
			}
			CheckSecurity(pr);
		}

		private void NextRectangleMap(ParseRecord pr)
		{
			for (int num = pr.PRrank - 1; num > -1; num--)
			{
				if (pr.PRrectangularMap[num] < pr.PRlengthA[num] - 1)
				{
					pr.PRrectangularMap[num]++;
					if (num < pr.PRrank - 1)
					{
						for (int i = num + 1; i < pr.PRrank; i++)
						{
							pr.PRrectangularMap[i] = 0;
						}
					}
					Array.Copy(pr.PRrectangularMap, pr.PRindexMap, pr.PRrank);
					break;
				}
			}
		}

		private void ParseArrayMember(ParseRecord pr)
		{
			ParseRecord parseRecord = (ParseRecord)stack.Peek();
			if (parseRecord.PRarrayTypeEnum == InternalArrayTypeE.Rectangular)
			{
				if (parseRecord.PRmemberIndex > 0)
				{
					NextRectangleMap(parseRecord);
				}
				if (parseRecord.PRisLowerBound)
				{
					for (int i = 0; i < parseRecord.PRrank; i++)
					{
						parseRecord.PRindexMap[i] = parseRecord.PRrectangularMap[i] + parseRecord.PRlowerBoundA[i];
					}
				}
			}
			else if (!parseRecord.PRisLowerBound)
			{
				parseRecord.PRindexMap[0] = parseRecord.PRmemberIndex;
			}
			else
			{
				parseRecord.PRindexMap[0] = parseRecord.PRlowerBoundA[0] + parseRecord.PRmemberIndex;
			}
			if (pr.PRmemberValueEnum == InternalMemberValueE.Reference)
			{
				object @object = m_objectManager.GetObject(pr.PRidRef);
				if (@object == null)
				{
					int[] array = new int[parseRecord.PRrank];
					Array.Copy(parseRecord.PRindexMap, 0, array, 0, parseRecord.PRrank);
					m_objectManager.RecordArrayElementFixup(parseRecord.PRobjectId, array, pr.PRidRef);
				}
				else if (parseRecord.PRobjectA != null)
				{
					parseRecord.PRobjectA[parseRecord.PRindexMap[0]] = @object;
				}
				else
				{
					((Array)parseRecord.PRnewObj).SetValue(@object, parseRecord.PRindexMap);
				}
			}
			else if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
			{
				if (pr.PRdtType == null)
				{
					pr.PRdtType = parseRecord.PRarrayElementType;
				}
				ParseObject(pr);
				stack.Push(pr);
				if (parseRecord.PRarrayElementType != null)
				{
					if (parseRecord.PRarrayElementType.IsValueType && pr.PRarrayElementTypeCode == InternalPrimitiveTypeE.Invalid)
					{
						pr.PRisValueTypeFixup = true;
						ValueFixupStack.Push(new ValueFixup((Array)parseRecord.PRnewObj, parseRecord.PRindexMap));
					}
					else if (parseRecord.PRobjectA != null)
					{
						parseRecord.PRobjectA[parseRecord.PRindexMap[0]] = pr.PRnewObj;
					}
					else
					{
						((Array)parseRecord.PRnewObj).SetValue(pr.PRnewObj, parseRecord.PRindexMap);
					}
				}
			}
			else if (pr.PRmemberValueEnum == InternalMemberValueE.InlineValue)
			{
				if (parseRecord.PRarrayElementType == Converter.typeofString || pr.PRdtType == Converter.typeofString)
				{
					ParseString(pr, parseRecord);
					if (parseRecord.PRobjectA != null)
					{
						parseRecord.PRobjectA[parseRecord.PRindexMap[0]] = pr.PRvalue;
					}
					else
					{
						((Array)parseRecord.PRnewObj).SetValue(pr.PRvalue, parseRecord.PRindexMap);
					}
				}
				else if (parseRecord.PRisArrayVariant)
				{
					if (pr.PRkeyDt == null)
					{
						throw new SerializationException(Environment.GetResourceString("Serialization_ArrayTypeObject"));
					}
					object obj = null;
					if (pr.PRdtType == Converter.typeofString)
					{
						ParseString(pr, parseRecord);
						obj = pr.PRvalue;
					}
					else if (pr.PRdtTypeCode != 0)
					{
						obj = ((pr.PRvarValue == null) ? Converter.FromString(pr.PRvalue, pr.PRdtTypeCode) : pr.PRvarValue);
					}
					else
					{
						CheckSerializable(pr.PRdtType);
						obj = ((!IsRemoting || formatterEnums.FEsecurityLevel == TypeFilterLevel.Full) ? FormatterServices.GetUninitializedObject(pr.PRdtType) : FormatterServices.GetSafeUninitializedObject(pr.PRdtType));
					}
					if (parseRecord.PRobjectA != null)
					{
						parseRecord.PRobjectA[parseRecord.PRindexMap[0]] = obj;
					}
					else
					{
						((Array)parseRecord.PRnewObj).SetValue(obj, parseRecord.PRindexMap);
					}
				}
				else if (parseRecord.PRprimitiveArray != null)
				{
					parseRecord.PRprimitiveArray.SetValue(pr.PRvalue, parseRecord.PRindexMap[0]);
				}
				else
				{
					object obj2 = null;
					obj2 = ((pr.PRvarValue == null) ? Converter.FromString(pr.PRvalue, parseRecord.PRarrayElementTypeCode) : pr.PRvarValue);
					if (parseRecord.PRobjectA != null)
					{
						parseRecord.PRobjectA[parseRecord.PRindexMap[0]] = obj2;
					}
					else
					{
						((Array)parseRecord.PRnewObj).SetValue(obj2, parseRecord.PRindexMap);
					}
				}
			}
			else if (pr.PRmemberValueEnum == InternalMemberValueE.Null)
			{
				parseRecord.PRmemberIndex += pr.PRnullCount - 1;
			}
			else
			{
				ParseError(pr, parseRecord);
			}
			parseRecord.PRmemberIndex++;
		}

		private void ParseArrayMemberEnd(ParseRecord pr)
		{
			if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
			{
				ParseObjectEnd(pr);
			}
		}

		private void ParseMember(ParseRecord pr)
		{
			ParseRecord parseRecord = (ParseRecord)stack.Peek();
			_ = parseRecord?.PRname;
			switch (pr.PRmemberTypeEnum)
			{
			case InternalMemberTypeE.Item:
				ParseArrayMember(pr);
				return;
			}
			if (pr.PRdtType == null && parseRecord.PRobjectInfo.isTyped)
			{
				pr.PRdtType = parseRecord.PRobjectInfo.GetType(pr.PRname);
				if (pr.PRdtType != null)
				{
					pr.PRdtTypeCode = Converter.ToCode(pr.PRdtType);
				}
			}
			if (pr.PRmemberValueEnum == InternalMemberValueE.Null)
			{
				parseRecord.PRobjectInfo.AddValue(pr.PRname, null, ref parseRecord.PRsi, ref parseRecord.PRmemberData);
			}
			else if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
			{
				ParseObject(pr);
				stack.Push(pr);
				if (pr.PRobjectInfo != null && pr.PRobjectInfo.objectType != null && pr.PRobjectInfo.objectType.IsValueType)
				{
					pr.PRisValueTypeFixup = true;
					ValueFixupStack.Push(new ValueFixup(parseRecord.PRnewObj, pr.PRname, parseRecord.PRobjectInfo));
				}
				else
				{
					parseRecord.PRobjectInfo.AddValue(pr.PRname, pr.PRnewObj, ref parseRecord.PRsi, ref parseRecord.PRmemberData);
				}
			}
			else if (pr.PRmemberValueEnum == InternalMemberValueE.Reference)
			{
				object @object = m_objectManager.GetObject(pr.PRidRef);
				if (@object == null)
				{
					parseRecord.PRobjectInfo.AddValue(pr.PRname, null, ref parseRecord.PRsi, ref parseRecord.PRmemberData);
					parseRecord.PRobjectInfo.RecordFixup(parseRecord.PRobjectId, pr.PRname, pr.PRidRef);
				}
				else
				{
					parseRecord.PRobjectInfo.AddValue(pr.PRname, @object, ref parseRecord.PRsi, ref parseRecord.PRmemberData);
				}
			}
			else if (pr.PRmemberValueEnum == InternalMemberValueE.InlineValue)
			{
				if (pr.PRdtType == Converter.typeofString)
				{
					ParseString(pr, parseRecord);
					parseRecord.PRobjectInfo.AddValue(pr.PRname, pr.PRvalue, ref parseRecord.PRsi, ref parseRecord.PRmemberData);
				}
				else if (pr.PRdtTypeCode == InternalPrimitiveTypeE.Invalid)
				{
					if (pr.PRarrayTypeEnum == InternalArrayTypeE.Base64)
					{
						parseRecord.PRobjectInfo.AddValue(pr.PRname, Convert.FromBase64String(pr.PRvalue), ref parseRecord.PRsi, ref parseRecord.PRmemberData);
						return;
					}
					if (pr.PRdtType == Converter.typeofObject)
					{
						throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_TypeMissing"), pr.PRname));
					}
					ParseString(pr, parseRecord);
					if (pr.PRdtType == Converter.typeofSystemVoid)
					{
						parseRecord.PRobjectInfo.AddValue(pr.PRname, pr.PRdtType, ref parseRecord.PRsi, ref parseRecord.PRmemberData);
					}
					else if (parseRecord.PRobjectInfo.isSi)
					{
						parseRecord.PRobjectInfo.AddValue(pr.PRname, pr.PRvalue, ref parseRecord.PRsi, ref parseRecord.PRmemberData);
					}
				}
				else
				{
					object obj = null;
					obj = ((pr.PRvarValue == null) ? Converter.FromString(pr.PRvalue, pr.PRdtTypeCode) : pr.PRvarValue);
					parseRecord.PRobjectInfo.AddValue(pr.PRname, obj, ref parseRecord.PRsi, ref parseRecord.PRmemberData);
				}
			}
			else
			{
				ParseError(pr, parseRecord);
			}
		}

		private void ParseMemberEnd(ParseRecord pr)
		{
			switch (pr.PRmemberTypeEnum)
			{
			case InternalMemberTypeE.Item:
				ParseArrayMemberEnd(pr);
				break;
			case InternalMemberTypeE.Field:
				if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
				{
					ParseObjectEnd(pr);
				}
				break;
			default:
				ParseError(pr, (ParseRecord)stack.Peek());
				break;
			}
		}

		private void ParseString(ParseRecord pr, ParseRecord parentPr)
		{
			if (!pr.PRisRegistered && pr.PRobjectId > 0)
			{
				RegisterObject(pr.PRvalue, pr, parentPr, bIsString: true);
			}
		}

		private void RegisterObject(object obj, ParseRecord pr, ParseRecord objectPr)
		{
			RegisterObject(obj, pr, objectPr, bIsString: false);
		}

		private void RegisterObject(object obj, ParseRecord pr, ParseRecord objectPr, bool bIsString)
		{
			if (pr.PRisRegistered)
			{
				return;
			}
			pr.PRisRegistered = true;
			SerializationInfo serializationInfo = null;
			long idOfContainingObj = 0L;
			MemberInfo member = null;
			int[] arrayIndex = null;
			if (objectPr != null)
			{
				arrayIndex = objectPr.PRindexMap;
				idOfContainingObj = objectPr.PRobjectId;
				if (objectPr.PRobjectInfo != null && !objectPr.PRobjectInfo.isSi)
				{
					member = objectPr.PRobjectInfo.GetMemberInfo(pr.PRname);
				}
			}
			serializationInfo = pr.PRsi;
			if (bIsString)
			{
				m_objectManager.RegisterString((string)obj, pr.PRobjectId, serializationInfo, idOfContainingObj, member);
			}
			else
			{
				m_objectManager.RegisterObject(obj, pr.PRobjectId, serializationInfo, idOfContainingObj, member, arrayIndex);
			}
		}

		internal long GetId(long objectId)
		{
			if (!bFullDeserialization)
			{
				InitFullDeserialization();
			}
			if (objectId > 0)
			{
				return objectId;
			}
			if (bOldFormatDetected || objectId == -1)
			{
				bOldFormatDetected = true;
				if (valTypeObjectIdTable == null)
				{
					valTypeObjectIdTable = new IntSizedArray();
				}
				long num = 0L;
				if ((num = valTypeObjectIdTable[(int)objectId]) == 0)
				{
					num = int.MaxValue + objectId;
					valTypeObjectIdTable[(int)objectId] = (int)num;
				}
				return num;
			}
			return -1 * objectId;
		}

		[Conditional("SER_LOGGING")]
		private void IndexTraceMessage(string message, int[] index)
		{
			StringBuilder stringBuilder = new StringBuilder(10);
			stringBuilder.Append("[");
			for (int i = 0; i < index.Length; i++)
			{
				stringBuilder.Append(index[i]);
				if (i != index.Length - 1)
				{
					stringBuilder.Append(",");
				}
			}
			stringBuilder.Append("]");
		}

		internal Type Bind(string assemblyString, string typeString)
		{
			Type type = null;
			if (m_binder != null)
			{
				type = m_binder.BindToType(assemblyString, typeString);
			}
			if (type == null)
			{
				type = FastBindToType(assemblyString, typeString);
			}
			return type;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal Type FastBindToType(string assemblyName, string typeName)
		{
			Type type = null;
			TypeNAssembly typeNAssembly = (TypeNAssembly)typeCache.GetCachedValue(typeName);
			if (typeNAssembly == null || typeNAssembly.assemblyName != assemblyName)
			{
				Assembly assembly = null;
				if (bSimpleAssembly)
				{
					try
					{
						sfileIOPermission.Assert();
						try
						{
							StackCrawlMark stackMark = StackCrawlMark.LookForMe;
							assembly = Assembly.LoadWithPartialNameInternal(assemblyName, null, ref stackMark);
							if (assembly == null && assemblyName != null)
							{
								assembly = Assembly.LoadWithPartialNameInternal(new AssemblyName(assemblyName).Name, null, ref stackMark);
							}
						}
						finally
						{
							CodeAccessPermission.RevertAssert();
						}
					}
					catch (Exception)
					{
					}
				}
				else
				{
					try
					{
						sfileIOPermission.Assert();
						try
						{
							assembly = Assembly.Load(assemblyName);
						}
						finally
						{
							CodeAccessPermission.RevertAssert();
						}
					}
					catch (Exception)
					{
					}
				}
				if (assembly == null)
				{
					return null;
				}
				try
				{
					sfileIOPermission.Assert();
					try
					{
						type = FormatterServices.GetTypeFromAssembly(assembly, typeName);
					}
					finally
					{
						CodeAccessPermission.RevertAssert();
					}
				}
				catch (Exception)
				{
				}
				if (type == null && bSimpleAssembly)
				{
					try
					{
						sfileIOPermission.Assert();
						try
						{
							type = TypeName.GetType(assembly, typeName);
						}
						finally
						{
							CodeAccessPermission.RevertAssert();
						}
					}
					catch (Exception)
					{
					}
				}
				if (type == null)
				{
					return null;
				}
				CheckTypeForwardedTo(assembly, type.Assembly);
				typeNAssembly = new TypeNAssembly();
				typeNAssembly.type = type;
				typeNAssembly.assemblyName = assemblyName;
				typeCache.SetCachedValue(typeNAssembly);
			}
			return typeNAssembly.type;
		}

		internal Type GetType(BinaryAssemblyInfo assemblyInfo, string name)
		{
			Type type = null;
			if (previousName != null && previousName.Length == name.Length && previousName.Equals(name) && previousAssemblyString != null && previousAssemblyString.Length == assemblyInfo.assemblyString.Length && previousAssemblyString.Equals(assemblyInfo.assemblyString))
			{
				type = previousType;
			}
			else
			{
				type = Bind(assemblyInfo.assemblyString, name);
				if (type == null)
				{
					Assembly assembly = assemblyInfo.GetAssembly();
					type = FormatterServices.GetTypeFromAssembly(assembly, name);
					if (type != null)
					{
						CheckTypeForwardedTo(assembly, type.Assembly);
					}
				}
				previousAssemblyString = assemblyInfo.assemblyString;
				previousName = name;
				previousType = type;
			}
			return type;
		}

		[SecuritySafeCritical]
		private static void CheckTypeForwardedTo(Assembly sourceAssembly, Assembly destAssembly)
		{
			if (!FormatterServices.UnsafeTypeForwardersIsEnabled() && sourceAssembly != destAssembly)
			{
				PermissionSet permissionSet = sourceAssembly.GetPermissionSet();
				if (!destAssembly.GetPermissionSet().IsSubsetOf(permissionSet))
				{
					SecurityException ex = new SecurityException();
					ex.Demanded = permissionSet;
					throw ex;
				}
			}
		}
	}
}
