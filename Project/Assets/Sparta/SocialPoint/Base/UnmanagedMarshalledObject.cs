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

        /*
        #if DEBUG
        public T Content
        {
            get
            {
                if(ptr != IntPtr.Zero)
                {
                    // Structure field of type Byte[] can't be marshalled as LPArray
                    //return (T)Marshal.PtrToStructure(ptr, typeof(T));
                    try
                    {
                        T content = (T)Marshal.PtrToStructure(ptr, typeof(T));
                        return content;
                    }
                    catch(Exception e)
                    {
                        Log.e(e.ToString());
                        return default(T);
                    }
                }
                else
                {
                    return default(T);
                }
            }
        }
        #endif
        */

        public void Dispose()
        {
            if(_ptr != IntPtr.Zero)
            {
                UnityEngine.Debug.LogWarning("DISPOSING: " + _content);
                Marshal.FreeHGlobal(_ptr);
                _ptr = IntPtr.Zero;
            }
        }
    }
}
