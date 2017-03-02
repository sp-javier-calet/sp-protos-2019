namespace SocialPoint.Extension.Helpshift
{
    public interface IHelpshiftDelegate
    {
        void OnSessionBegan(string message);

        void OnSessionEnded(string message);

        void OnNewConversation(string message);

        void OnUserReplied(string message);

        void OnSurveyCompleted(string json);

        void OnNotification(string count);

        void DisplayAttachment(string path);

        void RateAppAlert(string result);
    }
}
