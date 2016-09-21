using UnityEngine;
using UnityEditor;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryWebWindow : EditorWindow
    {
        public static GrayboxLibraryWebWindow window;
        public static GrayboxLibraryWebWindow Launch()
        {
            window = (GrayboxLibraryWebWindow)ScriptableObject.CreateInstance<GrayboxLibraryWebWindow>();
            window.ShowPopup();
            window.Focus();
            return window;
        }

        public void Draw(Rect viewRect)
        {
            window.position = viewRect;
        }
    }
}