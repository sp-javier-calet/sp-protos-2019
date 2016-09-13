using BulletSharp;

namespace SocialPoint.Multiplayer
{
    public interface ICollisionCallbackEventHandler
    {
        void OnVisitPersistentManifold(PersistentManifold pm);

        void OnFinishedVisitingManifolds();
    }
}
