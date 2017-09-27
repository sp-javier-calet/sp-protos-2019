using System;
using SocialPoint.Login;
using UnityEngine;
using SocialPoint.Hardware;

namespace SocialPoint.Utils
{
    public class UnityNativeUtils : INativeUtils
    {
        protected IAppInfo _appInfo;

        public UnityNativeUtils(IAppInfo appInfo)
        {
            _appInfo = appInfo;
        }

        public virtual bool IsInstalled(string appId)
        {
            return false;
        }

        public virtual void OpenApp(string appId)
        {
            throw new NotImplementedException();            
        }

        public virtual void OpenStore(string appId)
        {
            throw new NotImplementedException();
        }

        public virtual void OpenReview()
        {
            throw new NotImplementedException();
        }

        public virtual void OpenUpgrade()
        {
            OpenStore(_appInfo.Id);
        }

        public virtual bool SupportsReviewDialog
        {
            get
            {
                return false;
            }
        }

        public virtual void DisplayReviewDialog()
        {
            throw new NotImplementedException();
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
