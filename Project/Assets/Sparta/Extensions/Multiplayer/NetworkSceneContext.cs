using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public class NetworkSceneContext
    {
        ObjectPool _pool;
        public ObjectPool Pool
        {
            get
            {
                return _pool;
            }
        }

        TypeCache _typeCache;
        public TypeCache TypeCache
        {
            get
            {
                return _typeCache;
            }
        }

        public NetworkSceneContext()
        {
            DoClear();
        }

        public virtual void Clear()
        {
            DoClear();
        }

        void DoClear()
        {
            _pool = new ObjectPool();
            _typeCache = new TypeCache();
        }
    }
}
