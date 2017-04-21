using Photon;

namespace SocialPoint.IO
{
    class PhotonStreamReader : SimpleReader
    {
        PhotonStream _stream;

        public PhotonStreamReader(PhotonStream stream)
        {
            _stream = stream;
        }

        public override bool Finished
        {
            get
            {
                return _stream.PeekNext() == null;
            }
        }

        T ReceiveNext<T>()
        {
            return (T)_stream.ReceiveNext();
        }

        public override bool ReadBoolean()
        {
            return ReceiveNext<bool>();
        }

        public override byte ReadByte()
        {
            return ReceiveNext<byte>();
        }

        public override double ReadDouble()
        {
            return ReceiveNext<double>();
        }

        public override short ReadInt16()
        {
            return ReceiveNext<short>();
        }

        public override int ReadInt32()
        {
            return ReceiveNext<int>();
        }

        public override long ReadInt64()
        {
            return ReceiveNext<long>();
        }

        public override float ReadSingle()
        {
            return ReceiveNext<float>();
        }

        public override string ReadString()
        {
            return ReceiveNext<string>();
        }

        public override ushort ReadUInt16()
        {
            return ReceiveNext<ushort>();
        }

        public override uint ReadUInt32()
        {
            return ReceiveNext<uint>();
        }

        public override ulong ReadUInt64()
        {
            return ReceiveNext<ulong>();
        }
		
    }
}
