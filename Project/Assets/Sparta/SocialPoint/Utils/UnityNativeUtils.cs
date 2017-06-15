using System;
using SocialPoint.Login;
using UnityEngine;

namespace SocialPoint.Utils
{
    public class UnityNativeUtils : INativeUtils
    {
        ILoginData _loginData;

        public UnityNativeUtils(ILoginData loginData)
        {
            _loginData = loginData;
        }

        public virtual bool IsInstalled(string appId)
        {
            return false;
        }

        public virtual void OpenApp(string appId)
        {
            Application.OpenURL(appId);
        }

        public virtual void OpenStore(string appId)
        {
        }

        public virtual void OpenUpgrade()
        {
            if(_loginData == null)
            {
                throw new InvalidOperationException("No login data.");
            }
            if(_loginData.Data == null)
            {
                throw new InvalidOperationException("No login generic data.");
            }
            if(string.IsNullOrEmpty(_loginData.Data.StoreUrl))
            {
                throw new InvalidOperationException("No login generic data store url.");
            }
            Application.OpenURL(_loginData.Data.StoreUrl);
        }

        public virtual void OpenReview()
        {
            if(_loginData == null)
            {
                throw new InvalidOperationException("No login data.");
            }
            if(_loginData.Data == null)
            {
                throw new InvalidOperationException("No login generic data.");
            }
            if(string.IsNullOrEmpty(_loginData.Data.StoreUrl))
            {
                throw new InvalidOperationException("No login generic data rate url.");
            }
            Application.OpenURL(_loginData.Data.RateUrl);
        }

        public virtual bool UserAllowNotification
        {
            get
            {
                return true;
            }
        }

        public virtual ShortcutItem[] ShortcutItems{ get; set; }

    }
}
