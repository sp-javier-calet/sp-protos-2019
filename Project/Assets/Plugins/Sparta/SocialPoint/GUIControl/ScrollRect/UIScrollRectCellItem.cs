using UnityEngine;

namespace SocialPoint.GUIControl
{
    public abstract class UIScrollRectCellItem<TCellData> : UIViewController  where TCellData : UIScrollRectCellData 
    {
        [SerializeField]
        bool _expandable;

        protected TCellData _data;

        public int UID
        {
            get
            {
                return _data.UID;
            }
        }

        public void UpdateData(TCellData data)
        {
            if(data != null)
            {
                _data = data;
                ShowData();
            }
        }

        public abstract void ShowData();
    }
}