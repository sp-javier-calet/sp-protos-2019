using UnityEngine;
using UnityEditor;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.LevelCaptureTool
{
    public sealed class LevelCaptureToolGuiWindow : TLWindow
    {
        public GuiLevelCaptureToolView cpView;

        static LevelCaptureToolGuiWindow        _instance;
        public static LevelCaptureToolGuiWindow instance { get { return _instance; } private set { _instance = value; } }
        
        public void Init()
        {
            titleContent.text = "Level Capturing";

            var model = ScriptableObject.CreateInstance<GuiLevelCaptureToolModel>();
            cpView = new GuiLevelCaptureToolView(this, model);
            AddView(cpView);
            cpView.SetController(new GUILevelCaptureToolController(cpView, model));
        }
        
        [MenuItem("Tools/Level Capture")]
        public static void GetOrCreateWindow()
        {
            if(instance == null)
            {
                instance = ScriptableObject.CreateInstance<LevelCaptureToolGuiWindow>();
                instance.Show();
            }
            else
            {
                instance.Focus();
            }
        }

        public void OnEnable()
        {
            if(instance == null)
            {
                instance = this;
                instance.Init();
                instance.LoadView(instance.cpView);
            }
        }
    }
}