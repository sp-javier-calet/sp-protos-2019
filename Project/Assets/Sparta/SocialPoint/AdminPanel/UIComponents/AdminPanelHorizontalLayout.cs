using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public HorizontalLayout CreateHorizontalLayout()
        {
            return new HorizontalLayout(this);
        }

        public sealed class HorizontalLayout : AdminPanelLayout
        {
            public HorizontalLayout(AdminPanelLayout parentLayout) : base(parentLayout)
            {
                var rectTrans = CreateUIObject("Horizontal Layout", parentLayout.Parent);

                HorizontalLayoutGroup layoutGroup = rectTrans.gameObject.AddComponent<HorizontalLayoutGroup>();
                layoutGroup.padding = new RectOffset(DefaultPadding, DefaultPadding, DefaultPadding, DefaultPadding);
                layoutGroup.spacing = DefaultMargin;
                layoutGroup.childForceExpandHeight = true;
                layoutGroup.childForceExpandWidth = false;

                Parent = rectTrans;
            }
        }
    }
}