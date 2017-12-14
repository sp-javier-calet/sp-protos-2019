using System;
using System.Runtime.InteropServices;

namespace SocialPoint.Base
{
    public class UnmanagedMarshaledObject<T> : IDisposable
    {
        IntPtr _ptr = IntPtr.Zero;
        #if DEBUG
        readonly T _content;
        #endif

        public UnmanagedMarshaledObject(T content)
        {
            #if DEBUG
            _content = content;
            #endif
            _ptr = Marshal.AllocHGlobal(Marshal.SizeOf(content));
            Marshal.StructureToPtr(content, _ptr, false);
        }

        ~UnmanagedMarshaledObject()
        {
            Dispose();
        }
            
        public static implicit operator IntPtr(UnmanagedMarshaledObject<T> obj)
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
