using System;
using UnityEngine;
using SocialPoint.Base;

namespace SocialPoint.Alert
{
    #if UNITY_ANDROID

    public delegate void ClickDelegate(int which);

    class DialogClickListener : AndroidJavaProxy
    {
        public ClickDelegate _delegate;

        public DialogClickListener(ClickDelegate dlg) : base("android.content.DialogInterface$OnClickListener")
        {
            _delegate = dlg;
        }

        void onClick(AndroidJavaObject dialog, int which)
        {
            if(_delegate != null)
            {
                _delegate(which);
            }
        }
    }

    public class AndroidAlertView : IAlertView
    {
        AndroidJavaObject _inputView;
        IntPtr _dialogPtr;

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
            AndroidContext.CurrentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => DoShow(dlg)));
        }

        static void SetButton(AndroidJavaObject builder, string text, string method, ClickDelegate dlg, int pos)
        {
            var methodId = AndroidJNI.GetMethodID(builder.GetRawClass(), method, "(Ljava/lang/CharSequence;Landroid/content/DialogInterface$OnClickListener;)Landroid/app/AlertDialog$Builder;");
            var parms = new jvalue[2];
            parms[0] = new jvalue();
            parms[0].l = AndroidJNI.NewStringUTF(text);
            parms[1] = new jvalue();
            parms[1].l = AndroidJNIHelper.CreateJavaProxy(new DialogClickListener(which => {
                if(dlg != null)
                {
                    dlg(pos);
                }
            }));
            AndroidJNI.CallObjectMethod(builder.GetRawObject(), methodId, parms);
        }

        void DoShow(ResultDelegate dlg)
        {
            var activity = AndroidContext.CurrentActivity;
            using(var builder = new AndroidJavaObject("android.app.AlertDialog$Builder", activity))
            {
                builder.Call<AndroidJavaObject>("setCancelable", false);

                if(_title != null)
                {
                    var setTitle = AndroidJNI.GetMethodID(builder.GetRawClass(), "setTitle", "(Ljava/lang/CharSequence;)Landroid/app/AlertDialog$Builder;");
                    var setTitleParams = new jvalue[1];
                    setTitleParams[0] = new jvalue();
                    setTitleParams[0].l = AndroidJNI.NewStringUTF(_title);
                    AndroidJNI.CallObjectMethod(builder.GetRawObject(), setTitle, setTitleParams);
                }
                if(_message != null)
                {
                    var setMsg = AndroidJNI.GetMethodID(builder.GetRawClass(), "setMessage", "(Ljava/lang/CharSequence;)Landroid/app/AlertDialog$Builder;");
                    var setMsgParams = new jvalue[1];
                    setMsgParams[0] = new jvalue();
                    var msg = _message;
                    if(_signature != null)
                    {
                        msg += " (" + _signature + ")";
                    }
                    setMsgParams[0].l = AndroidJNI.NewStringUTF(msg);
                    AndroidJNI.CallObjectMethod(builder.GetRawObject(), setMsg, setMsgParams);
                }
            
                if(_input)
                {
                    _inputView = new AndroidJavaObject("android.widget.EditText", activity);
                    builder.Call<AndroidJavaObject>("setView", _inputView);
                }

                ClickDelegate clicked = which => {
                    if(_inputView != null)
                    {
                        _inputText = _inputView.Call<AndroidJavaObject>("getText").Call<string>("toString");
                    }
                    if(dlg != null)
                    {
                        dlg(which);
                    }
                };
                
                if(_buttons != null && _buttons.Length > 0)
                {
                    if(_buttons.Length == 1)
                    {
                        SetButton(builder, _buttons[0], "setPositiveButton", clicked, 0);
                    }
                    else if(_buttons.Length == 2)
                    {
                        SetButton(builder, _buttons[0], "setPositiveButton", clicked, 0);
                        SetButton(builder, _buttons[1], "setNegativeButton", clicked, 1);
                    }
                    else if(_buttons.Length == 3)
                    {
                        SetButton(builder, _buttons[0], "setPositiveButton", clicked, 0);
                        SetButton(builder, _buttons[1], "setNegativeButton", clicked, 1);
                        SetButton(builder, _buttons[2], "setNeutralButton", clicked, 2);
                    }
                    else
                    {
                        var listener = new DialogClickListener(clicked);
                        var btnArr = AndroidJNI.NewObjectArray(_buttons.Length, AndroidJNI.FindClass("java/lang/CharSequence"), IntPtr.Zero);
                        for(int i = 0; i < _buttons.Length; i++)
                        {
                            AndroidJNI.SetObjectArrayElement(btnArr, i, AndroidJNI.NewStringUTF(_buttons[i]));
                        }
                    
                        var setItems = AndroidJNI.GetMethodID(builder.GetRawClass(), "setItems", "([Ljava/lang/CharSequence;Landroid/content/DialogInterface$OnClickListener;)Landroid/app/AlertDialog$Builder;");
                        var setItemsParams = new jvalue[2];
                        setItemsParams[0] = new jvalue();
                        setItemsParams[0].l = btnArr;
                        setItemsParams[1] = new jvalue();
                        setItemsParams[1].l = AndroidJNIHelper.CreateJavaProxy(listener);
                        AndroidJNI.CallObjectMethod(builder.GetRawObject(), setItems, setItemsParams);
                    }
                }
            
                var show = AndroidJNI.GetMethodID(builder.GetRawClass(), "show", "()Landroid/app/AlertDialog;");
                var showParams = new jvalue[0];
                _dialogPtr = AndroidJNI.CallObjectMethod(builder.GetRawObject(), show, showParams);
            }
        }

        public void Dispose()
        {
            if(_dialogPtr != IntPtr.Zero)
            {
                var clsPtr = AndroidJNI.GetObjectClass(_dialogPtr);
                var dismiss = AndroidJNI.GetMethodID(clsPtr, "dismiss", "()V");
                var dismissParams = new jvalue[0];
                AndroidJNI.CallVoidMethod(_dialogPtr, dismiss, dismissParams);
                AndroidJNI.DeleteLocalRef(_dialogPtr);
                _dialogPtr = IntPtr.Zero;
            }
            _inputView.Dispose();
        }

        public object Clone()
        {
            var clone = new AndroidAlertView();
            clone._message = _message;
            clone._signature = _signature;
            clone._title = _title;
            clone._buttons = _buttons;
            clone._input = _input;
            clone._inputText = _inputText;
            return clone;
        }

    }

    #endif
}
