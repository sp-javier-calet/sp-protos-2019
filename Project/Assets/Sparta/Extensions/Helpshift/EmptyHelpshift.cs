using UnityEngine;

namespace SocialPoint.Extension.Helpshift
{
    class EmptyHelpshift : MonoBehaviour, IHelpshift
    {
        #region IHelpshift implementation

        public HelpshiftConfiguration Configuration
        { 
            get
            {
                return new HelpshiftConfiguration();
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

        public void OpenFromPush(string issueId)
        {
        }

        #endregion
    }
}
