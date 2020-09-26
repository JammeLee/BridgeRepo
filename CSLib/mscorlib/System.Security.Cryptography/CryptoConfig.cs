using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Security.Util;
using System.Threading;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public class CryptoConfig
	{
		private static Hashtable defaultOidHT = null;

		private static Hashtable defaultNameHT = null;

		private static string machineConfigDir = Config.MachineDirectory;

		private static Hashtable machineOidHT = null;

		private static Hashtable machineNameHT = null;

		private static string machineConfigFilename = "machine.config";

		private static bool isInitialized = false;

		private static string _Version = null;

		private static object s_InternalSyncObject;

		private static object InternalSyncObject
		{
			get
			{
				if (s_InternalSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref s_InternalSyncObject, value, null);
				}
				return s_InternalSyncObject;
			}
		}

		private static Hashtable DefaultOidHT
		{
			get
			{
				if (defaultOidHT == null)
				{
					Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
					hashtable.Add("SHA", "1.3.14.3.2.26");
					hashtable.Add("SHA1", "1.3.14.3.2.26");
					hashtable.Add("System.Security.Cryptography.SHA1", "1.3.14.3.2.26");
					hashtable.Add("System.Security.Cryptography.SHA1CryptoServiceProvider", "1.3.14.3.2.26");
					hashtable.Add("System.Security.Cryptography.SHA1Managed", "1.3.14.3.2.26");
					hashtable.Add("SHA256", "2.16.840.1.101.3.4.2.1");
					hashtable.Add("System.Security.Cryptography.SHA256", "2.16.840.1.101.3.4.2.1");
					hashtable.Add("System.Security.Cryptography.SHA256Managed", "2.16.840.1.101.3.4.2.1");
					hashtable.Add("SHA384", "2.16.840.1.101.3.4.2.2");
					hashtable.Add("System.Security.Cryptography.SHA384", "2.16.840.1.101.3.4.2.2");
					hashtable.Add("System.Security.Cryptography.SHA384Managed", "2.16.840.1.101.3.4.2.2");
					hashtable.Add("SHA512", "2.16.840.1.101.3.4.2.3");
					hashtable.Add("System.Security.Cryptography.SHA512", "2.16.840.1.101.3.4.2.3");
					hashtable.Add("System.Security.Cryptography.SHA512Managed", "2.16.840.1.101.3.4.2.3");
					hashtable.Add("RIPEMD160", "1.3.36.3.2.1");
					hashtable.Add("System.Security.Cryptography.RIPEMD160", "1.3.36.3.2.1");
					hashtable.Add("System.Security.Cryptography.RIPEMD160Managed", "1.3.36.3.2.1");
					hashtable.Add("MD5", "1.2.840.113549.2.5");
					hashtable.Add("System.Security.Cryptography.MD5", "1.2.840.113549.2.5");
					hashtable.Add("System.Security.Cryptography.MD5CryptoServiceProvider", "1.2.840.113549.2.5");
					hashtable.Add("System.Security.Cryptography.MD5Managed", "1.2.840.113549.2.5");
					hashtable.Add("TripleDESKeyWrap", "1.2.840.113549.1.9.16.3.6");
					hashtable.Add("RC2", "1.2.840.113549.3.2");
					hashtable.Add("System.Security.Cryptography.RC2CryptoServiceProvider", "1.2.840.113549.3.2");
					hashtable.Add("DES", "1.3.14.3.2.7");
					hashtable.Add("System.Security.Cryptography.DESCryptoServiceProvider", "1.3.14.3.2.7");
					hashtable.Add("TripleDES", "1.2.840.113549.3.7");
					hashtable.Add("System.Security.Cryptography.TripleDESCryptoServiceProvider", "1.2.840.113549.3.7");
					defaultOidHT = hashtable;
				}
				return defaultOidHT;
			}
		}

		private static Hashtable DefaultNameHT
		{
			get
			{
				if (defaultNameHT == null)
				{
					Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
					Type typeFromHandle = typeof(SHA1CryptoServiceProvider);
					Type typeFromHandle2 = typeof(MD5CryptoServiceProvider);
					Type typeFromHandle3 = typeof(SHA256Managed);
					Type typeFromHandle4 = typeof(SHA384Managed);
					Type typeFromHandle5 = typeof(SHA512Managed);
					Type typeFromHandle6 = typeof(RIPEMD160Managed);
					Type typeFromHandle7 = typeof(HMACMD5);
					Type typeFromHandle8 = typeof(HMACRIPEMD160);
					Type typeFromHandle9 = typeof(HMACSHA1);
					Type typeFromHandle10 = typeof(HMACSHA256);
					Type typeFromHandle11 = typeof(HMACSHA384);
					Type typeFromHandle12 = typeof(HMACSHA512);
					Type typeFromHandle13 = typeof(MACTripleDES);
					Type typeFromHandle14 = typeof(RSACryptoServiceProvider);
					Type typeFromHandle15 = typeof(DSACryptoServiceProvider);
					Type typeFromHandle16 = typeof(DESCryptoServiceProvider);
					Type typeFromHandle17 = typeof(TripleDESCryptoServiceProvider);
					Type typeFromHandle18 = typeof(RC2CryptoServiceProvider);
					Type typeFromHandle19 = typeof(RijndaelManaged);
					Type typeFromHandle20 = typeof(DSASignatureDescription);
					Type typeFromHandle21 = typeof(RSAPKCS1SHA1SignatureDescription);
					Type typeFromHandle22 = typeof(RNGCryptoServiceProvider);
					hashtable.Add("RandomNumberGenerator", typeFromHandle22);
					hashtable.Add("System.Security.Cryptography.RandomNumberGenerator", typeFromHandle22);
					hashtable.Add("SHA", typeFromHandle);
					hashtable.Add("SHA1", typeFromHandle);
					hashtable.Add("System.Security.Cryptography.SHA1", typeFromHandle);
					hashtable.Add("System.Security.Cryptography.HashAlgorithm", typeFromHandle);
					hashtable.Add("MD5", typeFromHandle2);
					hashtable.Add("System.Security.Cryptography.MD5", typeFromHandle2);
					hashtable.Add("SHA256", typeFromHandle3);
					hashtable.Add("SHA-256", typeFromHandle3);
					hashtable.Add("System.Security.Cryptography.SHA256", typeFromHandle3);
					hashtable.Add("SHA384", typeFromHandle4);
					hashtable.Add("SHA-384", typeFromHandle4);
					hashtable.Add("System.Security.Cryptography.SHA384", typeFromHandle4);
					hashtable.Add("SHA512", typeFromHandle5);
					hashtable.Add("SHA-512", typeFromHandle5);
					hashtable.Add("System.Security.Cryptography.SHA512", typeFromHandle5);
					hashtable.Add("RIPEMD160", typeFromHandle6);
					hashtable.Add("RIPEMD-160", typeFromHandle6);
					hashtable.Add("System.Security.Cryptography.RIPEMD160", typeFromHandle6);
					hashtable.Add("System.Security.Cryptography.RIPEMD160Managed", typeFromHandle6);
					hashtable.Add("System.Security.Cryptography.HMAC", typeFromHandle9);
					hashtable.Add("System.Security.Cryptography.KeyedHashAlgorithm", typeFromHandle9);
					hashtable.Add("HMACMD5", typeFromHandle7);
					hashtable.Add("System.Security.Cryptography.HMACMD5", typeFromHandle7);
					hashtable.Add("HMACRIPEMD160", typeFromHandle8);
					hashtable.Add("System.Security.Cryptography.HMACRIPEMD160", typeFromHandle8);
					hashtable.Add("HMACSHA1", typeFromHandle9);
					hashtable.Add("System.Security.Cryptography.HMACSHA1", typeFromHandle9);
					hashtable.Add("HMACSHA256", typeFromHandle10);
					hashtable.Add("System.Security.Cryptography.HMACSHA256", typeFromHandle10);
					hashtable.Add("HMACSHA384", typeFromHandle11);
					hashtable.Add("System.Security.Cryptography.HMACSHA384", typeFromHandle11);
					hashtable.Add("HMACSHA512", typeFromHandle12);
					hashtable.Add("System.Security.Cryptography.HMACSHA512", typeFromHandle12);
					hashtable.Add("MACTripleDES", typeFromHandle13);
					hashtable.Add("System.Security.Cryptography.MACTripleDES", typeFromHandle13);
					hashtable.Add("RSA", typeFromHandle14);
					hashtable.Add("System.Security.Cryptography.RSA", typeFromHandle14);
					hashtable.Add("System.Security.Cryptography.AsymmetricAlgorithm", typeFromHandle14);
					hashtable.Add("DSA", typeFromHandle15);
					hashtable.Add("System.Security.Cryptography.DSA", typeFromHandle15);
					hashtable.Add("DES", typeFromHandle16);
					hashtable.Add("System.Security.Cryptography.DES", typeFromHandle16);
					hashtable.Add("3DES", typeFromHandle17);
					hashtable.Add("TripleDES", typeFromHandle17);
					hashtable.Add("Triple DES", typeFromHandle17);
					hashtable.Add("System.Security.Cryptography.TripleDES", typeFromHandle17);
					hashtable.Add("RC2", typeFromHandle18);
					hashtable.Add("System.Security.Cryptography.RC2", typeFromHandle18);
					hashtable.Add("Rijndael", typeFromHandle19);
					hashtable.Add("System.Security.Cryptography.Rijndael", typeFromHandle19);
					hashtable.Add("System.Security.Cryptography.SymmetricAlgorithm", typeFromHandle19);
					hashtable.Add("http://www.w3.org/2000/09/xmldsig#dsa-sha1", typeFromHandle20);
					hashtable.Add("System.Security.Cryptography.DSASignatureDescription", typeFromHandle20);
					hashtable.Add("http://www.w3.org/2000/09/xmldsig#rsa-sha1", typeFromHandle21);
					hashtable.Add("System.Security.Cryptography.RSASignatureDescription", typeFromHandle21);
					hashtable.Add("http://www.w3.org/2000/09/xmldsig#sha1", typeFromHandle);
					hashtable.Add("http://www.w3.org/2001/04/xmlenc#sha256", typeFromHandle3);
					hashtable.Add("http://www.w3.org/2001/04/xmlenc#sha512", typeFromHandle5);
					hashtable.Add("http://www.w3.org/2001/04/xmlenc#ripemd160", typeFromHandle6);
					hashtable.Add("http://www.w3.org/2001/04/xmlenc#des-cbc", typeFromHandle16);
					hashtable.Add("http://www.w3.org/2001/04/xmlenc#tripledes-cbc", typeFromHandle17);
					hashtable.Add("http://www.w3.org/2001/04/xmlenc#kw-tripledes", typeFromHandle17);
					hashtable.Add("http://www.w3.org/2001/04/xmlenc#aes128-cbc", typeFromHandle19);
					hashtable.Add("http://www.w3.org/2001/04/xmlenc#kw-aes128", typeFromHandle19);
					hashtable.Add("http://www.w3.org/2001/04/xmlenc#aes192-cbc", typeFromHandle19);
					hashtable.Add("http://www.w3.org/2001/04/xmlenc#kw-aes192", typeFromHandle19);
					hashtable.Add("http://www.w3.org/2001/04/xmlenc#aes256-cbc", typeFromHandle19);
					hashtable.Add("http://www.w3.org/2001/04/xmlenc#kw-aes256", typeFromHandle19);
					hashtable.Add("http://www.w3.org/TR/2001/REC-xml-c14n-20010315", "System.Security.Cryptography.Xml.XmlDsigC14NTransform, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments", "System.Security.Cryptography.Xml.XmlDsigC14NWithCommentsTransform, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/2001/10/xml-exc-c14n#", "System.Security.Cryptography.Xml.XmlDsigExcC14NTransform, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/2001/10/xml-exc-c14n#WithComments", "System.Security.Cryptography.Xml.XmlDsigExcC14NWithCommentsTransform, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/2000/09/xmldsig#base64", "System.Security.Cryptography.Xml.XmlDsigBase64Transform, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/TR/1999/REC-xpath-19991116", "System.Security.Cryptography.Xml.XmlDsigXPathTransform, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/TR/1999/REC-xslt-19991116", "System.Security.Cryptography.Xml.XmlDsigXsltTransform, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/2000/09/xmldsig#enveloped-signature", "System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/2002/07/decrypt#XML", "System.Security.Cryptography.Xml.XmlDecryptionTransform, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("urn:mpeg:mpeg21:2003:01-REL-R-NS:licenseTransform", "System.Security.Cryptography.Xml.XmlLicenseTransform, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/2000/09/xmldsig# X509Data", "System.Security.Cryptography.Xml.KeyInfoX509Data, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/2000/09/xmldsig# KeyName", "System.Security.Cryptography.Xml.KeyInfoName, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/2000/09/xmldsig# KeyValue/DSAKeyValue", "System.Security.Cryptography.Xml.DSAKeyValue, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/2000/09/xmldsig# KeyValue/RSAKeyValue", "System.Security.Cryptography.Xml.RSAKeyValue, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/2000/09/xmldsig# RetrievalMethod", "System.Security.Cryptography.Xml.KeyInfoRetrievalMethod, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/2001/04/xmlenc# EncryptedKey", "System.Security.Cryptography.Xml.KeyInfoEncryptedKey, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("http://www.w3.org/2001/04/xmldsig-more#md5", typeFromHandle2);
					hashtable.Add("http://www.w3.org/2001/04/xmldsig-more#sha384", typeFromHandle4);
					hashtable.Add("http://www.w3.org/2001/04/xmldsig-more#hmac-ripemd160", typeFromHandle8);
					hashtable.Add("http://www.w3.org/2001/04/xmldsig-more#hmac-sha256", typeFromHandle10);
					hashtable.Add("http://www.w3.org/2001/04/xmldsig-more#hmac-sha384", typeFromHandle11);
					hashtable.Add("http://www.w3.org/2001/04/xmldsig-more#hmac-sha512", typeFromHandle12);
					hashtable.Add("2.5.29.10", "System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension, System, Culture=neutral, PublicKeyToken=b77a5c561934e089, Version=" + _Version);
					hashtable.Add("2.5.29.19", "System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension, System, Culture=neutral, PublicKeyToken=b77a5c561934e089, Version=" + _Version);
					hashtable.Add("2.5.29.14", "System.Security.Cryptography.X509Certificates.X509SubjectKeyIdentifierExtension, System, Culture=neutral, PublicKeyToken=b77a5c561934e089, Version=" + _Version);
					hashtable.Add("2.5.29.15", "System.Security.Cryptography.X509Certificates.X509KeyUsageExtension, System, Culture=neutral, PublicKeyToken=b77a5c561934e089, Version=" + _Version);
					hashtable.Add("2.5.29.37", "System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension, System, Culture=neutral, PublicKeyToken=b77a5c561934e089, Version=" + _Version);
					hashtable.Add("X509Chain", "System.Security.Cryptography.X509Certificates.X509Chain, System, Culture=neutral, PublicKeyToken=b77a5c561934e089, Version=" + _Version);
					hashtable.Add("1.2.840.113549.1.9.3", "System.Security.Cryptography.Pkcs.Pkcs9ContentType, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("1.2.840.113549.1.9.4", "System.Security.Cryptography.Pkcs.Pkcs9MessageDigest, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("1.2.840.113549.1.9.5", "System.Security.Cryptography.Pkcs.Pkcs9SigningTime, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("1.3.6.1.4.1.311.88.2.1", "System.Security.Cryptography.Pkcs.Pkcs9DocumentName, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					hashtable.Add("1.3.6.1.4.1.311.88.2.2", "System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription, System.Security, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, Version=" + _Version);
					defaultNameHT = hashtable;
				}
				return defaultNameHT;
			}
		}

		private static void InitializeConfigInfo()
		{
			Type typeFromHandle = typeof(CryptoConfig);
			_Version = typeFromHandle.Assembly.GetVersion().ToString();
			if (machineNameHT == null && machineOidHT == null)
			{
				lock (InternalSyncObject)
				{
					string text = machineConfigDir + machineConfigFilename;
					new FileIOPermission(FileIOPermissionAccess.Read, text).Assert();
					if (File.Exists(text))
					{
						ConfigTreeParser configTreeParser = new ConfigTreeParser();
						ConfigNode configNode = configTreeParser.Parse(text, "configuration");
						if (configNode != null)
						{
							ArrayList children = configNode.Children;
							ConfigNode configNode2 = null;
							foreach (ConfigNode item in children)
							{
								if (!item.Name.Equals("mscorlib"))
								{
									continue;
								}
								ArrayList attributes = item.Attributes;
								if (attributes.Count > 0)
								{
									DictionaryEntry dictionaryEntry = (DictionaryEntry)item.Attributes[0];
									if (dictionaryEntry.Key.Equals("version") && dictionaryEntry.Value.Equals(_Version))
									{
										configNode2 = item;
										break;
									}
								}
								else
								{
									configNode2 = item;
								}
							}
							if (configNode2 != null)
							{
								ArrayList children2 = configNode2.Children;
								ConfigNode configNode4 = null;
								foreach (ConfigNode item2 in children2)
								{
									if (item2.Name.Equals("cryptographySettings"))
									{
										configNode4 = item2;
										break;
									}
								}
								if (configNode4 != null)
								{
									ConfigNode configNode6 = null;
									foreach (ConfigNode child in configNode4.Children)
									{
										if (child.Name.Equals("cryptoNameMapping"))
										{
											configNode6 = child;
											break;
										}
									}
									if (configNode6 != null)
									{
										ArrayList children3 = configNode6.Children;
										ConfigNode configNode8 = null;
										foreach (ConfigNode item3 in children3)
										{
											if (item3.Name.Equals("cryptoClasses"))
											{
												configNode8 = item3;
												break;
											}
										}
										if (configNode8 != null)
										{
											Hashtable hashtable = new Hashtable();
											Hashtable hashtable2 = new Hashtable();
											foreach (ConfigNode child2 in configNode8.Children)
											{
												if (child2.Name.Equals("cryptoClass") && child2.Attributes.Count > 0)
												{
													DictionaryEntry dictionaryEntry2 = (DictionaryEntry)child2.Attributes[0];
													hashtable.Add(dictionaryEntry2.Key, dictionaryEntry2.Value);
												}
											}
											foreach (ConfigNode item4 in children3)
											{
												if (!item4.Name.Equals("nameEntry"))
												{
													continue;
												}
												string text2 = null;
												string text3 = null;
												foreach (DictionaryEntry attribute in item4.Attributes)
												{
													if (((string)attribute.Key).Equals("name"))
													{
														text2 = (string)attribute.Value;
													}
													else if (((string)attribute.Key).Equals("class"))
													{
														text3 = (string)attribute.Value;
													}
												}
												if (text2 != null && text3 != null)
												{
													string text4 = (string)hashtable[text3];
													if (text4 != null)
													{
														hashtable2.Add(text2, text4);
													}
												}
											}
											machineNameHT = hashtable2;
										}
									}
									ConfigNode configNode12 = null;
									foreach (ConfigNode child3 in configNode4.Children)
									{
										if (child3.Name.Equals("oidMap"))
										{
											configNode12 = child3;
											break;
										}
									}
									if (configNode12 != null)
									{
										Hashtable hashtable3 = new Hashtable();
										foreach (ConfigNode child4 in configNode12.Children)
										{
											if (!child4.Name.Equals("oidEntry"))
											{
												continue;
											}
											string text5 = null;
											string text6 = null;
											foreach (DictionaryEntry attribute2 in child4.Attributes)
											{
												if (((string)attribute2.Key).Equals("OID"))
												{
													text5 = (string)attribute2.Value;
												}
												else if (((string)attribute2.Key).Equals("name"))
												{
													text6 = (string)attribute2.Value;
												}
											}
											if (text6 != null && text5 != null)
											{
												hashtable3.Add(text6, text5);
											}
										}
										machineOidHT = hashtable3;
									}
								}
							}
						}
					}
				}
			}
			isInitialized = true;
		}

		public static object CreateFromName(string name, params object[] args)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			Type type = null;
			if (!isInitialized)
			{
				InitializeConfigInfo();
			}
			if (machineNameHT != null)
			{
				string text = (string)machineNameHT[name];
				if (text != null)
				{
					type = Type.GetType(text, throwOnError: false, ignoreCase: false);
					if (type != null && !type.IsVisible)
					{
						type = null;
					}
				}
			}
			if (type == null)
			{
				object obj = DefaultNameHT[name];
				if (obj != null)
				{
					if (obj is Type)
					{
						type = (Type)obj;
					}
					else if (obj is string)
					{
						type = Type.GetType((string)obj, throwOnError: false, ignoreCase: false);
						if (type != null && !type.IsVisible)
						{
							type = null;
						}
					}
				}
			}
			if (type == null)
			{
				type = Type.GetType(name, throwOnError: false, ignoreCase: false);
				if (type != null && !type.IsVisible)
				{
					type = null;
				}
			}
			if (type == null)
			{
				return null;
			}
			RuntimeType runtimeType = type as RuntimeType;
			if (runtimeType == null)
			{
				return null;
			}
			if (args == null)
			{
				args = new object[0];
			}
			MethodBase[] constructors = runtimeType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance);
			if (constructors == null)
			{
				return null;
			}
			ArrayList arrayList = new ArrayList();
			foreach (MethodBase methodBase in constructors)
			{
				if (methodBase.GetParameters().Length == args.Length)
				{
					arrayList.Add(methodBase);
				}
			}
			if (arrayList.Count == 0)
			{
				return null;
			}
			constructors = arrayList.ToArray(typeof(MethodBase)) as MethodBase[];
			object state;
			RuntimeConstructorInfo runtimeConstructorInfo = Type.DefaultBinder.BindToMethod(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, constructors, ref args, null, null, null, out state) as RuntimeConstructorInfo;
			if (runtimeConstructorInfo == null || typeof(Delegate).IsAssignableFrom(runtimeConstructorInfo.DeclaringType))
			{
				return null;
			}
			object result = runtimeConstructorInfo.Invoke(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, Type.DefaultBinder, args, null);
			if (state != null)
			{
				Type.DefaultBinder.ReorderArgumentArray(ref args, state);
			}
			return result;
		}

		public static object CreateFromName(string name)
		{
			return CreateFromName(name, null);
		}

		public static string MapNameToOID(string name)
		{
			return MapNameToOID(name, OidGroup.AllGroups);
		}

		internal static string MapNameToOID(string name, OidGroup group)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (!isInitialized)
			{
				InitializeConfigInfo();
			}
			string text = null;
			if (machineOidHT != null)
			{
				text = machineOidHT[name] as string;
			}
			if (text == null)
			{
				text = DefaultOidHT[name] as string;
			}
			if (text == null)
			{
				text = X509Utils._GetOidFromFriendlyName(name, group);
			}
			return text;
		}

		public static byte[] EncodeOID(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			char[] separator = new char[1]
			{
				'.'
			};
			string[] array = str.Split(separator);
			uint[] array2 = new uint[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = (uint)int.Parse(array[i], CultureInfo.InvariantCulture);
			}
			byte[] array3 = new byte[array2.Length * 5];
			int num = 0;
			if (array2.Length < 2)
			{
				throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_InvalidOID"));
			}
			uint dwValue = array2[0] * 40 + array2[1];
			byte[] array4 = EncodeSingleOIDNum(dwValue);
			Array.Copy(array4, 0, array3, num, array4.Length);
			num += array4.Length;
			for (int j = 2; j < array2.Length; j++)
			{
				array4 = EncodeSingleOIDNum(array2[j]);
				Buffer.InternalBlockCopy(array4, 0, array3, num, array4.Length);
				num += array4.Length;
			}
			if (num > 127)
			{
				throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_Config_EncodedOIDError"));
			}
			array4 = new byte[num + 2];
			array4[0] = 6;
			array4[1] = (byte)num;
			Buffer.InternalBlockCopy(array3, 0, array4, 2, num);
			return array4;
		}

		private static byte[] EncodeSingleOIDNum(uint dwValue)
		{
			if ((int)dwValue < 128)
			{
				return new byte[1]
				{
					(byte)dwValue
				};
			}
			if (dwValue < 16384)
			{
				return new byte[2]
				{
					(byte)((dwValue >> 7) | 0x80u),
					(byte)(dwValue & 0x7Fu)
				};
			}
			if (dwValue < 2097152)
			{
				return new byte[3]
				{
					(byte)((dwValue >> 14) | 0x80u),
					(byte)((dwValue >> 7) | 0x80u),
					(byte)(dwValue & 0x7Fu)
				};
			}
			if (dwValue < 268435456)
			{
				return new byte[4]
				{
					(byte)((dwValue >> 21) | 0x80u),
					(byte)((dwValue >> 14) | 0x80u),
					(byte)((dwValue >> 7) | 0x80u),
					(byte)(dwValue & 0x7Fu)
				};
			}
			return new byte[5]
			{
				(byte)((dwValue >> 28) | 0x80u),
				(byte)((dwValue >> 21) | 0x80u),
				(byte)((dwValue >> 14) | 0x80u),
				(byte)((dwValue >> 7) | 0x80u),
				(byte)(dwValue & 0x7Fu)
			};
		}
	}
}
