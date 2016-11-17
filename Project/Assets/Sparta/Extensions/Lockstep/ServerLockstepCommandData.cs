using System;
using System.IO;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public class ServerLockstepCommandData : INetworkShareable
    {
        byte[] _command;

        public byte PlayerNumber;

        public uint Id{ get; private set; }

        public ServerLockstepCommandData()
        {
        }

        public ClientLockstepCommandData ToClient(LockstepCommandFactory factory)
        {
            var stream = new MemoryStream();
            var writer = new SystemBinaryWriter(stream);
            Serialize(writer);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new SystemBinaryReader(stream);
            var client = new ClientLockstepCommandData();
            client.Deserialize(factory, reader);
            return client;
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Id);
            writer.Write(PlayerNumber);
            if(_command == null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(_command.Length);
                writer.Write(_command, _command.Length);
            }
        }

        public void Deserialize(IReader reader)
        {
            Id = reader.ReadUInt32();
            PlayerNumber = reader.ReadByte();
            var cmdLen = reader.ReadInt32();
            _command = null;
            if(cmdLen > 0)
            {
                _command = reader.ReadBytes(cmdLen);
            }
        }

        public override string ToString()
        {
            return string.Format("[ServerLockstepCommandData:{0} {1}]", Id, PlayerNumber);
        }
    }
}