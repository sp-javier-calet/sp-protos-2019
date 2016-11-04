using UnityEngine;

namespace SocialPoint.Extension.Helpshift
{
    class EmptyHelpshift : MonoBehaviour, IHelpshift
    {
        #region IHelpshift implementation

        public void Setup(string gameObjectName, HelpshiftConfiguration config, HelpshiftCustomer customerData = null)
        {
        }

        public void SetCustomerData(HelpshiftCustomer customerData)
        {
        }

        public void SetLanguage(string locale)
        {
        }

        public void ShowFAQ(string sectionId = null)
        {
            Debug.LogWarning("ShowFAQ not available. Empty Helpshift implementation");
        }

        public void ShowConversation()
        {
            Debug.LogWarning("ShowConversation not available. Empty Helpshift implementation");
        }

        public void RegisterPushNotificationToken(string token)
        {
            Debug.LogWarning("RegisterPushNotificationToken not available. Empty Helpshift implementation");
        }

        #endregion
    }
}
