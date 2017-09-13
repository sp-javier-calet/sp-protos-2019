using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.Alert
{
    public sealed class UnityAlertViewController : BaseUnityAlertViewController
    {
        public Text TitleLabel;
        public Text MessageLabel;
        public Text SignatureLabel;
        public InputField InputLabel;
        public RectTransform Buttons;
        public GameObject ButtonPrefab;
        public int ButtonSeparation;

        public override event ResultDelegate Result;

        void OnButtonClicked(int i)
        {
            if(Result != null)
            {
                Result(i);
            }
        }

        public override string MessageText
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

        public override string TitleText
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

        public override string Signature
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

        public override bool InputEnabled
        {
            set
            {
                if(InputLabel != null)
                {
                    InputLabel.gameObject.SetActive(value);
                }
            }
        }

        public override string InputText
        {
            get
            {
                return InputLabel != null ? InputLabel.text : string.Empty;
            }
        }

        public override string[] ButtonTitles
        {
            set
            {
                if(Buttons == null)
                {
                    throw new MissingComponentException("Buttons container widget not set.");
                }
                if(value.Length == 0)
                {
                    throw new MissingComponentException("No buttons defined.");
                }
                if(ButtonPrefab == null)
                {
                    throw new MissingComponentException("Could not load button prefab.");
                }

                for(int i = 0; i < value.Length; ++i)
                {
                    var btnGo = Instantiate(ButtonPrefab);

                    #if UNITY_EDITOR
                    btnGo.name = string.Format("{0} ({1})", ButtonPrefab, value[i]);
                    #endif

                    btnGo.transform.SetParent(Buttons.transform);
                    btnGo.transform.localScale = Vector3.one;

                    var btn = btnGo.GetComponent<UnityAlertViewButton>();
                    if(btn != null)
                    {
                        btn.Text = value[i];
                        btn.Position = i;
                        btn.Clicked += OnButtonClicked;
                    }
                }
                Buttons.gameObject.SetActive(true);
            }
        }
    }
}
