using System;
using SocialPoint.Attributes;

namespace SocialPoint.Marketing
{
    public struct TrackerAttributionData
    {
        public string trackerName;
        public string data;

        public AttrDic ToAttrDic()
        {
            var dic = new AttrDic();
            dic.Set("source", new AttrString(trackerName));
            dic.Set("detail", new AttrString(data));
            return dic;
        }
    }

    public interface IMarketingTracker : IDisposable
    {
        string Name { get; }

        void Init();

        void SetUserID(string userID);

        void TrackInstall(bool isNewInstall);

        void SetDebugMode(bool debugMode);

        event Action<TrackerAttributionData> OnDataReceived;
    }
}

