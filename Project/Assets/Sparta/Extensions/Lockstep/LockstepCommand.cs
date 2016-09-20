using System;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public interface ILockstepCommand : INetworkShareable, ICloneable
    {
    }

    public class LockstepCommandData
    {
        public ILockstepCommand Command;

        public int Id;
        public int ClientId;
        public int Turn;
        public int Retries;
        public ILockstepCommandLogic Logic;

        public void Serialize(LockstepCommandFactory factory, IWriter writer)
        {
            writer.Write(Id);
            writer.Write(ClientId);
            writer.Write(Turn);
            factory.Write(writer, Command);
        }

        public void Deserialize(LockstepCommandFactory factory, IReader reader)
        {
            Id = reader.ReadInt32();
            ClientId = reader.ReadInt32();
            Turn = reader.ReadInt32();
            Command = factory.Read(reader);
        }

        public void Discard()
        {
            if(Logic != null)
            {
                Logic.Apply(Command);
            }
        }

        public void Apply()
        {
            if(Logic != null)
            {
                Logic.Apply(Command);
            }
        }

        public bool Retry(int turn)
        {
            Turn = turn;
            Retries++;
            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LockstepCommandData);
        }

        public bool Equals(LockstepCommandData obj)
        {
            if((object)obj == null)
            {
                return false;
            }
            return this == obj;
        }

        public override int GetHashCode()
        {
            var hash = Id.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, ClientId.GetHashCode());
            return hash;
        }

        public static bool operator ==(LockstepCommandData a, LockstepCommandData b)
        {
            return a.Id == b.Id && a.ClientId == b.ClientId;
        }

        public static bool operator !=(LockstepCommandData a, LockstepCommandData b)
        {
            return !(a == b);
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
            if(data is T)
            {
                _inner.Apply((T)data);
            }
        }
    }
}