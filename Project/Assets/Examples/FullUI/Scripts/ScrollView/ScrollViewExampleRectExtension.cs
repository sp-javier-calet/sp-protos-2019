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
