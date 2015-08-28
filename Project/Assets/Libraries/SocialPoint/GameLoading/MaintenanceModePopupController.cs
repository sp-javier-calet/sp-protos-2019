using UnityEngine.UI;
using SocialPoint.GUI;

public class MaintenanceModePopupController : UIViewController
{
    public Text TitleLabel;
    public Text MessageLabel;
    public Text SignatureLabel;

    public MaintenanceModePopupController()
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
}


