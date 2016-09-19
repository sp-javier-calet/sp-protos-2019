using System;
using System.Collections;
using Jitter;
using Jitter.Dynamics;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public class PhysicsCollisionObject : INetworkBehaviour
    {
        public NetworkGameObject NetworkGameObject
        {
            get;
            private set;
        }

        public virtual ICollisionCallbackEventHandler CollisionCallbackEventHandler
        {
            get;
            private set;
        }

        /*public virtual CollisionObject CollisionObject
        {
            get
            {
                return _collisionObject;
            }
        }*/

        /* ***TEST DUMMY TO AVOID ERRORS */
        public enum CollisionFlags
        {
            None,
            KinematicObject,
            StaticObject
        }

        /* ***TEST DUMMY TO AVOID ERRORS */
        public enum CollisionFilterGroups
        {
            DefaultFilter,
            AllFilter
        }

        protected RigidBody _collisionObject;
        protected PhysicsWorld _physicsWorld;
        protected PhysicsCollisionShape _collisionShape;
        protected CollisionFlags _collisionFlags;
        protected CollisionFilterGroups _collisionMask;
        protected CollisionFilterGroups _groupsIBelongTo;
        protected PhysicsDebugger _debugger;
        protected bool _isInWorld = false;

        public PhysicsCollisionObject(PhysicsCollisionShape shape, PhysicsWorld physicsWorld, PhysicsDebugger debugger)
            : this(shape, physicsWorld, debugger, CollisionFlags.None)
        {
        }

        public PhysicsCollisionObject(PhysicsCollisionShape shape, PhysicsWorld physicsWorld, PhysicsDebugger debugger, 
                                      CollisionFlags collisionFlags)
            : this(shape, physicsWorld, debugger, collisionFlags, CollisionFilterGroups.AllFilter, CollisionFilterGroups.DefaultFilter)
        {
        }

        public PhysicsCollisionObject(PhysicsCollisionShape shape, PhysicsWorld physicsWorld, PhysicsDebugger debugger, 
                                      CollisionFlags collisionFlags, 
                                      CollisionFilterGroups collisionMask)
            : this(shape, physicsWorld, debugger, collisionFlags, collisionMask, CollisionFilterGroups.DefaultFilter)
        {
        }

        public PhysicsCollisionObject(PhysicsCollisionShape shape, PhysicsWorld physicsWorld, PhysicsDebugger debugger, 
                                      CollisionFlags collisionFlags, 
                                      CollisionFilterGroups collisionMask, 
                                      CollisionFilterGroups belongGroups)
        {
            _collisionShape = shape;
            _physicsWorld = physicsWorld;
            _debugger = debugger;

            _collisionFlags = collisionFlags;
            _collisionMask = collisionMask;
            _groupsIBelongTo = belongGroups;

            BuildCollisionObject();
        }

        public virtual Object Clone()
        {
            PhysicsCollisionShape shapeClone = (PhysicsCollisionShape)_collisionShape.Clone();
            var behavior = new PhysicsCollisionObject(shapeClone, _physicsWorld, _debugger, _collisionFlags, _collisionMask, _groupsIBelongTo);
            return behavior;
        }

        public virtual void OnStart(NetworkGameObject go)
        {
            NetworkGameObject = go;
            _collisionObject.Tag = NetworkGameObject;
            UpdateTransformFromGameObject();

            AddObjectToBulletWorld();
        }

        public virtual void Update(float dt)
        {
        }

        public virtual void OnDestroy()
        {
            RemoveObjectFromBulletWorld();

            PhysicsUtilities.DisposeMember(ref _collisionShape);
            //PhysicsUtilities.DisposeMember(ref _collisionObject);
        }

        protected void UpdateTransformFromGameObject()
        {
            _collisionObject.Position = NetworkGameObject.Transform.Position;
        }

        public virtual void AddOnCollisionCallbackEventHandler(ICollisionCallbackEventHandler callback)
        {
            if(CollisionCallbackEventHandler != null)
            {
                _debugger.LogError("PhysicsCollisionObject already has a collision callback. You must remove it before adding another.");
            }
            CollisionCallbackEventHandler = callback;
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
                if(_isInWorld && _physicsWorld != null)
                {
                    _physicsWorld.RemoveCollisionObject(_collisionObject);
                }
            }

            if(_collisionShape == null)
            {
                _debugger.LogError("There was no collision shape component attached to this PhysicsCollisionObject");
                return false;
            }

            Shape cs = _collisionShape.GetCollisionShape();

            if(_collisionObject == null)
            {
                _collisionObject = new RigidBody(cs);
            }
            //_collisionObject.CollisionShape = cs;
            //_collisionObject.UserObject = this;
            //_collisionObject.CollisionFlags = _collisionFlags;

            return true;
        }

        protected virtual void AddObjectToBulletWorld()
        {
            if(!_isInWorld)
            {
                _physicsWorld.AddCollisionObject(_collisionObject, _groupsIBelongTo, _collisionMask);
                _isInWorld = true;
            }
        }

        protected virtual void RemoveObjectFromBulletWorld()
        {
            if(_isInWorld)
            {
                _physicsWorld.RemoveCollisionObject(_collisionObject);
                _isInWorld = false;
            }
        }
    }
}
