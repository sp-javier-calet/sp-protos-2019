﻿using System;
using SocialPoint.GUIControl;
using UnityEngine;
using UnityEngine.UI;

public class GameLoadingErrorPopupController : UIViewController
{
    [HideInInspector]
    public string Text;

    public event Action Dismissed;

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