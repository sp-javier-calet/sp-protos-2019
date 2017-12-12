#if ADMIN_PANEL 

using System;
using UnityEngine;
using UnityEngine.UI;
using SocialPoint.AdminPanel;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.Hardware;
using SocialPoint.Utils;

namespace SocialPoint.Marketing
{
    public class AdminPanelMarketingAppsFlyer : IAdminPanelGUI
    {
        readonly SocialPointAppsFlyer _appsFlyerTracker;
        readonly IDeviceInfo _deviceInfo;
        readonly INativeUtils _nativeUtils;
        readonly IAppEvents _appEvents;
        readonly IAlertView _alertPrototype;
        string _customMediaSource;
        string _customCampaign;

        public AdminPanelMarketingAppsFlyer(SocialPointAppsFlyer appsFlyerTracker, IDeviceInfo deviceInfo, INativeUtils nativeUtils, IAppEvents appEvents, IAlertView alertPrototype)
        {
            _appsFlyerTracker = appsFlyerTracker;
            _deviceInfo = deviceInfo;
            _nativeUtils = nativeUtils;
            _appEvents = appEvents;
            _alertPrototype = alertPrototype;
            _customMediaSource = "AdminPanelSource";
            _customCampaign = "AdminPanelCampaign";
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Apps Flyer");
            layout.CreateMargin();

            DisplayTitle(layout, "< Device Info >");
            DisplayInfoField(layout, "IDFA:", _deviceInfo.AdvertisingId);
            layout.CreateMargin();

            DisplayTitle(layout, "< Install Data Info >");
            SocialPointAppsFlyer.AFConversionData convData = _appsFlyerTracker.ConversionData;
            DisplayConversionData(layout, convData);
            layout.CreateMargin();

            DisplayTitle(layout, "< Attribution Actions >");
            layout.CreateButton("Clean", () => {
                DisplayActionConfirm(() => {
                    ClearAndKillApp();
                });
            });
            layout.CreateButton("Install", () => {
                DisplayActionConfirm(() => {
                    string appId = _appsFlyerTracker.AppID;
                    string idfaValue = _deviceInfo.AdvertisingId;
                    string url = CreateInstallURL(appId, _customMediaSource, _customCampaign, idfaValue);

                    _appEvents.WillGoBackground.Add(int.MaxValue, ClearAndKillApp);
                    Application.OpenURL(url);
                });
            });

            DisplayTitle(layout, "< Edit Install >");
            DisplayEditionField(layout, "Media Source:", _customMediaSource, value => _customMediaSource = value);
            DisplayEditionField(layout, "Campaign:", _customCampaign, value => _customCampaign = value);
        }

        void ClearAndKillApp()
        {
            _nativeUtils.ClearDataAndKillApp();
        }

        string CreateInstallURL(string appId, string mediaSource, string campaign, string idfaValue)
        {
            return string.Format("https://app.appsflyer.com/{0}?pid={1}&c={2}&{3}={4}", appId, mediaSource, campaign,
                SocialPointAppsFlyer.AdvertisingIdentifierKey, idfaValue);
        }

        void DisplayTitle(AdminPanelLayout layout, string name)
        {
            var label = layout.CreateLabel(name);
            label.color = Color.yellow;
        }

        void DisplayEditionField(AdminPanelLayout layout, string name, string content, Action<string> callback)
        {
            var hLayout = layout.CreateHorizontalLayout();
            {
                hLayout.CreateLabel(name);
            }
            {
                var input = hLayout.CreateTextInput(name, (value) => {
                    if(!string.IsNullOrEmpty(value))
                    {
                        callback(value);
                    }
                    else
                    {
                        DisplayError("Install custom fields cannot be empty");
                    }
                    layout.Refresh();
                });
                input.text = content;
            }
        }

        void DisplayInfoField(AdminPanelLayout layout, string name, string content)
        {
            DisplayInfoField(layout, name, content, Color.white);
        }

        void DisplayInfoField(AdminPanelLayout layout, string name, string content, Color color)
        {
            var hLayout = layout.CreateHorizontalLayout();
            {
                hLayout.CreateLabel(name);
            }
            {
                var label = hLayout.CreateLabel(content);
                label.color = color;
            }
        }

        void DisplayNonOrganicData(AdminPanelLayout layout, SocialPointAppsFlyer.AFConversionData convData)
        {
            DisplayInfoField(layout, "Install Status:", convData.Status, Color.green);
            DisplayInfoField(layout, "IDFA:", convData.AdId);
            DisplayInfoField(layout, "Media Source:", convData.MediaSource);
            DisplayInfoField(layout, "Campaign:", convData.Campaign);
        }

        void DisplayOrganicData(AdminPanelLayout layout, SocialPointAppsFlyer.AFConversionData convData)
        {
            DisplayInfoField(layout, "Install Status:", convData.Status, Color.green);
        }

        void DisplayWaitForData(AdminPanelLayout layout)
        {
            var label = layout.CreateLabel("Apps Flyer status not received yet. Try again later");
            label.color = Color.red;
            layout.CreateButton("Refresh", () => {
                layout.Refresh();
            });
        }

        void DisplayConversionData(AdminPanelLayout layout, SocialPointAppsFlyer.AFConversionData convData)
        {
            if(convData.Status == SocialPointAppsFlyer.NonOrganicInstall)
            {
                DisplayNonOrganicData(layout, convData);
            }
            else if(convData.Status == SocialPointAppsFlyer.OrganicInstall)
            {
                DisplayOrganicData(layout, convData);
            }
            else
            {
                DisplayWaitForData(layout);
            }
        }

        void DisplayActionConfirm(Action callback)
        {
            var alertView = (IAlertView)_alertPrototype.Clone();
            alertView.Title = "Apps Flyer Action";
            alertView.Message = "App will close and should be restarted manually.\n"
            + "All data will be deleted.\n"
            + "Device must be listed in Apps Flyer's whitelist for the install tests to work properly.";
            alertView.Input = false;
            alertView.Buttons = new []{ "Ok", "Cancel" };
            alertView.Show(result => {
                if(result == 0)
                {
                    callback();
                }
            });
        }

        void DisplayError(string error)
        {
            var alertView = (IAlertView)_alertPrototype.Clone();
            alertView.Title = "Apps Flyer Config Error";
            alertView.Message = error;
            alertView.Input = false;
            alertView.Buttons = new []{ "Ok" };
            alertView.Show(result => {
            });
        }
    }
}

#endif
