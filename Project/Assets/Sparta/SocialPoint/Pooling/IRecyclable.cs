
namespace SocialPoint.Pooling
{
    interface IRecyclable
    {
        void OnSpawn();
        void OnRecycle();
    }
}