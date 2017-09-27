﻿
namespace SocialPoint.Utils
{
    public class EmptyNativeUtils : INativeUtils
    {
        public bool IsInstalled(string appId)
        {
            return false;
        }
        
        public void OpenApp(string appId)
        {
        }
        
        public void OpenStore(string appId)
        {
        }

        public void OpenUpgrade()
        {
        }

        public void OpenReview()
        {
        }

        public void DisplayReviewDialog()
        {
        }

        public bool UserAllowNotification
        {
            get
            {
                return false;
            }
        }
            
        public ShortcutItem[] ShortcutItems{ get; set; }

    }
}