using System;
using System.Collections;
using BulletSharp;
using BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    public class PhysicsCollisionObject : IDisposable
    {
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

        public NetworkGameObject GameObject
        {
            get;
            set;
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

        public interface BICollisionCallbackEventHandler
        {
            void OnVisitPersistentManifold(PersistentManifold pm);

            void OnFinishedVisitingManifolds();
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

        BICollisionCallbackEventHandler _onCollisionCallback;

        public virtual BICollisionCallbackEventHandler collisionCallbackEventHandler
        {
            get { return _onCollisionCallback; }
        }

        public virtual void AddOnCollisionCallbackEventHandler(BICollisionCallbackEventHandler myCallback)
        {
            PhysicsWorld bhw = PhysicsWorld;
            if(_onCollisionCallback != null)
            {
                _debugger.LogErrorFormat("BCollisionObject {0} already has a collision callback. You must remove it before adding another. ", GameObject.Id);

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
                _debugger.LogError("There was no collision shape component attached to this BRigidBody. " + GameObject.Id);
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
                Quaternion q = GameObject.Transform.Rotation;
                Matrix.RotationQuaternion(ref q, out worldTrans);
                worldTrans.Origin = GameObject.Transform.Position;
                _collisionObject.WorldTransform = worldTrans;
                _collisionObject.CollisionFlags = _collisionFlags;
            }
            else
            {
                _collisionObject.CollisionShape = cs;
                Matrix worldTrans;
                Quaternion q = GameObject.Transform.Rotation;
                Matrix.RotationQuaternion(ref q, out worldTrans);
                worldTrans.Origin = GameObject.Transform.Position;
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

        //Don't try to call functions on other objects such as the Physics world since they may not exit.
        protected virtual void Awake()
        {
            _collisionShape = CollisionShape;
            if(_collisionShape == null)
            {
                _debugger.LogError("A BCollisionObject component must be on an object with a PhysicsCollisionShape component.");
            }
        }

        protected virtual void AddObjectToBulletWorld()
        {
            PhysicsWorld.AddCollisionObject(this);
        }

        protected virtual void RemoveObjectFromBulletWorld()
        {
            PhysicsWorld.RemoveCollisionObject(_collisionObject);
        }


        //Add this object to the world on Start. We are doing this so that scripts which add this componnet to
        //game objects have a chance to configure them before the object is added to the bullet world.
        //Be aware that Start is not affected by script execution order so objects such as constraints should
        //make sure that objects they depend on have been added to the world before they add themselves.
        public virtual void Start()
        {
            _startHasBeenCalled = true;
            AddObjectToBulletWorld();
        }

        //OnEnable and OnDisable are called when a game object is Activated and Deactivated.
        //Unfortunately the first call comes before Awake and Start. We suppress this call so that the component
        //has a chance to initialize itself. Objects that depend on other objects such as constraints should make
        //sure those objects have been added to the world first.
        //don't try to call functions on world before Start is called. It may not exist.
        public virtual void OnEnable()
        {
            if(!isInWorld && _startHasBeenCalled)
            {
                AddObjectToBulletWorld();
            }
        }

        // when scene is closed objects, including the physics world, are destroyed in random order.
        // There is no way to distinquish between scene close destruction and normal gameplay destruction.
        // Objects cannot depend on world existing when they Dispose of themselves. World may have been destroyed first.
        public virtual void OnDisable()
        {
            if(isInWorld)
            {
                RemoveObjectFromBulletWorld();
            }
        }

        public virtual void OnDestroy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isdisposing)
        {
            if(isInWorld && isdisposing && _collisionObject != null)
            {
                PhysicsWorld pw = PhysicsWorld;
                if(pw != null && pw.world != null)
                {
                    ((DiscreteDynamicsWorld)pw.world).RemoveCollisionObject(_collisionObject);
                }
            }
            if(_collisionObject != null)
            {

                _collisionObject.Dispose();
                _collisionObject = null;
            }
        }

        public virtual void SetPosition(Vector3 position)
        {
            if(isInWorld)
            {
                Matrix newTrans = _collisionObject.WorldTransform;
                newTrans.Origin = position;
                _collisionObject.WorldTransform = newTrans;
                GameObject.Transform.Position = position;
            }
            else
            {
                GameObject.Transform.Position = position;
            }

        }

        public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            if(isInWorld)
            {
                Matrix newTrans = _collisionObject.WorldTransform;
                Quaternion q = rotation;
                Matrix.RotationQuaternion(ref q, out newTrans);
                newTrans.Origin = GameObject.Transform.Position;
                _collisionObject.WorldTransform = newTrans;
                GameObject.Transform.Position = position;
                GameObject.Transform.Rotation = rotation;
            }
            else
            {
                GameObject.Transform.Position = position;
                GameObject.Transform.Rotation = rotation;
            }
        }

        public virtual void SetRotation(Quaternion rotation)
        {
            if(isInWorld)
            {
                Matrix newTrans = _collisionObject.WorldTransform;
                Quaternion q = rotation;
                Matrix.RotationQuaternion(ref q, out newTrans);
                newTrans.Origin = GameObject.Transform.Position;
                _collisionObject.WorldTransform = newTrans;
                GameObject.Transform.Rotation = rotation;
            }
            else
            {
                GameObject.Transform.Rotation = rotation;
            }
        }

    }
}
