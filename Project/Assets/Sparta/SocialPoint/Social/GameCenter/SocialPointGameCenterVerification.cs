using System;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;
using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace SocialPoint.Social
{

    public class SocialPointGameCenterVerification
    {
        bool _loaded = false;
        bool _inited = false;
        GameCenterValidationDelegate _delegate;
        GameCenterUserVerification _verification;
        Error _error;
        NativeCallsHandler _handler;

        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport ("__Internal")]
        private static extern void SPUnityGameCenter_UserVerificationInit();
        #else
        private static void SPUnityGameCenter_UserVerificationInit()
        {
        }
        #endif

        public SocialPointGameCenterVerification(NativeCallsHandler handler)
        {
            _handler = handler;
            _handler.RegisterListener("Notify",Notify);
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
        /// <param name="verfication">Verfication.</param>
        void Notify(string verfication)
        {
            var parser = new JsonAttrParser();
            var data = parser.ParseString(verfication).AsDic;
            if(data.GetValue("error").ToBool())
            {
                _verification = null;
                _error = new Error(data.GetValue("errorCode").ToInt(), data.GetValue("errorMessage").ToString());
                DebugUtils.Log("Game Center Verification got error: "+_error);
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
