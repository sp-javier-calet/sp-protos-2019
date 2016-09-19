using UnityEngine;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public static class UnityModelExtensions
    {
        public static UnityEngine.Vector3 ToUnity(this Jitter.LinearMath.JVector v)
        {
            return new UnityEngine.Vector3(v.X, v.Y, v.Z);
        }

        public static UnityEngine.Quaternion ToUnity(this Jitter.LinearMath.JQuaternion q)
        {
            return new UnityEngine.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        /*public static UnityEngine.Matrix4x4 ToUnity(this BulletSharp.Math.Matrix bm)
        {
            Matrix4x4 um = new Matrix4x4();
            um[0, 0] = bm[0, 0];
            um[0, 1] = bm[1, 0];
            um[0, 2] = bm[2, 0];
            um[0, 3] = bm[3, 0];

            um[1, 0] = bm[0, 1];
            um[1, 1] = bm[1, 1];
            um[1, 2] = bm[2, 1];
            um[1, 3] = bm[3, 1];

            um[2, 0] = bm[0, 2];
            um[2, 1] = bm[1, 2];
            um[2, 2] = bm[2, 2];
            um[2, 3] = bm[3, 2];

            um[3, 0] = bm[0, 3];
            um[3, 1] = bm[1, 3];
            um[3, 2] = bm[2, 3];
            um[3, 3] = bm[3, 3];
            return um;
        }*/

        public static Jitter.LinearMath.JVector ToMultiplayer(this UnityEngine.Vector3 v)
        {
            return new Jitter.LinearMath.JVector(v.x, v.y, v.z);
        }

        public static Jitter.LinearMath.JQuaternion ToMultiplayer(this UnityEngine.Quaternion q)
        {
            return new Jitter.LinearMath.JQuaternion(q.x, q.y, q.z, q.w);
        }

        /*public static BulletSharp.Math.Matrix ToMultiplayer(this UnityEngine.Matrix4x4 um)
        {
            BulletSharp.Math.Matrix bm = new BulletSharp.Math.Matrix();
            um.ToMultiplayer(ref bm);
            return bm;
        }*/

        /*public static void ToMultiplayer(this UnityEngine.Matrix4x4 um, ref BulletSharp.Math.Matrix bm)
        {
            bm[0, 0] = um[0, 0];
            bm[0, 1] = um[1, 0];
            bm[0, 2] = um[2, 0];
            bm[0, 3] = um[3, 0];

            bm[1, 0] = um[0, 1];
            bm[1, 1] = um[1, 1];
            bm[1, 2] = um[2, 1];
            bm[1, 3] = um[3, 1];

            bm[2, 0] = um[0, 2];
            bm[2, 1] = um[1, 2];
            bm[2, 2] = um[2, 2];
            bm[2, 3] = um[3, 2];

            bm[3, 0] = um[0, 3];
            bm[3, 1] = um[1, 3];
            bm[3, 2] = um[2, 3];
            bm[3, 3] = um[3, 3];
        }*/

        /// <summary>
        /// Extract translation from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Translation offset.
        /// </returns>
        public static UnityEngine.Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
        {
            UnityEngine.Vector3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        /*public static UnityEngine.Vector3 ExtractTranslationFromMatrix(ref BulletSharp.Math.Matrix matrix)
        {
            UnityEngine.Vector3 translate;
            translate.x = matrix.M41;
            translate.y = matrix.M42;
            translate.z = matrix.M43;
            return translate;
        }*/

        /// <summary>
        /// Extract rotation quaternion from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Quaternion representation of rotation transform.
        /// </returns>
        public static UnityEngine.Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
        {
            UnityEngine.Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            UnityEngine.Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return UnityEngine.Quaternion.LookRotation(forward, upwards);
        }

        /*public static UnityEngine.Quaternion ExtractRotationFromMatrix(ref BulletSharp.Math.Matrix matrix)
        {
            UnityEngine.Vector3 forward;
            forward.x = matrix.M31;
            forward.y = matrix.M32;
            forward.z = matrix.M33;

            UnityEngine.Vector3 upwards;
            upwards.x = matrix.M21;
            upwards.y = matrix.M22;
            upwards.z = matrix.M23;

            return UnityEngine.Quaternion.LookRotation(forward, upwards);
        }*/

        /// <summary>
        /// Extract scale from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Scale vector.
        /// </returns>
        public static UnityEngine.Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
        {
            UnityEngine.Vector3 scale;
            scale.x = new UnityEngine.Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new UnityEngine.Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new UnityEngine.Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        /*public static UnityEngine.Vector3 ExtractScaleFromMatrix(ref BulletSharp.Math.Matrix matrix)
        {
            UnityEngine.Vector3 scale;
            scale.x = new UnityEngine.Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14).magnitude;
            scale.y = new UnityEngine.Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24).magnitude;
            scale.z = new UnityEngine.Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34).magnitude;
            return scale;
        }*/

        /// <summary>
        /// Extract position, rotation and scale from TRS matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <param name="localPosition">Output position.</param>
        /// <param name="localRotation">Output rotation.</param>
        /// <param name="localScale">Output scale.</param>
        public static void DecomposeMatrix(ref Matrix4x4 matrix, out UnityEngine.Vector3 localPosition, out UnityEngine.Quaternion localRotation, out UnityEngine.Vector3 localScale)
        {
            localPosition = ExtractTranslationFromMatrix(ref matrix);
            localRotation = ExtractRotationFromMatrix(ref matrix);
            localScale = ExtractScaleFromMatrix(ref matrix);
        }
    }
}