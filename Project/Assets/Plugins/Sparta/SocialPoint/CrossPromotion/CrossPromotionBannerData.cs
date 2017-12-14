using SocialPoint.Attributes;

namespace SocialPoint.CrossPromotion
{
    public sealed class CrossPromotionBannerData
    {
        public int Uid { get; private set; }

        public string StoreId { get; private set; }

        public string AppId { get; private set; }

        public string ButtonTextImage { get; private set; }

        public string BgImage { get; private set; }

        public string IconImage { get; private set; }

        public string Game { get; private set; }

        public bool CurrentGame { get; private set; }

        public bool ShowRibbon { get; private set; }

        public CrossPromotionBannerData(AttrDic config)
        {
            Uid = config.GetValue("id").ToInt();
            #if (UNITY_IOS || UNITY_TVOS)
            StoreId = config.GetValue("store_id").ToString();
            #elif UNITY_ANDROID
            StoreId = config.GetValue("app_id").ToString();
            #endif
            AppId = config.GetValue("app_id").ToString();
            ButtonTextImage = config.GetValue("button").ToString();
            BgImage = config.GetValue("background").ToString();
            IconImage = config.GetValue("icon").ToString();
            Game = config.GetValue("game").ToString();
            ShowRibbon = config.GetValue("ribbon").ToBool();
            CurrentGame = config.ContainsKey("current") && config.GetValue("current").ToBool();
        }
    }
}
