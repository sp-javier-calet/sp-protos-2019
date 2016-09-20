using System;
using System.Collections;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
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

        public void Log(string message)
        {
        }

        public void Log(object message)
        {
        }

        public void Log(object message, params object[] arguments)
        {
        }

        public void LogFormat(string message, params object[] arguments)
        {
        }

        public void LogWarning(string message)
        {
        }

        public void LogWarning(object message)
        {
        }

        public void LogWarning(object message, params object[] arguments)
        {
        }

        public void LogWarningFormat(string message, params object[] arguments)
        {
        }

        public void LogError(string message)
        {
        }

        public void LogError(object message)
        {
        }

        public void LogError(object message, params object[] arguments)
        {
        }

        public void LogErrorFormat(string message, params object[] arguments)
        {
        }

        public void Assert(bool condition)
        {
        }

        public void Assert(bool condition, object message)
        {
        }
    }
}
