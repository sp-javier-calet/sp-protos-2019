//-----------------------------------------------------------------------
// TooltipTextExample.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using UnityEngine;
using SocialPoint.GUIControl;
using TMPro;

public class TooltipTextExample : SPTooltipViewController
{
    [SerializeField]
    TextMeshProUGUI _infoText;

    public override void SetTooltipInfo(BaseTooltipData data)
    {
        if(!(data is SimpleTooltipData tooltipData))
        {
            return;
        }

        _infoText.text = tooltipData.Text;
    }
}
