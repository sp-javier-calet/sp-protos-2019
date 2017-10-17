using SocialPoint.GUIControl;

public class MyData : UIScrollRectCellData
{
    public int Id;
    public string Name;
    public string Description;

    public MyData(int id, string name, string description, string prefabName)
    {
        Id = id;
        Name = name;
        Description = description;
        PrefabName = prefabName;
    }
}