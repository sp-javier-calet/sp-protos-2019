using System;
using System.IO;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public class ClientCommandData
    {
        ILockstepCommand _command;
        ILockstepCommandLogic _finish;
        byte _playerNum;

        public uint Id{ get; private set; }

        public ClientCommandData(ILockstepCommand cmd, ILockstepCommandLogic finish, byte playerNum)
        {
            Id = RandomUtils.GenerateUint();
            _command = cmd;
            _finish = finish;
            _playerNum = playerNum;
        }

        public ClientCommandData()
        {
        }

        public ServerCommandData ToServer(LockstepCommandFactory factory)
        {
            var stream = new MemoryStream();
            var writer = new SystemBinaryWriter(stream);
            Serialize(factory, writer);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new SystemBinaryReader(stream);
            var server = new ServerCommandData();
            server.Deserialize(reader);
            return server;
        }

        public void Serialize(LockstepCommandFactory factory, IWriter writer)
        {
            writer.Write(Id);
            writer.Write(_playerNum);
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
                writer.Write(stream.ToArray(), len);
            }
        }

        public void Deserialize(LockstepCommandFactory factory, IReader reader)
        {
            Id = reader.ReadUInt32();
            _playerNum = reader.ReadByte();
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
                _finish.Apply(_command, _playerNum);
            }
        }

        public bool Apply(Type type, ILockstepCommandLogic logic)
        {
            if(type.IsAssignableFrom(_command.GetType()))
            {
                logic.Apply(_command, _playerNum);
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ClientCommandData);
        }

        public bool Equals(ClientCommandData obj)
        {
            if((object)obj == null)
            {
                return false;
            }
            return Compare(this, obj);
        }

        public override int GetHashCode()
        {
            var hash = Id.GetHashCode();

            return hash;
        }

        static bool Compare(ClientCommandData a, ClientCommandData b)
        {
            return a.Id == b.Id;
        }

        public static bool operator ==(ClientCommandData a, ClientCommandData b)
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

        public static bool operator !=(ClientCommandData a, ClientCommandData b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return string.Format("[ClientCommandData:{0} {1} {2}]", Id, _playerNum, _command);
        }
    }
}