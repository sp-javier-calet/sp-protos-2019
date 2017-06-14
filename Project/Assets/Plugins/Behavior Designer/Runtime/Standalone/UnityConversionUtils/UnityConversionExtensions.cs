#if UNITY_5_3_OR_NEWER
using BehaviorDesigner.Runtime.Standalone;

public static class UnityConversionExtensions
{
    public static Vector2 ToBDesigner(this UnityEngine.Vector2 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static UnityEngine.Vector2 ToUnity(this Vector2 v)
    {
        return new UnityEngine.Vector2(v.x, v.y);
    }

    public static Vector3 ToBDesigner(this UnityEngine.Vector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static UnityEngine.Vector3 ToUnity(this Vector3 v)
    {
        return new UnityEngine.Vector3(v.x, v.y, v.z);
    }

    public static Vector4 ToBDesigner(this UnityEngine.Vector4 v)
    {
        return new Vector4(v.x, v.y, v.z, v.w);
    }

    public static UnityEngine.Vector4 ToUnity(this Vector4 v)
    {
        return new UnityEngine.Vector4(v.x, v.y, v.z, v.w);
    }

    public static Quaternion ToBDesigner(this UnityEngine.Quaternion v)
    {
        return new Quaternion(v.x, v.y, v.z, v.w);
    }

    public static UnityEngine.Quaternion ToUnity(this Quaternion v)
    {
        return new UnityEngine.Quaternion(v.x, v.y, v.z, v.w);
    }

    public static Matrix4x4 ToBDesigner(this UnityEngine.Matrix4x4 v)
    {
        var matrix = new Matrix4x4();
        matrix.m00 = v.m00;
        matrix.m01 = v.m01;
        matrix.m02 = v.m02;
        matrix.m03 = v.m03;
        matrix.m10 = v.m10;
        matrix.m11 = v.m11;
        matrix.m12 = v.m12;
        matrix.m13 = v.m13;
        matrix.m20 = v.m20;
        matrix.m21 = v.m21;
        matrix.m22 = v.m22;
        matrix.m23 = v.m23;
        matrix.m30 = v.m30;
        matrix.m31 = v.m31;
        matrix.m32 = v.m32;
        matrix.m33 = v.m33;
        return matrix;
    }

    public static UnityEngine.Matrix4x4 ToUnity(this Matrix4x4 v)
    {
        var matrix = new UnityEngine.Matrix4x4();
        matrix.m00 = v.m00;
        matrix.m01 = v.m01;
        matrix.m02 = v.m02;
        matrix.m03 = v.m03;
        matrix.m10 = v.m10;
        matrix.m11 = v.m11;
        matrix.m12 = v.m12;
        matrix.m13 = v.m13;
        matrix.m20 = v.m20;
        matrix.m21 = v.m21;
        matrix.m22 = v.m22;
        matrix.m23 = v.m23;
        matrix.m30 = v.m30;
        matrix.m31 = v.m31;
        matrix.m32 = v.m32;
        matrix.m33 = v.m33;
        return matrix;
    }

}
#endif