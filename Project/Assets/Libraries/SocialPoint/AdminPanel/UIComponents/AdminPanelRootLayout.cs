using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelRootLayout : AdminPanelLayout
    {
        public AdminPanelRootLayout(AdminPanelController controller) : base(controller)
        {
            var canvasObject = new GameObject("AdminPanel - Canvas");
            var rectTransform = canvasObject.AddComponent<RectTransform>();
            
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            
            var aspectFitter = canvasObject.AddComponent<AspectRatioFitter>();
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            aspectFitter.aspectRatio = 1.6f;
            
            var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.referenceResolution = new Vector2(480, 320);
            
            canvasObject.AddComponent<GraphicRaycaster>();

            Parent = rectTransform;
        }
    }
}
