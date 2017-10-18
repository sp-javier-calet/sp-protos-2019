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
            Clear();
        }

        public void Clear()
        {
            _pool = new ObjectPool();
            _typeCache = new TypeCache();
        }
    }
}
