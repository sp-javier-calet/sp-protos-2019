using System;
using SocialPoint.GUIControl;
using SocialPoint.Locale;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameLoadingErrorPopupController : UIViewController
{
    [HideInInspector]
    public string Text;

    public event Action Dismissed;

    [Localize]
    public Text Message;

    protected override void OnLoad()
    {
        base.OnLoad();
        Message.text = Text;
    }

    public void OnButtonClicked()
    {
        Hide();
        if(Dismissed != null)
        {
            Dismissed();
        }
    }

}
