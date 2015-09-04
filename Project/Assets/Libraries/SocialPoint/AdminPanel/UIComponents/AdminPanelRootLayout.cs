using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelRootLayout : AdminPanelLayout
    {
        private AspectRatioFitter _aspectFitter;
        public AdminPanelRootLayout(AdminPanelController controller) : base(controller)
        {
            var canvasObject = new GameObject("AdminPanel - Canvas");
            var rectTransform = canvasObject.AddComponent<RectTransform>();
            
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            
            _aspectFitter = canvasObject.AddComponent<AspectRatioFitter>();
            _aspectFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            _aspectFitter.aspectRatio = (float)(Screen.width) / Screen.height;
            
            var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.referenceResolution = new Vector2(480, 320);
            
            canvasObject.AddComponent<GraphicRaycaster>();

            Parent = rectTransform;
        }

        public void SetActive(bool active)
        {
            base.SetActive(active);
            if(_aspectFitter != null)
            {
                _aspectFitter.aspectRatio = (float)(Screen.width) / Screen.height;
            }
        }
    }
}
