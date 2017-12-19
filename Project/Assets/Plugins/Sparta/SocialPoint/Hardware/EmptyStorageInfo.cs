namespace SocialPoint.Hardware
{
    public class EmptyStorageInfo : IStorageInfo
    {
        public EmptyStorageInfo()
        {
        }

        public ulong TotalStorage
        {
            get;
            set;
        }

        public ulong FreeStorage
        {
            get;
            set;
        }

        public ulong UsedStorage
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

