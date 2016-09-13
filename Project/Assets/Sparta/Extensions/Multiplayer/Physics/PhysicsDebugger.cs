using System.Collections;
using BulletSharp;

public abstract class PhysicsDebugger : DebugDraw
{
    public abstract void Log(string message);

    public abstract void Log(object message);

    public abstract void Log(object message, params object[] arguments);

    public abstract void LogFormat(string message, params object[] arguments);

    public abstract void LogWarning(string message);

    public abstract void LogWarning(object message);

    public abstract void LogWarning(object message, params object[] arguments);

    public abstract void LogWarningFormat(string message, params object[] arguments);

    public abstract void LogError(string message);

    public abstract void LogError(object message);

    public abstract void LogError(object message, params object[] arguments);

    public abstract void LogErrorFormat(string message, params object[] arguments);

    public abstract void Assert(bool condition);

    public abstract void Assert(bool condition, object message);
}
