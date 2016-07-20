using System;
using System.Runtime.InteropServices;
using SocialPoint.Utils;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Alert
{
    public class IosAlertView : IAlertView
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
            [MarshalAs(UnmanagedType.I1)]
            public bool Input;
        };

        public delegate void NativeResultDelegate(int result, string input);

        Data _data;
        public NativeCallsHandler NativeHandler;
        NativeResultDelegate _resultDelegate;

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
            DebugUtils.Assert(NativeHandler != null, "Handler is null, asign a NativeCallsHandler");
            NativeHandler.RegisterListener("ResultMessage", ResultMessage);

            _resultDelegate = (result, inputText) => {
                _inputText = inputText;
                if(dlg != null)
                {
                    dlg(result);
                }
            };

            SPUnityAlertViewShow(_data);
        }

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

        public void OnResponse(string data)
        {
            _inputText = data;
        }

        public void Dispose()
        {
            SPUnityAlertViewHide();
            NativeHandler.RemoveListener("ResultMessage", ResultMessage);
        }

        public object Clone()
        {
            var clone = new IosAlertView();
            clone._data = _data;
            clone.NativeHandler = NativeHandler;
            return clone;
        }

    }
}
