using System;
using System.Collections;
using BulletSharp;
using BM = BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    public class UnityPhysicsDebugger : PhysicsDebugger
    {
        public override DebugDrawModes DebugMode
        {
            get;
            set;
        }

        public override void DrawLine(ref BM.Vector3 from, ref BM.Vector3 to, ref BM.Vector3 fromColor)
        {
            UnityEngine.Color color = new UnityEngine.Color(fromColor.X, fromColor.Y, fromColor.Z);
            UnityEngine.Debug.DrawLine(from.ToUnity(), to.ToUnity(), color);
        }

        public override void DrawLine(ref BM.Vector3 from, ref BM.Vector3 to, ref BM.Vector3 fromColor, ref BM.Vector3 toColor)
        {
            UnityEngine.Color color = new UnityEngine.Color(fromColor.X, fromColor.Y, fromColor.Z);
            UnityEngine.Debug.DrawLine(from.ToUnity(), to.ToUnity(), color);
        }

        public override void DrawBox(ref BM.Vector3 bbMin, ref BM.Vector3 bbMax, ref BM.Vector3 color)
        {
            BM.Matrix matrix = BM.Matrix.Identity;
            BM.Vector3 halfSize = new BM.Vector3(bbMax.X - bbMin.X, bbMax.Y - bbMin.Y, bbMax.Z - bbMin.Z);
            halfSize /= 2;
            matrix.Origin = new BM.Vector3(bbMin.X + halfSize.X, bbMin.Y + halfSize.Y, bbMin.Z + halfSize.Z);
            DrawBox(ref bbMin, ref bbMax, ref matrix, ref color);
        }

        public override void DrawBox(ref BM.Vector3 bbMin, ref BM.Vector3 bbMax, ref BM.Matrix trans, ref BM.Vector3 color)
        {
            UnityEngine.Vector3 pos = UnityModelExtensions.ExtractTranslationFromMatrix(ref trans);
            UnityEngine.Quaternion rot = UnityModelExtensions.ExtractRotationFromMatrix(ref trans);
            UnityEngine.Vector3 scale = UnityModelExtensions.ExtractScaleFromMatrix(ref trans);
            UnityEngine.Vector3 size = (bbMax - bbMin).ToUnity();
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawBox(pos, rot, scale, size, c);
        }

        public override void DrawSphere(ref BM.Vector3 p, float radius, ref BM.Vector3 color)
        {
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawSphere(p.ToUnity(), UnityEngine.Quaternion.identity, UnityEngine.Vector3.one, UnityEngine.Vector3.one * radius, c);
        }

        public override void DrawSphere(float radius, ref BM.Matrix trans, ref BM.Vector3 color)
        {
            UnityEngine.Vector3 pos = UnityModelExtensions.ExtractTranslationFromMatrix(ref trans);
            UnityEngine.Quaternion rot = UnityModelExtensions.ExtractRotationFromMatrix(ref trans);
            UnityEngine.Vector3 scale = UnityModelExtensions.ExtractScaleFromMatrix(ref trans);
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawSphere(pos, rot, scale, UnityEngine.Vector3.one * radius, c);
        }

        public override void DrawTriangle(ref BM.Vector3 v0, ref BM.Vector3 v1, ref BM.Vector3 v2, ref BM.Vector3 n0, ref BM.Vector3 n1, ref BM.Vector3 n2, ref BM.Vector3 color, float alpha)
        {
            UnityEngine.Color uicolor = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityEngine.Debug.DrawLine(v0.ToUnity(), v1.ToUnity(), uicolor);
            UnityEngine.Debug.DrawLine(v1.ToUnity(), v2.ToUnity(), uicolor);
            UnityEngine.Debug.DrawLine(v2.ToUnity(), v0.ToUnity(), uicolor);

        }

        public override void DrawTriangle(ref BM.Vector3 v0, ref BM.Vector3 v1, ref BM.Vector3 v2, ref BM.Vector3 color, float alpha)
        {
            UnityEngine.Color uicolor = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityEngine.Debug.DrawLine(v0.ToUnity(), v1.ToUnity(), uicolor);
            UnityEngine.Debug.DrawLine(v1.ToUnity(), v2.ToUnity(), uicolor);
            UnityEngine.Debug.DrawLine(v2.ToUnity(), v0.ToUnity(), uicolor);
        }

        public override void DrawContactPoint(ref BM.Vector3 pointOnB, ref BM.Vector3 normalOnB, float distance, int lifeTime, ref BM.Vector3 color)
        {
            UnityEngine.Debug.LogError("Not implemented");
        }

        public override void ReportErrorWarning(String warningString)
        {
            UnityEngine.Debug.LogError(warningString);
        }

        public override void Draw3dText(ref BM.Vector3 location, String textString)
        {
            UnityEngine.Debug.LogError("Not implemented");
        }

        public override void DrawAabb(ref BM.Vector3 from, ref BM.Vector3 to, ref BM.Vector3 color)
        {
            DrawBox(ref from, ref to, ref color);
        }

        public override void DrawTransform(ref BM.Matrix trans, float orthoLen)
        {
            UnityEngine.Vector3 pos = UnityModelExtensions.ExtractTranslationFromMatrix(ref trans);
            UnityEngine.Quaternion rot = UnityModelExtensions.ExtractRotationFromMatrix(ref trans);
            UnityEngine.Vector3 p1 = pos + rot * UnityEngine.Vector3.up * orthoLen;
            UnityEngine.Vector3 p2 = pos - rot * UnityEngine.Vector3.up * orthoLen;
            UnityEngine.Debug.DrawLine(p1, p2);
            p1 = pos + rot * UnityEngine.Vector3.right * orthoLen;
            p2 = pos - rot * UnityEngine.Vector3.right * orthoLen;
            UnityEngine.Debug.DrawLine(p1, p2);
            p1 = pos + rot * UnityEngine.Vector3.forward * orthoLen;
            p2 = pos - rot * UnityEngine.Vector3.forward * orthoLen;
            UnityEngine.Debug.DrawLine(p1, p2);
        }

        public override void DrawArc(ref BM.Vector3 center, ref BM.Vector3 normal, ref BM.Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle,
                                     ref BM.Vector3 color, bool drawSect)
        {
            UnityEngine.Debug.LogError("Not implemented");
        }

        public override void DrawArc(ref BM.Vector3 center, ref BM.Vector3 normal, ref BM.Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle,
                                     ref BM.Vector3 color, bool drawSect, float stepDegrees)
        {
            UnityEngine.Color col = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawArc(center.ToUnity(), normal.ToUnity(), axis.ToUnity(), radiusA, radiusB, minAngle, maxAngle, col, drawSect, stepDegrees);
        }

        public override void DrawSpherePatch(ref BM.Vector3 center, ref BM.Vector3 up, ref BM.Vector3 axis, float radius,
                                             float minTh, float maxTh, float minPs, float maxPs, ref BM.Vector3 color)
        {
            UnityEngine.Debug.LogError("Not implemented");

        }

        public override void DrawSpherePatch(ref BM.Vector3 center, ref BM.Vector3 up, ref BM.Vector3 axis, float radius,
                                             float minTh, float maxTh, float minPs, float maxPs, ref BM.Vector3 color, float stepDegrees)
        {
            UnityEngine.Debug.LogError("Not implemented");
        }

        public override void DrawCapsule(float radius, float halfHeight, int upAxis, ref BM.Matrix trans, ref BM.Vector3 color)
        {
            UnityEngine.Vector3 pos = UnityModelExtensions.ExtractTranslationFromMatrix(ref trans);
            UnityEngine.Quaternion rot = UnityModelExtensions.ExtractRotationFromMatrix(ref trans);
            UnityEngine.Vector3 scale = UnityModelExtensions.ExtractScaleFromMatrix(ref trans);
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawCapsule(pos, rot, scale, radius, halfHeight, upAxis, c);
        }

        public override void DrawCylinder(float radius, float halfHeight, int upAxis, ref BM.Matrix trans, ref BM.Vector3 color)
        {
            UnityEngine.Vector3 pos = UnityModelExtensions.ExtractTranslationFromMatrix(ref trans);
            UnityEngine.Quaternion rot = UnityModelExtensions.ExtractRotationFromMatrix(ref trans);
            UnityEngine.Vector3 scale = UnityModelExtensions.ExtractScaleFromMatrix(ref trans);
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawCylinder(pos, rot, scale, radius, halfHeight, upAxis, c);
        }

        public override void DrawCone(float radius, float height, int upAxis, ref BM.Matrix trans, ref BM.Vector3 color)
        {
            UnityEngine.Vector3 pos = UnityModelExtensions.ExtractTranslationFromMatrix(ref trans);
            UnityEngine.Quaternion rot = UnityModelExtensions.ExtractRotationFromMatrix(ref trans);
            UnityEngine.Vector3 scale = UnityModelExtensions.ExtractScaleFromMatrix(ref trans);
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawCone(pos, rot, scale, radius, height, upAxis, c);
        }

        public override void DrawPlane(ref BM.Vector3 planeNormal, float planeConst, ref BM.Matrix trans, ref BM.Vector3 color)
        {
            UnityEngine.Vector3 pos = UnityModelExtensions.ExtractTranslationFromMatrix(ref trans);
            UnityEngine.Quaternion rot = UnityModelExtensions.ExtractRotationFromMatrix(ref trans);
            UnityEngine.Vector3 scale = UnityModelExtensions.ExtractScaleFromMatrix(ref trans);
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawPlane(pos, rot, scale, planeNormal.ToUnity(), planeConst, c);
        }


        public override void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public override void Log(DebugType debugType, object message)
        {
            UnityEngine.Debug.Log(message);
        }

        public override void Log(DebugType debugType, object message, params object[] arguments)
        {
            UnityEngine.Debug.LogFormat(message.ToString(), arguments);
        }

        public override void LogFormat(string message, params object[] arguments)
        {
            UnityEngine.Debug.LogFormat(message, arguments);
        }

        public override void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public override void LogWarning(DebugType debugType, object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public override void LogWarning(DebugType debugType, object message, params object[] arguments)
        {
            UnityEngine.Debug.LogWarningFormat(message.ToString(), arguments);
        }

        public override void LogWarningFormat(string message, params object[] arguments)
        {
            UnityEngine.Debug.LogWarningFormat(message, arguments);
        }

        public override void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public override void LogError(DebugType debugType, object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public override void LogError(DebugType debugType, object message, params object[] arguments)
        {
            UnityEngine.Debug.LogErrorFormat(message.ToString(), arguments);
        }

        public override void LogErrorFormat(string message, params object[] arguments)
        {
            UnityEngine.Debug.LogErrorFormat(message, arguments);
        }

        public override void Assert(bool condition)
        {
            UnityEngine.Debug.Assert(condition);
        }

        public override void Assert(bool condition, object message)
        {
            UnityEngine.Debug.Assert(condition, message);
        }
    }
}
