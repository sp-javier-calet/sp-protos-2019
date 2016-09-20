using System.Collections;
using Jitter;
using Jitter.LinearMath;

public interface IPhysicsDebugger : IDebugDrawer
{
    void DrawLine(JVector from, JVector to, JVector color);

    void DrawBox(JVector position, JQuaternion rotation, JVector size, JVector color);

    void DrawSphere(JVector p, float radius, JVector color);

    void DrawTriangle(JVector v0, JVector v1, JVector v2, JVector n0, JVector n1, JVector n2, JVector color, float alpha);

    void DrawTriangle(JVector v0, JVector v1, JVector v2, JVector color);

    void DrawCapsule(float radius, float halfHeight, int upAxis, JVector position, JQuaternion rotation, JVector color);

    void DrawCylinder(float radius, float halfHeight, int upAxis, JVector position, JQuaternion rotation, JVector color);

    void DrawCone(float radius, float height, int upAxis, JVector position, JQuaternion rotation, JVector color);

    void DrawPlane(JVector planeNormal, float planeConst, JVector position, JQuaternion rotation, JVector color);

    void Log(string message);

    void Log(object message);

    void Log(object message, params object[] arguments);

    void LogFormat(string message, params object[] arguments);

    void LogWarning(string message);

    void LogWarning(object message);

    void LogWarning(object message, params object[] arguments);

    void LogWarningFormat(string message, params object[] arguments);

    void LogError(string message);

    void LogError(object message);

    void LogError(object message, params object[] arguments);

    void LogErrorFormat(string message, params object[] arguments);

    void Assert(bool condition);

    void Assert(bool condition, object message);
}
