#if UNITY_IOS && !UNITY_EDITOR
#define IOS_DEVICE
#endif

using System;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public sealed class SocialPointGameCenterVerification
    {
        GameCenterValidationDelegate _delegate;
        NativeCallsHandler _handler;

        #if IOS_DEVICE
        [System.Runtime.InteropServices.DllImport ("__Internal")]
        private static extern void SPUnityGameCenter_UserVerificationInit();
        #else
        void SPUnityGameCenter_UserVerificationInit()
        {
            // just to get the callbacks called on unity to test the whole flow.
            Notify(string.Empty);
        }
        #endif

        public SocialPointGameCenterVerification(NativeCallsHandler handler)
        {
            _handler = handler;
            _handler.RegisterListener("Notify", Notify);
        }

        public void LoadData(GameCenterValidationDelegate cbk)
        {
            if(cbk != null)
            {
                _delegate = cbk;
                SPUnityGameCenter_UserVerificationInit();
            }
        }

        /// <summary>
        /// receives the verification from the plugin as a serialized json
        /// </summary>
        /// <param name="verification">Verification.</param>
        void Notify(string verification)
        {
            Error error;
            GameCenterUserVerification gcUserVerification = null;

            if(string.IsNullOrEmpty(verification))
            {
                error = new Error("Game Center Verification only supported on iOS device.");
                Log.i("Game Center Verification only supported on iOS device.");
            }
            else
            {
                var parser = new JsonAttrParser();
                var data = parser.ParseString(verification).AsDic;
                if(data.GetValue("error").ToBool())
                {
                    error = new Error(data.GetValue("errorCode").ToInt(), data.GetValue("errorMessage").ToString());
                    Log.i("Game Center Verification got error: " + error);
                }
                else
                {
                    var url = data.GetValue("url").ToString();
                    var signature = Convert.FromBase64String(data.GetValue("signature").ToString());
                    var salt = Convert.FromBase64String(data.GetValue("salt").ToString());
                    var time = (ulong)data.GetValue("timestamp").ToLong();
                    gcUserVerification = new GameCenterUserVerification(url, signature, salt, time);
                    error = null;
                }
            }

            DebugUtils.Assert(_delegate != null, "SocialPointGameCenterVerification, _delegate must not be null");
            if(_delegate != null)
            {
                _delegate(error, gcUserVerification);
                _delegate = null;
            }
        }

    }
}
