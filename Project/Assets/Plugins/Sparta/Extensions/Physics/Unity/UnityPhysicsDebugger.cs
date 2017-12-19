using System;
using System.Collections;
using Jitter.LinearMath;

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
            UnityEngine.Debug.DrawLine(from.ToUnity(), to.ToUnity(), ucolor);
        }

        public void DrawBox(JVector position, JQuaternion rotation, JVector size, JVector color)
        {
            UnityEngine.Vector3 pos = position.ToUnity();
            UnityEngine.Quaternion rot = rotation.ToUnity();
            UnityEngine.Vector3 scale = JVector.One.ToUnity();
            UnityEngine.Vector3 dimensions = size.ToUnity();
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityPhysicsDebuggerUtility.DebugDrawBox(pos, rot, scale, dimensions, ucolor);
        }

        public void DrawSphere(JVector p, float radius, JVector color)
        {
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityPhysicsDebuggerUtility.DebugDrawSphere(p.ToUnity(), UnityEngine.Quaternion.identity, UnityEngine.Vector3.one, UnityEngine.Vector3.one * radius, ucolor);
        }

        public void DrawTriangle(JVector v0, JVector v1, JVector v2, JVector n0, JVector n1, JVector n2, JVector color, float alpha)
        {
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityEngine.Debug.DrawLine(v0.ToUnity(), v1.ToUnity(), ucolor);
            UnityEngine.Debug.DrawLine(v1.ToUnity(), v2.ToUnity(), ucolor);
            UnityEngine.Debug.DrawLine(v2.ToUnity(), v0.ToUnity(), ucolor);

        }

        public void DrawTriangle(JVector v0, JVector v1, JVector v2, JVector color)
        {
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityEngine.Debug.DrawLine(v0.ToUnity(), v1.ToUnity(), ucolor);
            UnityEngine.Debug.DrawLine(v1.ToUnity(), v2.ToUnity(), ucolor);
            UnityEngine.Debug.DrawLine(v2.ToUnity(), v0.ToUnity(), ucolor);
        }

        public void DrawCapsule(float radius, float halfHeight, int upAxis, JVector position, JQuaternion rotation, JVector color)
        {
            UnityEngine.Vector3 pos = position.ToUnity();
            UnityEngine.Quaternion rot = rotation.ToUnity();
            UnityEngine.Vector3 scale = JVector.One.ToUnity();
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityPhysicsDebuggerUtility.DebugDrawCapsule(pos, rot, scale, radius, halfHeight, upAxis, ucolor);
        }

        public void DrawCylinder(float radius, float halfHeight, int upAxis, JVector position, JQuaternion rotation, JVector color)
        {
            UnityEngine.Vector3 pos = position.ToUnity();
            UnityEngine.Quaternion rot = rotation.ToUnity();
            UnityEngine.Vector3 scale = JVector.One.ToUnity();
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityPhysicsDebuggerUtility.DebugDrawCylinder(pos, rot, scale, radius, halfHeight, upAxis, ucolor);
        }

        public void DrawCone(float radius, float height, int upAxis, JVector position, JQuaternion rotation, JVector color)
        {
            UnityEngine.Vector3 pos = position.ToUnity();
            UnityEngine.Quaternion rot = rotation.ToUnity();
            UnityEngine.Vector3 scale = JVector.One.ToUnity();
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityPhysicsDebuggerUtility.DebugDrawCone(pos, rot, scale, radius, height, upAxis, ucolor);
        }

        public void DrawPlane(JVector planeNormal, float planeConst, JVector position, JQuaternion rotation, JVector color)
        {
            UnityEngine.Vector3 pos = position.ToUnity();
            UnityEngine.Quaternion rot = rotation.ToUnity();
            UnityEngine.Vector3 scale = JVector.One.ToUnity();
            UnityEngine.Color ucolor = ColorFromVector(color);
            UnityPhysicsDebuggerUtility.DebugDrawPlane(pos, rot, scale, planeNormal.ToUnity(), planeConst, ucolor);
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
