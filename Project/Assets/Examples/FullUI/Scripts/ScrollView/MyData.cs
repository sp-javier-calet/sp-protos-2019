using SocialPoint.GUIControl;

public class MyData : UIScrollRectCellData
{
    public string Name;
    public string Description;

    public MyData(string name, string description, int prefabIndex)
    {
        Name = name;
        Description = description;
        PrefabIndex = prefabIndex;
    }
}