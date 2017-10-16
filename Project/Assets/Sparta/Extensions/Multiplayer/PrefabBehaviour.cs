using SocialPoint.IO;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public class PrefabNameBehaviour : NetworkBehaviour, ICopyable
    {
        public string Path = "";
        public bool UseInstantiationPosition;
        public JVector InstantiationPosition;

        void ICopyable.Copy(object other)
        {
            var behaviour = other as PrefabNameBehaviour;
            if(behaviour == null)
            {
                return;
            }
            base.Copy(behaviour);
            Path = behaviour.Path;
            UseInstantiationPosition = behaviour.UseInstantiationPosition;
            InstantiationPosition = behaviour.InstantiationPosition;
        }

        public override object Clone()
        {
            var behaviour = GameObject.Context.Pool.Get<PrefabNameBehaviour>();
            behaviour.Path = Path;
            behaviour.UseInstantiationPosition = UseInstantiationPosition;
            behaviour.InstantiationPosition = InstantiationPosition;
            return behaviour;
        }
    }

    public class PrefabServerSerializer : IDiffWriteSerializer<PrefabNameBehaviour>
    {
        public void Compare(PrefabNameBehaviour newObj, PrefabNameBehaviour oldObj, Bitset dirty)
        {
            dirty.Set(newObj.Path != oldObj.Path);
            dirty.Set(newObj.UseInstantiationPosition != oldObj.UseInstantiationPosition);
            dirty.Set(newObj.UseInstantiationPosition && newObj.InstantiationPosition != oldObj.InstantiationPosition);
        }

        public void Serialize(PrefabNameBehaviour newObj, IWriter writer)
        {
            writer.Write(newObj.Path);
            writer.Write(newObj.UseInstantiationPosition);
            if(newObj.UseInstantiationPosition)
            {
                writer.Write(newObj.InstantiationPosition.X);
                writer.Write(newObj.InstantiationPosition.Y);
                writer.Write(newObj.InstantiationPosition.Z);
            }
        }

        public void Serialize(PrefabNameBehaviour newObj, PrefabNameBehaviour oldObj, IWriter writer, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.Path);
            }

            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.UseInstantiationPosition);
            }

            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.InstantiationPosition.X);
                writer.Write(newObj.InstantiationPosition.Y);
                writer.Write(newObj.InstantiationPosition.Z);
            }
        }
    }

    public class PrefabClientParser : IDiffReadParser<PrefabNameBehaviour>
    {
        NetworkSceneContext _context = null;
        public NetworkSceneContext Context
        {
            get
            {
                SocialPoint.Base.DebugUtils.Assert(_context != null);
                return _context;
            }
            set
            {
                _context = value;
            }
        }

        public PrefabClientParser(NetworkSceneContext context)
        {
            Context = context;
        }

        public int GetDirtyBitsSize(PrefabNameBehaviour obj)
        {
            return 3;
        }

        public PrefabNameBehaviour Parse(IReader reader)
        {
            var obj = Context.Pool.Get<PrefabNameBehaviour>();
            obj.Path = reader.ReadString();

            obj.UseInstantiationPosition = reader.ReadBoolean();

            if(obj.UseInstantiationPosition)
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                obj.InstantiationPosition = new JVector(x, y, z);
            }

            return obj;
        }

        public PrefabNameBehaviour Parse(PrefabNameBehaviour oldObj, IReader reader, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                oldObj.Path = reader.ReadString();
            }

            if(Bitset.NullOrGet(dirty))
            {
                oldObj.UseInstantiationPosition = reader.ReadBoolean();
            }

            if(Bitset.NullOrGet(dirty))
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                oldObj.InstantiationPosition = new JVector(x, y, z);
            }
            return oldObj;
        }
    }
}
