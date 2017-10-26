using UnityEngine;

namespace SocialPoint.GUIControl
{
    public abstract class UIScrollRectCellItem<TCellData> : MonoBehaviour  where TCellData : UIScrollRectCellData 
    {
        [SerializeField]
        bool _expandable;

        protected TCellData _data;

        public int Index
        {
            get
            {
                return _data.Index;
            }
        }

        public void UpdateData(TCellData data)
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