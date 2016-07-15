﻿using System;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public class SocialPointGameCenterVerification
    {
        bool _loaded;
        bool _inited;
        GameCenterValidationDelegate _delegate;
        GameCenterUserVerification _verification;
        Error _error;
        NativeCallsHandler _handler;

        #if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport ("__Internal")]
        private static extern void SPUnityGameCenter_UserVerificationInit();
        #else
        void SPUnityGameCenter_UserVerificationInit()
        {
        }
        #endif

        public SocialPointGameCenterVerification(NativeCallsHandler handler)
        {
            _handler = handler;
            _handler.RegisterListener("Notify", Notify);
        }

        public void LoadData(GameCenterValidationDelegate cbk)
        {
            if(!_inited)
            {
                _inited = true;
                SPUnityGameCenter_UserVerificationInit();
            }
            if(cbk != null)
            {
                if(_loaded)
                {
                    cbk(_error, _verification);
                }
                else
                {
                    _delegate += cbk;
                }
            }
        }

        /// <summary>
        /// receives the verification from the plugin as a serialized json
        /// </summary>
        /// <param name="verification">Verification.</param>
        void Notify(string verification)
        {
            var parser = new JsonAttrParser();
            var data = parser.ParseString(verification).AsDic;
            if(data.GetValue("error").ToBool())
            {
                _verification = null;
                _error = new Error(data.GetValue("errorCode").ToInt(), data.GetValue("errorMessage").ToString());
                Log.i("Game Center Verification got error: " + _error);
            }
            else
            {
                var url = data.GetValue("url").ToString();
                var signature = Convert.FromBase64String(data.GetValue("signature").ToString());
                var salt = Convert.FromBase64String(data.GetValue("salt").ToString());
                var time = (ulong)data.GetValue("timestamp").ToLong();
                _verification = new GameCenterUserVerification(url, signature, salt, time);
                _error = null;
            }
            _loaded = true;
            if(_delegate != null)
            {
                _delegate(_error, _verification);
                _delegate = null;
            }
        }

    }
}
