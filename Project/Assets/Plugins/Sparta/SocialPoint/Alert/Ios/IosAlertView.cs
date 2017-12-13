#if UNITY_IOS && !UNITY_EDITOR
#define IOS_DEVICE
#endif

using System;
using System.Runtime.InteropServices;
using SocialPoint.Utils;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Alert
{
    public class IosAlertView : IAlertView
    {
        [StructLayout(LayoutKind.Sequential)]
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

        #if IOS_DEVICE
        [DllImport ("__Internal")]
        public static extern void SPUnityAlertViewShow(Data data);
        #else
        public static void SPUnityAlertViewShow(Data data)
        {
        }
        #endif
        
        #if IOS_DEVICE
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

            _data = new Data {
                Message = string.Empty, Title = string.Empty, Signature = string.Empty, Buttons = string.Empty, Input = false
            };
        }

        public string Message
        {
            set
            {
                _data.Message = value ?? string.Empty;
            }
        }

        public string Title
        {
            set
            {
                _data.Title = value ?? string.Empty;
            }
        }

        public string Signature
        {
            set
            {
                _data.Signature = value ?? string.Empty;
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
