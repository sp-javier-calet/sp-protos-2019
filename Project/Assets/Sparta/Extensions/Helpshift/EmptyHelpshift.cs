
namespace SocialPoint.Extension.Helpshift
{
    public class EmptyHelpshift : IHelpshift
    {
        #region IHelpshift implementation

        static HelpshiftConfiguration _config = new HelpshiftConfiguration(string.Empty, string.Empty, string.Empty);

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

        public void OpenFromPush(string issueId, object extra)
        {
        }

        #endregion
    }
}
