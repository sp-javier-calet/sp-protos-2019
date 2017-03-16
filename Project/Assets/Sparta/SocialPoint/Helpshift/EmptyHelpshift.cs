﻿
namespace SocialPoint.Helpshift
{
    public class EmptyHelpshift : IHelpshift
    {
        #region IHelpshift implementation

        static HelpshiftConfiguration _config = new HelpshiftConfiguration();

        public HelpshiftConfiguration Configuration
        { 
            get
            {
                return _config;
            }
        }

        public HelpshiftCustomer UserData
        {
            set
            {
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

        #endregion
    }
}
