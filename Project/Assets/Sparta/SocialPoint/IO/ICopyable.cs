using SocialPoint.Utils;

namespace SocialPoint.IO
{
    public interface ICopyable
    {
        void Copy(object other);
    }

    public interface IPoolCloneable
    {
        object Clone(ObjectPool pool);
    }
}
