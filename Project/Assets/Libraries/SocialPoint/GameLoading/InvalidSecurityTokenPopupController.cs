using System;
using UnityEngine.UI;
using SocialPoint.Utils;
using SocialPoint.Alert;
using SocialPoint.GUI;

public class InvalidSecurityTokenPopupController : UIViewController
{
    public Text TitleLabel;
    public Text MessageLabel;
    public Text SignatureLabel;
    public Button ContactButton;
    public Button RestartButton;

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
        restartAlert.Title = "Restart Game";
        restartAlert.Message = "This action will delete all your progress and data, are you sure you want to restart your game?";
        restartAlert.Buttons = new string[]{ "YES", "NO"};
        restartAlert.Show((int restartResult) => {
            if(restartResult == 0)
            {
                Restart();
            }
        });
    }
}


