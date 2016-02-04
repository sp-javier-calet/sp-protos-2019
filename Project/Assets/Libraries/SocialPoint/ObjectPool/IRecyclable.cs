
namespace SocialPoint.ObjectPool
{
    interface IRecyclable
    {
        void OnSpawn();
        void OnRecycle();
    }
}