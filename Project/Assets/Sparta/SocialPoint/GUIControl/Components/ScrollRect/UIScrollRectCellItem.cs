using UnityEngine;

namespace SocialPoint.GUIControl
{
    public abstract class UIScrollRectCellItem<UIScrollRectCellData> : MonoBehaviour 
    {
        [SerializeField]
        bool _expandable;

        protected UIScrollRectCellData _data;

        public void UpdateData(UIScrollRectCellData data)
        {
            if(data != null)
            {
                _data = data;

                Show();
            }
        }

        public abstract void Show();
    }
}