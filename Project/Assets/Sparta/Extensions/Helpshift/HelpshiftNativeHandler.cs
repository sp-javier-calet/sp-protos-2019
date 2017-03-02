using UnityEngine;

namespace SocialPoint.Extension.Helpshift
{
    class HelpshiftNativeHandler : MonoBehaviour
    {
        internal IHelpshiftDelegate Delegate { private get; set; }

        internal IHelpshift Helpshift { private get; set; }

        public void updateMetaData(string nothing)
        {
            Helpshift.UpdateMetadata();
        }

        public void helpshiftSessionBegan(string message)
        {
            if(Delegate != null)
            {
                Delegate.OnSessionBegan(message);
            }
        }

        public void helpshiftSessionEnded(string message)
        {
            if(Delegate != null)
            {
                Delegate.OnSessionEnded(message);
            }
        }

        public void alertToRateAppAction(string result)
        {
            if(Delegate != null)
            {
                Delegate.RateAppAlert(result);
            }
        }

        public void didReceiveNotificationCount(string count)
        {
            if(Delegate != null)
            {
                Delegate.OnNotification(count);
            }
        }

        public void didReceiveInAppNotificationCount(string count)
        {
            if(Delegate != null)
            {
                Delegate.OnNotification(count);
            }
        }

        public void newConversationStarted(string message)
        {
            if(Delegate != null)
            {
                Delegate.OnNewConversation(message);
            }
        }

        public void userRepliedToConversation(string newMessage)
        {
            if(Delegate != null)
            {
                Delegate.OnUserReplied(newMessage);
            }
        }

        public void userCompletedCustomerSatisfactionSurvey(string json)
        {
            if(Delegate != null)
            {
                Delegate.OnSurveyCompleted(json);
            }
        }

        public void displayAttachmentFile(string path)
        {
            if(Delegate != null)
            {
                Delegate.DisplayAttachment(path);
            }
        }
    }
}