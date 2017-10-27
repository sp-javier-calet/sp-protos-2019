using SocialPoint.GUIControl;

public class MyCell: UIScrollRectCellItem<MyData>
{
    public SPText NameText;
    public SPText DescriptionText;

    public override void ShowData()
    {
        if(NameText != null)
        {
            NameText.text = _data.Name;
        }

        if(DescriptionText != null)
        {
            DescriptionText.text = _data.Description;
        }
    }
}