using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public abstract class UIScrollRectCellExtension<TData> : MonoBehaviour 
    {
//        int _index;
//        TData _data;

        public void UpdateCell(int index, TData data)
        {
            if(data != null)
            {
//                _index = index;
//                _data = data;
            }
        }
    }
}