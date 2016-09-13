using System;
using System.Collections;
using BulletSharp;
using BM = BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    public class EmptyPhysicsDebugger : PhysicsDebugger
    {
        public override DebugDrawModes DebugMode
        {
            get;
            set;
        }

        public override void DrawLine(ref BM.Vector3 from, ref BM.Vector3 to, ref BM.Vector3 fromColor)
        {
        }

        public override void DrawLine(ref BM.Vector3 from, ref BM.Vector3 to, ref BM.Vector3 fromColor, ref BM.Vector3 toColor)
        {
        }

        public override void DrawBox(ref BM.Vector3 bbMin, ref BM.Vector3 bbMax, ref BM.Vector3 color)
        {
        }

        public override void DrawBox(ref BM.Vector3 bbMin, ref BM.Vector3 bbMax, ref BM.Matrix trans, ref BM.Vector3 color)
        {
        }

        public override void DrawSphere(ref BM.Vector3 p, float radius, ref BM.Vector3 color)
        {
        }

        public override void DrawSphere(float radius, ref BM.Matrix trans, ref BM.Vector3 color)
        {
        }

        public override void DrawTriangle(ref BM.Vector3 v0, ref BM.Vector3 v1, ref BM.Vector3 v2, ref BM.Vector3 n0, ref BM.Vector3 n1, ref BM.Vector3 n2, ref BM.Vector3 color, float alpha)
        {
        }

        public override void DrawTriangle(ref BM.Vector3 v0, ref BM.Vector3 v1, ref BM.Vector3 v2, ref BM.Vector3 color, float alpha)
        {
        }

        public override void DrawContactPoint(ref BM.Vector3 pointOnB, ref BM.Vector3 normalOnB, float distance, int lifeTime, ref BM.Vector3 color)
        {
        }

        public override void ReportErrorWarning(String warningString)
        {
        }

        public override void Draw3dText(ref BM.Vector3 location, String textString)
        {
        }

        public override void DrawAabb(ref BM.Vector3 from, ref BM.Vector3 to, ref BM.Vector3 color)
        {
        }

        public override void DrawTransform(ref BM.Matrix trans, float orthoLen)
        {
        }

        public override void DrawArc(ref BM.Vector3 center, ref BM.Vector3 normal, ref BM.Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle,
                                     ref BM.Vector3 color, bool drawSect)
        {
        }

        public override void DrawArc(ref BM.Vector3 center, ref BM.Vector3 normal, ref BM.Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle,
                                     ref BM.Vector3 color, bool drawSect, float stepDegrees)
        {
        }

        public override void DrawSpherePatch(ref BM.Vector3 center, ref BM.Vector3 up, ref BM.Vector3 axis, float radius,
                                             float minTh, float maxTh, float minPs, float maxPs, ref BM.Vector3 color)
        {
        }

        public override void DrawSpherePatch(ref BM.Vector3 center, ref BM.Vector3 up, ref BM.Vector3 axis, float radius,
                                             float minTh, float maxTh, float minPs, float maxPs, ref BM.Vector3 color, float stepDegrees)
        {
        }

        public override void DrawCapsule(float radius, float halfHeight, int upAxis, ref BM.Matrix trans, ref BM.Vector3 color)
        {
        }

        public override void DrawCylinder(float radius, float halfHeight, int upAxis, ref BM.Matrix trans, ref BM.Vector3 color)
        {
        }

        public override void DrawCone(float radius, float height, int upAxis, ref BM.Matrix trans, ref BM.Vector3 color)
        {
        }

        public override void DrawPlane(ref BM.Vector3 planeNormal, float planeConst, ref BM.Matrix trans, ref BM.Vector3 color)
        {
        }


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
