namespace SocialPoint.GUIControl
{
    public class UIScrollRectCellData
    {
        public string PrefabName;
        public float CellWidth;
        public float CellHeight;
        public float PrefabStartPosition;
        public float CellAccumulatedWidth;
        public float CellAccumulatedHeight;

        public void SetupPrefabSizes(float prefabWidth, float prefabHeight)
        {
            CellWidth = prefabWidth;
            CellHeight = prefabHeight;
        }

        public void SetupAcumulatedPrefabSizes(float cellAccumulatedWidth, float cellAccumulatedHeight)
        {
            CellAccumulatedWidth = cellAccumulatedWidth;
            CellAccumulatedHeight = cellAccumulatedHeight;
        }
    }
}
