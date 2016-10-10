using System;
using System.IO;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{    
    public class ServerLockstepCommandData : INetworkShareable
    {
        byte[] _command;
        uint _id;

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
            writer.Write(_id);
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
            _id = reader.ReadUInt32();
            var cmdLen = reader.ReadInt32();
            _command = null;
            if(cmdLen > 0)
            {
                _command = reader.ReadBytes(cmdLen);
            }
        }

        public override string ToString()
        {
            return string.Format("[ServerLockstepCommandData:{0}]", _id);
        }
    }        
}