﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelRootLayout : AdminPanelLayout
    {
        public AdminPanelRootLayout(AdminPanelController controller) : base(controller)
        {
            var canvasObject = new GameObject("AdminPanel");
            canvasObject.transform.SetParent(controller.transform);
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
