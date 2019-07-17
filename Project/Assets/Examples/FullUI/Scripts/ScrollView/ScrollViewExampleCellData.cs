//-----------------------------------------------------------------------
// ScrollViewExampleCellData.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

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
