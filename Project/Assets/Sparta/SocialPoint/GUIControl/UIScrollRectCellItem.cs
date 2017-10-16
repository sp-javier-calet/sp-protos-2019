using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public abstract class UIScrollRectCellItem<UIScrollRectCellData> : MonoBehaviour 
    {
        protected int _index;
        protected UIScrollRectCellData _data;

        public void UpdateData(int index, UIScrollRectCellData data)
        {
            if(data != null)
            {
                _index = index;
                _data = data;

                Show();
            }
        }

        public abstract void Show();
    }
}