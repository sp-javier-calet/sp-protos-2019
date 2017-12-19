#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.VideoAds
{
    
    public sealed class AdminPanelVideoAds :  IAdminPanelGUI, IAdminPanelConfigurer
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
                Log.i("VideoAds request video");
                _manager.RequestAd(OnRequestVideo);
            }, !_manager.AdAvailable);

            layout.CreateButton("Show VideoAd", () => {
                Log.i("VideoAds show video");
                _manager.ShowAd(OnShowVideo);
            }, _manager.AdAvailable);
        }

        #endregion


        void OnRequestVideo(Error error, RequestVideoResult result)
        {
            _layout.Refresh();
            Log.i(string.Format("VideoAds request video result = {0}", result));
        }

        void OnShowVideo(Error error, ShowVideoResult result)
        {
            _layout.Refresh();
            Log.i(string.Format("VideoAds show video result = {0}", result));
        }
    }
}

#endif
