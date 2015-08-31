using System;
using UnityEngine.UI;
using UnityEngine.Assertions;
using SocialPoint.Locale;
using SocialPoint.Utils;
using SocialPoint.Alert;
using SocialPoint.GUI;

public class InvalidSecurityTokenPopupController : UIViewController
{
    private const string RestartGameKey = "restart_game";
    private const string RestartGameMessageKey = "restart_game_message";
    private const string YesKey = "yes";
    private const string NoKey = "no";


    public Text TitleLabel;
    public Text MessageLabel;
    public Text SignatureLabel;
    public Button ContactButton;
    public Button RestartButton;

    public Localization Localization;

    public string ContactEmail;
    public string Subject;
    public string DefaultBody;

    public Action Restart;

    public InvalidSecurityTokenPopupController()
    {
    }

    public string MessageText
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

    public string TitleText
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

    public string ContactButtonText
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

    public string RestartButtonText
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

    public void OnContactButtonPressed()
    {
        EmailUtils.SendEmail(ContactEmail, Subject, DefaultBody);
    }

    public void OnRestartButtonPressed()
    {
        var restartAlert = new AlertView();
        Assert.IsNotNull(Localization, "Localization is null");
        restartAlert.Title = new LocalizedString(RestartGameKey, "Restart Game", Localization);
        restartAlert.Message = new LocalizedString(RestartGameMessageKey, "This action will delete all your progress and data, are you sure you want to restart your game?", Localization);
        var buttonYesLocalized = new LocalizedString(YesKey, "Yes", Localization);
        var buttonNoLocalized = new LocalizedString(NoKey, "No", Localization);
        restartAlert.Buttons = new string[]{ buttonYesLocalized, buttonNoLocalized};
        restartAlert.Show((int restartResult) => {
            if(restartResult == 0)
            {
                Restart();
            }
        });
    }
}


