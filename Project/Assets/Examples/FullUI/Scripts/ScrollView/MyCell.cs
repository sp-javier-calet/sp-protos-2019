using SocialPoint.GUIControl;

public class MyCell: UIScrollRectCellItem<MyData>
{
    public SPText NameText;
    public SPText DescriptionText;

    public override void Show()
    {
        if(NameText != null)
        {
            NameText.text = _index + " -- " + _data.Name;
        }

        if(DescriptionText != null)
        {
            DescriptionText.text = _data.Description;
        }
    }
}