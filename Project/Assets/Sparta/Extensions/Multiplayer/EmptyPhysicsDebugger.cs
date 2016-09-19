using System;
using System.Collections;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public class EmptyPhysicsDebugger : PhysicsDebugger
    {
        /*public override DebugDrawModes DebugMode
        {
            get;
            set;
        }*/

        public override void DrawLine(JVector start, JVector end)
        {
        }

        public override void DrawPoint(JVector pos)
        {
        }

        public override void DrawTriangle(JVector pos1, JVector pos2, JVector pos3)
        {
        }

        /*
        public override void DrawLine(ref JVector from, ref JVector to, ref JVector fromColor)
        {
        }

        public override void DrawLine(ref JVector from, ref JVector to, ref JVector fromColor, ref JVector toColor)
        {
        }

        public override void DrawBox(ref JVector bbMin, ref JVector bbMax, ref JVector color)
        {
        }

        public override void DrawBox(ref JVector bbMin, ref JVector bbMax, ref JMatrix trans, ref JVector color)
        {
        }

        public override void DrawSphere(ref JVector p, float radius, ref JVector color)
        {
        }

        public override void DrawSphere(float radius, ref JMatrix trans, ref JVector color)
        {
        }

        public override void DrawTriangle(ref JVector v0, ref JVector v1, ref JVector v2, ref JVector n0, ref JVector n1, ref JVector n2, ref JVector color, float alpha)
        {
        }

        public override void DrawTriangle(ref JVector v0, ref JVector v1, ref JVector v2, ref JVector color, float alpha)
        {
        }

        public override void DrawContactPoint(ref JVector pointOnB, ref JVector normalOnB, float distance, int lifeTime, ref JVector color)
        {
        }

        public override void ReportErrorWarning(String warningString)
        {
        }

        public override void Draw3dText(ref JVector location, String textString)
        {
        }

        public override void DrawAabb(ref JVector from, ref JVector to, ref JVector color)
        {
        }

        public override void DrawTransform(ref JMatrix trans, float orthoLen)
        {
        }

        public override void DrawArc(ref JVector center, ref JVector normal, ref JVector axis, float radiusA, float radiusB, float minAngle, float maxAngle,
                                     ref JVector color, bool drawSect)
        {
        }

        public override void DrawArc(ref JVector center, ref JVector normal, ref JVector axis, float radiusA, float radiusB, float minAngle, float maxAngle,
                                     ref JVector color, bool drawSect, float stepDegrees)
        {
        }

        public override void DrawSpherePatch(ref JVector center, ref JVector up, ref JVector axis, float radius,
                                             float minTh, float maxTh, float minPs, float maxPs, ref JVector color)
        {
        }

        public override void DrawSpherePatch(ref JVector center, ref JVector up, ref JVector axis, float radius,
                                             float minTh, float maxTh, float minPs, float maxPs, ref JVector color, float stepDegrees)
        {
        }

        public override void DrawCapsule(float radius, float halfHeight, int upAxis, ref JMatrix trans, ref JVector color)
        {
        }

        public override void DrawCylinder(float radius, float halfHeight, int upAxis, ref JMatrix trans, ref JVector color)
        {
        }

        public override void DrawCone(float radius, float height, int upAxis, ref JMatrix trans, ref JVector color)
        {
        }

        public override void DrawPlane(ref JVector planeNormal, float planeConst, ref JMatrix trans, ref JVector color)
        {
        }
//*/

        public override void Log(string message)
        {
        }

        public override void Log(object message)
        {
        }

        public override void Log(object message, params object[] arguments)
        {
        }

        public override void LogFormat(string message, params object[] arguments)
        {
        }

        public override void LogWarning(string message)
        {
        }

        public override void LogWarning(object message)
        {
        }

        public override void LogWarning(object message, params object[] arguments)
        {
        }

        public override void LogWarningFormat(string message, params object[] arguments)
        {
        }

        public override void LogError(string message)
        {
        }

        public override void LogError(object message)
        {
        }

        public override void LogError(object message, params object[] arguments)
        {
        }

        public override void LogErrorFormat(string message, params object[] arguments)
        {
        }

        public override void Assert(bool condition)
        {
        }

        public override void Assert(bool condition, object message)
        {
        }
    }
}
