namespace SocialPoint.GUIControl
{
    public class UIScrollRectCellData
    {
        public string PrefabName;
        public float PrefabWidth;
        public float PrefabHeight;

        public void SetupPrefabSizes(float prefabWidth, float prefabHeight)
        {
            PrefabWidth = prefabWidth;
            PrefabHeight = prefabHeight;
        }
    }
}
