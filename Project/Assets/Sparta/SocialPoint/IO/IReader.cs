using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;

namespace SocialPoint.IO
{
    public interface IReader
    {
        bool ReadBoolean();

        byte ReadByte();

        byte[] ReadBytes(int count);

        double ReadDouble();

        short ReadInt16();

        int ReadInt32();

        long ReadInt64();

        float ReadSingle();

        string ReadString();

        ushort ReadUInt16();

        uint ReadUInt32();

        ulong ReadUInt64();
    }
}