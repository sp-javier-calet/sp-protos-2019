using UnityEngine;
using System;

namespace SocialPoint.Alert
{
    public abstract class BaseUnityAlertViewController : MonoBehaviour
    {
        public abstract event ResultDelegate Result;

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public abstract string MessageText{ set; }
        public abstract string TitleText{ set; }
        public abstract bool InputEnabled{ set; }
        public abstract string InputText{ get; }
        public abstract string[] ButtonTitles{ set; }
    }

    public delegate void UnityAlertViewButtonClicked(int position);
    public delegate void UnityAlertViewGameObjectDelegate(GameObject go);


    public class UnityAlertView : IAlertView {

        BaseUnityAlertViewController _controller;
        ResultDelegate _delegate;

        public static string DefaultPrefab = "AlertView";
        public static UnityAlertViewGameObjectDelegate SetupDelegate;
        public static UnityAlertViewGameObjectDelegate ShowDelegate;
        public static UnityAlertViewGameObjectDelegate HideDelegate;
        
        public UnityAlertView(string prefab = null)
        {
            if(prefab == null)
            {
                prefab = DefaultPrefab;
            }

            var res = Resources.Load(prefab);
            if(res == null)
            {
                throw new MissingComponentException("Could not load prefab.");
            }
            var go = (GameObject)GameObject.Instantiate(res);
            go.name = prefab;
            _controller = go.GetComponent(typeof(BaseUnityAlertViewController)) as BaseUnityAlertViewController;
            if(_controller == null)
            {
                throw new MissingComponentException("Prefab does not have a BaseUnityAlertViewController component.");
            }
            Setup();
        }

        private void Setup()
        {
            _controller.Result += OnControllerResult;
            Input = false;
            if(SetupDelegate != null)
            {
                SetupDelegate(_controller.gameObject);
            }
        }

        public void Dispose()
        {
            MonoBehaviour.Destroy(_controller.gameObject);
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

        public string Message
        {
            set
            {
                _controller.MessageText = value;
            }
        }
        
        public string Title
        {
            set
            {
                _controller.TitleText = value;
            }
        }
        
        public string[] Buttons
        {
            set
            {
                _controller.ButtonTitles = value;
            }
        }
        
        public bool Input
        {
            set
            {
                _controller.InputEnabled = value;
            }
        }
        
        public string InputText
        {
            get
            {
                return _controller.InputText;
            }
        }
        
        public void Show(ResultDelegate dlg)
        {
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
            var clone = new UnityAlertView();
            clone._controller = GameObject.Instantiate(_controller) as BaseUnityAlertViewController;
            clone.Setup();
            return clone;
        }
    }

}