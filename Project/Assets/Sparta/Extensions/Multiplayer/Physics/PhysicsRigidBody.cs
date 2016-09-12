using System;
using BulletSharp;
using BulletSharp.Math;
using System.Collections;

namespace SocialPoint.Multiplayer
{
    public class PhysicsRigidBody : PhysicsCollisionObject
    {
        RigidBody _rigidBody;
        PhysicsGameObjectMotionState _motionState;
        Vector3 _localInertia = Vector3.Zero;

        public Vector3 localInertia
        {
            get
            {
                return _localInertia;
            }
        }

        public bool isDynamic()
        {
            return (_collisionFlags & BulletSharp.CollisionFlags.StaticObject) != BulletSharp.CollisionFlags.StaticObject
            && (_collisionFlags & BulletSharp.CollisionFlags.KinematicObject) != BulletSharp.CollisionFlags.KinematicObject;
        }

        //[SerializeField]
        float _friction = .5f;

        public float friction
        {
            get { return _friction; }
            set
            {
                if(_collisionObject != null && _friction != value)
                {
                    _collisionObject.Friction = value;
                }
                _friction = value;
            }
        }

        //[SerializeField]
        float _rollingFriction = 0f;

        public float rollingFriction
        {
            get { return _rollingFriction; }
            set
            {
                if(_collisionObject != null && _rollingFriction != value)
                {
                    _collisionObject.RollingFriction = value;
                }
                _rollingFriction = value;
            }
        }

        //[SerializeField]
        float _linearDamping = 0f;

        public float linearDamping
        {
            get { return _linearDamping; }
            set
            {
                if(_collisionObject != null && _linearDamping != value)
                {
                    _rigidBody.SetDamping(value, _angularDamping);
                }
                _linearDamping = value;
            }
        }

        //[SerializeField]
        float _angularDamping = 0f;

        public float angularDamping
        {
            get { return _angularDamping; }
            set
            {
                if(_collisionObject != null && _angularDamping != value)
                {
                    _rigidBody.SetDamping(_linearDamping, value);
                }
                _angularDamping = value;
            }
        }

        //[SerializeField]
        float _restitution = 0f;

        public float restitution
        {
            get { return _restitution; }
            set
            {
                if(_collisionObject != null && _restitution != value)
                {
                    _collisionObject.Restitution = value;
                }
                _restitution = value;
            }
        }

        //[SerializeField]
        float _linearSleepingThreshold = .8f;

        public float linearSleepingThreshold
        {
            get { return _linearSleepingThreshold; }
            set
            {
                if(_collisionObject != null && _linearSleepingThreshold != value)
                {
                    _rigidBody.SetSleepingThresholds(value, _angularSleepingThreshold);
                }
                _linearSleepingThreshold = value;
            }
        }

        //[SerializeField]
        float _angularSleepingThreshold = 1f;

        public float angularSleepingThreshold
        {
            get { return _angularSleepingThreshold; }
            set
            {
                if(_collisionObject != null && _angularSleepingThreshold != value)
                {
                    _rigidBody.SetSleepingThresholds(_linearSleepingThreshold, value);
                }
                _angularSleepingThreshold = value;
            }
        }

        //[SerializeField]
        bool _additionalDamping = false;

        public bool additionalDamping
        {
            get { return _additionalDamping; }
            set
            {
                if(isInWorld && _additionalDamping != value)
                {
                    _debugger.LogError(debugType, "Need to remove and re-add the rigid body to change additional damping setting");
                    return;
                }
                _additionalDamping = value;
            }
        }

        //[SerializeField]
        float _additionalDampingFactor = .005f;

        public float additionalDampingFactor
        {
            get { return _additionalDampingFactor; }
            set
            {
                if(_collisionObject != null && _additionalDampingFactor != value)
                {
                    _debugger.LogError(debugType, "Additional Damping settings cannot be changed once the Rigid Body has been created");
                    return;
                }
                _additionalDampingFactor = value;
            }
        }

        //[SerializeField]
        float _additionalLinearDampingThresholdSqr = .01f;

        public float additionalLinearDampingThresholdSqr
        {
            get { return _additionalLinearDampingThresholdSqr; }
            set
            {
                if(_collisionObject != null && _additionalLinearDampingThresholdSqr != value)
                {
                    _debugger.LogError(debugType, "Additional Damping settings cannot be changed once the Rigid Body has been created");
                    return;
                }
                _additionalLinearDampingThresholdSqr = value;
            }
        }

        //[SerializeField]
        float _additionalAngularDampingThresholdSqr = .01f;

        public float additionalAngularDampingThresholdSqr
        {
            get { return _additionalAngularDampingThresholdSqr; }
            set
            {
                if(_collisionObject != null && _additionalAngularDampingThresholdSqr != value)
                {
                    _debugger.LogError(debugType, "Additional Damping settings cannot be changed once the Rigid Body has been created");
                    return;
                }
                _additionalAngularDampingThresholdSqr = value;
            }
        }

