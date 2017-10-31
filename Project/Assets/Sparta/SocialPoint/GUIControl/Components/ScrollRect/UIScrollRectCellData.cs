using UnityEngine;
using System;

namespace SocialPoint.GUIControl
{
    public class UIScrollRectCellData
    {
        public string UID;

        public int PrefabIndex;
        public Vector2 Size;
        public Vector2 AccumulatedSize;

        public UIScrollRectCellData()
        {
            // TODO perhaps we can use a numeric GUID to optimize
            UID = Guid.NewGuid().ToString();
        }
    }
}
