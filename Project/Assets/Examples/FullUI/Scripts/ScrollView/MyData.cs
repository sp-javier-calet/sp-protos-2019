using SocialPoint.GUIControl;

public class MyData : 
{
    public int Id;
    public string Name;
    public string Description;
    public string PrefabName;
    public float PrefabWidth;
    public float PrefabHeight;

    public MyData(int id, string name, string description, string prefabName)
    {
        Id = id;
        Name = name;
        Description = description;
        PrefabName = prefabName;
        PrefabWidth = 0f;
        PrefabHeight = 0f;
    }
}