        //[SerializeField]
        float _additionalAngularDampingFactor = .01f;

        public float additionalAngularDampingFactor
        {
            get { return _additionalAngularDampingFactor; }
            set
            {
                if(_collisionObject != null && _additionalAngularDampingFactor != value)
                {
                    _debugger.LogError(debugType, "Additional Damping settings cannot be changed once the Rigid Body has been created");
                    return;
                }
                _additionalAngularDampingFactor = value;
            }
        }

        //can lock axis with this
        //[SerializeField]
        Vector3 _linearFactor = Vector3.One;

        public Vector3 linearFactor
        {
            get { return _linearFactor; }
            set
            {
                if(_collisionObject != null && _linearFactor != value)
                {
                    _rigidBody.LinearFactor = value;
                }
                _linearFactor = value;
            }
        }

        //[SerializeField]
        Vector3 _angularFactor = Vector3.One;

        public Vector3 angularFactor
        {
            get { return _angularFactor; }
            set
            {
                if(_rigidBody != null && _angularFactor != value)
                {
                    _rigidBody.AngularFactor = value;
                }
                _angularFactor = value;
            }
        }

        //[SerializeField]
        float _mass = 1f;

        public float mass
        {
            set
            {
                if(_mass != value)
                {
                    if(_mass == 0f && isDynamic())
                    {
                        _debugger.LogError(debugType, "Rigid bodies that are not static or kinematic must have positive mass");
                        return;
                    }
                    if(_rigidBody != null)
                    {
                        _localInertia = Vector3.Zero;
                        if(isDynamic())
                        {
                            _collisionShape.GetCollisionShape().CalculateLocalInertia(_mass, out _localInertia);
                        }
                        _rigidBody.SetMassProps(_mass, _localInertia);
                    }
                    _mass = value;
                }
            }
            get
            {
                return _mass;
            }
        }

        //[SerializeField]
        protected Vector3 _linearVelocity;

        public Vector3 velocity
        {
            get
            {
                if(isInWorld)
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
                if(isInWorld)
                {
                    _rigidBody.LinearVelocity = value;
                }
                _linearVelocity = value;
            }
        }

        //[SerializeField]
        protected Vector3 _angularVelocity;

        public Vector3 angularVelocity
        {
            get
            {
                if(isInWorld)
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
                if(isInWorld)
                {
                    _rigidBody.AngularVelocity = value;
                }
                _angularVelocity = value;
            }
        }

        public PhysicsDebugger.DebugType debugType;

        //called by Physics World just before rigid body is added to world.
        //the current rigid body properties are used to rebuild the rigid body.
        internal override bool _BuildCollisionObject()
        {
            PhysicsWorld world = PhysicsWorld;
            if(_rigidBody != null)
            {
                if(isInWorld && world != null)
                {
                    isInWorld = false;
                    world.RemoveRigidBody(_rigidBody);
                }
            }
            
            /*if(transform.localScale != Vector3.One)
            {
                _debugger.LogError(debugType, "The local scale on this rigid body is not one. Bullet physics does not support scaling on a rigid body world transform. Instead alter the dimensions of the CollisionShape.");
            }*/

            _collisionShape = CollisionShape;
            if(_collisionShape == null)
            {
                _debugger.LogError(debugType, "There was no collision shape component attached to this BRigidBody. {0}", NetworkGameObject.Id);
                return false;
            }

            CollisionShape cs = _collisionShape.GetCollisionShape();
            //rigidbody is dynamic if and only if mass is non zero, otherwise static
            _localInertia = Vector3.Zero;
            if(isDynamic())
            {
                cs.CalculateLocalInertia(_mass, out _localInertia);
            }

            if(_rigidBody == null)
            {
                _motionState = new PhysicsGameObjectMotionState(NetworkGameObject.Transform);
                float bulletMass = _mass;
                if(!isDynamic())
                {
                    bulletMass = 0f;
                }

                RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(bulletMass, _motionState, cs, _localInertia);
                rbInfo.Friction = _friction;
                rbInfo.RollingFriction = _rollingFriction;
                rbInfo.LinearDamping = _linearDamping;
                rbInfo.AngularDamping = _angularDamping;
                rbInfo.Restitution = _restitution;
                rbInfo.LinearSleepingThreshold = _linearSleepingThreshold;
                rbInfo.AngularSleepingThreshold = _angularSleepingThreshold;
                rbInfo.AdditionalDamping = _additionalDamping;
                rbInfo.AdditionalAngularDampingFactor = _additionalAngularDampingFactor;
                rbInfo.AdditionalAngularDampingThresholdSqr = _additionalAngularDampingThresholdSqr;
                rbInfo.AdditionalDampingFactor = _additionalDampingFactor;
                rbInfo.AdditionalLinearDampingThresholdSqr = _additionalLinearDampingThresholdSqr;

                //Important: Base _collisionObject must be the same as _rigidBody
                _rigidBody = new RigidBody(rbInfo);
                _collisionObject = _rigidBody;

                _rigidBody.UserObject = this;
                _rigidBody.AngularVelocity = _angularVelocity;
                _rigidBody.LinearVelocity = _linearVelocity;
                rbInfo.Dispose();
            }
            else
            {
                float usedMass = 0f;
                if(isDynamic())
                {
                    usedMass = _mass;
                }
                _rigidBody.SetMassProps(usedMass, _localInertia);
                _rigidBody.Friction = _friction;
                _rigidBody.RollingFriction = _rollingFriction;
                _rigidBody.SetDamping(_linearDamping, _angularDamping);
                _rigidBody.Restitution = _restitution;
                _rigidBody.SetSleepingThresholds(_linearSleepingThreshold, _angularSleepingThreshold);
                _rigidBody.AngularVelocity = _angularVelocity;
                _rigidBody.LinearVelocity = _linearVelocity;
                _rigidBody.CollisionShape = cs;
                
            }
            _rigidBody.CollisionFlags = _collisionFlags;
            _rigidBody.LinearFactor = _linearFactor;
            _rigidBody.AngularFactor = _angularFactor;

            //if kinematic then disable deactivation
            if((_collisionFlags & BulletSharp.CollisionFlags.KinematicObject) != 0)
            {
                _rigidBody.ActivationState = ActivationState.DisableDeactivation;
            }
            return true;
        }

