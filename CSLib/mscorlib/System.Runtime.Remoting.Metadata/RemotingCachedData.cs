using System.Reflection;

namespace System.Runtime.Remoting.Metadata
{
	internal class RemotingCachedData
	{
		protected object RI;

		private SoapAttribute _soapAttr;

		internal RemotingCachedData(object ri)
		{
			RI = ri;
		}

		internal SoapAttribute GetSoapAttribute()
		{
			if (_soapAttr == null)
			{
				lock (this)
				{
					if (_soapAttr == null)
					{
						SoapAttribute soapAttribute = null;
						ICustomAttributeProvider customAttributeProvider = (ICustomAttributeProvider)RI;
						if (RI is Type)
						{
							object[] customAttributes = customAttributeProvider.GetCustomAttributes(typeof(SoapTypeAttribute), inherit: true);
							soapAttribute = ((customAttributes == null || customAttributes.Length == 0) ? new SoapTypeAttribute() : ((SoapAttribute)customAttributes[0]));
						}
						else if (RI is MethodBase)
						{
							object[] customAttributes2 = customAttributeProvider.GetCustomAttributes(typeof(SoapMethodAttribute), inherit: true);
							soapAttribute = ((customAttributes2 == null || customAttributes2.Length == 0) ? new SoapMethodAttribute() : ((SoapAttribute)customAttributes2[0]));
						}
						else if (RI is FieldInfo)
						{
							object[] customAttributes3 = customAttributeProvider.GetCustomAttributes(typeof(SoapFieldAttribute), inherit: false);
							soapAttribute = ((customAttributes3 == null || customAttributes3.Length == 0) ? new SoapFieldAttribute() : ((SoapAttribute)customAttributes3[0]));
						}
						else if (RI is ParameterInfo)
						{
							object[] customAttributes4 = customAttributeProvider.GetCustomAttributes(typeof(SoapParameterAttribute), inherit: true);
							soapAttribute = ((customAttributes4 == null || customAttributes4.Length == 0) ? new SoapParameterAttribute() : ((SoapParameterAttribute)customAttributes4[0]));
						}
						soapAttribute.SetReflectInfo(RI);
						_soapAttr = soapAttribute;
					}
				}
			}
			return _soapAttr;
		}
	}
}
