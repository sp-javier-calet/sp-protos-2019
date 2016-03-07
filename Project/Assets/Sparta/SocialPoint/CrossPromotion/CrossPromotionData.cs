using System.Collections;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.CrossPromotion
{
    public class CrossPromotionData
    {
        public string id { get; private set; }

        public string iconImage { get; private set; }

        public string popupTitleImage { get; private set; }

        public float aspectRatio { get; private set; }

        public float popupHeightFactor { get; private set; }

        public int popupFrequency { get; private set; }

        public int popupTimeout { get; private set; }

        public int trackTimeout { get; private set; }

        public int iconId { get; private set; }

        public List<string> appsToCheck { get; private set; }

        public Dictionary<int, CrossPromotionBannerData> bannerInfo { get; private set; }

        public bool showPopup { get; private set; }

        public bool showIcon { get; private set; }

        public CrossPromotionData(AttrDic config)
        {
            id = config.GetValue("id").ToString();
            iconImage = config.GetValue("icon_src").ToString();
            popupTitleImage = config.GetValue("popup_title").ToString();
            aspectRatio = config.GetValue("aspect_ratio").ToFloat();
            popupHeightFactor = config.GetValue("popup_height_factor").ToFloat();
            popupFrequency = config.GetValue("popup_freq").ToInt();
            popupTimeout = config.GetValue("popup_timeout").ToInt();
            trackTimeout = config.GetValue("track_timeout").ToInt();
            iconId = config.GetValue("icon_id").ToInt();
            showPopup = config.GetValue("show_popup").ToBool();
            showIcon = config.GetValue("show_icon").ToBool();

            appsToCheck = new List<string>();
            AttrDic appsToCheckDict = config.Get("check_apps").AsDic;
            foreach(var data in appsToCheckDict)
            {
                appsToCheck.Add(data.Value.AsValue.ToString());
            }

            bannerInfo = new Dictionary<int, CrossPromotionBannerData>();
            AttrDic bannerInfoDict = config.Get("banners").AsDic;
            foreach(var data in bannerInfoDict)
            {
                CrossPromotionBannerData crossPromoData = new CrossPromotionBannerData(data.Value.AsDic);
                if(!bannerInfo.ContainsKey(crossPromoData.uid))
                { 
                    bannerInfo.Add(crossPromoData.uid, crossPromoData);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("CrossPromo UID is not unique: " + crossPromoData.uid);
                }
            }
        }
    }
}
