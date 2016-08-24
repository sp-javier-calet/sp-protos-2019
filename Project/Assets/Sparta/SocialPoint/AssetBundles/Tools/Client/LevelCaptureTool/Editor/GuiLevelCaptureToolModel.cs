using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.LevelCaptureTool
{
    public sealed class GuiLevelCaptureToolModel : TLModel
    {
        public bool isCapturing;
        public int numCaptures;
        public Vector2 resolution;
        public List<string> remainingCaptures;
        public RenderTexture rt;
        public Texture2D screenShot;

        public GuiLevelCaptureToolModel() : base()
        {
            Clear();
            resolution = new Vector2(1024, 768);
        }

        public void Clear()
        {
            isCapturing = false;
            numCaptures = 0;
            remainingCaptures = new List<string>();

            if(rt != null)
            {
                UnityEngine.Object.DestroyImmediate(rt);
            }
            
            if(screenShot != null)
            {
                UnityEngine.Object.DestroyImmediate(screenShot);
            }
        }
    }
}