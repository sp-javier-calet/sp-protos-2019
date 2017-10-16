using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryWebDrawer
    {
        private object _thisWindowGuiView;
        private Type _webViewType;
        private ScriptableObject _webView;

        public GrayboxLibraryWebDrawer(EditorWindow window, string url, Rect webViewRect)
        {
            _thisWindowGuiView = typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);

            _webViewType = GetTypeFromAllAssemblies("WebView");
            _webView = ScriptableObject.CreateInstance(_webViewType);

            if(_webView != null)
            {
                _webViewType.GetMethod("InitWebView").Invoke(_webView, new object[] {
                    _thisWindowGuiView,
                    (int)webViewRect.x,
                    (int)webViewRect.y,
                    (int)webViewRect.width,
                    (int)webViewRect.height,
                    true
                });
                _webViewType.GetMethod("LoadFile").Invoke(_webView, new object[] { url });
            }
        }

        private Type GetTypeFromAllAssemblies(string typeName)
        {
            Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach(Assembly assembly in assemblies)
            {
                try
                {
                    Type[] types = assembly.GetTypes();
                    foreach(Type type in types)
                    {
                        if(type.Name.Equals(typeName, StringComparison.CurrentCultureIgnoreCase) || type.Name.Contains('+' + typeName)) //+ check forinline classes
                            return type;
                    }
                }
                catch(ReflectionTypeLoadException e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
            return null;
        }

        public void Draw(Rect drawRect)
        {
            if(_webView != null)
                _webViewType.GetMethod("SetSizeAndPosition").Invoke(_webView, new object[] {
                    (int)drawRect.x,
                    (int)drawRect.y,
                    (int)drawRect.width,
                    (int)drawRect.height
                });
        }

        public void ClearView()
        {
            if(_webView != null && _webViewType != null)
            {
                _webViewType.GetMethod("OnDestroy").Invoke(_webView, new object[] { });
                _webView = null;
            }
        }

        void OnDestroy()
        {
            ClearView();
        }
    }
}