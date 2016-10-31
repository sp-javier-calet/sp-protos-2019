using System;
using System.Reflection;
using System.Collections.Generic;

namespace SpartaTools.Editor.Utils
{
    public class AssemblyInspector
    {
        readonly Assembly[] _assemblies;

        public AssemblyInspector()
        {
            _assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        public Type TypeByName(string type)
        {
            foreach(var assembly in _assemblies)
            {
                foreach(var t in assembly.GetTypes())
                {
                    if(t.Name == type)
                    {
                        return t;
                    }
                }
            }
            return null;
        }

        public T[] WichInherits<T>() where T : class
        {
            var list = new List<T>();
            var type = typeof(T);
            bool isInterface = type.IsInterface;
            foreach(var assembly in _assemblies)
            {
                foreach(var t in assembly.GetTypes())
                {
                    var casted = t as T;
                    if(casted != null)
                    {
                        list.Add(casted);
                    }
                }
            }

            return list.ToArray();
        }

        public Type[] WichImplements(Type interfaceType)
        {
            return WichInherits(interfaceType);
        }

        public Type[] WichInherits(Type baseType)
        {
            var list = new List<Type>();
            bool isInterface = baseType.IsInterface;
            foreach(var assembly in _assemblies)
            {
                foreach(var t in assembly.GetTypes())
                {
                    if(t.GetType().IsSubclassOf(baseType.GetType()))
                    {
                        list.Add(t);
                    }
                }
            }

            return list.ToArray();
        }

        public MethodInfo[] WithMethodAttribute<T>() where T : class
        {
            var list = new List<MethodInfo>();
            foreach(var assembly in _assemblies)
            {
                foreach(var type in assembly.GetTypes())
                {
                    foreach(var method in type.GetMethods())
                    {
                        foreach(var attribute in method.GetCustomAttributes(true))
                        {
                            if(attribute is T)
                            {
                                list.Add(method);
                            }
                        }
                    }
                }
            }

            return list.ToArray();
        }

        public void Invoke(object instance, string method)
        {
            Invoke(new object[] { instance }, method);
        }

        public void Invoke(object[] instances, string method)
        {
            foreach(var instance in instances)
            {
                BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
                var methodInfo = instance.GetType().GetMethod(method, bindFlags);
                if(methodInfo != null)
                {
                    methodInfo.Invoke(instance, null);
                }
            }
        }
    }
}