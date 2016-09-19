using System;
using System.Collections;
using Jitter.LinearMath;
using Jitter.Dynamics;
using Jitter.Collision;
using Jitter.Collision.Shapes;

namespace SocialPoint.Multiplayer
{
    public class PhysicsRigidBody : PhysicsCollisionObject
    {
        RigidBody _rigidBody;
        //PhysicsGameObjectMotionState _motionState;

        float _mass = 1f;
        JVector _linearVelocity;
        JVector _angularVelocity;

        float _friction = .5f;
        float _rollingFriction = 0f;
        float _linearDamping = 0f;
        float _angularDamping = 0f;
        float _restitution = 0f;
        float _linearSleepingThreshold = .8f;
        float _angularSleepingThreshold = 1f;
        bool _additionalDamping = false;
        float _additionalDampingFactor = .005f;
        float _additionalAngularDampingFactor = .01f;
        float _additionalLinearDampingThresholdSqr = .01f;
        float _additionalAngularDampingThresholdSqr = .01f;

        JVector _linearFactor = JVector.One;
        JVector _angularFactor = JVector.One;
        //JVector _localInertia = JVector.Zero;

        public float Mass
        {
            get
            {
                return _mass;
            }
            set
            {
                if(_mass != value)
                {
                    if(_mass == 0f && IsDynamic())
                    {
                        _debugger.LogError("Rigid bodies that are not static or kinematic must have positive mass");
                        return;
                    }
                    if(_rigidBody != null)
                    {
                        /*_localInertia = JVector.Zero;
                        if(IsDynamic())
                        {
                            _collisionShape.GetCollisionShape().CalculateLocalInertia(_mass, out _localInertia);
                        }
                        _rigidBody.SetMassProps(_mass, _localInertia);*/
                    }
                    _mass = value;
                }
            }
        }

        public JVector Velocity
        {
            get
            {
                if(_isInWorld)
                {
                    return _rigidBody.LinearVelocity;
                }
                else
                {
                    return _linearVelocity;
                }
            }
            set
            {
                if(_isInWorld)
                {
                    _rigidBody.LinearVelocity = value;
                }
                _linearVelocity = value;
            }
        }

        public JVector AngularVelocity
        {
            get
            {
                if(_isInWorld)
                {
                    return _rigidBody.AngularVelocity;
                }
                else
                {
                    return _angularVelocity;
                }
            }
            set
            {
                if(_isInWorld)
                {
                    _rigidBody.AngularVelocity = value;
                }
                _angularVelocity = value;
            }
        }

        public float Friction
        {
            get
            { 
                return _friction; 
            }
            set
            {
                if(_rigidBody != null && _friction != value)
                {
                    //_rigidBody.Friction = value;
                }
                _friction = value;
            }
        }

        public float RollingFriction
        {
            get
            { 
                return _rollingFriction; 
            }
            set
            {
                if(_rigidBody != null && _rollingFriction != value)
                {
                    //_rigidBody.RollingFriction = value;
                }
                _rollingFriction = value;
            }
        }

        public float LinearDamping
        {
            get
            { 
                return _linearDamping; 
            }
            set
            {
                if(_rigidBody != null && _linearDamping != value)
                {
                    //_rigidBody.SetDamping(value, _angularDamping);
                }
                _linearDamping = value;
            }
        }

        public float AngularDamping
        {
            get
            { 
                return _angularDamping; 
            }
            set
            {
                if(_rigidBody != null && _angularDamping != value)
                {
                    //_rigidBody.SetDamping(_linearDamping, value);
                }
                _angularDamping = value;
            }
        }

        public float Restitution
        {
            get
            { 
                return _restitution; 
            }
            set
            {
                if(_rigidBody != null && _restitution != value)
                {
                    //_rigidBody.Restitution = value;
                }
                _restitution = value;
            }
        }

        public float LinearSleepingThreshold
        {
            get
            { 
                return _linearSleepingThreshold; 
            }
            set
            {
                if(_rigidBody != null && _linearSleepingThreshold != value)
                {
                    //_rigidBody.SetSleepingThresholds(value, _angularSleepingThreshold);
                }
                _linearSleepingThreshold = value;
            }
        }

