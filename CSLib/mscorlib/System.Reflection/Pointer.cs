using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	[CLSCompliant(false)]
	[ComVisible(true)]
	public sealed class Pointer : ISerializable
	{
		private unsafe void* _ptr;

		private Type _ptrType;

		private Pointer()
		{
		}

		private unsafe Pointer(SerializationInfo info, StreamingContext context)
		{
			_ptr = ((IntPtr)info.GetValue("_ptr", typeof(IntPtr))).ToPointer();
			_ptrType = (Type)info.GetValue("_ptrType", typeof(Type));
		}

		public unsafe static object Box(void* ptr, Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!type.IsPointer)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBePointer"), "ptr");
			}
			Pointer pointer = new Pointer();
			pointer._ptr = ptr;
			pointer._ptrType = type;
			return pointer;
		}

		public unsafe static void* Unbox(object ptr)
		{
			if (!(ptr is Pointer))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBePointer"), "ptr");
			}
			return ((Pointer)ptr)._ptr;
		}

		internal Type GetPointerType()
		{
			return _ptrType;
		}

		internal unsafe object GetPointerValue()
		{
			return (IntPtr)_ptr;
		}

		unsafe void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("_ptr", new IntPtr(_ptr));
			info.AddValue("_ptrType", _ptrType);
		}
	}
}
