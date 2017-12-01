using UnityEngine;
using SocialPoint.GUIControl;
using UnityEngine.UI;

public class TooltipTextExample : SPTooltipViewController 
{
    [SerializeField]
    Text _infoText;

    public override void SetTooltipInfo()
    {
        _infoText.text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus eget efficitur dolor. Proin et neque nisl. Sed eget ligula lacinia, maximus velit quis, sagittis leo. Aliquam id ultricies justo, sed ornare lacus. Nunc tempor felis in orci varius semper vitae eleifend mi.";
    }

}
