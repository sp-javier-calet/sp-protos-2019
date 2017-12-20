using UnityEngine;
using UnityEditor;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryWebWindow : EditorWindow
    {
        public static GrayboxLibraryWebWindow Window;

        public static GrayboxLibraryWebWindow Launch()
        {
            Window = (GrayboxLibraryWebWindow)ScriptableObject.CreateInstance<GrayboxLibraryWebWindow>();
            Window.ShowPopup();
            Window.Focus();
            return Window;
        }

        public void Draw(Rect viewRect)
        {
            Window.position = viewRect;
        }
    }
}