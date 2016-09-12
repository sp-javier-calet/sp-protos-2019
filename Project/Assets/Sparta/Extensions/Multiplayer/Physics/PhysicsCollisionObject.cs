using System;
using System.Collections;
using BulletSharp;
using BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    public class PhysicsCollisionObject : INetworkBehaviour
    {

        public interface ICollisionCallbackEventHandler
        {
            void OnVisitPersistentManifold(PersistentManifold pm);

            void OnFinishedVisitingManifolds();
        }

        ICollisionCallbackEventHandler _onCollisionCallback;

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

        public NetworkGameObject NetworkGameObject
        {
            get;
            set;//TODO: Private set
        }

        public PhysicsWorld PhysicsWorld
        {
            get;
            set;
        }

        //TODO: Set debugger
        protected PhysicsDebugger _debugger;

        public PhysicsDebugger Debugger
        {
            get
            {
                return _debugger;
            }
            set
            {
                _debugger = value;
            }
        }

        //This is used to handle a design problem.
        //We want OnEnable to add physics object to world and OnDisable to remove.
        //We also want user to be able to in script: AddComponent<CollisionObject>, configure it, add it to world, potentialy disable to delay it being added to world
        //Problem is OnEnable gets called before Awake and Start so that developer has no chance to configure object before it is added to world or prevent
        //It from being added.
        //Solution is not to add object to the world until after Start has been called. Start will do the first add to world.
        protected bool _startHasBeenCalled = false;

        protected CollisionObject _collisionObject;
        protected PhysicsCollisionShape _collisionShape;
        internal bool isInWorld = false;
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
                _debugger.LogErrorFormat("BCollisionObject {0} already has a collision callback. You must remove it before adding another. ", NetworkGameObject.Id);
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

        //called by Physics World just before rigid body is added to world.
        //the current rigid body properties are used to rebuild the rigid body.
        internal virtual bool _BuildCollisionObject()
        {
            PhysicsWorld world = PhysicsWorld;
            if(_collisionObject != null)
            {
                if(isInWorld && world != null)
                {
                    world.RemoveCollisionObject(_collisionObject);
                }
            }

            /*if(GameObject.Transform.localScale != UnityEngine.Vector3.one)
            {
                _debugger.LogError("The local scale on this collision shape is not one. Bullet physics does not support scaling on a rigid body world transform. Instead alter the dimensions of the CollisionShape.");
            }*/

            _collisionShape = CollisionShape;
            if(_collisionShape == null)
            {
                _debugger.LogError("There was no collision shape component attached to this BRigidBody. " + NetworkGameObject.Id);
                return false;
            }

            CollisionShape cs = _collisionShape.GetCollisionShape();
            //rigidbody is dynamic if and only if mass is non zero, otherwise static


            if(_collisionObject == null)
            {
                _collisionObject = new CollisionObject();
                _collisionObject.CollisionShape = cs;
                _collisionObject.UserObject = this;

                Matrix worldTrans;
                Quaternion q = NetworkGameObject.Transform.Rotation;
                Matrix.RotationQuaternion(ref q, out worldTrans);
                worldTrans.Origin = NetworkGameObject.Transform.Position;
                _collisionObject.WorldTransform = worldTrans;
                _collisionObject.CollisionFlags = _collisionFlags;
            }
            else
            {
                _collisionObject.CollisionShape = cs;
                Matrix worldTrans;
                Quaternion q = NetworkGameObject.Transform.Rotation;
                Matrix.RotationQuaternion(ref q, out worldTrans);
                worldTrans.Origin = NetworkGameObject.Transform.Position;
                _collisionObject.WorldTransform = worldTrans;
                _collisionObject.CollisionFlags = _collisionFlags;
            }
            return true;
        }

        public virtual CollisionObject GetCollisionObject()
        {
            if(_collisionObject == null)
            {
                _BuildCollisionObject();
            }
            return _collisionObject;
        }

        protected virtual void AddObjectToBulletWorld()
        {
            PhysicsWorld.AddCollisionObject(this);
        }

        protected virtual void RemoveObjectFromBulletWorld()
        {
            PhysicsWorld.RemoveCollisionObject(_collisionObject);
        }


        public virtual void OnStart(NetworkGameObject go)
        {
            NetworkGameObject = go;

            _collisionShape = CollisionShape;
            if(_collisionShape == null)
            {
                _debugger.LogError("A PhysicsCollisionObject component must be on an object with a PhysicsCollisionShape component.");
            }

            _startHasBeenCalled = true;
            AddObjectToBulletWorld();
        }

        public virtual void Update(float dt)
        {
            //TODO: Do only on physics step? use dirty? try to reduce the number of matrix creations
            _collisionObject.WorldTransform = NetworkGameObject.Transform.WorldToLocalMatrix();
        }

        public virtual void OnDestroy()
        {
            if(isInWorld && _collisionObject != null)//&& isdisposing
            {
                PhysicsWorld pw = PhysicsWorld;
                if(pw != null && pw.world != null)
                {
                    ((DiscreteDynamicsWorld)pw.world).RemoveCollisionObject(_collisionObject);
                }
            }
            PhysicsUtilities.Dispose(ref _collisionShape);
            PhysicsUtilities.Dispose(ref _collisionObject);
        }

        public Object Clone()
        {
            //TODO: Improve Clone
            var behavior = new PhysicsCollisionObject();
            behavior.NetworkGameObject = NetworkGameObject;
            behavior.CollisionShape = CollisionShape;
            behavior.PhysicsWorld = PhysicsWorld;
            behavior._debugger = _debugger;
            behavior._collisionObject = _collisionObject;
            behavior._collisionShape = _collisionShape;//Use this or CollisionShape setter/getter?
            behavior.isInWorld = isInWorld;
            behavior._onCollisionCallback = _onCollisionCallback;
            return behavior;
        }

        public virtual void SetPosition(Vector3 position)
        {
            if(isInWorld)
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
            if(isInWorld)
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
            if(isInWorld)
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
