using System;
using System.Runtime.InteropServices;

namespace SocialPoint.Base
{
    class UnmanagedMarshalledObject<T> : IDisposable where T : struct
    {
        IntPtr _ptr = IntPtr.Zero;
        #if DEBUG
        readonly T _content;
        #endif

        public UnmanagedMarshalledObject(T content)
        {
            #if DEBUG
            _content = content;
            #endif
            _ptr = Marshal.AllocHGlobal(Marshal.SizeOf(content));
            Marshal.StructureToPtr(content, _ptr, false);
        }

        ~UnmanagedMarshalledObject()
        {
            Dispose();
        }
            
        public static implicit operator IntPtr(UnmanagedMarshalledObject<T> obj)
        {
            return obj._ptr;
        }

        #if DEBUG
        public T Content
        {
            get
            {
                return _content;
            }
        }
        #endif

        public void Dispose()
        {
            if(_ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_ptr);
                _ptr = IntPtr.Zero;
            }
        }
    }
}
