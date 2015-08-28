using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public void CreateLabel(string label)
        {
            var rectTransform = CreateUIObject("Admin Panel - Label", Parent);

            //rectTransform.sizeDelta = new Vector2(1.0f, 25.0f);
            
            var text = rectTransform.gameObject.AddComponent<Text>();
            text.text = label;
            text.font = Resources.FindObjectsOfTypeAll<Font>()[0];
            text.fontSize = DefaultFontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperCenter;
            
            LayoutElement layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;
        }
    }
}
