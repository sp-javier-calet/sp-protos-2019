using System.Collections;
using System.Collections.Generic;
using BulletSharp;

namespace SocialPoint.Multiplayer
{
    public interface IPhysicsCollisionHandler
    {
        void RegisterCollisionCallbackListener(ICollisionCallbackEventHandler toBeAdded);

        void DeregisterCollisionCallbackListener(ICollisionCallbackEventHandler toBeRemoved);

        void OnPhysicsStep(CollisionWorld world);
    }

    public class PhysicsDefaultCollisionHandler : IPhysicsCollisionHandler
    {
        HashSet<ICollisionCallbackEventHandler> collisionCallbackListeners = new HashSet<ICollisionCallbackEventHandler>();

        public void RegisterCollisionCallbackListener(ICollisionCallbackEventHandler toBeAdded)
        {
            collisionCallbackListeners.Add(toBeAdded);
        }

        public void DeregisterCollisionCallbackListener(ICollisionCallbackEventHandler toBeRemoved)
        {
            collisionCallbackListeners.Remove(toBeRemoved);
        }

        public void OnPhysicsStep(CollisionWorld world)
        {
            Dispatcher dispatcher = world.Dispatcher;
            int numManifolds = dispatcher.NumManifolds;
            for(int i = 0; i < numManifolds; i++)
            {
                PersistentManifold contactManifold = dispatcher.GetManifoldByIndexInternal(i);
                CollisionObject a = contactManifold.Body0;
                CollisionObject b = contactManifold.Body1;
                if(a is CollisionObject && a.UserObject is PhysicsCollisionObject && ((PhysicsCollisionObject)a.UserObject).collisionCallbackEventHandler != null)
                {
                    ((PhysicsCollisionObject)a.UserObject).collisionCallbackEventHandler.OnVisitPersistentManifold(contactManifold);
                }
                if(b is CollisionObject && b.UserObject is PhysicsCollisionObject && ((PhysicsCollisionObject)b.UserObject).collisionCallbackEventHandler != null)
                {
                    ((PhysicsCollisionObject)b.UserObject).collisionCallbackEventHandler.OnVisitPersistentManifold(contactManifold);
                }
            }
            foreach(ICollisionCallbackEventHandler coeh in collisionCallbackListeners)
            {
                if(coeh != null)
                {
                    coeh.OnFinishedVisitingManifolds();
                }
            }
        }
    }
}
