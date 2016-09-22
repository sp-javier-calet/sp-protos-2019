using System;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public class LockstepCommandFactory
    {
        Dictionary<byte, ILockstepCommand> _prototypes = new Dictionary<byte, ILockstepCommand>();

        public void Register(byte typeId, ILockstepCommand prototype)
        {
            Register(prototype.GetType(), typeId, prototype);
        }

        public void Register<T>(byte typeId, T prototype) where T : ILockstepCommand
        {
            Register(typeof(T), typeId, prototype);
        }

        void Register(Type type, byte typeId, ILockstepCommand prototype)
        {
            _prototypes[typeId] = prototype;
        }

        public void Register<T>(byte type) where T : ILockstepCommand, new()
        {
            Register<T>(type, new T());
        }

        ILockstepCommand Create(byte type)
        {
            ILockstepCommand prototype;
            if(_prototypes.TryGetValue(type, out prototype))
            {
                return (ILockstepCommand)prototype.Clone();
            }
            return null;
        }

        public ILockstepCommand Read(IReader reader)
        {
            var type = reader.ReadByte();
            var cmd = Create(type);
            if(cmd == null)
            {
                throw new InvalidOperationException("Command type '" + type + "' not registered!");
            }
            cmd.Deserialize(reader);
            return cmd;
        }

        public void Write(IWriter writer, ILockstepCommand cmd)
        {
            var itr = _prototypes.GetEnumerator();
            var success = false;
            while(itr.MoveNext())
            {
                if(itr.Current.Value.GetType().IsAssignableFrom(cmd.GetType()))
                {
                    writer.Write(itr.Current.Key);
                    cmd.Serialize(writer);
                    success = true;
                    break;
                }
            }
            itr.Dispose();
            if(!success)
            {
                throw new InvalidOperationException("Command type '" + cmd.GetType() + "' not registered!");
            }
        }
    }
}