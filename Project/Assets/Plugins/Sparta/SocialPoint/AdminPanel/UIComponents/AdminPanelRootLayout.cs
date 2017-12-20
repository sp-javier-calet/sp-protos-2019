#if ADMIN_PANEL

using UnityEngine;

namespace SocialPoint.AdminPanel
{
    public sealed class AdminPanelRootLayout : AdminPanelLayout
    {
        public AdminPanelRootLayout(IPanelController controller, Transform trans=null) : base(controller)
        {
            var canvasObject = new GameObject("AdminPanel");
            if(trans != null)
            {
                canvasObject.transform.SetParent(trans, false);
            }
            var rectTransform = canvasObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            Parent = rectTransform;
        }
    }
}

#endif
