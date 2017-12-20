using System;
using System.Collections;
using Jitter.LinearMath;

namespace SocialPoint.Physics
{
    public class EmptyPhysicsDebugger : IPhysicsDebugger
    {
        public void DrawLine(JVector start, JVector end)
        {
        }

        public void DrawPoint(JVector pos)
        {
        }

        public void DrawTriangle(JVector pos1, JVector pos2, JVector pos3)
        {
        }

        public void DrawLine(JVector from, JVector to, JVector color)
        {
            
        }

        public void DrawBox(JVector position, JQuaternion rotation, JVector size, JVector color)
        {
        }

        public void DrawSphere(JVector p, float radius, JVector color)
        {
        }

        public void DrawTriangle(JVector v0, JVector v1, JVector v2, JVector n0, JVector n1, JVector n2, JVector color, float alpha)
        {
        }

        public void DrawTriangle(JVector v0, JVector v1, JVector v2, JVector color)
        {
        }

        public void DrawCapsule(float radius, float halfHeight, int upAxis, JVector position, JQuaternion rotation, JVector color)
        {
        }

        public void DrawCylinder(float radius, float halfHeight, int upAxis, JVector position, JQuaternion rotation, JVector color)
        {
        }

        public void DrawCone(float radius, float height, int upAxis, JVector position, JQuaternion rotation, JVector color)
        {
        }

        public void DrawPlane(JVector planeNormal, float planeConst, JVector position, JQuaternion rotation, JVector color)
        {
        }
    }
}
