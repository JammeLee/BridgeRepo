using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Policy;

namespace System.Text.RegularExpressions
{
	[Serializable]
	public class Regex : ISerializable
	{
		internal const int MaxOptionShift = 10;

		protected internal string pattern;

		protected internal RegexRunnerFactory factory;

		protected internal RegexOptions roptions;

		protected internal Hashtable caps;

		protected internal Hashtable capnames;

		protected internal string[] capslist;

		protected internal int capsize;

		internal ExclusiveReference runnerref;

		internal SharedReference replref;

		internal RegexCode code;

		internal bool refsInitialized;

		internal static LinkedList<CachedCodeEntry> livecode = new LinkedList<CachedCodeEntry>();

		internal static int cacheSize = 15;

		public static int CacheSize
		{
			get
			{
				return cacheSize;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				cacheSize = value;
				if (livecode.Count <= cacheSize)
				{
					return;
				}
				lock (livecode)
				{
					while (livecode.Count > cacheSize)
					{
						livecode.RemoveLast();
					}
				}
			}
		}

		public RegexOptions Options => roptions;

		public bool RightToLeft => UseOptionR();

		protected Regex()
		{
		}

		public Regex(string pattern)
			: this(pattern, RegexOptions.None, useCache: false)
		{
		}

		public Regex(string pattern, RegexOptions options)
			: this(pattern, options, useCache: false)
		{
		}

		private Regex(string pattern, RegexOptions options, bool useCache)
		{
			CachedCodeEntry cachedCodeEntry = null;
			string text = null;
			if (pattern == null)
			{
				throw new ArgumentNullException("pattern");
			}
			if (options < RegexOptions.None || (int)options >> 10 != 0)
			{
				throw new ArgumentOutOfRangeException("options");
			}
			if ((options & RegexOptions.ECMAScript) != 0 && ((uint)options & 0xFFFFFCF4u) != 0)
			{
				throw new ArgumentOutOfRangeException("options");
			}
			text = (((options & RegexOptions.CultureInvariant) == 0) ? CultureInfo.CurrentCulture.ThreeLetterWindowsLanguageName : CultureInfo.InvariantCulture.ThreeLetterWindowsLanguageName);
			string[] array = new string[5];
			int num = (int)options;
			array[0] = num.ToString(NumberFormatInfo.InvariantInfo);
			array[1] = ":";
			array[2] = text;
			array[3] = ":";
			array[4] = pattern;
			string key = string.Concat(array);
			cachedCodeEntry = LookupCachedAndUpdate(key);
			this.pattern = pattern;
			roptions = options;
			if (cachedCodeEntry == null)
			{
				RegexTree regexTree = RegexParser.Parse(pattern, roptions);
				capnames = regexTree._capnames;
				capslist = regexTree._capslist;
				code = RegexWriter.Write(regexTree);
				caps = code._caps;
				capsize = code._capsize;
				InitializeReferences();
				regexTree = null;
				if (useCache)
				{
					cachedCodeEntry = CacheCode(key);
				}
			}
			else
			{
				caps = cachedCodeEntry._caps;
				capnames = cachedCodeEntry._capnames;
				capslist = cachedCodeEntry._capslist;
				capsize = cachedCodeEntry._capsize;
				code = cachedCodeEntry._code;
				factory = cachedCodeEntry._factory;
				runnerref = cachedCodeEntry._runnerref;
				replref = cachedCodeEntry._replref;
				refsInitialized = true;
			}
			if (UseOptionC() && factory == null)
			{
				factory = Compile(code, roptions);
				if (useCache)
				{
					cachedCodeEntry?.AddCompiled(factory);
				}
				code = null;
			}
		}

		protected Regex(SerializationInfo info, StreamingContext context)
			: this(info.GetString("pattern"), (RegexOptions)info.GetInt32("options"))
		{
		}

