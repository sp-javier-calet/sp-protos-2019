using System;
using System.Collections;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public class UnityPhysicsDebugger : PhysicsDebugger
    {
        /*public override DebugDrawModes DebugMode
        {
            get;
            set;
        }*/

        public override void DrawLine(JVector start, JVector end)
        {
            UnityEngine.Color color = UnityEngine.Color.green;
            UnityEngine.Debug.DrawLine(start.ToUnity(), end.ToUnity(), color);
        }

        public override void DrawPoint(JVector pos)
        {
            UnityEngine.Color color = UnityEngine.Color.green;
            UnityPhysicsDebuggerUtility.DebugDrawSphere(pos.ToUnity(), UnityEngine.Quaternion.identity, UnityEngine.Vector3.one, UnityEngine.Vector3.one * 1, color);
        }

        public override void DrawTriangle(JVector pos1, JVector pos2, JVector pos3)
        {
            UnityEngine.Color uicolor = UnityEngine.Color.green;
            UnityEngine.Debug.DrawLine(pos1.ToUnity(), pos2.ToUnity(), uicolor);
            UnityEngine.Debug.DrawLine(pos2.ToUnity(), pos3.ToUnity(), uicolor);
            UnityEngine.Debug.DrawLine(pos3.ToUnity(), pos1.ToUnity(), uicolor);
        }

        /*public void DrawLine(ref JVector from, ref JVector to, ref JVector fromColor)
        {
            UnityEngine.Color color = new UnityEngine.Color(fromColor.X, fromColor.Y, fromColor.Z);
            UnityEngine.Debug.DrawLine(from.ToUnity(), to.ToUnity(), color);
        }

        public void DrawLine(ref JVector from, ref JVector to, ref JVector fromColor, ref JVector toColor)
        {
            UnityEngine.Color color = new UnityEngine.Color(fromColor.X, fromColor.Y, fromColor.Z);
            UnityEngine.Debug.DrawLine(from.ToUnity(), to.ToUnity(), color);
        }

        public override void DrawBox(ref JVector bbMin, ref JVector bbMax, ref JVector color)
        {
            BM.Matrix matrix = BM.Matrix.Identity;
            JVector halfSize = new JVector(bbMax.X - bbMin.X, bbMax.Y - bbMin.Y, bbMax.Z - bbMin.Z);
            halfSize /= 2;
            matrix.Origin = new JVector(bbMin.X + halfSize.X, bbMin.Y + halfSize.Y, bbMin.Z + halfSize.Z);
            DrawBox(ref bbMin, ref bbMax, ref matrix, ref color);
        }

        /*public void DrawBox(ref JVector bbMin, ref JVector bbMax, ref BM.Matrix trans, ref JVector color)
        {
            UnityEngine.Vector3 pos = UnityModelExtensions.ExtractTranslationFromMatrix(ref trans);
            UnityEngine.Quaternion rot = UnityModelExtensions.ExtractRotationFromMatrix(ref trans);
            UnityEngine.Vector3 scale = UnityModelExtensions.ExtractScaleFromMatrix(ref trans);
            UnityEngine.Vector3 size = (bbMax - bbMin).ToUnity();
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawBox(pos, rot, scale, size, c);
        }

        public void DrawSphere(ref JVector p, float radius, ref JVector color)
        {
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawSphere(p.ToUnity(), UnityEngine.Quaternion.identity, UnityEngine.Vector3.one, UnityEngine.Vector3.one * radius, c);
        }

        public override void DrawSphere(float radius, ref BM.Matrix trans, ref JVector color)
        {
            UnityEngine.Vector3 pos = UnityModelExtensions.ExtractTranslationFromMatrix(ref trans);
            UnityEngine.Quaternion rot = UnityModelExtensions.ExtractRotationFromMatrix(ref trans);
            UnityEngine.Vector3 scale = UnityModelExtensions.ExtractScaleFromMatrix(ref trans);
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawSphere(pos, rot, scale, UnityEngine.Vector3.one * radius, c);
        }

        public void DrawTriangle(ref JVector v0, ref JVector v1, ref JVector v2, ref JVector n0, ref JVector n1, ref JVector n2, ref JVector color, float alpha)
        {
            UnityEngine.Color uicolor = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityEngine.Debug.DrawLine(v0.ToUnity(), v1.ToUnity(), uicolor);
            UnityEngine.Debug.DrawLine(v1.ToUnity(), v2.ToUnity(), uicolor);
            UnityEngine.Debug.DrawLine(v2.ToUnity(), v0.ToUnity(), uicolor);

        }

        public void DrawTriangle(ref JVector v0, ref JVector v1, ref JVector v2, ref JVector color, float alpha)
        {
            UnityEngine.Color uicolor = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityEngine.Debug.DrawLine(v0.ToUnity(), v1.ToUnity(), uicolor);
            UnityEngine.Debug.DrawLine(v1.ToUnity(), v2.ToUnity(), uicolor);
            UnityEngine.Debug.DrawLine(v2.ToUnity(), v0.ToUnity(), uicolor);
        }

        public void DrawContactPoint(ref JVector pointOnB, ref JVector normalOnB, float distance, int lifeTime, ref JVector color)
        {
            UnityEngine.Debug.LogError("Not implemented");
        }

        public void ReportErrorWarning(String warningString)
        {
            UnityEngine.Debug.LogError(warningString);
        }

        public void Draw3dText(ref JVector location, String textString)
        {
            UnityEngine.Debug.LogError("Not implemented");
        }

        public override void DrawAabb(ref JVector from, ref JVector to, ref JVector color)
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

        public void DrawArc(ref JVector center, ref JVector normal, ref JVector axis, float radiusA, float radiusB, float minAngle, float maxAngle,
                            ref JVector color, bool drawSect)
        {
            UnityEngine.Debug.LogError("Not implemented");
        }

        public void DrawArc(ref JVector center, ref JVector normal, ref JVector axis, float radiusA, float radiusB, float minAngle, float maxAngle,
                            ref JVector color, bool drawSect, float stepDegrees)
        {
            UnityEngine.Color col = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawArc(center.ToUnity(), normal.ToUnity(), axis.ToUnity(), radiusA, radiusB, minAngle, maxAngle, col, drawSect, stepDegrees);
        }

        public void DrawSpherePatch(ref JVector center, ref JVector up, ref JVector axis, float radius,
                                    float minTh, float maxTh, float minPs, float maxPs, ref JVector color)
        {
            UnityEngine.Debug.LogError("Not implemented");

        }

        public void DrawSpherePatch(ref JVector center, ref JVector up, ref JVector axis, float radius,
                                    float minTh, float maxTh, float minPs, float maxPs, ref JVector color, float stepDegrees)
        {
            UnityEngine.Debug.LogError("Not implemented");
        }

        public void DrawCapsule(float radius, float halfHeight, int upAxis, ref BM.Matrix trans, ref JVector color)
        {
            UnityEngine.Vector3 pos = UnityModelExtensions.ExtractTranslationFromMatrix(ref trans);
            UnityEngine.Quaternion rot = UnityModelExtensions.ExtractRotationFromMatrix(ref trans);
            UnityEngine.Vector3 scale = UnityModelExtensions.ExtractScaleFromMatrix(ref trans);
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawCapsule(pos, rot, scale, radius, halfHeight, upAxis, c);
        }

        public void DrawCylinder(float radius, float halfHeight, int upAxis, ref BM.Matrix trans, ref JVector color)
        {
            UnityEngine.Vector3 pos = UnityModelExtensions.ExtractTranslationFromMatrix(ref trans);
            UnityEngine.Quaternion rot = UnityModelExtensions.ExtractRotationFromMatrix(ref trans);
            UnityEngine.Vector3 scale = UnityModelExtensions.ExtractScaleFromMatrix(ref trans);
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawCylinder(pos, rot, scale, radius, halfHeight, upAxis, c);
        }

        public void DrawCone(float radius, float height, int upAxis, ref BM.Matrix trans, ref JVector color)
        {
            UnityEngine.Vector3 pos = UnityModelExtensions.ExtractTranslationFromMatrix(ref trans);
            UnityEngine.Quaternion rot = UnityModelExtensions.ExtractRotationFromMatrix(ref trans);
            UnityEngine.Vector3 scale = UnityModelExtensions.ExtractScaleFromMatrix(ref trans);
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawCone(pos, rot, scale, radius, height, upAxis, c);
        }

        public void DrawPlane(ref JVector planeNormal, float planeConst, ref BM.Matrix trans, ref JVector color)
        {
            UnityEngine.Vector3 pos = UnityModelExtensions.ExtractTranslationFromMatrix(ref trans);
            UnityEngine.Quaternion rot = UnityModelExtensions.ExtractRotationFromMatrix(ref trans);
            UnityEngine.Vector3 scale = UnityModelExtensions.ExtractScaleFromMatrix(ref trans);
            UnityEngine.Color c = new UnityEngine.Color(color.X, color.Y, color.Z);
            UnityPhysicsDebuggerUtility.DebugDrawPlane(pos, rot, scale, planeNormal.ToUnity(), planeConst, c);
        }
//*/

        public override void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public override void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        public override void Log(object message, params object[] arguments)
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

        public override void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public override void LogWarning(object message, params object[] arguments)
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

        public override void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public override void LogError(object message, params object[] arguments)
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
