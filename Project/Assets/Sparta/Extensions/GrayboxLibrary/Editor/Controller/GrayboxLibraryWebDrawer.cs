using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryWebDrawer
    {
        private object thisWindowGuiView;
        private Type webViewType;
        private ScriptableObject webView;

        public GrayboxLibraryWebDrawer(EditorWindow window, string url, Rect webViewRect)
        {
            thisWindowGuiView = typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);

            webViewType = GetTypeFromAllAssemblies("WebView");
            webView = ScriptableObject.CreateInstance(webViewType);

            if (webView != null)
            {
                webViewType.GetMethod("InitWebView").Invoke(webView, new object[] { thisWindowGuiView, (int)webViewRect.x, (int)webViewRect.y, (int)webViewRect.width, (int)webViewRect.height, true });
                webViewType.GetMethod("LoadFile").Invoke(webView, new object[] { url });
            }
        }

        private Type GetTypeFromAllAssemblies(string typeName)
        {
            Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.Name.Equals(typeName, StringComparison.CurrentCultureIgnoreCase) || type.Name.Contains('+' + typeName)) //+ check for inline classes
                        return type;
                }
            }
            return null;
        }

        public void Draw(Rect drawRect)
        {
            if (webView != null)
                webViewType.GetMethod("SetSizeAndPosition").Invoke(webView, new object[] { (int)drawRect.x, (int)drawRect.y, (int)drawRect.width, (int)drawRect.height });
        }

        public void ClearView()
        {
            if (webView != null && webViewType != null)
            {
                webViewType.GetMethod("OnDestroy").Invoke(webView, new object[] { });
                webView = null;
            }
        }

        void OnDestroy()
        {
            ClearView();
        }
    }
}