using UnityEngine;

namespace SocialPoint.GUIControl
{
    public class UIScrollRectCellData
    {
        public int Index { get; set; }

        public string Prefab { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 AccumulatedSize { get; set; }
    }
}
