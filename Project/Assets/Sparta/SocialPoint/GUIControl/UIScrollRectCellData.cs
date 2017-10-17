namespace SocialPoint.GUIControl
{
    public class UIScrollRectCellData
    {
        public string PrefabName;
        public float PrefabWidth;
        public float PrefabHeight;
        public float PrefabStartPosition;
        public float PrefabAcumulatedWidth;
        public float PrefabAcumulatedHeight;

        public void SetupPrefabSizes(float prefabWidth, float prefabHeight)
        {
            PrefabWidth = prefabWidth;
            PrefabHeight = prefabHeight;
        }

        public void SetupAcumulatedPrefabSizes(float prefabAcumulatedWidth, float prefabAcumulatedHeight)
        {
            PrefabAcumulatedWidth = prefabAcumulatedWidth;
            PrefabAcumulatedHeight = prefabAcumulatedHeight;
        }

        public float PrefabTotalAcumulatedWidth
        {
            get
            {
                return PrefabAcumulatedWidth + PrefabWidth;
            }
        }
    }
}
