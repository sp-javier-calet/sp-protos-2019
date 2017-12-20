using System;
using System.Collections.Generic;
using System.Reflection;

public class TypeCache
{
    readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

    // If assembly name is null or empty, Type.GetType method will search the type in the current assembly.
    // Assembly name could be obtained using this code: typeof(AnyClassDefinedInAssembly).Assembly.FullName.
    public Type GetType(string typename, string assemblyName = null)
    {
        Type result;
        if(!_types.TryGetValue(typename, out result))
        {
            if(string.IsNullOrEmpty(assemblyName))
            {
                result = Type.GetType(typename);
            }
            else
            {
                result = Type.GetType(typename + ", " + assemblyName);
            }

            SocialPoint.Base.DebugUtils.Assert(result != null, string.Format("Type name {0} not found in {1} assembly",
                typename, string.IsNullOrEmpty(assemblyName) ? "current" : assemblyName));
            _types.Add(typename, result);
        }
        return result;
    }

    public void Clear()
    {
        _types.Clear();
    }
}

public class TypeCacheSingleton
{
    private static TypeCache _instance = null;

    public static TypeCache GetInstance()
    {
        if(_instance == null)
        {
            _instance = new TypeCache();
        }

        return _instance;
    }
}