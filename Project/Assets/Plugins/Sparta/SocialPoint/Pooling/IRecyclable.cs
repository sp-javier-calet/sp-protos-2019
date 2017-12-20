
namespace SocialPoint.Pooling
{
    public interface IRecyclable
    {
        void OnSpawn();
        void OnRecycle();
    }
}