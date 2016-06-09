using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace SocialPoint.Utils
{
    public class NativeCallsHandler : MonoBehaviour
    {
        const string MethodName = "ReceiveNativeMessage";
        const string Separator = ";";

        IDictionary<string, EventMethodHolder> _listeners;

        public class EventMethodHolder
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

        #if UNITY_ANDROID
        const string PluginModuleName = "sp_unity_base";
        const string JavaFullClassName = "es.socialpoint.unity.base.SPNativeCallsSender";
        const string JavaFunctionInit = "Init";
        AndroidJavaClass _nativeCallsSender;
        #else
        const string PluginModuleName = "__Internal";
        #endif

        #if !UNITY_EDITOR
        [DllImport (PluginModuleName)]
        private static extern void SPNativeCallsSender_Init(string gameObjectName, string methodName, string separator);
        #else
        void SPNativeCallsSender_Init(string gameObjectName, string methodName, string separator)
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
            #if UNITY_IOS
            SPNativeCallsSender_Init(gameObject.name, MethodName, Separator);
            #elif UNITY_ANDROID && !UNITY_EDITOR
            print(string.Format("calling {0} with args {1}, {2}, {3}",JavaFullClassName,gameObject.name, MethodName, Separator));
            _nativeCallsSender = new AndroidJavaClass(JavaFullClassName);
            _nativeCallsSender.CallStatic(JavaFunctionInit, gameObject.name, MethodName, Separator);
            #endif
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
            else
            {
                _listeners[methodName].NoArgMethod -= method;
            } 
        }

        public void RemoveListener(string methodName, Action<string> method)
        {
            if(!_listeners.ContainsKey(methodName))
            {
                return;
            }
            else
            {
                _listeners[methodName].ArgMethod -= method;
            } 
        }

        /// <summary>
        /// Single object that receives the native calls and sends them to the listeners.
        /// </summary>
        /// <param name="message">Message. A string containing the method and argument, separated by a defined separator</param>
        public void ReceiveNativeMessage(string message)
        {
            print(string.Format("Received native message {0}", message));
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

