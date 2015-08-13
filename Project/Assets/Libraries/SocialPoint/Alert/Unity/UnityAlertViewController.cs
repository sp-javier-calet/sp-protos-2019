using UnityEngine;
using System;
using UnityEngine.UI;

namespace SocialPoint.Alert
{
    public class UnityAlertViewController : BaseUnityAlertViewController {

        public Text TitleLabel;
        public Text MessageLabel;
        public Text InputLabel;
        public RectTransform Buttons;
        public GameObject ButtonPrefab;
        public int ButtonSeparation = 0;

        public UnityAlertViewController()
        {
        }

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
        
        public override bool InputEnabled
        {
            set
            {
                if(InputLabel != null)
                {
                    InputLabel.enabled = value;
                }
            }
        }
        
        public override string InputText
        {
            get
            {
                if(InputLabel != null)
                {
                    return InputLabel.text;
                }
                return string.Empty;
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

                float diffY = 0.0f;

                float y = 0;
                for(int i=0; i<value.Length; ++i)
                {
                    var btnGo = ((GameObject)Instantiate(ButtonPrefab));
                    btnGo.name = string.Format("{0} ({1})", ButtonPrefab, value[i]);
                    btnGo.transform.parent = Buttons.transform;
                    btnGo.transform.localScale = Vector3.one;

                    var btn = btnGo.GetComponent<UnityAlertViewButton>();
                    if(btn != null)
                    {
                        btn.Text = value[i];
                        btn.Position = i;
                        btn.Clicked += OnButtonClicked;
                    }

                    var trans = btn.GetComponent<RectTransform>();
                    if(trans != null)
                    {
                        float sy = trans.sizeDelta.y;


                        y += sy;
                        if(i < value.Length - 1)
                        {
                            y += ButtonSeparation;
                        }
                    }
                }
                Buttons.gameObject.SetActive(true);
                diffY = y - Buttons.sizeDelta.y;
                var size = Buttons.sizeDelta;
                size.y += diffY;
                Buttons.sizeDelta = size;
            }
        }
    }
}