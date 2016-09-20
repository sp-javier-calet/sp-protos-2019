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

        void InitializePhysicsWorld()
        {
            _collisionSystem = new CollisionSystemPersistentSAP();
            _world = new World(_collisionSystem);
        }
    }
}
