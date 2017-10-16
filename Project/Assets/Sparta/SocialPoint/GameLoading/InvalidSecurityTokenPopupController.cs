using System;
using SocialPoint.Alert;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.Helpshift;
using SocialPoint.Locale;
using SocialPoint.Utils;
using UnityEngine.UI;

public sealed class InvalidSecurityTokenPopupController : UIViewController
{
    const string RestartGameTitleKey = "gameloading.restart_game_title";
    const string RestartGameTitleDef = "Restart Game";
    const string RestartGameMessageKey = "gameloading.restart_game_message";
    const string RestartGameMessageDef = "This action will delete all your progress and data, are you sure you want to restart your game?";
    const string RestartGameYesButtonKey = "gameloading.restart_game_yes_button";
    const string RestartGameYesButtonDef = "Yes";
    const string RestartGameNoButtonKey = "gameloading.restart_game_no_button";
    const string RestartGameNoButtonDef = "No";
    const string TitleKey = "gameloading.invalid_security_token_title";
    const string TitleDef = "Invalid Security token";
    const string MessageKey = "gameloading.invalid_security_token_message";
    const string MessageDef = "The game state has been corrupted and cannot recoverered automatically.\nPlease contact our support team or restart the game.";
    const string ContactButtonKey = "gameloading.invalid_security_token_contact_button";
    const string ContactButtonDef = "Contact";
    const string RestartButtonKey = "gameloading.invalid_security_token_restart_button";
    const string RestartButtonDef = "Restart";

    public Text TitleLabel;
    public Text MessageLabel;
    public Text SignatureLabel;
    public Button ContactButton;
    public Button RestartButton;

    public Localization Localization;
    public IAlertView AlertView;

    public string ContactEmail;
    public string Subject;
    public string DefaultBody;

    public Action Restart;

    string MessageText
    {
        set
        {
            if(MessageLabel != null)
            {
                if(value == null)
                {
                    MessageLabel.gameObject.SetActive(false);
                }
                else
                {
                    MessageLabel.gameObject.SetActive(true);
                    MessageLabel.text = value;
                }
            }
        }
    }

    string TitleText
    {
        set
        {
            if(TitleLabel != null)
            {
                if(value == null)
                {
                    TitleLabel.gameObject.SetActive(false);
                }
                else
                {
                    TitleLabel.gameObject.SetActive(true);
                    TitleLabel.text = value;
                }
            }
        }
    }

    public string Signature
    {
        set
        {
            if(SignatureLabel != null)
            {
                if(value == null)
                {
                    SignatureLabel.gameObject.SetActive(false);
                }
                else
                {
                    SignatureLabel.gameObject.SetActive(true);
                    SignatureLabel.text = value;
                }
            }
        }
    }

    string ContactButtonText
    {
        set
        {
            if(ContactButton != null)
            {
                if(value == null)
                {
                    TitleLabel.gameObject.SetActive(false);
                }
                else
                {
                    ContactButton.gameObject.SetActive(true);
                    ContactButton.GetComponentInChildren<Text>().text = value;
                }
            }
        }
    }

    string RestartButtonText
    {
        set
        {
            if(RestartButton != null)
            {
                if(value == null)
                {
                    RestartButton.gameObject.SetActive(false);
                }
                else
                {
                    RestartButton.gameObject.SetActive(true);
                    RestartButton.GetComponentInChildren<Text>().text = value;
                }
            }
        }
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        DebugUtils.Assert(Localization != null, "Localization can not be null");
        DebugUtils.Assert(AlertView != null, "AlertView can not be null");

        TitleText = Localization.Get(TitleKey, TitleDef);
        MessageText = Localization.Get(MessageKey, MessageDef);
        ContactButtonText = Localization.Get(ContactButtonKey, ContactButtonDef);
        RestartButtonText = Localization.Get(RestartButtonKey, RestartButtonDef);
    }

    public void OnContactButtonPressed()
    {
        Services.Instance.Resolve<IHelpshift>().ShowConversation();
    }

    public void OnRestartButtonPressed()
    {
        var restartAlert = (IAlertView)AlertView.Clone();
        restartAlert.Title = Localization.Get(RestartGameTitleKey, RestartGameTitleDef);
        restartAlert.Message = Localization.Get(RestartGameMessageKey, RestartGameMessageDef);
        var buttonYesLocalized = Localization.Get(RestartGameYesButtonKey, RestartGameYesButtonDef);
        var buttonNoLocalized = Localization.Get(RestartGameNoButtonKey, RestartGameNoButtonDef);
        restartAlert.Buttons = new []{ buttonYesLocalized, buttonNoLocalized };
        restartAlert.Show(restartResult => {
            if(restartResult == 0)
            {
                Restart();
            }
        });
    }
}
