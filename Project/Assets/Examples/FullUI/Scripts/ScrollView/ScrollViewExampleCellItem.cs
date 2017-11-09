﻿using SocialPoint.GUIControl;

public class ScrollViewExampleCellItem: UIScrollRectCellItem<ScrollViewExampleCellData>
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