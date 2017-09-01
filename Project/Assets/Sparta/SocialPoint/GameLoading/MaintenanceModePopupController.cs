using System;
using SocialPoint.GUIControl;
using UnityEngine.UI;

public sealed class MaintenanceModePopupController : UIViewController
{
    public Text TitleLabel;
    public Text MessageLabel;
    public Text SignatureLabel;
    public Text ButtonLabel;
    public Button Button;
    public Action Dismissed;

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
                MessageLabel.gameObject.SetActive(!string.IsNullOrEmpty(value));
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
                TitleLabel.gameObject.SetActive(!string.IsNullOrEmpty(value));
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
                    button.gameObject.SetActive(!string.IsNullOrEmpty(value));
                }
                ButtonLabel.text = value;
            }

            if(Button != null)
            {
                Button.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }
    }

    public string Signature
    {
        set
        {
            if(SignatureLabel != null)
            {
                SignatureLabel.gameObject.SetActive(!string.IsNullOrEmpty(value));
                SignatureLabel.text = value;
            }
        }
    }
}
