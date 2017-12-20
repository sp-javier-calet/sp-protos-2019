using SocialPoint.IosKeychain;

namespace SocialPoint.Hardware
{
    public sealed class IosAppInfo : IAppInfo
    {
        public IosAppInfo()
        {
        }

        private string _seedId = null;
        public string SeedId
        {
            get
            {
                if(_seedId == null)
                {
                    _seedId = KeychainItem.SeedId;
                }
                return _seedId;
            }
        }

        private string _id = null;
        public string Id
        {
            get
            {
                if(_id == null)
                {
                    _id = IosHardwareBridge.SPUnityHardwareGetAppId();
                }
                return _id;
            }
        }


        private string _version = null;
        public string Version
        {
            get
            {
                if(_version == null)
                {
                    _version = IosHardwareBridge.SPUnityHardwareGetAppVersion();
                }
                return _version;
            }
        }

        private string _shortVersion = null;
        public string ShortVersion
        {
            get
            {
                if(_shortVersion == null)
                {
                    _shortVersion = IosHardwareBridge.SPUnityHardwareGetAppShortVersion();
                }
                return _shortVersion;
            }
        }

        private string _language = null;
        public string Language
        {
            get
            {
                if(_language == null)
                {
                    _language = IosHardwareBridge.SPUnityHardwareGetAppLanguage();
                }
                return _language;
            }
        }

        private string _country = null;
        public string Country
        {
            get
            {
                if(_country == null)
                {
                    _country = IosHardwareBridge.SPUnityHardwareGetAppCountry();
                }
                return _country;
            }
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }
    }
}