        public float AngularSleepingThreshold
        {
            get
            { 
                return _angularSleepingThreshold; 
            }
            set
            {
                if(_rigidBody != null && _angularSleepingThreshold != value)
                {
                    //_rigidBody.SetSleepingThresholds(_linearSleepingThreshold, value);
                }
                _angularSleepingThreshold = value;
            }
        }

        public bool AdditionalDamping
        {
            get
            { 
                return _additionalDamping; 
            }
            set
            {
                if(_isInWorld && _additionalDamping != value)
                {
                    _debugger.LogError("Need to remove and re-add the rigid body to change additional damping setting");
                    return;
                }
                _additionalDamping = value;
            }
        }

        public float AdditionalDampingFactor
        {
            get
            { 
                return _additionalDampingFactor; 
            }
            set
            {
                if(_rigidBody != null && _additionalDampingFactor != value)
                {
                    _debugger.LogError("Additional Damping settings cannot be changed once the Rigid Body has been created");
                    return;
                }
                _additionalDampingFactor = value;
            }
        }

        public float AdditionalLinearDampingThresholdSqr
        {
            get
            { 
                return _additionalLinearDampingThresholdSqr; 
            }
            set
            {
                if(_rigidBody != null && _additionalLinearDampingThresholdSqr != value)
                {
                    _debugger.LogError("Additional Damping settings cannot be changed once the Rigid Body has been created");
                    return;
                }
                _additionalLinearDampingThresholdSqr = value;
            }
        }

        public float AdditionalAngularDampingThresholdSqr
        {
            get
            { 
                return _additionalAngularDampingThresholdSqr; 
            }
            set
            {
                if(_rigidBody != null && _additionalAngularDampingThresholdSqr != value)
                {
                    _debugger.LogError("Additional Damping settings cannot be changed once the Rigid Body has been created");
                    return;
                }
                _additionalAngularDampingThresholdSqr = value;
            }
        }

        public float AdditionalAngularDampingFactor
        {
            get
            { 
                return _additionalAngularDampingFactor; 
            }
            set
            {
                if(_rigidBody != null && _additionalAngularDampingFactor != value)
                {
                    _debugger.LogError("Additional Damping settings cannot be changed once the Rigid Body has been created");
                    return;
                }
                _additionalAngularDampingFactor = value;
            }
        }

        public JVector LinearFactor
        {
            get
            { 
                return _linearFactor; 
            }
            set
            {
                if(_rigidBody != null && _linearFactor != value)
                {
                    //_rigidBody.LinearFactor = value;
                }
                _linearFactor = value;
            }
        }

        public JVector AngularFactor
        {
            get
            { 
                return _angularFactor; 
            }
            set
            {
                if(_rigidBody != null && _angularFactor != value)
                {
                    //_rigidBody.AngularFactor = value;
                }
                _angularFactor = value;
            }
        }

        public PhysicsRigidBody(PhysicsCollisionShape shape, PhysicsWorld physicsWorld, PhysicsDebugger debugger)
            : base(shape, physicsWorld, debugger)
        {
        }

        public PhysicsRigidBody(PhysicsCollisionShape shape, PhysicsWorld physicsWorld, PhysicsDebugger debugger, 
                                CollisionFlags collisionFlags)
            : base(shape, physicsWorld, debugger, collisionFlags)
        {
        }

        public PhysicsRigidBody(PhysicsCollisionShape shape, PhysicsWorld physicsWorld, PhysicsDebugger debugger, 
                                CollisionFlags collisionFlags, 
                                CollisionFilterGroups collisionMask)
            : base(shape, physicsWorld, debugger, collisionFlags, collisionMask)
        {
        }

        public PhysicsRigidBody(PhysicsCollisionShape shape, PhysicsWorld physicsWorld, PhysicsDebugger debugger, 
                                CollisionFlags collisionFlags, 
                                CollisionFilterGroups collisionMask, 
                                CollisionFilterGroups belongGroups)
            : base(shape, physicsWorld, debugger, collisionFlags, collisionMask, belongGroups)
        {
        }

