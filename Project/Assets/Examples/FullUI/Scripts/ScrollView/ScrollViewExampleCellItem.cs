//-----------------------------------------------------------------------
// ScrollViewExampleCellItem.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.GUIControl;
using TMPro;

public class ScrollViewExampleCellItem : UIScrollRectCellItem<ScrollViewExampleCellData>
{
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;

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
