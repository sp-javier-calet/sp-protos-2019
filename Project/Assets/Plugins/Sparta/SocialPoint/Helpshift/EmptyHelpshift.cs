using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Helpshift
{
    public class EmptyHelpshift : IHelpshift
    {
        #region IHelpshift implementation

        static HelpshiftConfiguration _config = new HelpshiftConfiguration();
        static HelpshiftCustomer _customer = new HelpshiftCustomer("0", new string[0], new Dictionary<string, object>());

        public HelpshiftConfiguration Configuration
        { 
            get
            {
                return _config;
            }
        }

        public HelpshiftCustomer UserData
        {
            get
            {
                return _customer;
            }
            set
            {
                _customer = value;
            }
        }

        public bool IsEnabled
        {
            get
            {
                return false;
            }
        }

        public void Enable()
        {
        }

        public void ShowFAQ(string sectionId = null)
        {
        }

        public void ShowConversation()
        {
        }

        public void OpenFromPush(string issueId)
        {
            
        }

        public void AddFlows(Dictionary<string, object>[] flows)
        {

        }

        public int PendingNotificationsCount
        {
            get
            {
                return 0;
            }
        }

        #endregion
    }
}
