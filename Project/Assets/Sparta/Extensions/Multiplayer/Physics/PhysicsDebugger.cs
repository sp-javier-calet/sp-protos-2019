using System.Collections;
using BulletSharp;

public abstract class PhysicsDebugger : DebugDraw
{
    //[Flags]
    public enum DebugType
    {
        Error = 1,
        Warning = 2,
        Debug = 8,
        Info = 4,
        Trace = 16,
    }

    public abstract void Log(string message);

    public abstract void Log(DebugType debugType, object message);

    public abstract void Log(DebugType debugType, object message, params object[] arguments);

    public abstract void LogFormat(string message, params object[] arguments);

    public abstract void LogWarning(string message);

    public abstract void LogWarning(DebugType debugType, object message);

    public abstract void LogWarning(DebugType debugType, object message, params object[] arguments);

    public abstract void LogWarningFormat(string message, params object[] arguments);

    public abstract void LogError(string message);

    public abstract void LogError(DebugType debugType, object message);

    public abstract void LogError(DebugType debugType, object message, params object[] arguments);

    public abstract void LogErrorFormat(string message, params object[] arguments);

    public abstract void Assert(bool condition);

    public abstract void Assert(bool condition, object message);
}
