#if ADMIN_PANEL

using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public FoldoutLayout CreateFoldoutLayout(string label)
        {
            return CreateFoldoutLayout(label, false, ButtonColor.Default, true);
        }

        public FoldoutLayout CreateFoldoutLayout(string label, ButtonColor color)
        {
            return CreateFoldoutLayout(label, false, color, true);
        }

        public FoldoutLayout CreateFoldoutLayout(string label, bool status)
        {
            return CreateFoldoutLayout(label, status, ButtonColor.Default, true);
        }

        public FoldoutLayout CreateFoldoutLayout(string label, bool status, ButtonColor color)
        {
            return CreateFoldoutLayout(label, status, color, true);
        }

        public FoldoutLayout CreateFoldoutLayout(string label, bool status, ButtonColor color, bool enabled)
        {
            return new FoldoutLayout(this, label, status, color, enabled);
        }

        public sealed class FoldoutLayout : AdminPanelLayout
        {
            RectTransform _root;

            bool Visible
            {
                set
                {
                    _root.gameObject.SetActive(value);
                }
            }

            public FoldoutLayout(AdminPanelLayout parentLayout, string label, bool status, ButtonColor buttonColor, bool enabled) : base(parentLayout)
            {
                parentLayout.CreateFoldoutButton(label, status, buttonColor, value => {
                    Visible = value;
                }, enabled);
                
                _root = CreateUIObject("Foldout Layout", parentLayout.Parent);

                var layoutElement = _root.gameObject.AddComponent<LayoutElement>();
                layoutElement.flexibleWidth = 1;
                layoutElement.flexibleHeight = 1;

                var layoutGroup = _root.gameObject.AddComponent<VerticalLayoutGroup>();
                layoutGroup.padding = new RectOffset(DefaultPadding, DefaultPadding, DefaultPadding, DefaultPadding);
                layoutGroup.spacing = DefaultMargin;
                layoutGroup.childForceExpandHeight = true;
                layoutGroup.childForceExpandWidth = false;

                Parent = _root;
                Visible = status;
            }
        }
    }
}

#endif
