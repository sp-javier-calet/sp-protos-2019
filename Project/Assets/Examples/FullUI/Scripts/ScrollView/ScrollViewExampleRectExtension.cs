//-----------------------------------------------------------------------
// ScrollViewExampleRectExtension.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.GUIControl;

public class ScrollViewExampleRectExtension : UIScrollRectExtension<ScrollViewExampleCellData, ScrollViewExampleCellItem>
{
    public void Init(UIScrollRectBaseDataSource<ScrollViewExampleCellData> dataSource)
    {
        DataSource = dataSource;

        // Start populating Scroll Rect cells with data
        LoadData();
    }
}
