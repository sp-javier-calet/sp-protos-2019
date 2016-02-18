namespace SocialPoint.Hardware
{
    public class EmptyAppInfo : IAppInfo
    {
        public EmptyAppInfo()
        {
        }

        public string SeedId
        {
            get;
            set;
        }

        public string Id
        {
            get;
            set;
        }

        public string Version
        {
            get;
            set;
        }

        public string ShortVersion
        {
            get;
            set;
        }

        public string Language
        {
            get;
            set;
        }

        public string Country
        {
            get;
            set;
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }
    }
}

