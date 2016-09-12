using System.Collections;
using BulletSharp;

namespace SocialPoint.Multiplayer
{
    /**
    This script is last in the script execution order. Its purpose is to ensure that StepSimulation is called after other scripts LateUpdate calls
    */
    public class PhysicsWorldLateHelper
    {
        internal PhysicsWorld _physicsWorld;
        internal PhysicsDefaultCollisionHandler _collisionEventHandler = new PhysicsDefaultCollisionHandler();

        public void RegisterCollisionCallbackListener(PhysicsCollisionObject.ICollisionCallbackEventHandler toBeAdded)
        {
            if(_collisionEventHandler != null)
            {
                _collisionEventHandler.RegisterCollisionCallbackListener(toBeAdded);
            }
        }

        public void DeregisterCollisionCallbackListener(PhysicsCollisionObject.ICollisionCallbackEventHandler toBeRemoved)
        {
            if(_collisionEventHandler != null)
            {
                _collisionEventHandler.DeregisterCollisionCallbackListener(toBeRemoved);
            }
        }

        internal DiscreteDynamicsWorld _ddWorld;
        internal CollisionWorld _world;
        internal int _frameCount = 0;
        internal float _lastSimulationStepTime = 0;
        internal float _fixedTimeStep = 1f / 60f;
        internal int _maxSubsteps = 3;

        void Awake()
        {
            _lastSimulationStepTime = UnityEngine.Time.time;
        }

        //protected virtual void FixedUpdate()
        /*public virtual void FixedUpdate()
        {
            if(_ddWorld != null)
            {
                _frameCount++;
                float deltaTime = UnityEngine.Time.time - _lastSimulationStepTime;
                if(deltaTime > 0f)
                {
                    ///stepSimulation proceeds the simulation over 'timeStep', units in preferably in seconds.
                    ///By default, Bullet will subdivide the timestep in constant substeps of each 'fixedTimeStep'.
                    ///in order to keep the simulation real-time, the maximum number of substeps can be clamped to 'maxSubSteps'.
                    ///You can disable subdividing the timestep/substepping by passing maxSubSteps=0 as second argument to stepSimulation, but in that case you have to keep the timeStep constant.
                    _ddWorld.StepSimulation(deltaTime, _maxSubsteps, _fixedTimeStep);
                    //int numSteps = _ddWorld.StepSimulation(deltaTime, _maxSubsteps, _fixedTimeStep);
                    //Debug.Log("FixedUpdate " + numSteps);
                    _lastSimulationStepTime = UnityEngine.Time.time;
                }
            }

            //collisions
            if(_collisionEventHandler != null)
            {
                _collisionEventHandler.OnPhysicsStep(_world);
            }
        }*/

        //This is needed for rigidBody interpolation. The motion states will update the positions of the rigidbodies
        public void Update(float dt)
        {
            //StepSimulation returns number of steps
            _ddWorld.StepSimulation(dt, _maxSubsteps, _fixedTimeStep);

            //collisions
            if(_collisionEventHandler != null)
            {
                _collisionEventHandler.OnPhysicsStep(_world);
            }
        }
    }
}
