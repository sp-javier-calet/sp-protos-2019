using Jitter.LinearMath;
using SocialPoint.Geometry;

namespace SocialPoint.Physics
{
    public class UnityPhysicsDebugger : IPhysicsDebugger
    {
        public void DrawLine(JVector start, JVector end)
        {
            JVector color = VectorFromColor(UnityEngine.Color.green);
            DrawLine(start, end, color);
        }

        public void DrawPoint(JVector pos)
        {
            JVector color = VectorFromColor(UnityEngine.Color.green);
            DrawSphere(pos, 1, color);
        }

        public void DrawTriangle(JVector pos1, JVector pos2, JVector pos3)
        {
            JVector color = VectorFromColor(UnityEngine.Color.green);
            DrawTriangle(pos1, pos2, pos3, color);
        }

        public void DrawLine(JVector from, JVector to, JVector color)
        {
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityEngine.Debug.DrawLine(Vector.Convert(from), Vector.Convert(to), ucolor);
        }

        public void DrawBox(JVector position, JQuaternion rotation, JVector size, JVector color)
        {
            UnityEngine.Vector3 pos = Vector.Convert(position);
            UnityEngine.Quaternion rot = Quat.Convert(rotation);
            UnityEngine.Vector3 scale = Vector.Convert(JVector.One);
            UnityEngine.Vector3 dimensions = Vector.Convert(size);
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityPhysicsDebuggerUtility.DebugDrawBox(pos, rot, scale, dimensions, ucolor);
        }

        public void DrawSphere(JVector p, float radius, JVector color)
        {
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityPhysicsDebuggerUtility.DebugDrawSphere(Vector.Convert(p), UnityEngine.Quaternion.identity, UnityEngine.Vector3.one, UnityEngine.Vector3.one * radius, ucolor);
        }

        public void DrawTriangle(JVector v0, JVector v1, JVector v2, JVector n0, JVector n1, JVector n2, JVector color, float alpha)
        {
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityEngine.Debug.DrawLine(Vector.Convert(v0), Vector.Convert(v1), ucolor);
            UnityEngine.Debug.DrawLine(Vector.Convert(v1), Vector.Convert(v2), ucolor);
            UnityEngine.Debug.DrawLine(Vector.Convert(v2), Vector.Convert(v0), ucolor);

        }

        public void DrawTriangle(JVector v0, JVector v1, JVector v2, JVector color)
        {
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityEngine.Debug.DrawLine(Vector.Convert(v0), Vector.Convert(v1), ucolor);
            UnityEngine.Debug.DrawLine(Vector.Convert(v1), Vector.Convert(v2), ucolor);
            UnityEngine.Debug.DrawLine(Vector.Convert(v2), Vector.Convert(v0), ucolor);
        }

        public void DrawCapsule(float radius, float halfHeight, int upAxis, JVector position, JQuaternion rotation, JVector color)
        {
            UnityEngine.Vector3 pos = Vector.Convert(position);
            UnityEngine.Quaternion rot = Quat.Convert(rotation);
            UnityEngine.Vector3 scale = Vector.Convert(JVector.One);
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityPhysicsDebuggerUtility.DebugDrawCapsule(pos, rot, scale, radius, halfHeight, upAxis, ucolor);
        }

        public void DrawCylinder(float radius, float halfHeight, int upAxis, JVector position, JQuaternion rotation, JVector color)
        {
            UnityEngine.Vector3 pos = Vector.Convert(position);
            UnityEngine.Quaternion rot = Quat.Convert(rotation);
            UnityEngine.Vector3 scale = Vector.Convert(JVector.One);
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityPhysicsDebuggerUtility.DebugDrawCylinder(pos, rot, scale, radius, halfHeight, upAxis, ucolor);
        }

        public void DrawCone(float radius, float height, int upAxis, JVector position, JQuaternion rotation, JVector color)
        {
            UnityEngine.Vector3 pos = Vector.Convert(position);
            UnityEngine.Quaternion rot = Quat.Convert(rotation);
            UnityEngine.Vector3 scale = Vector.Convert(JVector.One);
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityPhysicsDebuggerUtility.DebugDrawCone(pos, rot, scale, radius, height, upAxis, ucolor);
        }

        public void DrawPlane(JVector planeNormal, float planeConst, JVector position, JQuaternion rotation, JVector color)
        {
            UnityEngine.Vector3 pos = Vector.Convert(position);
            UnityEngine.Quaternion rot = Quat.Convert(rotation);
            UnityEngine.Vector3 scale = Vector.Convert(JVector.One);
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityPhysicsDebuggerUtility.DebugDrawPlane(pos, rot, scale, Vector.Convert(planeNormal), planeConst, ucolor);
        }

        UnityEngine.Color ColorFromVector(JVector v)
        {
            return new UnityEngine.Color(v.X, v.Y, v.Z);
        }

        JVector VectorFromColor(UnityEngine.Color c)
        {
            return new JVector(c.r, c.g, c.b);
        }
    }
}
