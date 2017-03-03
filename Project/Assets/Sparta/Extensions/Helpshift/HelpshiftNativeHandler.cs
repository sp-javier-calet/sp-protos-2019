using UnityEngine;

namespace SocialPoint.Extension.Helpshift
{
    class HelpshiftNativeHandler : MonoBehaviour
    {
        internal IHelpshiftDelegate Delegate { private get; set; }

        internal IHelpshift Helpshift { private get; set; }

        public void updateMetaData(string nothing)
        {
            if(Helpshift != null)
            {
                Helpshift.UpdateMetadata();
            }
        }

        public void helpshiftSessionBegan(string message)
        {
            Debug.Log("#### helpshiftSessionBegan");
            if(Delegate != null)
            {
                Delegate.OnSessionBegan(message);
            }
        }

        public void helpshiftSessionEnded(string message)
        {
            Debug.Log("#### helpshiftSessionEnded");
            if(Delegate != null)
            {
                Delegate.OnSessionEnded(message);
            }
        }

        public void alertToRateAppAction(string result)
        {
            Debug.Log("#### alertToRateAppAction");
            if(Delegate != null)
            {
                Delegate.RateAppAlert(result);
            }
        }

        public void didReceiveNotificationCount(string count)
        {
            Debug.Log("#### didReceiveNotificationCount");
            if(Delegate != null)
            {
                Delegate.OnNotification(count);
            }
        }

        public void didReceiveInAppNotificationCount(string count)
        {
            Debug.Log("#### didReceiveInAppNotificationCount");
            if(Delegate != null)
            {
                Delegate.OnNotification(count);
            }
        }

        public void newConversationStarted(string message)
        {
            Debug.Log("#### newConversationStarted");
            if(Delegate != null)
            {
                Delegate.OnNewConversation(message);
            }
        }

        public void userRepliedToConversation(string newMessage)
        {
            Debug.Log("#### userRepliedToConversation");
            if(Delegate != null)
            {
                Delegate.OnUserReplied(newMessage);
            }
        }

        public void userCompletedCustomerSatisfactionSurvey(string json)
        {
            Debug.Log("#### userCompletedCustomerSatisfactionSurvey");
            if(Delegate != null)
            {
                Delegate.OnSurveyCompleted(json);
            }
        }

        public void displayAttachmentFile(string path)
        {
            Debug.Log("#### displayAttachmentFile");
            if(Delegate != null)
            {
                Delegate.DisplayAttachment(path);
            }
        }
    }
}