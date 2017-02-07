using UnityEditor;
using UnityEngine;

namespace SocialPoint.TransparentBundles
{
    public class DownloadWindow: EditorWindow
    {
        public static DownloadWindow Window;
        private static EditorClientController _controller;
        
        private static void Init()
        {
            _controller = EditorClientController.GetInstance();
        }

        public static void OpenWindow()
        {
            Window = (DownloadWindow)EditorWindow.GetWindow(typeof(DownloadWindow),true,"",false);
            Window.position = new Rect(0,0,5,5);
            Window.maxSize = new Vector2(5,5);
            Init();
        }

        void Update()
        {
            int pendingBundles = _controller.InstantiateDownloadedBundles();

            if (pendingBundles == 0)
            {
                Close();
            }
        }
    }
}