        public override void OnStart(NetworkGameObject go)
        {
            base.OnStart(go);

            /*BRigidBody[] rbs = GetComponentsInParent<BRigidBody>();
            if(rbs.Length != 1)
            {
                _debugger.LogError(debugType, "Can't nest rigid bodies. The transforms are updated by Bullet in undefined order which can cause spasing. Object {0}", GameObject.Id);
            }*/
        }

        public override void OnDestroy()
        {
            if(isInWorld && _rigidBody != null)
            {
                PhysicsWorld pw = PhysicsWorld;
                if(pw != null && pw.world != null)
                {
                    ((DiscreteDynamicsWorld)pw.world).RemoveRigidBody(_rigidBody);
                }
            }
            if(_rigidBody != null && _rigidBody.MotionState != null)
            {
                _rigidBody.MotionState.Dispose();
            }
            PhysicsUtilities.DisposeMember(ref _rigidBody);

            base.OnDestroy();
        }

        protected override void AddObjectToBulletWorld()
        {
            PhysicsWorld.AddRigidBody(this);
        }

        protected override void RemoveObjectFromBulletWorld()
        {
            PhysicsWorld pw = PhysicsWorld;
            if(pw != null && _rigidBody != null && isInWorld)
            {
                _debugger.Assert(_rigidBody.NumConstraintRefs == 0, "Removing rigid body that still had constraints. Remove constraints first.");
                //constraints must be removed before rigid body is removed
                pw.RemoveRigidBody((RigidBody)_collisionObject);
            }
        }

        public void AddImpulse(Vector3 impulse)
        {
            if(isInWorld)
            {
                _rigidBody.ApplyCentralImpulse(impulse);
            }
        }


        public void AddImpulseAtPosition(Vector3 impulse, Vector3 relativePostion)
        {
            if(isInWorld)
            {
                _rigidBody.ApplyImpulse(impulse, relativePostion);
            }
        }

        public void AddTorqueImpulse(Vector3 impulseTorque)
        {
            if(isInWorld)
            {
                _rigidBody.ApplyTorqueImpulse(impulseTorque);
            }
        }

        
        //Warning for single pulses use AddImpulse. AddForce should only be used over a period of time (several fixedTimeSteps or longer)
        //The force accumulator is cleared after every StepSimulation call including interpolation StepSimulation calls which clear the force
        //accumulator and do nothing.
        public void AddForce(Vector3 force)
        {
            if(isInWorld)
            {
                _rigidBody.ApplyCentralForce(force);
            }
        }

        
        //Warning for single pulses use AddImpulse. AddForce should only be used over a period of time (several fixedTimeSteps or longer)
        //The force accumulator is cleared after every StepSimulation call including interpolation StepSimulation calls which clear the force
        //accumulator and do nothing.
        public void AddForceAtPosition(Vector3 force, Vector3 relativePostion)
        {
            if(isInWorld)
            {
                _rigidBody.ApplyForce(force, relativePostion);
            }
        }

        
        //Warning for single pulses use AddImpulse. AddForce should only be used over a period of time (several fixedTimeSteps or longer)
        //The force accumulator is cleared after every StepSimulation call including interpolation StepSimulation calls which clear the force
        //accumulator and do nothing.
        public void AddTorque(Vector3 torque)
        {
            if(isInWorld)
            {
                _rigidBody.ApplyTorque(torque);
            }
        }
    }
}
