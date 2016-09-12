using System;
using System.Collections;
using BulletSharp;
using BulletSharp.Math;
using BulletSharp.SoftBody;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public class PhysicsWorld : INetworkServerSceneBehaviour, IDisposable
    {
        public enum WorldType
        {
            CollisionOnly,
            RigidBodyDynamics,
            MultiBodyWorld,
            SoftBodyAndRigidBody,
        }

        public enum CollisionConfType
        {
            DefaultDynamicsWorldCollisionConf,
            SoftBodyRigidBodyCollisionConf,
        }

        public enum BroadphaseType
        {
            DynamicAABBBroadphase,
            Axis3SweepBroadphase,
            Axis3SweepBroadphase_32bit,
            SimpleBroadphase,
        }

        const int axis3SweepMaxProxies = 32766;

        PhysicsDebugger _debugger;

        public PhysicsWorld(PhysicsDebugger debugger, PhysicsWorldLateHelper lateHelper)
        {
            _debugger = debugger;
            lateUpdateHelper = lateHelper;
        }

        //[SerializeField]
        protected DebugDrawModes _debugDrawMode = DebugDrawModes.DrawWireframe;

        public DebugDrawModes DebugDrawMode
        {
            get { return _debugDrawMode; }
            set
            {
                _debugDrawMode = value;
                if(_doDebugDraw && _world != null && _world.DebugDrawer != null)
                {
                    _world.DebugDrawer.DebugMode = value;
                }
            }
        }

        //[SerializeField]
        protected bool _doDebugDraw = false;

        public bool DoDebugDraw
        {
            get { return _doDebugDraw; }
            set
            {
                if(_doDebugDraw != value && _world != null)
                {
                    if(value == true)
                    {
                        _debugger.DebugMode = _debugDrawMode;
                        _world.DebugDrawer = _debugger;
                    }
                    else
                    {
                        IDebugDraw db = _world.DebugDrawer;
                        if(db != null && db is IDisposable)
                        {
                            ((IDisposable)db).Dispose();
                        }
                        _world.DebugDrawer = null;
                    }
                }
                _doDebugDraw = value;
            }
        }

        //[SerializeField]
        WorldType _worldType = WorldType.RigidBodyDynamics;

        public WorldType worldType
        {
            get { return _worldType; }
            set
            {
                if(value != _worldType && _world != null)
                {
                    _debugger.LogError("Can't modify a Physics World after simulation has started");
                    return;
                }
                _worldType = value;
            }
        }

        //[SerializeField]
        CollisionConfType _collisionType = CollisionConfType.DefaultDynamicsWorldCollisionConf;

        public CollisionConfType collisionType
        {
            get { return _collisionType; }
            set
            {
                if(value != _collisionType && _world != null)
                {
                    _debugger.LogError(debugType, "Can't modify a Physics World after simulation has started");
                    return;
                }
                _collisionType = value;
            }
        }

        //[SerializeField]
        BroadphaseType _broadphaseType = BroadphaseType.DynamicAABBBroadphase;

        public BroadphaseType broadphaseType
        {
            get { return _broadphaseType; }
            set
            {
                if(value != _broadphaseType && _world != null)
                {
                    _debugger.LogError(debugType, "Can't modify a Physics World after simulation has started");
                    return;
                }
                _broadphaseType = value;
            }
        }

        //[SerializeField]
        Vector3 _axis3SweepBroadphaseMin = new Vector3(-1000f, -1000f, -1000f);

        public Vector3 axis3SweepBroadphaseMin
        {
            get { return _axis3SweepBroadphaseMin; }
            set
            {
                if(value != _axis3SweepBroadphaseMin && _world != null)
                {
                    _debugger.LogError(debugType, "Can't modify a Physics World after simulation has started");
                    return;
                }
                _axis3SweepBroadphaseMin = value;
            }
        }

        //[SerializeField]
        Vector3 _axis3SweepBroadphaseMax = new Vector3(1000f, 1000f, 1000f);

        public Vector3 axis3SweepBroadphaseMax
        {
            get { return _axis3SweepBroadphaseMax; }
            set
            {
                if(value != _axis3SweepBroadphaseMax && _world != null)
                {
                    _debugger.LogError(debugType, "Can't modify a Physics World after simulation has started");
                    return;
                }
                _axis3SweepBroadphaseMax = value;
            }
        }

        //[SerializeField]
        Vector3 _gravity = new Vector3(0f, -9.8f, 0f);

        public Vector3 gravity
        {
            get { return _gravity; }
            set
            {
                if(_ddWorld != null)
                {
                    Vector3 grav = value;
                    _ddWorld.SetGravity(ref grav);
                }
                _gravity = value;
            }
        }

        //[SerializeField]
        float _fixedTimeStep = 1f / 60f;

        public float fixedTimeStep
        {
            get
            {
                return _fixedTimeStep;
            }
            set
            {
                if(lateUpdateHelper != null)
                {
                    lateUpdateHelper._fixedTimeStep = value;
                }
                _fixedTimeStep = value;
            }
        }

        //[SerializeField]
        int _maxSubsteps = 3;

        public int maxSubsteps
        {
            get
            {
                return _maxSubsteps;
            }
            set
            {
                if(lateUpdateHelper != null)
                {
                    lateUpdateHelper._maxSubsteps = value;
                }
                _maxSubsteps = value;
            }
        }

        public PhysicsDebugger.DebugType debugType;

        /*
        [SerializeField]
        bool _doCollisionCallbacks = true;
        public bool doCollisionCallbacks
        {
            get { return _doCollisionCallbacks; }
            set { _doCollisionCallbacks = value; }
        }
        */

        PhysicsWorldLateHelper lateUpdateHelper;

        CollisionConfiguration CollisionConf;
        CollisionDispatcher Dispatcher;
        BroadphaseInterface Broadphase;
        SoftBodyWorldInfo softBodyWorldInfo;
        SequentialImpulseConstraintSolver Solver;
        //GhostPairCallback ghostPairCallback = null;
        ulong sequentialImpulseConstraintSolverRandomSeed = 12345;



        CollisionWorld _world;

        public CollisionWorld world
        {
            get { return _world; }
            set { _world = value; }
        }

        private DiscreteDynamicsWorld _ddWorld;
        // convenience variable so we arn't typecasting all the time.

        public int frameCount
        {
            get
            {
                if(lateUpdateHelper != null)
                {
                    return lateUpdateHelper._frameCount;
                }
                else
                {
                    return -1;
                }
            }
        }

        public float timeStr;

        public void RegisterCollisionCallbackListener(PhysicsCollisionObject.ICollisionCallbackEventHandler toBeAdded)
        {
            if(lateUpdateHelper != null)
                lateUpdateHelper.RegisterCollisionCallbackListener(toBeAdded);
        }

        public void DeregisterCollisionCallbackListener(PhysicsCollisionObject.ICollisionCallbackEventHandler toBeRemoved)
        {
            if(lateUpdateHelper != null)
                lateUpdateHelper.DeregisterCollisionCallbackListener(toBeRemoved);
        }

        public void DrawGizmos()
        {
            if(_doDebugDraw && _world != null)
            {
                _world.DebugDrawWorld();
            }
        }

        //It is critical that Awake be called before any other scripts call BPhysicsWorld.Get()
        //Set this script and any derived classes very early in script execution order.
        public virtual void Awake()
        {
            InitializePhysicsWorld();
        }

        protected virtual void OnDestroy()
        {
            _debugger.Log(debugType, "Destroying Physics World");
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Update(float dt, NetworkScene scene, NetworkScene oldScene)
        {
            lateUpdateHelper.Update(dt);
            if(DoDebugDraw)
            {
                DrawGizmos();
            }
        }

        public void OnClientConnected(byte clientId)
        {
        }

        public void OnClientDisconnected(byte clientId)
        {
        }

        public void AddAction(IAction action)
        {
            if(_worldType < WorldType.RigidBodyDynamics)
            {
                _debugger.LogError("World type must not be collision only");
            }
            else
            {
                ((DynamicsWorld)world).AddAction(action);
            }
        }

        public void RemoveAction(IAction action)
        {
            if(_worldType < WorldType.RigidBodyDynamics)
            {
                _debugger.LogError(debugType, "World type must not be collision only");
            }
            ((DiscreteDynamicsWorld)_world).RemoveAction(action);
        }

        public void AddCollisionObject(PhysicsCollisionObject co)
        {
            if(debugType >= PhysicsDebugger.DebugType.Debug)
                _debugger.LogFormat("Adding collision object {0} to world", co);
            if(co._BuildCollisionObject())
            {
                _world.AddCollisionObject(co.GetCollisionObject(), co.groupsIBelongTo, co.collisionMask);
                co.isInWorld = true;
                /*
                if(ghostPairCallback == null && co is BGhostObject && world is DynamicsWorld)
                {
                    ghostPairCallback = new GhostPairCallback();
                    ((DynamicsWorld)world).PairCache.SetInternalGhostPairCallback(ghostPairCallback);
                }
                if(co is BCharacterController && world is DynamicsWorld)
                {
                    AddAction(((BCharacterController)co).GetKinematicCharacterController());
                }
                //*/
            }
               
        }

        public void RemoveCollisionObject(BulletSharp.CollisionObject co)
        {
            if(debugType >= PhysicsDebugger.DebugType.Debug)
                _debugger.LogFormat("Removing collisionObject {0} from world", co.UserObject);
            _world.RemoveCollisionObject(co);
            if(co.UserObject is PhysicsCollisionObject)
                ((PhysicsCollisionObject)co.UserObject).isInWorld = false;
            //TODO handle removing kinematic character controller action
        }

        public void AddRigidBody(PhysicsRigidBody rb)
        {
            if(_worldType < WorldType.RigidBodyDynamics)
            {
                _debugger.LogError(debugType, "World type must not be collision only");
            }
            if(debugType >= PhysicsDebugger.DebugType.Debug)
                _debugger.LogFormat("Adding rigidbody {0} to world", rb);
            if(rb._BuildCollisionObject())
            {
                ((DiscreteDynamicsWorld)_world).AddRigidBody((RigidBody)rb.GetCollisionObject(), rb.groupsIBelongTo, rb.collisionMask);
                rb.isInWorld = true;
            }
        }

        public void RemoveRigidBody(BulletSharp.RigidBody rb)
        {
            if(_worldType < WorldType.RigidBodyDynamics)
            {
                _debugger.LogError(debugType, "World type must not be collision only");
            }
            if(debugType >= PhysicsDebugger.DebugType.Debug)
                _debugger.LogFormat("Removing rigidbody {0} from world", rb.UserObject);
            ((DiscreteDynamicsWorld)_world).RemoveRigidBody(rb);
            if(rb.UserObject is PhysicsCollisionObject)
                ((PhysicsCollisionObject)rb.UserObject).isInWorld = false;
        }

        protected virtual void InitializePhysicsWorld()
        {
            if(_worldType == WorldType.SoftBodyAndRigidBody && _collisionType == CollisionConfType.DefaultDynamicsWorldCollisionConf)
            {
                _debugger.LogError(debugType, "For World Type = SoftBodyAndRigidBody collisionType must be collisionType=SoftBodyRigidBodyCollisionConf. Switching");
                _collisionType = CollisionConfType.SoftBodyRigidBodyCollisionConf;
            }

            if(_collisionType == CollisionConfType.DefaultDynamicsWorldCollisionConf)
            {
                CollisionConf = new DefaultCollisionConfiguration();
            }
            else if(_collisionType == CollisionConfType.SoftBodyRigidBodyCollisionConf)
            {
                CollisionConf = new SoftBodyRigidBodyCollisionConfiguration();
            }

            Dispatcher = new CollisionDispatcher(CollisionConf);

            if(_broadphaseType == BroadphaseType.DynamicAABBBroadphase)
            {
                Broadphase = new DbvtBroadphase();
            }
            else if(_broadphaseType == BroadphaseType.Axis3SweepBroadphase)
            {
                Broadphase = new AxisSweep3(_axis3SweepBroadphaseMin, _axis3SweepBroadphaseMax, axis3SweepMaxProxies);
            }
            else if(_broadphaseType == BroadphaseType.Axis3SweepBroadphase_32bit)
            {
                Broadphase = new AxisSweep3_32Bit(_axis3SweepBroadphaseMin, _axis3SweepBroadphaseMax, axis3SweepMaxProxies);
            }
            else
            {
                Broadphase = null;
            }

            if(_worldType == WorldType.CollisionOnly)
            {
                _world = new CollisionWorld(Dispatcher, Broadphase, CollisionConf);
                _ddWorld = null;
            }
            else if(_worldType == WorldType.RigidBodyDynamics)
            {
                _world = new DiscreteDynamicsWorld(Dispatcher, Broadphase, null, CollisionConf);
                _ddWorld = (DiscreteDynamicsWorld)_world;
            }
            else if(_worldType == WorldType.MultiBodyWorld)
            {
                _world = new MultiBodyDynamicsWorld(Dispatcher, Broadphase, null, CollisionConf);
                _ddWorld = (DiscreteDynamicsWorld)_world;
            }
            else if(_worldType == WorldType.SoftBodyAndRigidBody)
            {
                Solver = new SequentialImpulseConstraintSolver();
                Solver.RandSeed = sequentialImpulseConstraintSolverRandomSeed;
                softBodyWorldInfo = new SoftBodyWorldInfo {
                    AirDensity = 1.2f,
                    WaterDensity = 0,
                    WaterOffset = 0,
                    WaterNormal = Vector3.Zero,
                    Gravity = _gravity,
                    Dispatcher = Dispatcher,
                    Broadphase = Broadphase
                };
                softBodyWorldInfo.SparseSdf.Initialize();

                _world = new SoftRigidDynamicsWorld(Dispatcher, Broadphase, Solver, CollisionConf);
                _ddWorld = (DiscreteDynamicsWorld)_world;

                _world.DispatchInfo.EnableSpu = true;
                softBodyWorldInfo.SparseSdf.Reset();
                softBodyWorldInfo.AirDensity = 1.2f;
                softBodyWorldInfo.WaterDensity = 0;
                softBodyWorldInfo.WaterOffset = 0;
                softBodyWorldInfo.WaterNormal = Vector3.Zero;
                softBodyWorldInfo.Gravity = _gravity;
            }
            if(_ddWorld != null)
            {
                _ddWorld.Gravity = _gravity;
            }
            if(_doDebugDraw)
            {
                _debugger.DebugMode = _debugDrawMode;
                _world.DebugDrawer = _debugger;
            }

            //Add a BPhysicsWorldLateHelper component to call FixedUpdate
            /*lateUpdateHelper = GetComponent<PhysicsWorldLateHelper>();
            if(lateUpdateHelper == null)
            {
                lateUpdateHelper = gameObject.AddComponent<PhysicsWorldLateHelper>();
            }*/
            lateUpdateHelper._world = _world;
            lateUpdateHelper._ddWorld = _ddWorld;
            lateUpdateHelper._physicsWorld = this;
            lateUpdateHelper._frameCount = 0;
            lateUpdateHelper._lastSimulationStepTime = 0;
        }

        protected void Dispose(bool disposing)
        {
            if(debugType >= PhysicsDebugger.DebugType.Debug)
                _debugger.Log("BDynamicsWorld Disposing physics.");

            if(lateUpdateHelper != null)
            {
                lateUpdateHelper._ddWorld = null;
                lateUpdateHelper._world = null;
            }
            if(_world != null)
            {
                //remove/dispose constraints
                int i;
                /*
                if(_ddWorld != null)
                {
                    if(debugType >= PhysicsDebugger.DebugType.Debug)
                        _debugger.LogFormat("Removing Constraints {0}", _ddWorld.NumConstraints);
                    for(i = _ddWorld.NumConstraints - 1; i >= 0; i--)
                    {
                        TypedConstraint constraint = _ddWorld.GetConstraint(i);
                        _ddWorld.RemoveConstraint(constraint);
                        if(constraint.Userobject is BTypedConstraint)
                            ((BTypedConstraint)constraint.Userobject)._isInWorld = false;
                        if(debugType >= PhysicsDebugger.DebugType.Debug)
                            _debugger.LogFormat("Removed Constaint {0}", constraint.Userobject);
                        constraint.Dispose();
                    }
                }
                //*/

                if(debugType >= PhysicsDebugger.DebugType.Debug)
                    _debugger.LogFormat("Removing Collision Objects {0}", _ddWorld.NumCollisionObjects);
                //remove the rigidbodies from the dynamics world and delete them
                for(i = _world.NumCollisionObjects - 1; i >= 0; i--)
                {
                    CollisionObject obj = _world.CollisionObjectArray[i];
                    RigidBody body = obj as RigidBody;
                    if(body != null && body.MotionState != null)
                    {
                        _debugger.Assert(body.NumConstraintRefs == 0, "Rigid body still had constraints");
                        body.MotionState.Dispose();
                    }
                    _world.RemoveCollisionObject(obj);
                    if(obj.UserObject is PhysicsCollisionObject)
                        ((PhysicsCollisionObject)obj.UserObject).isInWorld = false;
                    if(debugType >= PhysicsDebugger.DebugType.Debug)
                        _debugger.LogFormat("Removed CollisionObject {0}", obj.UserObject);
                    obj.Dispose();
                }

                if(_world.DebugDrawer != null)
                {
                    if(_world.DebugDrawer is IDisposable)
                    {
                        IDisposable dis = (IDisposable)_world.DebugDrawer;
                        dis.Dispose();
                    }
                }

                _world.Dispose();
                Broadphase.Dispose();
                Dispatcher.Dispose();
                CollisionConf.Dispose();
                _ddWorld = null;
                _world = null;
            }

            if(Broadphase != null)
            {
                Broadphase.Dispose();
                Broadphase = null;
            }
            if(Dispatcher != null)
            {
                Dispatcher.Dispose();
                Dispatcher = null;
            }
            if(CollisionConf != null)
            {
                CollisionConf.Dispose();
                CollisionConf = null;
            }
            if(Solver != null)
            {
                Solver.Dispose();
                Solver = null;
            }
            if(softBodyWorldInfo != null)
            {
                softBodyWorldInfo.Dispose();
                softBodyWorldInfo = null;
            }
        }
    }

    public class PhysicsDefaultCollisionHandler
    {
        HashSet<PhysicsCollisionObject.ICollisionCallbackEventHandler> collisionCallbackListeners = new HashSet<PhysicsCollisionObject.ICollisionCallbackEventHandler>();

        public void RegisterCollisionCallbackListener(PhysicsCollisionObject.ICollisionCallbackEventHandler toBeAdded)
        {
            collisionCallbackListeners.Add(toBeAdded);
        }

        public void DeregisterCollisionCallbackListener(PhysicsCollisionObject.ICollisionCallbackEventHandler toBeRemoved)
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
            foreach(PhysicsCollisionObject.ICollisionCallbackEventHandler coeh in collisionCallbackListeners)
            {
                if(coeh != null)
                    coeh.OnFinishedVisitingManifolds();
            }
        }
    }
}