        /*public PhysicsRigidBody(RigidBodyConstructionInfo rbInfo, PhysicsCollisionShape shape, PhysicsWorld physicsWorld, PhysicsDebugger debugger, 
                                CollisionFlags collisionFlags, 
                                CollisionFilterGroups collisionMask, 
                                CollisionFilterGroups belongGroups)
            : base(shape, physicsWorld, debugger, collisionFlags, collisionMask, belongGroups)
        {
            _mass = rbInfo.Mass;
            _localInertia = rbInfo.LocalInertia;
            _friction = rbInfo.Friction;
            _rollingFriction = rbInfo.RollingFriction;
            _linearDamping = rbInfo.LinearDamping;
            _angularDamping = rbInfo.AngularDamping;
            _restitution = rbInfo.Restitution;
            _linearSleepingThreshold = rbInfo.LinearSleepingThreshold;
            _angularSleepingThreshold = rbInfo.AngularSleepingThreshold;
            _additionalDamping = rbInfo.AdditionalDamping;
            _additionalDampingFactor = rbInfo.AdditionalDampingFactor;
            _additionalAngularDampingFactor = rbInfo.AdditionalAngularDampingFactor;
            _additionalLinearDampingThresholdSqr = rbInfo.AdditionalLinearDampingThresholdSqr;
            _additionalAngularDampingThresholdSqr = rbInfo.AdditionalAngularDampingThresholdSqr;
        }*/

        public override Object Clone()
        {
            /*PhysicsCollisionShape shapeClone = (PhysicsCollisionShape)_collisionShape.Clone();
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(_mass, new PhysicsGameObjectMotionState(), shapeClone.GetCollisionShape(), _localInertia);
            var behavior = new PhysicsRigidBody(rbInfo, shapeClone, _physicsWorld, _debugger, _collisionFlags, _collisionMask, _groupsIBelongTo);
            return behavior;*/
            return null;
        }

        public override void OnStart(NetworkGameObject go)
        {
            //_motionState.Transform = go.Transform;
            base.OnStart(go);
        }

        public override void Update(float dt)
        {
            //If kinematic then update physic object with game object transform
            if((_collisionFlags & CollisionFlags.KinematicObject) != 0)
            {
                //_collisionObject.WorldTransform = NetworkGameObject.Transform.WorldToLocalMatrix();
            }
        }

        public override void OnDestroy()
        {
            RemoveObjectFromBulletWorld();

            /*if(_rigidBody != null && _rigidBody.MotionState != null)
            {
                _rigidBody.MotionState.Dispose();
            }*/
            //PhysicsUtilities.DisposeMember(ref _rigidBody);

            base.OnDestroy();
        }

        public bool IsDynamic()
        {
            return (_collisionFlags & CollisionFlags.StaticObject) != CollisionFlags.StaticObject
            && (_collisionFlags & CollisionFlags.KinematicObject) != CollisionFlags.KinematicObject;
        }

