namespace SocialPoint.Multiplayer
{
    public class NetworkSceneContext
    {
        SocialPoint.Utils.ObjectPool _pool = null;
        public SocialPoint.Utils.ObjectPool Pool
        {
            get
            {
                return _pool;
            }
        }

        TypeCache _typeCache = null;
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
            _pool = new SocialPoint.Utils.ObjectPool();
            _typeCache = new TypeCache();
        }
    }
}
