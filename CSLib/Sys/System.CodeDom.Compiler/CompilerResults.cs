using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace System.CodeDom.Compiler
{
	[Serializable]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public class CompilerResults
	{
		private CompilerErrorCollection errors = new CompilerErrorCollection();

		private StringCollection output = new StringCollection();

		private Assembly compiledAssembly;

		private string pathToAssembly;

		private int nativeCompilerReturnValue;

		private TempFileCollection tempFiles;

		private Evidence evidence;

		public TempFileCollection TempFiles
		{
			[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
			get
			{
				return tempFiles;
			}
			[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
			set
			{
				tempFiles = value;
			}
		}

		public Evidence Evidence
		{
			[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
			get
			{
				Evidence result = null;
				if (evidence != null)
				{
					result = CloneEvidence(evidence);
				}
				return result;
			}
			[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
			[SecurityPermission(SecurityAction.Demand, ControlEvidence = true)]
			set
			{
				if (value != null)
				{
					evidence = CloneEvidence(value);
				}
				else
				{
					evidence = null;
				}
			}
		}

		public Assembly CompiledAssembly
		{
			[SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.ControlEvidence)]
			get
			{
				if (compiledAssembly == null && pathToAssembly != null)
				{
					AssemblyName assemblyName = new AssemblyName();
					assemblyName.CodeBase = pathToAssembly;
					compiledAssembly = Assembly.Load(assemblyName, evidence);
				}
				return compiledAssembly;
			}
			[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
			set
			{
				compiledAssembly = value;
			}
		}

		public CompilerErrorCollection Errors => errors;

		public StringCollection Output
		{
			[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
			get
			{
				return output;
			}
		}

		public string PathToAssembly
		{
			[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
			get
			{
				return pathToAssembly;
			}
			[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
			set
			{
				pathToAssembly = value;
			}
		}

		public int NativeCompilerReturnValue
		{
			get
			{
				return nativeCompilerReturnValue;
			}
			[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
			set
			{
				nativeCompilerReturnValue = value;
			}
		}

		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		public CompilerResults(TempFileCollection tempFiles)
		{
			this.tempFiles = tempFiles;
		}

		internal static Evidence CloneEvidence(Evidence ev)
		{
			new PermissionSet(PermissionState.Unrestricted).Assert();
			MemoryStream memoryStream = new MemoryStream();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryFormatter.Serialize(memoryStream, ev);
			memoryStream.Position = 0L;
			return (Evidence)binaryFormatter.Deserialize(memoryStream);
		}
	}
}
