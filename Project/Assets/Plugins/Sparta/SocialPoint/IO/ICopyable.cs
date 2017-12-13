using SocialPoint.Pooling;

namespace SocialPoint.IO
{
    public interface ICopyable
    {
        void Copy(object other);
    }

    public interface IPoolCloneable
    {
        object Clone(ObjectPool pool = null);
    }
}
