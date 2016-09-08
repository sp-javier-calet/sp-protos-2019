using System;
using System.Collections;
using BulletSharp;
using BulletSharp.Math;
using BulletSharp.SoftBody;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public class PhysicsWorld : IDisposable
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
                if(_doDebugDraw && m_world != null && m_world.DebugDrawer != null)
                {
                    m_world.DebugDrawer.DebugMode = value;
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
                if(_doDebugDraw != value && m_world != null)
                {
                    if(value == true)
                    {
                        _debugger.DebugMode = _debugDrawMode;
                        m_world.DebugDrawer = _debugger;
                    }
                    else
                    {
                        IDebugDraw db = m_world.DebugDrawer;
                        if(db != null && db is IDisposable)
                        {
                            ((IDisposable)db).Dispose();
                        }
                        m_world.DebugDrawer = null;
                    }
                }
                _doDebugDraw = value;
            }
        }

        //[SerializeField]
        WorldType m_worldType = WorldType.RigidBodyDynamics;

        public WorldType worldType
        {
            get { return m_worldType; }
            set
            {
                if(value != m_worldType && m_world != null)
                {
                    _debugger.LogError("Can't modify a Physics World after simulation has started");
                    return;
                }
                m_worldType = value;
            }
        }

        //[SerializeField]
        CollisionConfType m_collisionType = CollisionConfType.DefaultDynamicsWorldCollisionConf;

        public CollisionConfType collisionType
        {
            get { return m_collisionType; }
            set
            {
                if(value != m_collisionType && m_world != null)
                {
                    _debugger.LogError(debugType, "Can't modify a Physics World after simulation has started");
                    return;
                }
                m_collisionType = value;
            }
        }

        //[SerializeField]
        BroadphaseType m_broadphaseType = BroadphaseType.DynamicAABBBroadphase;

        public BroadphaseType broadphaseType
        {
            get { return m_broadphaseType; }
            set
            {
                if(value != m_broadphaseType && m_world != null)
                {
                    _debugger.LogError(debugType, "Can't modify a Physics World after simulation has started");
                    return;
                }
                m_broadphaseType = value;
            }
        }

        //[SerializeField]
        Vector3 m_axis3SweepBroadphaseMin = new Vector3(-1000f, -1000f, -1000f);

        public Vector3 axis3SweepBroadphaseMin
        {
            get { return m_axis3SweepBroadphaseMin; }
            set
            {
                if(value != m_axis3SweepBroadphaseMin && m_world != null)
                {
                    _debugger.LogError(debugType, "Can't modify a Physics World after simulation has started");
                    return;
                }
                m_axis3SweepBroadphaseMin = value;
            }
        }

        //[SerializeField]
        Vector3 m_axis3SweepBroadphaseMax = new Vector3(1000f, 1000f, 1000f);

        public Vector3 axis3SweepBroadphaseMax
        {
            get { return m_axis3SweepBroadphaseMax; }
            set
            {
                if(value != m_axis3SweepBroadphaseMax && m_world != null)
                {
                    _debugger.LogError(debugType, "Can't modify a Physics World after simulation has started");
                    return;
                }
                m_axis3SweepBroadphaseMax = value;
            }
        }

        //[SerializeField]
        Vector3 m_gravity = new Vector3(0f, -9.8f, 0f);

        public Vector3 gravity
        {
            get { return m_gravity; }
            set
            {
                if(_ddWorld != null)
                {
                    Vector3 grav = value;
                    _ddWorld.SetGravity(ref grav);
                }
                m_gravity = value;
            }
        }

        //[SerializeField]
        float m_fixedTimeStep = 1f / 60f;

        public float fixedTimeStep
        {
            get
            {
                return m_fixedTimeStep;
            }
            set
            {
                if(lateUpdateHelper != null)
                {
                    lateUpdateHelper.m_fixedTimeStep = value;
                }
                m_fixedTimeStep = value;
            }
        }

        //[SerializeField]
        int m_maxSubsteps = 3;

        public int maxSubsteps
        {
            get
            {
                return m_maxSubsteps;
            }
            set
            {
                if(lateUpdateHelper != null)
                {
                    lateUpdateHelper.m_maxSubsteps = value;
                }
                m_maxSubsteps = value;
            }
        }

        public PhysicsDebugger.DebugType debugType;

        /*
        [SerializeField]
        bool m_doCollisionCallbacks = true;
        public bool doCollisionCallbacks
        {
            get { return m_doCollisionCallbacks; }
            set { m_doCollisionCallbacks = value; }
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



        CollisionWorld m_world;

        public CollisionWorld world
        {
            get { return m_world; }
            set { m_world = value; }
        }

        private DiscreteDynamicsWorld _ddWorld;
        // convenience variable so we arn't typecasting all the time.

        public int frameCount
        {
            get
            {
                if(lateUpdateHelper != null)
                {
                    return lateUpdateHelper.m__frameCount;
                }
                else
                {
                    return -1;
                }
            }
        }

        public float timeStr;

        public void RegisterCollisionCallbackListener(PhysicsCollisionObject.BICollisionCallbackEventHandler toBeAdded)
        {
            if(lateUpdateHelper != null)
                lateUpdateHelper.RegisterCollisionCallbackListener(toBeAdded);
        }

        public void DeregisterCollisionCallbackListener(PhysicsCollisionObject.BICollisionCallbackEventHandler toBeRemoved)
        {
            if(lateUpdateHelper != null)
                lateUpdateHelper.DeregisterCollisionCallbackListener(toBeRemoved);
        }

        public void OnDrawGizmos()
        {
            if(_doDebugDraw && m_world != null)
            {
                m_world.DebugDrawWorld();
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

        public void AddAction(IAction action)
        {
            if(m_worldType < WorldType.RigidBodyDynamics)
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
            if(m_worldType < WorldType.RigidBodyDynamics)
            {
                _debugger.LogError(debugType, "World type must not be collision only");
            }
            ((DiscreteDynamicsWorld)m_world).RemoveAction(action);
        }

        public void AddCollisionObject(PhysicsCollisionObject co)
        {
            if(debugType >= PhysicsDebugger.DebugType.Debug)
                _debugger.LogFormat("Adding collision object {0} to world", co);
            if(co._BuildCollisionObject())
            {
                m_world.AddCollisionObject(co.GetCollisionObject(), co.groupsIBelongTo, co.collisionMask);
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
            m_world.RemoveCollisionObject(co);
            if(co.UserObject is PhysicsCollisionObject)
                ((PhysicsCollisionObject)co.UserObject).isInWorld = false;
            //TODO handle removing kinematic character controller action
        }

        public void AddRigidBody(BRigidBody rb)
        {
            if(m_worldType < WorldType.RigidBodyDynamics)
            {
                _debugger.LogError(debugType, "World type must not be collision only");
            }
            if(debugType >= PhysicsDebugger.DebugType.Debug)
                _debugger.LogFormat("Adding rigidbody {0} to world", rb);
            if(rb._BuildCollisionObject())
            {
                ((DiscreteDynamicsWorld)m_world).AddRigidBody((RigidBody)rb.GetCollisionObject(), rb.groupsIBelongTo, rb.collisionMask);
                rb.isInWorld = true;
            }
        }

        public void RemoveRigidBody(BulletSharp.RigidBody rb)
        {
            if(m_worldType < WorldType.RigidBodyDynamics)
            {
                _debugger.LogError(debugType, "World type must not be collision only");
            }
            if(debugType >= PhysicsDebugger.DebugType.Debug)
                _debugger.LogFormat("Removing rigidbody {0} from world", rb.UserObject);
            ((DiscreteDynamicsWorld)m_world).RemoveRigidBody(rb);
            if(rb.UserObject is PhysicsCollisionObject)
                ((PhysicsCollisionObject)rb.UserObject).isInWorld = false;
        }

        /*
        public bool AddConstraint(BTypedConstraint c)
        {
            if(m_worldType < WorldType.RigidBodyDynamics)
            {
                B_debugger.LogError(debugType, "World type must not be collision only");
                return false;
            }
            if(debugType >= PhysicsDebugger.DebugType.Debug)
                _debugger.LogFormat("Adding constraint {0} to world", c);
            if(c._BuildConstraint())
            {
                ((DiscreteDynamicsWorld)m_world).AddConstraint(c.GetConstraint(), c.disableCollisionsBetweenConstrainedBodies);
                c.m_isInWorld = true;
            }
        }

        public void RemoveConstraint(BulletSharp.TypedConstraint c)
        {
            if(m_worldType < WorldType.RigidBodyDynamics)
            {
                _debugger.LogError(debugType, "World type must not be collision only");
            }
            if(debugType >= PhysicsDebugger.DebugType.Debug)
                _debugger.LogFormat("Removing constraint {0} from world", c.Userobject);
            ((DiscreteDynamicsWorld)m_world).RemoveConstraint(c);
            if(c.Userobject is BTypedConstraint)
                ((BTypedConstraint)c.Userobject).m_isInWorld = false;
        }
        //*/

        /*
        public bool AddSoftBody(BSoftBody softBody)
        {
            if(!(m_world is BulletSharp.SoftBody.SoftRigidDynamicsWorld))
            {
                if(debugType >= PhysicsDebugger.DebugType.Debug)
                    _debugger.LogFormat("The Physics World must be a BSoftBodyWorld for adding soft bodies");
                return false;
            }
            if(!_isDisposed)
            {
                if(debugType >= PhysicsDebugger.DebugType.Debug)
                    _debugger.LogFormat("Adding softbody {0} to world", softBody);
                if(softBody._BuildCollisionObject())
                {
                    ((BulletSharp.SoftBody.SoftRigidDynamicsWorld)m_world).AddSoftBody((SoftBody)softBody.GetCollisionObject());
                    softBody.isInWorld = true;
                }
                return true;
            }
            return false;
        }

        public void RemoveSoftBody(BulletSharp.SoftBody.SoftBody softBody)
        {
            if(m_world is BulletSharp.SoftBody.SoftRigidDynamicsWorld)
            {
                if(debugType >= PhysicsDebugger.DebugType.Debug)
                    _debugger.LogFormat("Removing softbody {0} from world", softBody.UserObject);
                ((BulletSharp.SoftBody.SoftRigidDynamicsWorld)m_world).RemoveSoftBody(softBody);
                if(softBody.UserObject is PhysicsCollisionObject)
                    ((PhysicsCollisionObject)softBody.UserObject).isInWorld = false;
            }
        }
        //*/

        protected virtual void InitializePhysicsWorld()
        {
            if(m_worldType == WorldType.SoftBodyAndRigidBody && m_collisionType == CollisionConfType.DefaultDynamicsWorldCollisionConf)
            {
                _debugger.LogError(debugType, "For World Type = SoftBodyAndRigidBody collisionType must be collisionType=SoftBodyRigidBodyCollisionConf. Switching");
                m_collisionType = CollisionConfType.SoftBodyRigidBodyCollisionConf;
            }

            if(m_collisionType == CollisionConfType.DefaultDynamicsWorldCollisionConf)
            {
                CollisionConf = new DefaultCollisionConfiguration();
            }
            else if(m_collisionType == CollisionConfType.SoftBodyRigidBodyCollisionConf)
            {
                CollisionConf = new SoftBodyRigidBodyCollisionConfiguration();
            }

            Dispatcher = new CollisionDispatcher(CollisionConf);

            if(m_broadphaseType == BroadphaseType.DynamicAABBBroadphase)
            {
                Broadphase = new DbvtBroadphase();
            }
            else if(m_broadphaseType == BroadphaseType.Axis3SweepBroadphase)
            {
                Broadphase = new AxisSweep3(m_axis3SweepBroadphaseMin, m_axis3SweepBroadphaseMax, axis3SweepMaxProxies);
            }
            else if(m_broadphaseType == BroadphaseType.Axis3SweepBroadphase_32bit)
            {
                Broadphase = new AxisSweep3_32Bit(m_axis3SweepBroadphaseMin, m_axis3SweepBroadphaseMax, axis3SweepMaxProxies);
            }
            else
            {
                Broadphase = null;
            }

            if(m_worldType == WorldType.CollisionOnly)
            {
                m_world = new CollisionWorld(Dispatcher, Broadphase, CollisionConf);
                _ddWorld = null;
            }
            else if(m_worldType == WorldType.RigidBodyDynamics)
            {
                m_world = new DiscreteDynamicsWorld(Dispatcher, Broadphase, null, CollisionConf);
                _ddWorld = (DiscreteDynamicsWorld)m_world;
            }
            else if(m_worldType == WorldType.MultiBodyWorld)
            {
                m_world = new MultiBodyDynamicsWorld(Dispatcher, Broadphase, null, CollisionConf);
                _ddWorld = (DiscreteDynamicsWorld)m_world;
            }
            else if(m_worldType == WorldType.SoftBodyAndRigidBody)
            {
                Solver = new SequentialImpulseConstraintSolver();
                Solver.RandSeed = sequentialImpulseConstraintSolverRandomSeed;
                softBodyWorldInfo = new SoftBodyWorldInfo {
                    AirDensity = 1.2f,
                    WaterDensity = 0,
                    WaterOffset = 0,
                    WaterNormal = Vector3.Zero,
                    Gravity = m_gravity,
                    Dispatcher = Dispatcher,
                    Broadphase = Broadphase
                };
                softBodyWorldInfo.SparseSdf.Initialize();

                m_world = new SoftRigidDynamicsWorld(Dispatcher, Broadphase, Solver, CollisionConf);
                _ddWorld = (DiscreteDynamicsWorld)m_world;

                m_world.DispatchInfo.EnableSpu = true;
                softBodyWorldInfo.SparseSdf.Reset();
                softBodyWorldInfo.AirDensity = 1.2f;
                softBodyWorldInfo.WaterDensity = 0;
                softBodyWorldInfo.WaterOffset = 0;
                softBodyWorldInfo.WaterNormal = Vector3.Zero;
                softBodyWorldInfo.Gravity = m_gravity;
            }
            if(_ddWorld != null)
            {
                _ddWorld.Gravity = m_gravity;
            }
            if(_doDebugDraw)
            {
                _debugger.DebugMode = _debugDrawMode;
                m_world.DebugDrawer = _debugger;
            }

            //Add a BPhysicsWorldLateHelper component to call FixedUpdate
            /*lateUpdateHelper = GetComponent<PhysicsWorldLateHelper>();
            if(lateUpdateHelper == null)
            {
                lateUpdateHelper = gameObject.AddComponent<PhysicsWorldLateHelper>();
            }*/
            lateUpdateHelper.m_world = m_world;
            lateUpdateHelper.m_ddWorld = _ddWorld;
            lateUpdateHelper.m_physicsWorld = this;
            lateUpdateHelper.m__frameCount = 0;
            lateUpdateHelper.m_lastSimulationStepTime = 0;
        }

        protected void Dispose(bool disposing)
        {
            if(debugType >= PhysicsDebugger.DebugType.Debug)
                _debugger.Log("BDynamicsWorld Disposing physics.");

            if(lateUpdateHelper != null)
            {
                lateUpdateHelper.m_ddWorld = null;
                lateUpdateHelper.m_world = null;
            }
            if(m_world != null)
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
                            ((BTypedConstraint)constraint.Userobject).m_isInWorld = false;
                        if(debugType >= PhysicsDebugger.DebugType.Debug)
                            _debugger.LogFormat("Removed Constaint {0}", constraint.Userobject);
                        constraint.Dispose();
                    }
                }
                //*/

                if(debugType >= PhysicsDebugger.DebugType.Debug)
                    _debugger.LogFormat("Removing Collision Objects {0}", _ddWorld.NumCollisionObjects);
                //remove the rigidbodies from the dynamics world and delete them
                for(i = m_world.NumCollisionObjects - 1; i >= 0; i--)
                {
                    CollisionObject obj = m_world.CollisionObjectArray[i];
                    RigidBody body = obj as RigidBody;
                    if(body != null && body.MotionState != null)
                    {
                        _debugger.Assert(body.NumConstraintRefs == 0, "Rigid body still had constraints");
                        body.MotionState.Dispose();
                    }
                    m_world.RemoveCollisionObject(obj);
                    if(obj.UserObject is PhysicsCollisionObject)
                        ((PhysicsCollisionObject)obj.UserObject).isInWorld = false;
                    if(debugType >= PhysicsDebugger.DebugType.Debug)
                        _debugger.LogFormat("Removed CollisionObject {0}", obj.UserObject);
                    obj.Dispose();
                }

                if(m_world.DebugDrawer != null)
                {
                    if(m_world.DebugDrawer is IDisposable)
                    {
                        IDisposable dis = (IDisposable)m_world.DebugDrawer;
                        dis.Dispose();
                    }
                }

                m_world.Dispose();
                Broadphase.Dispose();
                Dispatcher.Dispose();
                CollisionConf.Dispose();
                _ddWorld = null;
                m_world = null;
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
        HashSet<PhysicsCollisionObject.BICollisionCallbackEventHandler> collisionCallbackListeners = new HashSet<PhysicsCollisionObject.BICollisionCallbackEventHandler>();

        public void RegisterCollisionCallbackListener(PhysicsCollisionObject.BICollisionCallbackEventHandler toBeAdded)
        {
            collisionCallbackListeners.Add(toBeAdded);
        }

        public void DeregisterCollisionCallbackListener(PhysicsCollisionObject.BICollisionCallbackEventHandler toBeRemoved)
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
            foreach(PhysicsCollisionObject.BICollisionCallbackEventHandler coeh in collisionCallbackListeners)
            {
                if(coeh != null)
                    coeh.OnFinishedVisitingManifolds();
            }
        }
    }
}
