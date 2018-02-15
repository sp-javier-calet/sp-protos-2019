using SocialPoint.GUIControl;

public class ScrollViewExampleCellData : UIScrollRectCellData
{
    public string Name;
    public string Description;

    public ScrollViewExampleCellData(string name, string description, int prefabIndex)
    {
        Name = name;
        Description = description;
        PrefabIndex = prefabIndex;
    }
}