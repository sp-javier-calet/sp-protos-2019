using System.Collections;
using BulletSharp;
using BM = BulletSharp.Math;
using System;
using System.Runtime.InteropServices;
using AOT;

namespace SocialPoint.Multiplayer
{
    public class PhysicsGameObjectMotionState : MotionState
    {
        public Transform transform;
        BM.Matrix wt;

        public PhysicsGameObjectMotionState(Transform t)
        {
            transform = t;
        }

        public delegate void GetTransformDelegate(out BM.Matrix worldTrans);

        public delegate void SetTransformDelegate(ref BM.Matrix m);

        //Bullet wants me to fill in worldTrans
        //This is called by bullet once when rigid body is added to the the world
        //For kinematic rigid bodies it is called every simulation step
        //[MonoPInvokeCallback(typeof(GetTransformDelegate))]
        public override void GetWorldTransform(out BM.Matrix worldTrans)
        {
            //Matrix4x4 trans = transform.localToWorldMatrix;
            //worldTrans = trans;         
            BulletSharp.Math.Quaternion q = transform.Rotation;
            BulletSharp.Math.Matrix.RotationQuaternion(ref q, out worldTrans);
            worldTrans.Origin = transform.Position;
        }

        //Bullet calls this so I can copy bullet data to unity
        public override void SetWorldTransform(ref BM.Matrix m)
        {
            transform.Position = UnityModelExtensions.ExtractTranslationFromMatrix(ref m).ToMultiplayer();
            transform.Rotation = UnityModelExtensions.ExtractRotationFromMatrix(ref m).ToMultiplayer();
            //transform.localScale = BSExtensionMethods2.ExtractScaleFromMatrix(ref m);
        }
    }
}