		void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
		{
			si.AddValue("pattern", ToString());
			si.AddValue("options", Options);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
		internal RegexRunnerFactory Compile(RegexCode code, RegexOptions roptions)
		{
			return RegexCompiler.Compile(code, roptions);
		}

		public static string Escape(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			return RegexParser.Escape(str);
		}

		public static string Unescape(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			return RegexParser.Unescape(str);
		}

		public override string ToString()
		{
			return pattern;
		}

		public string[] GetGroupNames()
		{
			string[] array;
			if (capslist == null)
			{
				int num = capsize;
				array = new string[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = Convert.ToString(i, CultureInfo.InvariantCulture);
				}
			}
			else
			{
				array = new string[capslist.Length];
				Array.Copy(capslist, 0, array, 0, capslist.Length);
			}
			return array;
		}

		public int[] GetGroupNumbers()
		{
			int[] array;
			if (caps == null)
			{
				int num = capsize;
				array = new int[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = i;
				}
			}
			else
			{
				array = new int[caps.Count];
				IDictionaryEnumerator enumerator = caps.GetEnumerator();
				while (enumerator.MoveNext())
				{
					array[(int)enumerator.Value] = (int)enumerator.Key;
				}
			}
			return array;
		}

		public string GroupNameFromNumber(int i)
		{
			if (capslist == null)
			{
				if (i >= 0 && i < capsize)
				{
					return i.ToString(CultureInfo.InvariantCulture);
				}
				return string.Empty;
			}
			if (caps != null)
			{
				object obj = caps[i];
				if (obj == null)
				{
					return string.Empty;
				}
				i = (int)obj;
			}
			if (i >= 0 && i < capslist.Length)
			{
				return capslist[i];
			}
			return string.Empty;
		}

		public int GroupNumberFromName(string name)
		{
			int num = -1;
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (capnames != null)
			{
				object obj = capnames[name];
				if (obj == null)
				{
					return -1;
				}
				return (int)obj;
			}
			num = 0;
			foreach (char c in name)
			{
				if (c > '9' || c < '0')
				{
					return -1;
				}
				num *= 10;
				num += c - 48;
			}
			if (num >= 0 && num < capsize)
			{
				return num;
			}
			return -1;
		}

		public static bool IsMatch(string input, string pattern)
		{
			return new Regex(pattern, RegexOptions.None, useCache: true).IsMatch(input);
		}

		public static bool IsMatch(string input, string pattern, RegexOptions options)
		{
			return new Regex(pattern, options, useCache: true).IsMatch(input);
		}

		public bool IsMatch(string input)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return null == Run(quick: true, -1, input, 0, input.Length, UseOptionR() ? input.Length : 0);
		}

		public bool IsMatch(string input, int startat)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return null == Run(quick: true, -1, input, 0, input.Length, startat);
		}

		public static Match Match(string input, string pattern)
		{
			return new Regex(pattern, RegexOptions.None, useCache: true).Match(input);
		}

		public static Match Match(string input, string pattern, RegexOptions options)
		{
			return new Regex(pattern, options, useCache: true).Match(input);
		}

