//-----------------------------------------------------------------------
// SimpleExampleTooltipTrigger.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.GUIControl;

public class SimpleTooltipData : BaseTooltipData
{
    public string Text;
}

public class SimpleExampleTooltipTrigger : SPTooltipTrigger
{
    protected override BaseTooltipData Data
    {
        get
        {
            var data = new SimpleTooltipData()
            {
                Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus eget efficitur dolor. Proin et neque nisl. Sed eget ligula lacinia, maximus velit quis, sagittis leo. Aliquam id ultricies justo, sed ornare lacus. Nunc tempor felis in orci varius semper vitae eleifend mi."
            };

            return data;
        }
    }
}