using System;
using System.IO;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public class ClientLockstepCommandData
    {
        ILockstepCommand _command;
        ILockstepCommandLogic _finish;
        uint _id;

        public ClientLockstepCommandData(ILockstepCommand cmd, ILockstepCommandLogic finish)
        {
            _id = RandomUtils.GenerateUint();
            _command = cmd;
            _finish = finish;
        }

        public ClientLockstepCommandData()
        {
        }

        public ServerLockstepCommandData ToServer(LockstepCommandFactory factory)
        {
            var stream = new MemoryStream();
            var writer = new SystemBinaryWriter(stream);
            Serialize(factory, writer);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new SystemBinaryReader(stream);
            var server = new ServerLockstepCommandData();
            server.Deserialize(reader);
            return server;
        }

        public void Serialize(LockstepCommandFactory factory, IWriter writer)
        {
            writer.Write(_id);
            if(_command == null)
            {
                writer.Write(0);
            }
            else
            {
                // write to memory to get the size
                var stream = new MemoryStream();
                var memWriter = new SystemBinaryWriter(stream);
                factory.Write(memWriter, _command);
                var len = (int)stream.Length;

                writer.Write(len);
                writer.Write(stream.GetBuffer(), len);
            }
        }

        public void Deserialize(LockstepCommandFactory factory, IReader reader)
        {
            _id = reader.ReadUInt32();
            var cmdLen = reader.ReadInt32();
            _command = null;
            if(cmdLen > 0)
            {
                _command = factory.Read(reader);
            }
        }

        public void Finish()
        {
            if(_finish != null)
            {
                _finish.Apply(_command);
            }
        }

        public bool Apply(Type type, ILockstepCommandLogic logic)
        {
            if(type.IsAssignableFrom(_command.GetType()))
            {
                logic.Apply(_command);
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ClientLockstepCommandData);
        }

        public bool Equals(ClientLockstepCommandData obj)
        {
            if((object)obj == null)
            {
                return false;
            }
            return Compare(this, obj);
        }

        public override int GetHashCode()
        {
            var hash = _id.GetHashCode();

            return hash;
        }

        static bool Compare(ClientLockstepCommandData a, ClientLockstepCommandData b)
        {
            return a._id == b._id;
        }

        public static bool operator ==(ClientLockstepCommandData a, ClientLockstepCommandData b)
        {
            var na = (object)a == null;
            var nb = (object)b == null;
            if(na && nb)
            {
                return true;
            }
            else if(na || nb)
            {
                return false;
            }
            return Compare(a, b);
        }

        public static bool operator !=(ClientLockstepCommandData a, ClientLockstepCommandData b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return string.Format("[ClientLockstepCommandData:{0} {1}]", _id, _command);
        }
    }        
}