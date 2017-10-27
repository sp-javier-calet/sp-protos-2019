using UnityEngine;
using System;

namespace SocialPoint.GUIControl
{
    public class UIScrollRectCellData
    {
        public string UID { get; set; }

        public int PrefabIndex { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 AccumulatedSize { get; set; }

        public UIScrollRectCellData()
        {
            // TODO perhaps we can use a numeric GUID to optimize
            UID = Guid.NewGuid().ToString();
        }
    }
}