        protected override bool BuildCollisionObject()
        {
            if(_rigidBody != null)
            {
                if(_isInWorld && _physicsWorld != null)
                {
                    _isInWorld = false;
                    _physicsWorld.RemoveRigidBody(_rigidBody);
                }
            }

            if(_collisionShape == null)
            {
                _debugger.LogError("There was no collision shape component attached to this PhysicsRigidBody.");
                return false;
            }

            Shape cs = _collisionShape.GetCollisionShape();
            //rigidbody is dynamic if and only if mass is non zero, otherwise static
            //_localInertia = JVector.Zero;
            if(IsDynamic())
            {
                //cs.CalculateLocalInertia(_mass, out _localInertia);
            }

            if(_rigidBody == null)
            {
                //_motionState = new PhysicsGameObjectMotionState();
                /*float bulletMass = _mass;
                if(!IsDynamic())
                {
                    bulletMass = 0f;
                }*/

                /*RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(bulletMass, _motionState, cs, _localInertia);
                rbInfo.Friction = _friction;
                rbInfo.RollingFriction = _rollingFriction;
                rbInfo.LinearDamping = _linearDamping;
                rbInfo.AngularDamping = _angularDamping;
                rbInfo.Restitution = _restitution;
                rbInfo.LinearSleepingThreshold = _linearSleepingThreshold;
                rbInfo.AngularSleepingThreshold = _angularSleepingThreshold;
                rbInfo.AdditionalDamping = _additionalDamping;
                rbInfo.AdditionalDampingFactor = _additionalDampingFactor;
                rbInfo.AdditionalAngularDampingFactor = _additionalAngularDampingFactor;
                rbInfo.AdditionalLinearDampingThresholdSqr = _additionalLinearDampingThresholdSqr;
                rbInfo.AdditionalAngularDampingThresholdSqr = _additionalAngularDampingThresholdSqr;*/

                //Important: Base _collisionObject must be the same as _rigidBody
                _rigidBody = new RigidBody(cs);//rbInfo
                _collisionObject = _rigidBody;

                //_rigidBody.UserObject = this;
                _rigidBody.AngularVelocity = _angularVelocity;
                _rigidBody.LinearVelocity = _linearVelocity;
                //rbInfo.Dispose();
            }
            else
            {
                /*float usedMass = 0f;
                if(IsDynamic())
                {
                    usedMass = _mass;
                }*/
                /*_rigidBody.SetMassProps(usedMass, _localInertia);
                _rigidBody.Friction = _friction;
                _rigidBody.RollingFriction = _rollingFriction;
                _rigidBody.SetDamping(_linearDamping, _angularDamping);
                _rigidBody.Restitution = _restitution;
                _rigidBody.SetSleepingThresholds(_linearSleepingThreshold, _angularSleepingThreshold);
                _rigidBody.AngularVelocity = _angularVelocity;
                _rigidBody.LinearVelocity = _linearVelocity;
                _rigidBody.CollisionShape = cs;*/
            }
            /*_rigidBody.CollisionFlags = _collisionFlags;
            _rigidBody.LinearFactor = _linearFactor;
            _rigidBody.AngularFactor = _angularFactor;*/

            //if kinematic then disable deactivation
            if((_collisionFlags & CollisionFlags.KinematicObject) != 0)
            {
                //_rigidBody.ActivationState = ActivationState.DisableDeactivation;
            }
            return true;
        }

        protected override void AddObjectToBulletWorld()
        {
            if(!_isInWorld)
            {
                _physicsWorld.AddRigidBody(_rigidBody, _groupsIBelongTo, _collisionMask);
                _isInWorld = true;
            }
        }

        protected override void RemoveObjectFromBulletWorld()
        {
            if(_isInWorld)
            {
                //_debugger.Assert(_rigidBody.NumConstraintRefs == 0, "Removing rigid body that still had constraints. Remove constraints first.");
                //constraints must be removed before rigid body is removed

                _physicsWorld.RemoveRigidBody(_rigidBody);
                _isInWorld = false;
            }
        }

        public void AddImpulse(JVector impulse)
        {
            if(_isInWorld)
            {
                //_rigidBody.ApplyCentralImpulse(impulse);
            }
        }

        public void AddImpulseAtPosition(JVector impulse, JVector relativePostion)
        {
            if(_isInWorld)
            {
                _rigidBody.ApplyImpulse(impulse, relativePostion);
            }
        }

        public void AddTorqueImpulse(JVector impulseTorque)
        {
            if(_isInWorld)
            {
                //_rigidBody.ApplyTorqueImpulse(impulseTorque);
            }
        }
        
        //Warning for single pulses use AddImpulse. AddForce should only be used over a period of time (several fixedTimeSteps or longer)
        //The force accumulator is cleared after every StepSimulation call including interpolation StepSimulation calls which clear the force
        //accumulator and do nothing.
        public void AddForce(JVector force)
        {
            if(_isInWorld)
            {
                //_rigidBody.ApplyCentralForce(force);
            }
        }

        //Warning for single pulses use AddImpulse. AddForce should only be used over a period of time (several fixedTimeSteps or longer)
        //The force accumulator is cleared after every StepSimulation call including interpolation StepSimulation calls which clear the force
        //accumulator and do nothing.
        public void AddForceAtPosition(JVector force, JVector relativePostion)
        {
            if(_isInWorld)
            {
                //_rigidBody.ApplyForce(force, relativePostion);
            }
        }

        //Warning for single pulses use AddImpulse. AddForce should only be used over a period of time (several fixedTimeSteps or longer)
        //The force accumulator is cleared after every StepSimulation call including interpolation StepSimulation calls which clear the force
        //accumulator and do nothing.
        public void AddTorque(JVector torque)
        {
            if(_isInWorld)
            {
                //_rigidBody.ApplyTorque(torque);
            }
        }
    }
}
