using SocialPoint.AdminPanel;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.VideoAds
{
    
    public class AdminPanelVideoAds :  IAdminPanelGUI, IAdminPanelConfigurer
    {
        IVideoAdsManager _manager;
        AdminPanelLayout _layout;

        public AdminPanelVideoAds(IVideoAdsManager manager)
        {
            _manager = manager;
        }

        #region IAdminPanelConfigurer implementation

        public void OnConfigure(SocialPoint.AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("VideoAds", this));

        }

        #endregion

        #region IAdminPanelGUI implementation

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            _layout = layout;

            layout.CreateLabel("VideoAds");
            layout.CreateMargin();
            layout.CreateToggleButton("Enabled", _manager.IsEnabled, (enabled) => {
                if(enabled)
                {
                    _manager.Enable();
                }
                else
                {
                    _manager.Disable();
                }
            });
            layout.CreateButton("Request VideoAd", () => {
                Debug.Log("VideoAds request video");
                _manager.RequestAd(OnRequestVideo);
            }, !_manager.AdAvailable);

            layout.CreateButton("Show VideoAd", () => {
                Debug.Log("VideoAds show video");
                _manager.ShowAd(OnShowVideo);
            }, _manager.AdAvailable);
        }

        #endregion


        void OnRequestVideo(Error error, RequestVideoResult result)
        {
            _layout.Refresh();
            Debug.Log(string.Format("VideoAds request video result = {0}",result.ToString()));
        }

        void OnShowVideo(Error error, ShowVideoResult result)
        {
            _layout.Refresh();
            Debug.Log(string.Format("VideoAds show video result = {0}",result.ToString()));
        }
    }
}
