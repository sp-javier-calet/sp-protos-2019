using System;
using System.Collections;
using Jitter;
using Jitter.LinearMath;
using Jitter.Dynamics;
using Jitter.Collision;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public class PhysicsWorld : INetworkServerSceneBehaviour
    {
        public bool MultiThreaded
        {
            get;
            set;
        }

        public World World
        {
            get { return _world; }
        }

        World _world;
        CollisionSystem _collisionSystem;

        public PhysicsWorld(bool multithreaded)
        {
            MultiThreaded = multithreaded;
            InitializePhysicsWorld();
        }

        public void Update(float dt, NetworkScene scene, NetworkScene oldScene)
        {
            _world.Step(dt, MultiThreaded);
        }

        public void OnClientConnected(byte clientId)
        {
        }

        public void OnClientDisconnected(byte clientId)
        {
        }

        public void AddRigidBody(RigidBody rb)
        {
            if(rb != null)
            {
                _world.AddBody(rb);
            }
        }

        public void RemoveRigidBody(RigidBody rb)
        {
            if(rb != null)
            {
                _world.RemoveBody(rb);
            }
        }

        public void AddCollisionHandler(CollisionDetectedHandler handler)
        {
            _collisionSystem.CollisionDetected += handler;
        }

        public void RemoveCollisionHandler(CollisionDetectedHandler handler)
        {
            _collisionSystem.CollisionDetected -= handler;
        }

        void CollisionDetectedHandler(RigidBody body1, RigidBody body2, 
                                      JVector point1, JVector point2, JVector normal, float penetration)
        {
            var behavior1 = (PhysicsRigidBody)body1.Tag;
            var behavior2 = (PhysicsRigidBody)body2.Tag;
            behavior1.OnCollision(body2, point1, point2, normal, penetration);
            behavior2.OnCollision(body1, point2, point1, normal, penetration);
        }

        void InitializePhysicsWorld()
        {
            _collisionSystem = new CollisionSystemPersistentSAP();
            _collisionSystem.CollisionDetected += CollisionDetectedHandler;
            _world = new World(_collisionSystem);
        }
    }
}
