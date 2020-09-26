using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;

namespace System.Runtime.Serialization.Formatters.Binary
{
	[ComVisible(true)]
	public sealed class BinaryFormatter : IRemotingFormatter, IFormatter
	{
		internal ISurrogateSelector m_surrogates;

		internal StreamingContext m_context;

		internal SerializationBinder m_binder;

		internal FormatterTypeStyle m_typeFormat = FormatterTypeStyle.TypesAlways;

		internal FormatterAssemblyStyle m_assemblyFormat;

		internal TypeFilterLevel m_securityLevel = TypeFilterLevel.Full;

		internal object[] m_crossAppDomainArray;

		public FormatterTypeStyle TypeFormat
		{
			get
			{
				return m_typeFormat;
			}
			set
			{
				m_typeFormat = value;
			}
		}

		public FormatterAssemblyStyle AssemblyFormat
		{
			get
			{
				return m_assemblyFormat;
			}
			set
			{
				m_assemblyFormat = value;
			}
		}

		public TypeFilterLevel FilterLevel
		{
			get
			{
				return m_securityLevel;
			}
			set
			{
				m_securityLevel = value;
			}
		}

		public ISurrogateSelector SurrogateSelector
		{
			get
			{
				return m_surrogates;
			}
			set
			{
				m_surrogates = value;
			}
		}

		public SerializationBinder Binder
		{
			get
			{
				return m_binder;
			}
			set
			{
				m_binder = value;
			}
		}

		public StreamingContext Context
		{
			get
			{
				return m_context;
			}
			set
			{
				m_context = value;
			}
		}

		public BinaryFormatter()
		{
			m_surrogates = null;
			m_context = new StreamingContext(StreamingContextStates.All);
		}

		public BinaryFormatter(ISurrogateSelector selector, StreamingContext context)
		{
			m_surrogates = selector;
			m_context = context;
		}

		public object Deserialize(Stream serializationStream)
		{
			return Deserialize(serializationStream, null);
		}

		internal object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck)
		{
			return Deserialize(serializationStream, null, fCheck, null);
		}

		public object Deserialize(Stream serializationStream, HeaderHandler handler)
		{
			return Deserialize(serializationStream, handler, fCheck: true, null);
		}

		public object DeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)
		{
			return Deserialize(serializationStream, handler, fCheck: true, methodCallMessage);
		}

		[ComVisible(false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public object UnsafeDeserialize(Stream serializationStream, HeaderHandler handler)
		{
			return Deserialize(serializationStream, handler, fCheck: false, null);
		}

		[ComVisible(false)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public object UnsafeDeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)
		{
			return Deserialize(serializationStream, handler, fCheck: false, methodCallMessage);
		}

		internal object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck, IMethodCallMessage methodCallMessage)
		{
			return Deserialize(serializationStream, handler, fCheck, isCrossAppDomain: false, methodCallMessage);
		}

		internal object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck, bool isCrossAppDomain, IMethodCallMessage methodCallMessage)
		{
			if (serializationStream == null)
			{
				throw new ArgumentNullException("serializationStream", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentNull_WithParamName"), serializationStream));
			}
			if (serializationStream.CanSeek && serializationStream.Length == 0)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_Stream"));
			}
			InternalFE internalFE = new InternalFE();
			internalFE.FEtypeFormat = m_typeFormat;
			internalFE.FEserializerTypeEnum = InternalSerializerTypeE.Binary;
			internalFE.FEassemblyFormat = m_assemblyFormat;
			internalFE.FEsecurityLevel = m_securityLevel;
			ObjectReader objectReader = new ObjectReader(serializationStream, m_surrogates, m_context, internalFE, m_binder);
			objectReader.crossAppDomainArray = m_crossAppDomainArray;
			return objectReader.Deserialize(handler, new __BinaryParser(serializationStream, objectReader), fCheck, isCrossAppDomain, methodCallMessage);
		}

		public void Serialize(Stream serializationStream, object graph)
		{
			Serialize(serializationStream, graph, null);
		}

		public void Serialize(Stream serializationStream, object graph, Header[] headers)
		{
			Serialize(serializationStream, graph, headers, fCheck: true);
		}

		internal void Serialize(Stream serializationStream, object graph, Header[] headers, bool fCheck)
		{
			if (serializationStream == null)
			{
				throw new ArgumentNullException("serializationStream", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentNull_WithParamName"), serializationStream));
			}
			InternalFE internalFE = new InternalFE();
			internalFE.FEtypeFormat = m_typeFormat;
			internalFE.FEserializerTypeEnum = InternalSerializerTypeE.Binary;
			internalFE.FEassemblyFormat = m_assemblyFormat;
			ObjectWriter objectWriter = new ObjectWriter(m_surrogates, m_context, internalFE);
			__BinaryWriter serWriter = new __BinaryWriter(serializationStream, objectWriter, m_typeFormat);
			objectWriter.Serialize(graph, headers, serWriter, fCheck);
			m_crossAppDomainArray = objectWriter.crossAppDomainArray;
		}
	}
}
