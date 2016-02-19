using SocialPoint.GUIControl;
using UnityEngine.UI;
using System;

public class MaintenanceModePopupController : UIViewController
{
    public Text TitleLabel;
    public Text MessageLabel;
    public Text SignatureLabel;
    public Text ButtonLabel;
    public Action Dismissed;  

    public MaintenanceModePopupController()
    {
    }

    public void OnButtonClicked()
    {
        Hide();
        if(Dismissed != null)
        {
            Dismissed();
        }
    }

    public string MessageText
    {
        set
        {
            if(MessageLabel != null)
            {
                MessageLabel.gameObject.SetActive(value == null);
                MessageLabel.text = value;
            }
        }
    }

    public string TitleText
    {
        set
        {
            if(TitleLabel != null)
            {
                TitleLabel.gameObject.SetActive(value == null);
                TitleLabel.text = value;
            }
        }
    }

    public string ButtonText
    {
        set
        {
            if(ButtonLabel != null)
            {
                var button = ButtonLabel.GetComponentInParent<Button>();
                if(button != null && button.gameObject != null)
                {
                    button.gameObject.SetActive(value == null);
                }
                ButtonLabel.text = value;
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
}


