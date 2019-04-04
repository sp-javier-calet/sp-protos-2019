//-----------------------------------------------------------------------
// TooltipTextExample.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using UnityEngine;
using SocialPoint.GUIControl;
using UnityEngine.UI;

public class TooltipTextExample : SPTooltipViewController 
{
    [SerializeField]
    Text _infoText;

    public override void SetTooltipInfo(BaseTooltipData data)
    {
        var tooltipData = data as SimpleTooltipData;
        if(tooltipData == null)
        {
            return;
        }
        
        _infoText.text = tooltipData.Text;
    }
}
