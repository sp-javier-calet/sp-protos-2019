using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;

namespace SocialPoint.Utils
{
    public interface IWriterWrapper
    {
        void Write(bool value);

        void Write(byte[] buffer, int offset, int count);

        void Write(ushort value);

        void Write(short value);

        void Write(sbyte value);

        void Write(byte value);

        void Write(char value);

        void Write(int value);

        void Write(byte[] buffer, int count);

        void Write(string value);

        void Write(double value);

        void Write(float value);

        void Write(ulong value);

        void Write(long value);

        void Write(uint value);
    }
}