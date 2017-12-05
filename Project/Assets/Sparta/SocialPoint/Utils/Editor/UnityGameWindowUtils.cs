using System;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Utils
{
    public static class UnityGameWindowUtils
    {
        /// <summary>
        /// Size of the game view cannot be retrieved from Screen.width and Screen.height when the game view is hidden.
        /// </summary>
        // summa
        public static Vector2 GetMainGameViewSize()
        {
            Type type = Type.GetType("UnityEditor.GameView,UnityEditor");
            var methodInfo = type.GetMethod("GetMainGameViewTargetSize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Func<Vector2> s_GetSizeOfMainGameView = null;
            if(methodInfo != null)
            {
                s_GetSizeOfMainGameView = (Func<Vector2>)Delegate.CreateDelegate(typeof(Func<Vector2>), methodInfo);
            }
            else
            {
                Log.w("Unable to get the main game view size function");
            }

            return s_GetSizeOfMainGameView != null ? s_GetSizeOfMainGameView() : new Vector2(Screen.width, Screen.height);
        }
    }
}