		public Match Match(string input)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return Run(quick: false, -1, input, 0, input.Length, UseOptionR() ? input.Length : 0);
		}

		public Match Match(string input, int startat)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return Run(quick: false, -1, input, 0, input.Length, startat);
		}

		public Match Match(string input, int beginning, int length)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return Run(quick: false, -1, input, beginning, length, UseOptionR() ? (beginning + length) : beginning);
		}

		public static MatchCollection Matches(string input, string pattern)
		{
			return new Regex(pattern, RegexOptions.None, useCache: true).Matches(input);
		}

		public static MatchCollection Matches(string input, string pattern, RegexOptions options)
		{
			return new Regex(pattern, options, useCache: true).Matches(input);
		}

		public MatchCollection Matches(string input)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return new MatchCollection(this, input, 0, input.Length, UseOptionR() ? input.Length : 0);
		}

		public MatchCollection Matches(string input, int startat)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return new MatchCollection(this, input, 0, input.Length, startat);
		}

		public static string Replace(string input, string pattern, string replacement)
		{
			return new Regex(pattern, RegexOptions.None, useCache: true).Replace(input, replacement);
		}

		public static string Replace(string input, string pattern, string replacement, RegexOptions options)
		{
			return new Regex(pattern, options, useCache: true).Replace(input, replacement);
		}

		public string Replace(string input, string replacement)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return Replace(input, replacement, -1, UseOptionR() ? input.Length : 0);
		}

		public string Replace(string input, string replacement, int count)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return Replace(input, replacement, count, UseOptionR() ? input.Length : 0);
		}

		public string Replace(string input, string replacement, int count, int startat)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			if (replacement == null)
			{
				throw new ArgumentNullException("replacement");
			}
			RegexReplacement regexReplacement = (RegexReplacement)replref.Get();
			if (regexReplacement == null || !regexReplacement.Pattern.Equals(replacement))
			{
				regexReplacement = RegexParser.ParseReplacement(replacement, caps, capsize, capnames, roptions);
				replref.Cache(regexReplacement);
			}
			return regexReplacement.Replace(this, input, count, startat);
		}

		public static string Replace(string input, string pattern, MatchEvaluator evaluator)
		{
			return new Regex(pattern, RegexOptions.None, useCache: true).Replace(input, evaluator);
		}

		public static string Replace(string input, string pattern, MatchEvaluator evaluator, RegexOptions options)
		{
			return new Regex(pattern, options, useCache: true).Replace(input, evaluator);
		}

		public string Replace(string input, MatchEvaluator evaluator)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return Replace(input, evaluator, -1, UseOptionR() ? input.Length : 0);
		}

		public string Replace(string input, MatchEvaluator evaluator, int count)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return Replace(input, evaluator, count, UseOptionR() ? input.Length : 0);
		}

		public string Replace(string input, MatchEvaluator evaluator, int count, int startat)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return RegexReplacement.Replace(evaluator, this, input, count, startat);
		}

		public static string[] Split(string input, string pattern)
		{
			return new Regex(pattern, RegexOptions.None, useCache: true).Split(input);
		}

		public static string[] Split(string input, string pattern, RegexOptions options)
		{
			return new Regex(pattern, options, useCache: true).Split(input);
		}

		public string[] Split(string input)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return Split(input, 0, UseOptionR() ? input.Length : 0);
		}

		public string[] Split(string input, int count)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return RegexReplacement.Split(this, input, count, UseOptionR() ? input.Length : 0);
		}

		public string[] Split(string input, int count, int startat)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return RegexReplacement.Split(this, input, count, startat);
		}

		[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
		public static void CompileToAssembly(RegexCompilationInfo[] regexinfos, AssemblyName assemblyname)
		{
			CompileToAssemblyInternal(regexinfos, assemblyname, null, null, Assembly.GetCallingAssembly().Evidence);
		}

		[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
		public static void CompileToAssembly(RegexCompilationInfo[] regexinfos, AssemblyName assemblyname, CustomAttributeBuilder[] attributes)
		{
			CompileToAssemblyInternal(regexinfos, assemblyname, attributes, null, Assembly.GetCallingAssembly().Evidence);
		}

		[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
		public static void CompileToAssembly(RegexCompilationInfo[] regexinfos, AssemblyName assemblyname, CustomAttributeBuilder[] attributes, string resourceFile)
		{
			CompileToAssemblyInternal(regexinfos, assemblyname, attributes, resourceFile, Assembly.GetCallingAssembly().Evidence);
		}

		private static void CompileToAssemblyInternal(RegexCompilationInfo[] regexinfos, AssemblyName assemblyname, CustomAttributeBuilder[] attributes, string resourceFile, Evidence evidence)
		{
			if (assemblyname == null)
			{
				throw new ArgumentNullException("assemblyname");
			}
			if (regexinfos == null)
			{
				throw new ArgumentNullException("regexinfos");
			}
			RegexCompiler.CompileToAssembly(regexinfos, assemblyname, attributes, resourceFile, evidence);
		}

		protected void InitializeReferences()
		{
			if (refsInitialized)
			{
				throw new NotSupportedException(SR.GetString("OnlyAllowedOnce"));
			}
			refsInitialized = true;
			runnerref = new ExclusiveReference();
			replref = new SharedReference();
		}

		internal Match Run(bool quick, int prevlen, string input, int beginning, int length, int startat)
		{
			RegexRunner regexRunner = null;
			if (startat < 0 || startat > input.Length)
			{
				throw new ArgumentOutOfRangeException("start", SR.GetString("BeginIndexNotNegative"));
			}
			if (length < 0 || length > input.Length)
			{
				throw new ArgumentOutOfRangeException("length", SR.GetString("LengthNotNegative"));
			}
			regexRunner = (RegexRunner)runnerref.Get();
			if (regexRunner == null)
			{
				regexRunner = ((factory == null) ? new RegexInterpreter(code, UseOptionInvariant() ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture) : factory.CreateInstance());
			}
			Match result = regexRunner.Scan(this, input, beginning, beginning + length, startat, prevlen, quick);
			runnerref.Release(regexRunner);
			return result;
		}

		private static CachedCodeEntry LookupCachedAndUpdate(string key)
		{
			lock (livecode)
			{
				for (LinkedListNode<CachedCodeEntry> linkedListNode = livecode.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
				{
					if (linkedListNode.Value._key == key)
					{
						livecode.Remove(linkedListNode);
						livecode.AddFirst(linkedListNode);
						return linkedListNode.Value;
					}
				}
			}
			return null;
		}

		private CachedCodeEntry CacheCode(string key)
		{
			CachedCodeEntry result = null;
			lock (livecode)
			{
				for (LinkedListNode<CachedCodeEntry> linkedListNode = livecode.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
				{
					if (linkedListNode.Value._key == key)
					{
						livecode.Remove(linkedListNode);
						livecode.AddFirst(linkedListNode);
						return linkedListNode.Value;
					}
				}
				if (cacheSize != 0)
				{
					result = new CachedCodeEntry(key, capnames, capslist, code, caps, capsize, runnerref, replref);
					livecode.AddFirst(result);
					if (livecode.Count > cacheSize)
					{
						livecode.RemoveLast();
						return result;
					}
					return result;
				}
				return result;
			}
		}

		protected bool UseOptionC()
		{
			return (roptions & RegexOptions.Compiled) != 0;
		}

		protected bool UseOptionR()
		{
			return (roptions & RegexOptions.RightToLeft) != 0;
		}

		internal bool UseOptionInvariant()
		{
			return (roptions & RegexOptions.CultureInvariant) != 0;
		}
	}
}
