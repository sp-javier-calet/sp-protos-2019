using SocialPoint.GUIControl;
using UnityEngine;

namespace SocialPoint.Alert
{
    public abstract class BaseUnityAlertViewController : UIViewController
    {
        public abstract event ResultDelegate Result;

        public abstract string MessageText{ set; }

        public abstract string TitleText{ set; }

        public abstract string Signature{ set; }

        public abstract bool InputEnabled{ set; }

        public abstract string InputText{ get; }

        public abstract string[] ButtonTitles{ set; }
    }

    public delegate void UnityAlertViewButtonClicked(int position);
    public delegate void UnityAlertViewGameObjectDelegate(GameObject go);


    public class UnityAlertView : IAlertView
    {
        readonly GameObject _prefab;
        BaseUnityAlertViewController _controller;
        ResultDelegate _delegate;

        public static string DefaultPrefab = "AlertView";
        public static UnityAlertViewGameObjectDelegate SetupDelegate;
        public static UnityAlertViewGameObjectDelegate ShowDelegate;
        public static UnityAlertViewGameObjectDelegate HideDelegate;

        public UnityAlertView(GameObject prefab = null)
        {
            if(prefab == null)
            {
                prefab = Resources.Load(DefaultPrefab) as GameObject;
            }
            _prefab = prefab;
            if(_prefab == null)
            {
                throw new MissingComponentException("Could not load prefab.");
            }
        }

        public void Dispose()
        {
            if(_controller != null)
            {
                _controller.Close();
            }
        }

        void OnControllerResult(int result)
        {
            if(_delegate != null)
            {
                var dlg = _delegate;
                _delegate = null;
                dlg(result);
            }
            if(HideDelegate != null)
            {
                HideDelegate(_controller.gameObject);
            }
            else
            {
                _controller.Hide();
            }
        }

        string _message;

        public string Message
        {
            set
            {
                _message = value;
            }
        }

        string _title;

        public string Title
        {
            set
            {
                _title = value;
            }
        }

        string _signature;

        public string Signature
        {
            set
            {
                _signature = value;
            }
        }

        string[] _buttons;

        public string[] Buttons
        {
            set
            {
                _buttons = value;
            }
        }

        bool _input;

        public bool Input
        {
            set
            {
                _input = value;
            }
        }

        public string InputText
        {
            get
            {
                return _controller != null ? _controller.InputText : null;
            }
        }

        public void Show(ResultDelegate dlg)
        {
            if(_controller == null)
            {
                var go = Object.Instantiate(_prefab);
                if(go != null)
                {
                    go.name = _prefab.name;
                    if(SetupDelegate != null)
                    {
                        SetupDelegate(go);
                    }
                    _controller = go.GetComponent(typeof(BaseUnityAlertViewController)) as BaseUnityAlertViewController;
                }
            }
            if(_controller == null)
            {
                throw new MissingComponentException("Prefab does not have a BaseUnityAlertViewController component.");
            }
            _controller.MessageText = _message;
            _controller.TitleText = _title;
            _controller.Signature = _signature;
            _controller.ButtonTitles = _buttons;
            _controller.InputEnabled = _input;
            _controller.Result += OnControllerResult;
            _delegate += dlg;
            if(ShowDelegate != null)
            {
                ShowDelegate(_controller.gameObject);
            }
            else
            {
                _controller.Show();
            }
        }

        public object Clone()
        {
            return new UnityAlertView(_prefab);
        }
    }
}
