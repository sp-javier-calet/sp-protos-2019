using SocialPoint.GUIControl;

public class MyData : UIScrollRectCellData
{
    public string Name;
    public string Description;

    public MyData(string name, string description, string prefab)
    {
        Name = name;
        Description = description;
        Prefab = prefab;
    }
}