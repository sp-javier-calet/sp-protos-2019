using System;
using System.Collections;
using BulletSharp;
using BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    public class PhysicsCollisionObject : INetworkBehaviour
    {
        public NetworkGameObject NetworkGameObject
        {
            get;
            private set;
        }

        public CollisionFlags CollisionFlags
        {
            get;
            private set;
        }

        // A colliding object must match this mask in order to collide with me.
        public CollisionFilterGroups CollisionMask
        {
            get;
            private set;
        }

        // A bitmask
        public CollisionFilterGroups GroupsIBelongTo
        {
            get;
            private set;
        }

        public virtual ICollisionCallbackEventHandler CollisionCallbackEventHandler
        {
            get;
            private set;
        }

        public virtual CollisionObject CollisionObject
        {
            get
            {
                return _collisionObject;
            }
        }

        protected CollisionObject _collisionObject;
        protected PhysicsWorld _physicsWorld;
        protected PhysicsCollisionShape _collisionShape;
        protected PhysicsDebugger _debugger;
        internal bool IsInWorld = false;

        public PhysicsCollisionObject(PhysicsCollisionShape shape, PhysicsWorld physicsWorld, PhysicsDebugger debugger, 
                                      CollisionFlags collisionFlags = BulletSharp.CollisionFlags.None, 
                                      CollisionFilterGroups collisionMask = BulletSharp.CollisionFilterGroups.AllFilter, 
                                      CollisionFilterGroups belongGroups = BulletSharp.CollisionFilterGroups.DefaultFilter)
        {
            _collisionShape = shape;
            _physicsWorld = physicsWorld;
            _debugger = debugger;

            CollisionFlags = collisionFlags;
            CollisionMask = collisionMask;
            GroupsIBelongTo = belongGroups;

            BuildCollisionObject();
        }

        public virtual void OnStart(NetworkGameObject go)
        {
            NetworkGameObject = go;
            _collisionObject.WorldTransform = NetworkGameObject.Transform.WorldToLocalMatrix();

            AddObjectToBulletWorld();
        }

        public virtual void AddOnCollisionCallbackEventHandler(ICollisionCallbackEventHandler myCallback)
        {
            if(CollisionCallbackEventHandler != null)
            {
                _debugger.LogError("PhysicsCollisionObject already has a collision callback. You must remove it before adding another.");
            }
            CollisionCallbackEventHandler = myCallback;
            _physicsWorld.RegisterCollisionCallbackListener(CollisionCallbackEventHandler);
        }

        public virtual void RemoveOnCollisionCallbackEventHandler()
        {
            if(_physicsWorld != null && CollisionCallbackEventHandler != null)
            {
                _physicsWorld.DeregisterCollisionCallbackListener(CollisionCallbackEventHandler);
            }
            CollisionCallbackEventHandler = null;
        }

        protected virtual bool BuildCollisionObject()
        {
            if(_collisionObject != null)
            {
                if(IsInWorld && _physicsWorld != null)
                {
                    _physicsWorld.RemoveCollisionObject(_collisionObject);
                }
            }

            if(_collisionShape == null)
            {
                _debugger.LogError("There was no collision shape component attached to this PhysicsRigidBody");
                return false;
            }

            CollisionShape cs = _collisionShape.GetCollisionShape();

            if(_collisionObject == null)
            {
                _collisionObject = new CollisionObject();
            }
            _collisionObject.CollisionShape = cs;
            _collisionObject.UserObject = this;
            _collisionObject.CollisionFlags = CollisionFlags;

            return true;
        }

        protected virtual void AddObjectToBulletWorld()
        {
            _physicsWorld.AddCollisionObject(this);
        }

        protected virtual void RemoveObjectFromBulletWorld()
        {
            _physicsWorld.RemoveCollisionObject(_collisionObject);
        }

        public virtual void Update(float dt)
        {
            //TODO: Try to reduce the number of matrix creations (use dirty?)
            _collisionObject.WorldTransform = NetworkGameObject.Transform.WorldToLocalMatrix();
        }

        public virtual void OnDestroy()
        {
            if(IsInWorld && _collisionObject != null)
            {
                if(_physicsWorld != null && _physicsWorld.world != null)
                {
                    ((DiscreteDynamicsWorld)_physicsWorld.world).RemoveCollisionObject(_collisionObject);
                }
            }
            PhysicsUtilities.DisposeMember(ref _collisionShape);
            PhysicsUtilities.DisposeMember(ref _collisionObject);
        }

        public Object Clone()
        {
            //TODO: Improve Clone (clone shape, etc, to avoid multiple dispose)
            var behavior = new PhysicsCollisionObject(_collisionShape, _physicsWorld, _debugger, CollisionFlags);
            behavior.NetworkGameObject = NetworkGameObject;
            //behavior._collisionObject = _collisionObject;
            //behavior.IsInWorld = IsInWorld;
            behavior.CollisionCallbackEventHandler = CollisionCallbackEventHandler;
            return behavior;
        }

        public virtual void SetPosition(Vector3 position)
        {
            if(IsInWorld)
            {
                Matrix newTrans = _collisionObject.WorldTransform;
                newTrans.Origin = position;
                _collisionObject.WorldTransform = newTrans;
            }
            NetworkGameObject.Transform.Position = position;
        }

        public virtual void SetRotation(Quaternion rotation)
        {
            if(IsInWorld)
            {
                Matrix newTrans = _collisionObject.WorldTransform;
                Quaternion q = rotation;
                Matrix.RotationQuaternion(ref q, out newTrans);
                newTrans.Origin = NetworkGameObject.Transform.Position;
                _collisionObject.WorldTransform = newTrans;
            }
            NetworkGameObject.Transform.Rotation = rotation;
        }

        public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            if(IsInWorld)
            {
                Matrix newTrans = _collisionObject.WorldTransform;
                Quaternion q = rotation;
                Matrix.RotationQuaternion(ref q, out newTrans);
                newTrans.Origin = position;
                _collisionObject.WorldTransform = newTrans;
            }
            NetworkGameObject.Transform.Position = position;
            NetworkGameObject.Transform.Rotation = rotation;
        }
    }
}
