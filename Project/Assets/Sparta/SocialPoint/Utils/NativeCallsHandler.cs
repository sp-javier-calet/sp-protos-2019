#if (UNITY_ANDROID || UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
#define NATIVE_CALLHANDLER
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.Utils
{
    public sealed class NativeCallsHandler : MonoBehaviour
    {
        const string MethodName = "ReceiveNativeMessage";
        const string Separator = ";";

        IDictionary<string, EventMethodHolder> _listeners;

        public sealed class EventMethodHolder
        {
            public event Action<string> ArgMethod;
            public event Action NoArgMethod;

            public void Dispatch(string arg)
            {
                if(ArgMethod != null)
                {
                    ArgMethod(arg);
                }

                if(NoArgMethod != null)
                {
                    NoArgMethod();
                }
            }
        }

        #if NATIVE_CALLHANDLER
        
        #if UNITY_ANDROID
        const string PluginModuleName = "sp_unity_base";
        const string JavaFullClassName = "es.socialpoint.unity.base.SPNativeCallsSender";
        const string JavaFunctionInit = "Init";
        #elif (UNITY_IOS || UNITY_TVOS)
        const string PluginModuleName = "__Internal";
        #endif
        
        [System.Runtime.InteropServices.DllImport(PluginModuleName)]
        static extern void SPNativeCallsSender_Init(string gameObjectName, string methodName, string separator);

        static void NativeCallsSenderInit(string gameObjectName)
        {
            #if UNITY_ANDROID
            AndroidJavaClass nativeCallsSender = new AndroidJavaClass(JavaFullClassName);
            nativeCallsSender.CallStatic(JavaFunctionInit, gameObjectName, MethodName, Separator);
            #elif (UNITY_IOS || UNITY_TVOS)
            SPNativeCallsSender_Init(gameObjectName, MethodName, Separator);
            #endif
        }

        #else
        
        static void NativeCallsSenderInit(string gameObjectName)
        {

        }

        #endif

        void Awake()
        {
            _listeners = new Dictionary<string, EventMethodHolder>();
            gameObject.name = GetType().ToString();
            if(gameObject.transform.parent == null)
            {
                DontDestroyOnLoad(this);
            }

            NativeCallsSenderInit(gameObject.name);
        }

        public void RegisterListener(string methodName, Action method)
        {
            if(!_listeners.ContainsKey(methodName))
            {
                _listeners.Add(methodName, new EventMethodHolder());
            }

            _listeners[methodName].NoArgMethod += method;

        }

        public void RegisterListener(string methodName, Action<string> method)
        {
            if(!_listeners.ContainsKey(methodName))
            {
                _listeners.Add(methodName, new EventMethodHolder());
            }

            _listeners[methodName].ArgMethod += method;

        }

        public void RemoveListener(string methodName, Action method)
        {
            if(!_listeners.ContainsKey(methodName))
            {
                return;
            }
            _listeners[methodName].NoArgMethod -= method;
        }

        public void RemoveListener(string methodName, Action<string> method)
        {
            if(!_listeners.ContainsKey(methodName))
            {
                return;
            }
            _listeners[methodName].ArgMethod -= method;
        }

        /// <summary>
        /// Single object that receives the native calls and sends them to the listeners.
        /// </summary>
        /// <param name="message">Message. A string containing the method and argument, separated by a defined separator</param>
        public void ReceiveNativeMessage(string message)
        {
            var separatorPos = message.IndexOf(Separator);
            var methodName = message.Substring(0, separatorPos);
            var arg = String.Empty;
            if(separatorPos + 1 < message.Length)
            {
                arg = message.Substring(separatorPos + 1);
            }
            if(_listeners.ContainsKey(methodName))
            {
                _listeners[methodName].Dispatch(arg);
            }
        }
    }
}

