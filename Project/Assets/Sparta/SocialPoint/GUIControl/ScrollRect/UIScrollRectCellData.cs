using UnityEngine;
using System;

namespace SocialPoint.GUIControl
{
    public class UIScrollRectCellData
    {
        public int UID;

        public int PrefabIndex;
        public Vector2 Size;
        public Vector2 AccumulatedSize;

        void CreateUID()
        {
            UID = GetHashCode();
        }

        public UIScrollRectCellData()
        {
            CreateUID();
        }
    }
}
