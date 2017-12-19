using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.CrossPromotion
{
    public sealed class CrossPromotionData
    {
        public string Id { get; private set; }

        public string IconImage { get; private set; }

        public string PopupTitleImage { get; private set; }

        public float AspectRatio { get; private set; }

        public float PopupHeightFactor { get; private set; }

        public int PopupFrequency { get; private set; }

        public int PopupTimeout { get; private set; }

        public int TrackTimeout { get; private set; }

        public int IconId { get; private set; }

        public List<string> AppsToCheck { get; private set; }

        public Dictionary<int, CrossPromotionBannerData> BannerInfo { get; private set; }

        public bool ShowPopup { get; private set; }

        public bool ShowIcon { get; private set; }

        public CrossPromotionData(AttrDic config)
        {
            Id = config.GetValue("id").ToString();
            IconImage = config.GetValue("icon_src").ToString();
            PopupTitleImage = config.GetValue("popup_title").ToString();
            AspectRatio = config.GetValue("aspect_ratio").ToFloat();
            PopupHeightFactor = config.GetValue("popup_height_factor").ToFloat();
            PopupFrequency = config.GetValue("popup_freq").ToInt();
            PopupTimeout = config.GetValue("popup_timeout").ToInt();
            TrackTimeout = config.GetValue("track_timeout").ToInt();
            IconId = config.GetValue("icon_id").ToInt();
            ShowPopup = config.GetValue("show_popup").ToBool();
            ShowIcon = config.GetValue("show_icon").ToBool();

            AppsToCheck = new List<string>();
            AttrDic appsToCheckDict = config.Get("check_apps").AsDic;
            var itr = appsToCheckDict.GetEnumerator();
            while(itr.MoveNext())
            {
                var data = itr.Current;
                AppsToCheck.Add(data.Value.AsValue.ToString());
            }
            itr.Dispose();

            BannerInfo = new Dictionary<int, CrossPromotionBannerData>();
            AttrDic bannerInfoDict = config.Get("banners").AsDic;
            itr = bannerInfoDict.GetEnumerator();
            while(itr.MoveNext())
            {
                var data = itr.Current;
                var crossPromoData = new CrossPromotionBannerData(data.Value.AsDic);
                if(!BannerInfo.ContainsKey(crossPromoData.Uid))
                { 
                    BannerInfo.Add(crossPromoData.Uid, crossPromoData);
                }
                else
                {
                    Log.w("CrossPromo UID is not unique: " + crossPromoData.Uid);
                }
            }
            itr.Dispose();
        }
    }
}
