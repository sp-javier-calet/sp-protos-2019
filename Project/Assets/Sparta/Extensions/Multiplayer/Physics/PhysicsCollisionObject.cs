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

        protected PhysicsDebugger _debugger;

        ICollisionCallbackEventHandler _onCollisionCallback;

        public PhysicsCollisionObject(PhysicsCollisionShape shape, PhysicsDebugger debugger, CollisionFlags collisionFlags)
        {
            _debugger = debugger;
            _collisionShape = shape;
            _collisionFlags = collisionFlags;
            BuildCollisionObject();
        }

        public virtual void OnStart(NetworkGameObject go)
        {
            NetworkGameObject = go;
            _collisionObject.WorldTransform = NetworkGameObject.Transform.WorldToLocalMatrix();

            AddObjectToBulletWorld();
        }

        public virtual CollisionObject CollisionObject
        {
            get
            {
                return _collisionObject;
            }
        }

        public PhysicsCollisionShape CollisionShape
        {
            get
            {
                return _collisionShape;
            }
            set
            {
                _collisionShape = value;
            }
        }

        public PhysicsWorld PhysicsWorld
        {
            get;
            set;
        }

        protected CollisionObject _collisionObject;
        protected PhysicsCollisionShape _collisionShape;
        internal bool IsInWorld = false;
        //[SerializeField]
        protected BulletSharp.CollisionFlags _collisionFlags = BulletSharp.CollisionFlags.None;
        //[SerializeField]
        protected BulletSharp.CollisionFilterGroups _groupsIBelongTo = BulletSharp.CollisionFilterGroups.DefaultFilter;
        // A bitmask
        //[SerializeField]
        protected BulletSharp.CollisionFilterGroups _collisionMask = BulletSharp.CollisionFilterGroups.AllFilter;
        // A colliding object must match this mask in order to collide with me.

        public BulletSharp.CollisionFlags collisionFlags
        {
            get { return _collisionFlags; }
            set
            {
                _collisionFlags = value;
                if(_collisionObject != null && value != _collisionFlags)
                {
                    _collisionObject.CollisionFlags = value;
                }
            }
        }

        public BulletSharp.CollisionFilterGroups groupsIBelongTo
        {
            get { return _groupsIBelongTo; }
            set
            {
                if(_collisionObject != null && value != _groupsIBelongTo)
                {
                    _debugger.LogError("Cannot change the collision group once a collision object has been created");
                }
                else
                {
                    _groupsIBelongTo = value;
                }
            }
        }

        public BulletSharp.CollisionFilterGroups collisionMask
        {
            get { return _collisionMask; }
            set
            {
                if(_collisionObject != null && value != _collisionMask)
                {
                    _debugger.LogError("Cannot change the collision mask once a collision object has been created");
                }
                else
                {
                    _collisionMask = value;
                }
            }
        }

        public virtual ICollisionCallbackEventHandler collisionCallbackEventHandler
        {
            get { return _onCollisionCallback; }
        }

        public virtual void AddOnCollisionCallbackEventHandler(ICollisionCallbackEventHandler myCallback)
        {
            PhysicsWorld bhw = PhysicsWorld;
            if(_onCollisionCallback != null)
            {
                _debugger.LogError("PhysicsCollisionObject already has a collision callback. You must remove it before adding another.");
            }
            _onCollisionCallback = myCallback;
            bhw.RegisterCollisionCallbackListener(_onCollisionCallback);
        }

        public virtual void RemoveOnCollisionCallbackEventHandler()
        {
            PhysicsWorld bhw = PhysicsWorld;
            if(bhw != null && _onCollisionCallback != null)
            {
                bhw.DeregisterCollisionCallbackListener(_onCollisionCallback);
            }
            _onCollisionCallback = null;
        }

        protected virtual bool BuildCollisionObject()
        {
            PhysicsWorld world = PhysicsWorld;
            if(_collisionObject != null)
            {
                if(IsInWorld && world != null)
                {
                    world.RemoveCollisionObject(_collisionObject);
                }
            }

            _collisionShape = CollisionShape;
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
            _collisionObject.CollisionFlags = _collisionFlags;

            return true;
        }

        protected virtual void AddObjectToBulletWorld()
        {
            PhysicsWorld.AddCollisionObject(this);
        }

        protected virtual void RemoveObjectFromBulletWorld()
        {
            PhysicsWorld.RemoveCollisionObject(_collisionObject);
        }

        public virtual void Update(float dt)
        {
            //TODO: Do only on physics step? use dirty? try to reduce the number of matrix creations
            _collisionObject.WorldTransform = NetworkGameObject.Transform.WorldToLocalMatrix();
        }

        public virtual void OnDestroy()
        {
            if(IsInWorld && _collisionObject != null)//&& isdisposing
            {
                PhysicsWorld pw = PhysicsWorld;
                if(pw != null && pw.world != null)
                {
                    ((DiscreteDynamicsWorld)pw.world).RemoveCollisionObject(_collisionObject);
                }
            }
            PhysicsUtilities.DisposeMember(ref _collisionShape);
            PhysicsUtilities.DisposeMember(ref _collisionObject);
        }

        public Object Clone()
        {
            //TODO: Improve Clone
            var behavior = new PhysicsCollisionObject(_collisionShape, _debugger, _collisionFlags);
            behavior.NetworkGameObject = NetworkGameObject;
            behavior.PhysicsWorld = PhysicsWorld;
            behavior._collisionObject = _collisionObject;
            //behavior.IsInWorld = IsInWorld;
            behavior._onCollisionCallback = _onCollisionCallback;
            return behavior;
        }

        public virtual void SetPosition(Vector3 position)
        {
            if(IsInWorld)
            {
                Matrix newTrans = _collisionObject.WorldTransform;
                newTrans.Origin = position;
                _collisionObject.WorldTransform = newTrans;
                NetworkGameObject.Transform.Position = position;
            }
            else
            {
                NetworkGameObject.Transform.Position = position;
            }
        }

        public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            if(IsInWorld)
            {
                Matrix newTrans = _collisionObject.WorldTransform;
                Quaternion q = rotation;
                Matrix.RotationQuaternion(ref q, out newTrans);
                newTrans.Origin = NetworkGameObject.Transform.Position;
                _collisionObject.WorldTransform = newTrans;
                NetworkGameObject.Transform.Position = position;
                NetworkGameObject.Transform.Rotation = rotation;
            }
            else
            {
                NetworkGameObject.Transform.Position = position;
                NetworkGameObject.Transform.Rotation = rotation;
            }
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
                NetworkGameObject.Transform.Rotation = rotation;
            }
            else
            {
                NetworkGameObject.Transform.Rotation = rotation;
            }
        }

    }
}
