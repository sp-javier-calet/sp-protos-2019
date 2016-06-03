using System.Collections;
using System.IO;
using UnityEngine.Networking;
using System;
using SocialPoint.Lockstep.Network;

namespace  SocialPoint.Utils.Network.UNet
{
    public class NetworkReaderWrapper : IReaderWrapper
    {
        NetworkReader _reader;

        public NetworkReaderWrapper(NetworkReader reader)
        {
            _reader = reader;
        }

        public bool ReadBoolean()
        {
            return _reader.ReadBoolean();
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return _reader.ReadBytes(count);
        }

        public double ReadDouble()
        {
            return _reader.ReadDouble();
        }

        public short ReadInt16()
        {
            return _reader.ReadInt16();
        }

        public int ReadInt32()
        {
            return _reader.ReadInt32();
        }

        public long ReadInt64()
        {
            return _reader.ReadInt64();
        }

        public float ReadSingle()
        {
            return _reader.ReadSingle();
        }

        public string ReadString()
        {
            return _reader.ReadString();
        }

        public ushort ReadUInt16()
        {
            return _reader.ReadUInt16();
        }

        public uint ReadUInt32()
        {
            return _reader.ReadUInt32();
        }

        public ulong ReadUInt64()
        {
            return _reader.ReadUInt64();
        }
    }
}