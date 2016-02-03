using System;
using SocialPoint.Attributes;
using SocialPoint.Base;
using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace SocialPoint.Social
{

    public class SocialPointGameCenterVerification : MonoBehaviour
    {

        public GameCenterValidationDelegate Callback;

        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport ("__Internal")]
        private static extern void SPUnityGameCenterUserVerification_Init(string name);
        
#else
        private static void SPUnityGameCenterUserVerification_Init(string name)
        {

        }
        #endif

        void Awake()
        {
            gameObject.name = GetType().ToString();
            DontDestroyOnLoad(this);
            SPUnityGameCenterUserVerification_Init(gameObject.name);
        }

        /// <summary>
        /// recieves the verification from the plugin as a serialized json
        /// </summary>
        /// <param name="verfication">Verfication.</param>
        void Notify(string verfication)
        {
            Debug.Log(verfication);
            var parser = new JsonAttrParser();
            var data = parser.ParseString(verfication).AsDic;
            if(data.GetValue("error").ToBool())
            {
                Callback(new Error(data.GetValue("errorCode").ToInt(), data.GetValue("errorMessage").ToString()), null);
            }
            else
            {
                var url = data.GetValue("url").ToString();
                var signature = Convert.FromBase64String(data.GetValue("signature").ToString());
                var salt = Convert.FromBase64String(data.GetValue("salt").ToString());
                var time = (ulong)data.GetValue("timestamp").ToLong();
                var userVerification = new GameCenterUserVerification(url, signature, salt, time);
                if(Callback != null)
                {
                    Callback(new Error(), userVerification);
                }
            }
        }

    }
}
