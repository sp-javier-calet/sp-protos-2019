using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SocialPoint.Alert
{
    public class IosAlertViewBridge : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Data
        {
            [MarshalAs(UnmanagedType.LPTStr)]
            public string Message;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string Title;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string Signature;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string Buttons;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string ObjectName;
            [MarshalAs(UnmanagedType.SysInt)]
            public bool Input;
        };

        public delegate void ResultDelegate(int result, string input);

        ResultDelegate _resultDelegate;

        public void ResultMessage(string msg)
        {
            if(_resultDelegate == null)
            {
                return;
            }
            var i = msg.IndexOf(' ');
            int result;
            if(i != -1)
            {
                int.TryParse(msg.Substring(0, i), out result);
                _resultDelegate(result, msg.Substring(i + 1));
            }
            else
            {
                int.TryParse(msg, out result);
                _resultDelegate(result, null);
            }
        }

        public void Show(Data data, ResultDelegate dlg)
        {
            _resultDelegate = dlg;
            data.ObjectName = gameObject.name;
            SPUnityAlertViewShow(data);
        }

        public void Hide()
        {
            SPUnityAlertViewHide();
        }

        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern void SPUnityAlertViewShow(Data data);
        #else
        public static void SPUnityAlertViewShow(Data data)
        {
        }
        #endif
        
        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern void SPUnityAlertViewHide();
        #else
        public static void SPUnityAlertViewHide()
        {
        }
        #endif
    }

    public class IosAlertView : IAlertView
    {
        IosAlertViewBridge.Data _data;
        IosAlertViewBridge _bridge;
        const string kGameObjectName = "SocialPoint.AlertView.IosAlertView";

        public IosAlertView()
        {
            if(Application.platform != RuntimePlatform.IPhonePlayer)
            {
                throw new NotImplementedException("IosAlertView is only supported on Ios");
            }
        }

        public string Message
        {
            set
            {
                _data.Message = value;
            }
        }

        public string Title
        {
            set
            {
                _data.Title = value;
            }
        }

        public string Signature
        {
            set
            {
                _data.Signature = value;
            }
        }

        public string[] Buttons
        {
            set
            {
                _data.Buttons = string.Join("|", value);
            }
        }

        public bool Input
        {
            set
            {
                _data.Input = value;
            }
        }

        string _inputText;

        public string InputText
        {
            get
            {
                return _inputText;
            }
        }

        public void Show(ResultDelegate dlg)
        {
            var go = new GameObject();
            go.name = kGameObjectName;
            _bridge = go.AddComponent<IosAlertViewBridge>();
            _bridge.Show(_data, (result, inputText) => {
                _inputText = inputText;
                if(dlg != null)
                {
                    dlg(result);
                }
            });
        }

        public void OnResponse(string data)
        {
            _inputText = data;
        }

        public void Dispose()
        {
            _bridge.Hide();
            UnityEngine.Object.Destroy(_bridge.gameObject);
        }

        public object Clone()
        {
            var clone = new IosAlertView();
            clone._data = _data;
            return clone;
        }

    }
}
