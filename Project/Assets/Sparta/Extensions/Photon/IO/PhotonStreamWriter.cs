using System;
using Photon;

namespace SocialPoint.IO
{
    class PhotonStreamWriter : SimpleWriter
    {
        PhotonStream _stream;

        public PhotonStreamWriter(PhotonStream stream)
        {
            _stream = stream;
        }

        void SendNext<T>(T data)
        {
            _stream.SendNext(data);
        }

        public override void Write(bool value)
        {
            SendNext(value);
        }

        public override void Write(ushort value)
        {
            SendNext(value);
        }

        public override void Write(short value)
        {
            SendNext(value);
        }

        public override void Write(sbyte value)
        {
            SendNext(value);
        }

        public override void Write(byte value)
        {
            SendNext(value);
        }

        public override void Write(char value)
        {
            SendNext(value);
        }

        public override void Write(int value)
        {
            SendNext(value);
        }

        public override void Write(string value)
        {
            SendNext(value);
        }

        public override void Write(double value)
        {
            SendNext(value);
        }

        public override void Write(float value)
        {
            SendNext(value);
        }

        public override void Write(ulong value)
        {
            SendNext(value);
        }

        public override void Write(long value)
        {
            SendNext(value);
        }

        public override void Write(uint value)
        {
            SendNext(value);
        }
    }
}
