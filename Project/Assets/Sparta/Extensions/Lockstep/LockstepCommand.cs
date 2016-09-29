﻿using System;
using System.IO;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public interface ILockstepCommand : INetworkShareable, ICloneable
    {
    }

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

    public interface ILockstepCommandLogic<T>
    {
        void Apply(T data);
    }

    public class ActionLockstepCommandLogic<T> : ILockstepCommandLogic<T>
    {
        Action<T> _action;

        public ActionLockstepCommandLogic(Action<T> action)
        {
            _action = action;
        }

        public void Apply(T data)
        {
            if(_action != null)
            {
                _action(data);
            }
        }
    }

    public interface ILockstepCommandLogic : ILockstepCommandLogic<ILockstepCommand>
    {
    }

    public class LockstepCommandLogic<T> : ILockstepCommandLogic
    {
        ILockstepCommandLogic<T> _inner;

        public LockstepCommandLogic(Action<T> action) :
            this(new ActionLockstepCommandLogic<T>(action))
        {
        }

        public LockstepCommandLogic(ILockstepCommandLogic<T> inner)
        {
            _inner = inner;
        }

        public void Apply(ILockstepCommand data)
        {
            if(data is T && _inner != null)
            {
                _inner.Apply((T)data);
            }
        }
    }
}