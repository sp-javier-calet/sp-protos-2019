namespace BehaviorDesigner.Runtime.Standalone
{
    public class Matrix4x4
    {
        public static Matrix4x4 identity
        {
            get
            {
                Matrix4x4 matrix = new Matrix4x4();
                
                matrix.m00 = 1f; matrix.m01 = 0f; matrix.m02 = 0f; matrix.m03 = 0f;
                matrix.m10 = 0f; matrix.m11 = 1f; matrix.m12 = 0f; matrix.m13 = 0f;
                matrix.m20 = 0f; matrix.m21 = 0f; matrix.m22 = 1f; matrix.m23 = 0f;
                matrix.m30 = 0f; matrix.m31 = 0f; matrix.m32 = 0f; matrix.m33 = 1f;

                return matrix;
            }
        }
        
        public float m00 = 0f;
        public float m01 = 0f;
        public float m02 = 0f;
        public float m03 = 0f;
        public float m10 = 0f;
        public float m11 = 0f;
        public float m12 = 0f;
        public float m13 = 0f;
        public float m20 = 0f;
        public float m21 = 0f;
        public float m22 = 0f;
        public float m23 = 0f;
        public float m30 = 0f;
        public float m31 = 0f;
        public float m32 = 0f;
        public float m33 = 0f;
    }
}
