using UnityEditor;
using UnityEngine;

namespace SocialPoint.Utils
{
    public static class UnityGameWindowUtils 
    {
        public static Vector2 GetMainGameViewSize()
        {
            //Creates a game window. Only works if there isn't one already.
            EditorApplication.ExecuteMenuItem("Window/Game");

            System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
            return (Vector2)Res;
        }
    }
}
