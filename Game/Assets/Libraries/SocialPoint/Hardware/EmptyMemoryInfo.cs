namespace SocialPoint.Hardware
{
    public class EmptyMemoryInfo : IMemoryInfo
    {
        public EmptyMemoryInfo()
        {
        }

        public ulong TotalMemory
        {
            get;
            set;
        }

        public ulong FreeMemory
        {
            get;
            set;
        }

        public ulong UsedMemory
        {
            get;
            set;
        }

        public ulong ActiveMemory
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

