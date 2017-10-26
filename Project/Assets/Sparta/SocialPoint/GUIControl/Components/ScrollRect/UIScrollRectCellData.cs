using UnityEngine;
using System;

namespace SocialPoint.GUIControl
{
    public class UIScrollRectCellData
    {
        public string UID { get; set; }

        public string Prefab { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 AccumulatedSize { get; set; }

        public UIScrollRectCellData()
        {
            Guid uid = Guid.NewGuid();
            Debug.Log("uid: " + uid);
            UID = uid.ToString();
        }
    }
}
