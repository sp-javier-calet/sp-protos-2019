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
                return m_collisionShape;
            }
            set
            {
                m_collisionShape = value;
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
        protected bool m_startHasBeenCalled = false;

        protected CollisionObject m_collisionObject;
        protected PhysicsCollisionShape m_collisionShape;
        internal bool isInWorld = false;
        //[SerializeField]
        protected BulletSharp.CollisionFlags m_collisionFlags = BulletSharp.CollisionFlags.None;
        //[SerializeField]
        protected BulletSharp.CollisionFilterGroups m_groupsIBelongTo = BulletSharp.CollisionFilterGroups.DefaultFilter;
        // A bitmask
        //[SerializeField]
        protected BulletSharp.CollisionFilterGroups m_collisionMask = BulletSharp.CollisionFilterGroups.AllFilter;
        // A colliding object must match this mask in order to collide with me.

        public BulletSharp.CollisionFlags collisionFlags
        {
            get { return m_collisionFlags; }
            set
            {
                m_collisionFlags = value;
                if(m_collisionObject != null && value != m_collisionFlags)
                {
                    m_collisionObject.CollisionFlags = value;
                }
            }
        }

        public BulletSharp.CollisionFilterGroups groupsIBelongTo
        {
            get { return m_groupsIBelongTo; }
            set
            {
                if(m_collisionObject != null && value != m_groupsIBelongTo)
                {
                    _debugger.LogError("Cannot change the collision group once a collision object has been created");
                }
                else
                {
                    m_groupsIBelongTo = value;
                }
            }
        }

        public BulletSharp.CollisionFilterGroups collisionMask
        {
            get { return m_collisionMask; }
            set
            {
                if(m_collisionObject != null && value != m_collisionMask)
                {
                    _debugger.LogError("Cannot change the collision mask once a collision object has been created");
                }
                else
                {
                    m_collisionMask = value;
                }
            }
        }

        BICollisionCallbackEventHandler m_onCollisionCallback;

        public virtual BICollisionCallbackEventHandler collisionCallbackEventHandler
        {
            get { return m_onCollisionCallback; }
        }

        public virtual void AddOnCollisionCallbackEventHandler(BICollisionCallbackEventHandler myCallback)
        {
            PhysicsWorld bhw = PhysicsWorld;
            if(m_onCollisionCallback != null)
            {
                _debugger.LogErrorFormat("BCollisionObject {0} already has a collision callback. You must remove it before adding another. ", GameObject.Id);

            }
            m_onCollisionCallback = myCallback;
            bhw.RegisterCollisionCallbackListener(m_onCollisionCallback);
        }

        public virtual void RemoveOnCollisionCallbackEventHandler()
        {
            PhysicsWorld bhw = PhysicsWorld;
            if(bhw != null && m_onCollisionCallback != null)
            {
                bhw.DeregisterCollisionCallbackListener(m_onCollisionCallback);
            }
            m_onCollisionCallback = null;
        }

        //called by Physics World just before rigid body is added to world.
        //the current rigid body properties are used to rebuild the rigid body.
        internal virtual bool _BuildCollisionObject()
        {
            PhysicsWorld world = PhysicsWorld;
            if(m_collisionObject != null)
            {
                if(isInWorld && world != null)
                {
                    world.RemoveCollisionObject(m_collisionObject);
                }
            }

            /*if(GameObject.Transform.localScale != UnityEngine.Vector3.one)
            {
                _debugger.LogError("The local scale on this collision shape is not one. Bullet physics does not support scaling on a rigid body world transform. Instead alter the dimensions of the CollisionShape.");
            }*/

            m_collisionShape = CollisionShape;
            if(m_collisionShape == null)
            {
                _debugger.LogError("There was no collision shape component attached to this BRigidBody. " + GameObject.Id);
                return false;
            }

            CollisionShape cs = m_collisionShape.GetCollisionShape();
            //rigidbody is dynamic if and only if mass is non zero, otherwise static


            if(m_collisionObject == null)
            {
                m_collisionObject = new CollisionObject();
                m_collisionObject.CollisionShape = cs;
                m_collisionObject.UserObject = this;

                Matrix worldTrans;
                Quaternion q = GameObject.Transform.Rotation;
                Matrix.RotationQuaternion(ref q, out worldTrans);
                worldTrans.Origin = GameObject.Transform.Position;
                m_collisionObject.WorldTransform = worldTrans;
                m_collisionObject.CollisionFlags = m_collisionFlags;
            }
            else
            {
                m_collisionObject.CollisionShape = cs;
                Matrix worldTrans;
                Quaternion q = GameObject.Transform.Rotation;
                Matrix.RotationQuaternion(ref q, out worldTrans);
                worldTrans.Origin = GameObject.Transform.Position;
                m_collisionObject.WorldTransform = worldTrans;
                m_collisionObject.CollisionFlags = m_collisionFlags;
            }
            return true;
        }

        public virtual CollisionObject GetCollisionObject()
        {
            if(m_collisionObject == null)
            {
                _BuildCollisionObject();
            }
            return m_collisionObject;
        }

        //Don't try to call functions on other objects such as the Physics world since they may not exit.
        protected virtual void Awake()
        {
            m_collisionShape = CollisionShape;
            if(m_collisionShape == null)
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
            PhysicsWorld.RemoveCollisionObject(m_collisionObject);
        }


        //Add this object to the world on Start. We are doing this so that scripts which add this componnet to
        //game objects have a chance to configure them before the object is added to the bullet world.
        //Be aware that Start is not affected by script execution order so objects such as constraints should
        //make sure that objects they depend on have been added to the world before they add themselves.
        public virtual void Start()
        {
            m_startHasBeenCalled = true;
            AddObjectToBulletWorld();
        }

        //OnEnable and OnDisable are called when a game object is Activated and Deactivated.
        //Unfortunately the first call comes before Awake and Start. We suppress this call so that the component
        //has a chance to initialize itself. Objects that depend on other objects such as constraints should make
        //sure those objects have been added to the world first.
        //don't try to call functions on world before Start is called. It may not exist.
        public virtual void OnEnable()
        {
            if(!isInWorld && m_startHasBeenCalled)
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
            if(isInWorld && isdisposing && m_collisionObject != null)
            {
                PhysicsWorld pw = PhysicsWorld;
                if(pw != null && pw.world != null)
                {
                    ((DiscreteDynamicsWorld)pw.world).RemoveCollisionObject(m_collisionObject);
                }
            }
            if(m_collisionObject != null)
            {

                m_collisionObject.Dispose();
                m_collisionObject = null;
            }
        }

        public virtual void SetPosition(Vector3 position)
        {
            if(isInWorld)
            {
                Matrix newTrans = m_collisionObject.WorldTransform;
                newTrans.Origin = position;
                m_collisionObject.WorldTransform = newTrans;
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
                Matrix newTrans = m_collisionObject.WorldTransform;
                Quaternion q = rotation;
                Matrix.RotationQuaternion(ref q, out newTrans);
                newTrans.Origin = GameObject.Transform.Position;
                m_collisionObject.WorldTransform = newTrans;
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
                Matrix newTrans = m_collisionObject.WorldTransform;
                Quaternion q = rotation;
                Matrix.RotationQuaternion(ref q, out newTrans);
                newTrans.Origin = GameObject.Transform.Position;
                m_collisionObject.WorldTransform = newTrans;
                GameObject.Transform.Rotation = rotation;
            }
            else
            {
                GameObject.Transform.Rotation = rotation;
            }
        }

    }
}
