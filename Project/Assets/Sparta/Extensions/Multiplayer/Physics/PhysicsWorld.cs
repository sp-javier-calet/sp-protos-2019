using System;
using System.Collections;
using Jitter;
using Jitter.LinearMath;
using Jitter.Dynamics;
using Jitter.Collision;
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

        const int kAxis3SweepMaxProxies = 32766;
        const ulong kSequentialImpulseConstraintSolverRandomSeed = 12345;

        JVector _gravity;

        WorldType _worldType;
        CollisionSystem _collisionSystem;

        //CollisionConfType _collisionType;
        //BroadphaseType _broadphaseType;
        //JVector _axis3SweepBroadphaseMin;
        //JVector _axis3SweepBroadphaseMax;

        IPhysicsCollisionHandler _collisionEventHandler;
        //CollisionConfiguration _collisionConf;
        //CollisionDispatcher _dispatcher;
        //BroadphaseInterface _broadphase;
        //SoftBodyWorldInfo _softBodyWorldInfo;
        //SequentialImpulseConstraintSolver _solver;

        World _world;
        // Convenience variable so we arn't typecasting all the time.
        //DiscreteDynamicsWorld _ddWorld;

        PhysicsDebugger _debugger;
        //DebugDrawModes _debugDrawMode = DebugDrawModes.DrawWireframe;
        bool _doDebugDraw = false;

        public float FixedTimeStep
        {
            get;
            set;
        }

        public JVector Gravity
        {
            get
            { 
                return _gravity; 
            }
            set
            {
                /*if(_ddWorld != null)
                {
                    JVector grav = value;
                    _ddWorld.SetGravity(ref grav);
                }*/
                _gravity = value;
            }
        }

        public World CollisionWorld
        {
            get
            { 
                return _world; 
            }
        }

        public WorldType Type
        {
            get
            { 
                return _worldType; 
            }
        }

        /*public DebugDrawModes DebugDrawMode
        {
            get
            { 
                return _debugDrawMode; 
            }
            set
            {
                _debugDrawMode = value;
                if(_doDebugDraw && _world != null && _world.DebugDrawer != null)
                {
                    _world.DebugDrawer.DebugMode = value;
                }
            }
        }*/

        public bool DoDebugDraw
        {
            get
            { 
                return _doDebugDraw; 
            }
            set
            {
                /*if(_doDebugDraw != value && _world != null)
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
                }*/
                _doDebugDraw = value;
            }
        }

        public PhysicsWorld(IPhysicsCollisionHandler collisionHandler, PhysicsDebugger debugger)
            : this(collisionHandler, debugger, 
                   WorldType.RigidBodyDynamics, 
                   CollisionConfType.DefaultDynamicsWorldCollisionConf, 
                   BroadphaseType.DynamicAABBBroadphase,
                   new JVector(-1000f),
                   new JVector(1000f))
        {
        }

        public PhysicsWorld(IPhysicsCollisionHandler collisionHandler, PhysicsDebugger debugger,
                            WorldType worldType,
                            CollisionConfType collisionType,
                            BroadphaseType broadphaseType,
                            JVector axis3SweepBroadphaseMin,
                            JVector axis3SweepBroadphaseMax)
        {
            _collisionEventHandler = collisionHandler;
            _debugger = debugger;

            _worldType = worldType;
            //_collisionType = collisionType;
            //_broadphaseType = broadphaseType;
            //_axis3SweepBroadphaseMin = axis3SweepBroadphaseMin;
            //_axis3SweepBroadphaseMax = axis3SweepBroadphaseMax;

            FixedTimeStep = 1f / 60f;
            Gravity = new JVector(0f, -9.8f, 0f);

            InitializePhysicsWorld();
        }

        public void RegisterCollisionCallbackListener(ICollisionCallbackEventHandler toBeAdded)
        {
            if(_collisionEventHandler != null)
            {
                _collisionEventHandler.RegisterCollisionCallbackListener(toBeAdded);
            }
        }

        public void DeregisterCollisionCallbackListener(ICollisionCallbackEventHandler toBeRemoved)
        {
            if(_collisionEventHandler != null)
            {
                _collisionEventHandler.DeregisterCollisionCallbackListener(toBeRemoved);
            }
        }

        public void DrawGizmos()
        {
            if(_doDebugDraw && _world != null)
            {
                //_world.DebugDrawWorld();
            }
        }

        public void Update(float dt, NetworkScene scene, NetworkScene oldScene)
        {
            UpdatePhysics(dt, FixedTimeStep);
            if(DoDebugDraw)
            {
                DrawGizmos();
            }
        }

        void UpdatePhysics(float dt, float fixedTimeStep)
        {
            ///stepSimulation proceeds the simulation over 'timeStep', units in preferably in seconds.
            ///By default, Bullet will subdivide the timestep in constant substeps of each 'fixedTimeStep'.
            ///in order to keep the simulation real-time, the maximum number of substeps can be clamped to 'maxSubSteps'.
            ///You can disable subdividing the timestep/substepping by passing maxSubSteps=0 as second argument to stepSimulation, but in that case you have to keep the timeStep constant.
            /*int maxSubsteps = (int)System.Math.Ceiling(dt / fixedTimeStep);//Alternatively, use a const cap for maxSubsteps
            int numSteps = _ddWorld.StepSimulation(dt, maxSubsteps, fixedTimeStep);
            if(numSteps > 0)
            {
                FixedUpdate();
            }*/

            _world.Step(dt, true);
        }

        void FixedUpdate()
        {
            //Collision check
            /*if(_collisionEventHandler != null)
            {
                _collisionEventHandler.OnPhysicsStep(_world);
            }*/
        }

        public void OnClientConnected(byte clientId)
        {
        }

        public void OnClientDisconnected(byte clientId)
        {
        }

        public void AddCollisionObject(RigidBody co, PhysicsCollisionObject.CollisionFilterGroups collisionFilterGroup, PhysicsCollisionObject.CollisionFilterGroups collisionFilterMask)
        {
            if(co != null)
            {
                _world.AddBody(co);
            }
        }

        public void RemoveCollisionObject(RigidBody co)
        {
            _world.RemoveBody(co);
        }

        public void AddRigidBody(RigidBody rb, PhysicsCollisionObject.CollisionFilterGroups collisionFilterGroup, PhysicsCollisionObject.CollisionFilterGroups collisionFilterMask)
        {
            if(_worldType < WorldType.RigidBodyDynamics)
            {
                _debugger.LogError("World type must not be collision only");
            }
            if(rb != null)
            {
                _world.AddBody(rb);
                //_ddWorld.AddRigidBody(rb, collisionFilterGroup, collisionFilterMask);
            }
        }

        public void RemoveRigidBody(RigidBody rb)
        {
            if(_worldType < WorldType.RigidBodyDynamics)
            {
                _debugger.LogError("World type must not be collision only");
            }
            _world.RemoveBody(rb);
            //_ddWorld.RemoveRigidBody(rb);
        }

        protected void InitializePhysicsWorld()
        {
            _collisionSystem = new CollisionSystemPersistentSAP();
            _world = new World(_collisionSystem);
            /*if(_worldType == WorldType.SoftBodyAndRigidBody && _collisionType == CollisionConfType.DefaultDynamicsWorldCollisionConf)
            {
                _debugger.LogError("For World Type = SoftBodyAndRigidBody collisionType must be collisionType=SoftBodyRigidBodyCollisionConf. Switching");
                _collisionType = CollisionConfType.SoftBodyRigidBodyCollisionConf;
            }

            if(_collisionType == CollisionConfType.DefaultDynamicsWorldCollisionConf)
            {
                _collisionConf = new DefaultCollisionConfiguration();
            }
            else if(_collisionType == CollisionConfType.SoftBodyRigidBodyCollisionConf)
            {
                _collisionConf = new SoftBodyRigidBodyCollisionConfiguration();
            }

            _dispatcher = new CollisionDispatcher(_collisionConf);

            if(_broadphaseType == BroadphaseType.DynamicAABBBroadphase)
            {
                _broadphase = new DbvtBroadphase();
            }
            else if(_broadphaseType == BroadphaseType.Axis3SweepBroadphase)
            {
                _broadphase = new AxisSweep3(_axis3SweepBroadphaseMin, _axis3SweepBroadphaseMax, kAxis3SweepMaxProxies);
            }
            else if(_broadphaseType == BroadphaseType.Axis3SweepBroadphase_32bit)
            {
                _broadphase = new AxisSweep3_32Bit(_axis3SweepBroadphaseMin, _axis3SweepBroadphaseMax, kAxis3SweepMaxProxies);
            }
            else
            {
                _broadphase = null;
            }

            if(_worldType == WorldType.CollisionOnly)
            {
                _world = new CollisionWorld(_dispatcher, _broadphase, _collisionConf);
                _ddWorld = null;
            }
            else if(_worldType == WorldType.RigidBodyDynamics)
            {
                _world = new DiscreteDynamicsWorld(_dispatcher, _broadphase, null, _collisionConf);
                _ddWorld = (DiscreteDynamicsWorld)_world;
            }
            else if(_worldType == WorldType.MultiBodyWorld)
            {
                _world = new MultiBodyDynamicsWorld(_dispatcher, _broadphase, null, _collisionConf);
                _ddWorld = (DiscreteDynamicsWorld)_world;
            }
            else if(_worldType == WorldType.SoftBodyAndRigidBody)
            {
                _solver = new SequentialImpulseConstraintSolver();
                _solver.RandSeed = kSequentialImpulseConstraintSolverRandomSeed;
                _softBodyWorldInfo = new SoftBodyWorldInfo {
                    AirDensity = 1.2f,
                    WaterDensity = 0,
                    WaterOffset = 0,
                    WaterNormal = JVector.Zero,
                    Gravity = _gravity,
                    Dispatcher = _dispatcher,
                    Broadphase = _broadphase
                };
                _softBodyWorldInfo.SparseSdf.Initialize();

                _world = new SoftRigidDynamicsWorld(_dispatcher, _broadphase, _solver, _collisionConf);
                _ddWorld = (DiscreteDynamicsWorld)_world;

                _world.DispatchInfo.EnableSpu = true;
                _softBodyWorldInfo.SparseSdf.Reset();
                _softBodyWorldInfo.AirDensity = 1.2f;
                _softBodyWorldInfo.WaterDensity = 0;
                _softBodyWorldInfo.WaterOffset = 0;
                _softBodyWorldInfo.WaterNormal = JVector.Zero;
                _softBodyWorldInfo.Gravity = _gravity;
            }
            if(_ddWorld != null)
            {
                _ddWorld.Gravity = _gravity;
            }
            if(_doDebugDraw)
            {
                _debugger.DebugMode = _debugDrawMode;
                _world.DebugDrawer = _debugger;
            }*/
        }

        public void Dispose()
        {
            /*if(_world != null)
            {
                //Remove the rigidbodies from the dynamics world and delete them
                for(int i = _world.NumCollisionObjects - 1; i >= 0; i--)
                {
                    CollisionObject obj = _world.CollisionObjectArray[i];
                    RigidBody body = obj as RigidBody;
                    if(body != null && body.MotionState != null)
                    {
                        _debugger.Assert(body.NumConstraintRefs == 0, "Rigid body still had constraints");
                        body.MotionState.Dispose();
                    }
                    _world.RemoveCollisionObject(obj);
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
            }

            PhysicsUtilities.DisposeMember(ref _world);
            PhysicsUtilities.DisposeMember(ref _ddWorld);
            PhysicsUtilities.DisposeMember(ref _broadphase);
            PhysicsUtilities.DisposeMember(ref _dispatcher);
            PhysicsUtilities.DisposeMember(ref _collisionConf);
            PhysicsUtilities.DisposeMember(ref _solver);
            PhysicsUtilities.DisposeMember(ref _softBodyWorldInfo);

            GC.SuppressFinalize(this);*/
        }
    }
}
