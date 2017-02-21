#if ADMIN_PANEL

using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public RawImage CreateImage(Texture2D texture, Vector2 size)
        {
            var rectTransform = CreateUIObject("Admin Panel - Image", Parent);
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = size.x;
            layoutElement.preferredHeight = size.y;
            var image = rectTransform.gameObject.AddComponent<RawImage>();
            image.texture = texture;
            return image;
        }

        public RawImage CreateImage(Vector2 size)
        {
            return CreateImage(null, size);
        }
    }
}

#endif
