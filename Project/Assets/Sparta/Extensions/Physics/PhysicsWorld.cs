using System;
using System.Collections;
using Jitter;
using Jitter.LinearMath;
using Jitter.Dynamics;
using Jitter.Collision;
using System.Collections.Generic;

namespace SocialPoint.Physics
{
    public class PhysicsWorld
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

        public void Update(float dt)
        {
            _world.Step(dt, MultiThreaded);
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

        void CollisionDetectedHandler(RigidBody body1, RigidBody body2)
        {
            var behavior1 = (PhysicsRigidBody)body1.Tag;
            var behavior2 = (PhysicsRigidBody)body2.Tag;

            List<PhysicsContact> contacts1;
            List<PhysicsContact> contacts2;

            GetContactLists(body1, body2, out contacts1, out contacts2);

            behavior1.OnCollisionStay(body2, contacts1);
            behavior2.OnCollisionStay(body1, contacts2);
        }

        void OnCollisionBeginHandler(RigidBody body1, RigidBody body2)
        {
            var behavior1 = (PhysicsRigidBody)body1.Tag;
            var behavior2 = (PhysicsRigidBody)body2.Tag;

            List<PhysicsContact> contacts1;
            List<PhysicsContact> contacts2;

            GetContactLists(body1, body2, out contacts1, out contacts2);

            behavior1.OnCollisionEnter(body2, contacts1);
            behavior2.OnCollisionEnter(body1, contacts2);
        }

        void OnCollisionEndHandler(RigidBody body1, RigidBody body2)
        {
            var behavior1 = (PhysicsRigidBody)body1.Tag;
            var behavior2 = (PhysicsRigidBody)body2.Tag;

            List<PhysicsContact> contacts1;
            List<PhysicsContact> contacts2;

            GetContactLists(body1, body2, out contacts1, out contacts2);

            behavior1.OnCollisionExit(body2, contacts1);
            behavior2.OnCollisionExit(body1, contacts2);
        }

        void GetContactLists(RigidBody body1, RigidBody body2, out List<PhysicsContact> contacts1, out List<PhysicsContact> contacts2)
        {
            contacts1 = new List<PhysicsContact>();
            contacts2 = new List<PhysicsContact>();

            Arbiter arbiter = null;

            lock(_world.ArbiterMap)
            {
                _world.ArbiterMap.LookUpArbiter(body1, body2, out arbiter);
                if(arbiter != null)
                {
                    for(int i = 0; i < arbiter.ContactList.Count; i++)
                    {
                        var contact = arbiter.ContactList[i];
                        contacts1.Add(new PhysicsContact(contact, arbiter.Body1));
                        contacts2.Add(new PhysicsContact(contact, arbiter.Body2));
                    }
                }
            }
        }

        void InitializePhysicsWorld()
        {
            _collisionSystem = new CollisionSystemPersistentSAP();
            _world = new World(_collisionSystem);
            _world.Events.BodiesBeginCollide += OnCollisionBeginHandler;
            _collisionSystem.CollisionDetected += (body1, body2, point1, point2, normal, penetration) => CollisionDetectedHandler(body1, body2);
            _world.Events.BodiesEndCollide += OnCollisionEndHandler;
        }

        public bool IsCollisionEnabled(int layerIdxA, int layerIdxB)
        {
            return _collisionSystem.IsCollisionEnabled(layerIdxA, layerIdxB);
        }

        public void SetCollisionBetweenLayers(int layerIdxA, int layerIdxB, bool enable = true)
        {
            _collisionSystem.SetCollisionBetweenLayers(layerIdxA, layerIdxB, enable);
        }
    }
}
