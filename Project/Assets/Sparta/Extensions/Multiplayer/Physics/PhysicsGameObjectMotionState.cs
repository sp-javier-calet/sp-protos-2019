using System.Collections;

//using BulletSharp;
//using BM = BulletSharp.Math;
using System;
using System.Runtime.InteropServices;
using AOT;

namespace SocialPoint.Multiplayer
{
    public class PhysicsGameObjectMotionState //: MotionState
    {
        /* public Transform Transform
        {
            set
            {
                _transform = value;
            }
        }

        Transform _transform;

        public PhysicsGameObjectMotionState()
        {
            _transform = new Transform();//Use indentity transform while the real transform is not set
        }

        //Bullet wants me to fill in worldTrans
        //This is called by bullet once when rigid body is added to the the world
        //For kinematic rigid bodies it is called every simulation step
        public override void GetWorldTransform(out BM.Matrix worldTrans)
        {
            BulletSharp.Math.Quaternion q = _transform.Rotation;
            BulletSharp.Math.Matrix.RotationQuaternion(ref q, out worldTrans);
            worldTrans.Origin = _transform.Position;
        }

        //Bullet calls this so I can copy bullet data to unity
        public override void SetWorldTransform(ref BM.Matrix m)
        {
            _transform.Position = UnityModelExtensions.ExtractTranslationFromMatrix(ref m).ToMultiplayer();
            _transform.Rotation = UnityModelExtensions.ExtractRotationFromMatrix(ref m).ToMultiplayer();
        }*/
    }
}
