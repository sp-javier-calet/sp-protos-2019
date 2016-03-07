using SocialPoint.Attributes;

namespace SocialPoint.CrossPromotion
{
    public class CrossPromotionBannerData
    {
        public int uid { get; private set; }

        public string storeId { get; private set; }

        public string appId { get; private set; }

        public string buttonTextImage { get; private set; }

        public string bgImage { get; private set; }

        public string iconImage { get; private set; }

        public string game { get; private set; }

        public bool currentGame { get; private set; }

        public bool showRibbon { get; private set; }

        public CrossPromotionBannerData(AttrDic config)
        {
            uid = config.GetValue("id").ToInt();
            #if UNITY_IOS
            storeId = config.GetValue("store_id").ToString();
            #elif UNITY_ANDROID
            storeId = config.GetValue("app_id").ToString();
            #endif
            appId = config.GetValue("app_id").ToString();
            buttonTextImage = config.GetValue("button").ToString();
            bgImage = config.GetValue("background").ToString();
            iconImage = config.GetValue("icon").ToString();
            game = config.GetValue("game").ToString();
            showRibbon = config.GetValue("ribbon").ToBool();
            currentGame = config.ContainsKey("current") ? config.GetValue("current").ToBool() : false;
        }
    }
}
