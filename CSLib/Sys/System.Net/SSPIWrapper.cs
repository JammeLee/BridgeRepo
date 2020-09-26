using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Net
{
	internal static class SSPIWrapper
	{
		private enum OP
		{
			Encrypt = 1,
			Decrypt,
			MakeSignature,
			VerifySignature
		}

		internal static SecurityPackageInfoClass[] EnumerateSecurityPackages(SSPIInterface SecModule)
		{
			if (SecModule.SecurityPackages == null)
			{
				lock (SecModule)
				{
					if (SecModule.SecurityPackages == null)
					{
						int pkgnum = 0;
						SafeFreeContextBuffer pkgArray = null;
						try
						{
							int num = SecModule.EnumerateSecurityPackages(out pkgnum, out pkgArray);
							if (num != 0)
							{
								throw new Win32Exception(num);
							}
							SecurityPackageInfoClass[] array = new SecurityPackageInfoClass[pkgnum];
							if (Logging.On)
							{
								Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_enumerating_security_packages"));
							}
							for (int i = 0; i < pkgnum; i++)
							{
								array[i] = new SecurityPackageInfoClass(pkgArray, i);
								if (Logging.On)
								{
									Logging.PrintInfo(Logging.Web, "    " + array[i].Name);
								}
							}
							SecModule.SecurityPackages = array;
						}
						finally
						{
							pkgArray?.Close();
						}
					}
				}
			}
			return SecModule.SecurityPackages;
		}

		internal static SecurityPackageInfoClass GetVerifyPackageInfo(SSPIInterface secModule, string packageName)
		{
			return GetVerifyPackageInfo(secModule, packageName, throwIfMissing: false);
		}

		internal static SecurityPackageInfoClass GetVerifyPackageInfo(SSPIInterface secModule, string packageName, bool throwIfMissing)
		{
			SecurityPackageInfoClass[] array = EnumerateSecurityPackages(secModule);
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (string.Compare(array[i].Name, packageName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return array[i];
					}
				}
			}
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_security_package_not_found", packageName));
			}
			if (throwIfMissing)
			{
				throw new NotSupportedException(SR.GetString("net_securitypackagesupport"));
			}
			return null;
		}

		public static SafeFreeCredentials AcquireDefaultCredential(SSPIInterface SecModule, string package, CredentialUse intent)
		{
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, string.Concat("AcquireDefaultCredential(package = ", package, ", intent  = ", intent, ")"));
			}
			SafeFreeCredentials outCredential = null;
			int num = SecModule.AcquireDefaultCredential(package, intent, out outCredential);
			if (num != 0)
			{
				if (Logging.On)
				{
					Logging.PrintError(Logging.Web, SR.GetString("net_log_operation_failed_with_error", "AcquireDefaultCredential()", string.Format(CultureInfo.CurrentCulture, "0X{0:X}", num)));
				}
				throw new Win32Exception(num);
			}
			return outCredential;
		}

		public static SafeFreeCredentials AcquireCredentialsHandle(SSPIInterface SecModule, string package, CredentialUse intent, ref AuthIdentity authdata)
		{
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, string.Concat("AcquireCredentialsHandle(package  = ", package, ", intent   = ", intent, ", authdata = ", authdata, ")"));
			}
			SafeFreeCredentials outCredential = null;
			int num = SecModule.AcquireCredentialsHandle(package, intent, ref authdata, out outCredential);
			if (num != 0)
			{
				if (Logging.On)
				{
					Logging.PrintError(Logging.Web, SR.GetString("net_log_operation_failed_with_error", "AcquireCredentialsHandle()", string.Format(CultureInfo.CurrentCulture, "0X{0:X}", num)));
				}
				throw new Win32Exception(num);
			}
			return outCredential;
		}

		public static SafeFreeCredentials AcquireCredentialsHandle(SSPIInterface SecModule, string package, CredentialUse intent, SecureCredential scc)
		{
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, string.Concat("AcquireCredentialsHandle(package = ", package, ", intent  = ", intent, ", scc     = ", scc, ")"));
			}
			SafeFreeCredentials outCredential = null;
			int num = SecModule.AcquireCredentialsHandle(package, intent, ref scc, out outCredential);
			if (num != 0)
			{
				if (Logging.On)
				{
					Logging.PrintError(Logging.Web, SR.GetString("net_log_operation_failed_with_error", "AcquireCredentialsHandle()", string.Format(CultureInfo.CurrentCulture, "0X{0:X}", num)));
				}
				throw new Win32Exception(num);
			}
			return outCredential;
		}

		internal static int InitializeSecurityContext(SSPIInterface SecModule, ref SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, ContextFlags inFlags, Endianness datarep, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
		{
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, string.Concat("InitializeSecurityContext(credential = ", credential.ToString(), ", context = ", ValidationHelper.ToString(context), ", targetName = ", targetName, ", inFlags = ", inFlags, ")"));
			}
			int num = SecModule.InitializeSecurityContext(ref credential, ref context, targetName, inFlags, datarep, inputBuffer, outputBuffer, ref outFlags);
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_security_context_input_buffer", "InitializeSecurityContext", inputBuffer?.size ?? 0, outputBuffer.size, (SecurityStatus)num));
			}
			return num;
		}

		internal static int InitializeSecurityContext(SSPIInterface SecModule, SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, ContextFlags inFlags, Endianness datarep, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
		{
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, string.Concat("InitializeSecurityContext(credential = ", credential.ToString(), ", context = ", ValidationHelper.ToString(context), ", targetName = ", targetName, ", inFlags = ", inFlags, ")"));
			}
			int num = SecModule.InitializeSecurityContext(credential, ref context, targetName, inFlags, datarep, inputBuffers, outputBuffer, ref outFlags);
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_security_context_input_buffers", "InitializeSecurityContext", (inputBuffers != null) ? inputBuffers.Length : 0, outputBuffer.size, (SecurityStatus)num));
			}
			return num;
		}

		internal static int AcceptSecurityContext(SSPIInterface SecModule, ref SafeFreeCredentials credential, ref SafeDeleteContext context, ContextFlags inFlags, Endianness datarep, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
		{
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, string.Concat("AcceptSecurityContext(credential = ", credential.ToString(), ", context = ", ValidationHelper.ToString(context), ", inFlags = ", inFlags, ")"));
			}
			int num = SecModule.AcceptSecurityContext(ref credential, ref context, inputBuffer, inFlags, datarep, outputBuffer, ref outFlags);
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_security_context_input_buffer", "AcceptSecurityContext", inputBuffer?.size ?? 0, outputBuffer.size, (SecurityStatus)num));
			}
			return num;
		}

		internal static int AcceptSecurityContext(SSPIInterface SecModule, SafeFreeCredentials credential, ref SafeDeleteContext context, ContextFlags inFlags, Endianness datarep, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
		{
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, string.Concat("AcceptSecurityContext(credential = ", credential.ToString(), ", context = ", ValidationHelper.ToString(context), ", inFlags = ", inFlags, ")"));
			}
			int num = SecModule.AcceptSecurityContext(credential, ref context, inputBuffers, inFlags, datarep, outputBuffer, ref outFlags);
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_security_context_input_buffers", "AcceptSecurityContext", (inputBuffers != null) ? inputBuffers.Length : 0, outputBuffer.size, (SecurityStatus)num));
			}
			return num;
		}

		internal static int CompleteAuthToken(SSPIInterface SecModule, ref SafeDeleteContext context, SecurityBuffer[] inputBuffers)
		{
			int num = SecModule.CompleteAuthToken(ref context, inputBuffers);
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, SR.GetString("net_log_operation_returned_something", "CompleteAuthToken()", (SecurityStatus)num));
			}
			return num;
		}

		public static int QuerySecurityContextToken(SSPIInterface SecModule, SafeDeleteContext context, out SafeCloseHandle token)
		{
			return SecModule.QuerySecurityContextToken(context, out token);
		}

		public static int EncryptMessage(SSPIInterface secModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
		{
			return EncryptDecryptHelper(OP.Encrypt, secModule, context, input, sequenceNumber);
		}

		public static int DecryptMessage(SSPIInterface secModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
		{
			return EncryptDecryptHelper(OP.Decrypt, secModule, context, input, sequenceNumber);
		}

		internal static int MakeSignature(SSPIInterface secModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
		{
			return EncryptDecryptHelper(OP.MakeSignature, secModule, context, input, sequenceNumber);
		}

		public static int VerifySignature(SSPIInterface secModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
		{
			return EncryptDecryptHelper(OP.VerifySignature, secModule, context, input, sequenceNumber);
		}

		private unsafe static int EncryptDecryptHelper(OP op, SSPIInterface SecModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
		{
			SecurityBufferDescriptor securityBufferDescriptor = new SecurityBufferDescriptor(input.Length);
			SecurityBufferStruct[] array = new SecurityBufferStruct[input.Length];
			fixed (SecurityBufferStruct* unmanagedPointer = array)
			{
				securityBufferDescriptor.UnmanagedPointer = unmanagedPointer;
				GCHandle[] array2 = new GCHandle[input.Length];
				byte[][] array3 = new byte[input.Length][];
				try
				{
					for (int i = 0; i < input.Length; i++)
					{
						SecurityBuffer securityBuffer = input[i];
						array[i].count = securityBuffer.size;
						array[i].type = securityBuffer.type;
						if (securityBuffer.token == null || securityBuffer.token.Length == 0)
						{
							array[i].token = IntPtr.Zero;
							continue;
						}
						ref GCHandle reference = ref array2[i];
						reference = GCHandle.Alloc(securityBuffer.token, GCHandleType.Pinned);
						array[i].token = Marshal.UnsafeAddrOfPinnedArrayElement(securityBuffer.token, securityBuffer.offset);
						array3[i] = securityBuffer.token;
					}
					int num = op switch
					{
						OP.Encrypt => SecModule.EncryptMessage(context, securityBufferDescriptor, sequenceNumber), 
						OP.Decrypt => SecModule.DecryptMessage(context, securityBufferDescriptor, sequenceNumber), 
						OP.MakeSignature => SecModule.MakeSignature(context, securityBufferDescriptor, sequenceNumber), 
						OP.VerifySignature => SecModule.VerifySignature(context, securityBufferDescriptor, sequenceNumber), 
						_ => throw ExceptionHelper.MethodNotImplementedException, 
					};
					for (int j = 0; j < input.Length; j++)
					{
						SecurityBuffer securityBuffer2 = input[j];
						securityBuffer2.size = array[j].count;
						securityBuffer2.type = array[j].type;
						if (securityBuffer2.size == 0)
						{
							securityBuffer2.offset = 0;
							securityBuffer2.token = null;
							continue;
						}
						checked
						{
							int k;
							for (k = 0; k < input.Length; k++)
							{
								if (array3[k] != null)
								{
									byte* ptr = unchecked((byte*)(void*)Marshal.UnsafeAddrOfPinnedArrayElement(array3[k], 0));
									if ((void*)array[j].token >= ptr && unchecked((nuint)(void*)array[j].token) + unchecked((nuint)securityBuffer2.size) <= unchecked((nuint)ptr) + unchecked((nuint)array3[k].Length))
									{
										securityBuffer2.offset = (int)(unchecked((byte*)(void*)array[j].token) - ptr);
										securityBuffer2.token = array3[k];
										break;
									}
								}
							}
							if (k >= input.Length)
							{
								securityBuffer2.size = 0;
								securityBuffer2.offset = 0;
								securityBuffer2.token = null;
							}
						}
					}
					if (num != 0 && Logging.On)
					{
						if (num == 590625)
						{
							Logging.PrintError(Logging.Web, SR.GetString("net_log_operation_returned_something", op, "SEC_I_RENEGOTIATE"));
						}
						else
						{
							Logging.PrintError(Logging.Web, SR.GetString("net_log_operation_failed_with_error", op, string.Format(CultureInfo.CurrentCulture, "0X{0:X}", num)));
						}
					}
					return num;
				}
				finally
				{
					for (int l = 0; l < array2.Length; l++)
					{
						if (array2[l].IsAllocated)
						{
							array2[l].Free();
						}
					}
				}
			}
		}

		public static SafeFreeContextBufferChannelBinding QueryContextChannelBinding(SSPIInterface SecModule, SafeDeleteContext securityContext, ContextAttribute contextAttribute)
		{
			if (SecModule.QueryContextChannelBinding(securityContext, contextAttribute, out var refHandle) != 0)
			{
				return null;
			}
			return refHandle;
		}

		public static object QueryContextAttributes(SSPIInterface SecModule, SafeDeleteContext securityContext, ContextAttribute contextAttribute)
		{
			int errorCode;
			return QueryContextAttributes(SecModule, securityContext, contextAttribute, out errorCode);
		}

		public unsafe static object QueryContextAttributes(SSPIInterface SecModule, SafeDeleteContext securityContext, ContextAttribute contextAttribute, out int errorCode)
		{
			int num = IntPtr.Size;
			Type handleType = null;
			switch (contextAttribute)
			{
			case ContextAttribute.Sizes:
				num = SecSizes.SizeOf;
				break;
			case ContextAttribute.StreamSizes:
				num = StreamSizes.SizeOf;
				break;
			case ContextAttribute.Names:
				handleType = typeof(SafeFreeContextBuffer);
				break;
			case ContextAttribute.PackageInfo:
				handleType = typeof(SafeFreeContextBuffer);
				break;
			case ContextAttribute.NegotiationInfo:
				handleType = typeof(SafeFreeContextBuffer);
				num = Marshal.SizeOf(typeof(NegotiationInfo));
				break;
			case ContextAttribute.ClientSpecifiedSpn:
				handleType = typeof(SafeFreeContextBuffer);
				break;
			case ContextAttribute.RemoteCertificate:
				handleType = typeof(SafeFreeCertContext);
				break;
			case ContextAttribute.LocalCertificate:
				handleType = typeof(SafeFreeCertContext);
				break;
			case ContextAttribute.IssuerListInfoEx:
				num = Marshal.SizeOf(typeof(IssuerListInfoEx));
				handleType = typeof(SafeFreeContextBuffer);
				break;
			case ContextAttribute.ConnectionInfo:
				num = Marshal.SizeOf(typeof(SslConnectionInfo));
				break;
			default:
				throw new ArgumentException(SR.GetString("net_invalid_enum", "ContextAttribute"), "contextAttribute");
			}
			SafeHandle refHandle = null;
			object result = null;
			try
			{
				byte[] array = new byte[num];
				errorCode = SecModule.QueryContextAttributes(securityContext, contextAttribute, array, handleType, out refHandle);
				if (errorCode != 0)
				{
					return null;
				}
				if (contextAttribute <= ContextAttribute.NegotiationInfo)
				{
					switch (contextAttribute)
					{
					case ContextAttribute.Sizes:
						return new SecSizes(array);
					case ContextAttribute.StreamSizes:
						return new StreamSizes(array);
					case ContextAttribute.Names:
						if (ComNetOS.IsWin9x)
						{
							return Marshal.PtrToStringAnsi(refHandle.DangerousGetHandle());
						}
						return Marshal.PtrToStringUni(refHandle.DangerousGetHandle());
					case ContextAttribute.PackageInfo:
						return new SecurityPackageInfoClass(refHandle, 0);
					case ContextAttribute.NegotiationInfo:
						fixed (void* value = array)
						{
							return new NegotiationInfoClass(refHandle, Marshal.ReadInt32(new IntPtr(value), NegotiationInfo.NegotiationStateOffest));
						}
					default:
						return result;
					case ContextAttribute.Lifespan:
					case ContextAttribute.DceInfo:
						return result;
					case (ContextAttribute)11:
						return result;
					}
				}
				switch (contextAttribute)
				{
				case ContextAttribute.ClientSpecifiedSpn:
					return Marshal.PtrToStringUni(refHandle.DangerousGetHandle());
				case ContextAttribute.RemoteCertificate:
				case ContextAttribute.LocalCertificate:
					result = refHandle;
					refHandle = null;
					return result;
				case ContextAttribute.IssuerListInfoEx:
					result = new IssuerListInfoEx(refHandle, array);
					refHandle = null;
					return result;
				case ContextAttribute.ConnectionInfo:
					return new SslConnectionInfo(array);
				default:
					return result;
				}
			}
			finally
			{
				refHandle?.Close();
			}
		}

		public static string ErrorDescription(int errorCode)
		{
			if (errorCode == -1)
			{
				return "An exception when invoking Win32 API";
			}
			return errorCode switch
			{
				-2146893055 => "Invalid handle", 
				-2146893048 => "Invalid token", 
				590610 => "Continue needed", 
				-2146893032 => "Message incomplete", 
				-2146893022 => "Wrong principal", 
				-2146893053 => "Target unknown", 
				-2146893051 => "Package not found", 
				-2146893023 => "Buffer not enough", 
				-2146893041 => "Message altered", 
				-2146893019 => "Untrusted root", 
				_ => "0x" + errorCode.ToString("x", NumberFormatInfo.InvariantInfo), 
			};
		}
	}
}
