#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace SocialPoint.Marketing
{
    public class AdminPanelMarketingAppsFlyer : IAdminPanelGUI
    {
        SocialPointAppsFlyer _appsFlyerTracker;
        string _customMediaSource;
        string _customCampaign;

        public AdminPanelMarketingAppsFlyer(SocialPointAppsFlyer appsFlyerTracker)
        {
            _appsFlyerTracker = appsFlyerTracker;
            _customMediaSource = "AdminPanelSource";
            _customCampaign = "AdminPanelCampaign";
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Apps Flyer");
            layout.CreateMargin();

            DisplayTitle(layout, "< Device Info >");
            DisplayInfoField(layout, "IDFA:", "*** TODO ***");

            DisplayTitle(layout, "< Install Data Info >");
            SocialPointAppsFlyer.AFConversionData convData = _appsFlyerTracker.ConversionData;
            DisplayConversionData(layout, convData);

            DisplayTitle(layout, "< Attribution Actions >");
            layout.CreateButton("Clean", () => {
                /*displayActionConfirm([]()
                        {
                            clearAndKillApp();
                        });*/
            });
            layout.CreateButton("Install", () => {
                /*displayActionConfirm([this]()
                        {
                            std::string appId = _appsFlyerTracker.getAppIdentifier();
                            std::string idfaValue = _deviceInfo->getIFDA();
                            std::string url = createInstallURL(appId, _customMediaSource, _customCampaign, idfaValue);

                            NativeUtils::openUrl(url);
                            clearAndKillApp();
                        });*/
            });

            DisplayTitle(layout, "< Edit Install >");
            DisplayEditionField(layout, "Media Source:", _customMediaSource, value => _customMediaSource = value);
            DisplayEditionField(layout, "Campaign:", _customCampaign, value => _customCampaign = value);
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
                var label = hLayout.CreateLabel(name);
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

        void DisplayError(string error)
        {
            //TODO: Display alert view
        }
    }
}

#endif
