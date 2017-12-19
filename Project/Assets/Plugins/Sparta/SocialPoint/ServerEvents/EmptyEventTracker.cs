using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.ServerEvents
{
    public sealed class EmptyEventTracker : IEventTracker
    {
        public event EventTrackerErrorDelegate GeneralError
        {
            add { }
            remove { }
        }

        public event EventDataSetupDelegate DataSetup
        {
            add { }
            remove { }
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Reset()
        {
        }

        public void Dispose()
        {
        }

        public bool Send()
        {
            return true;
        }

        public void TrackSystemEvent(string eventName, AttrDic data = null, ErrorDelegate del = null)
        {
        }

        public void TrackUrgentSystemEvent(string eventName, AttrDic data = null, ErrorDelegate del = null)
        {
        }

        public void TrackEvent(string eventName, AttrDic data = null, ErrorDelegate del = null)
        {
        }

        public void TrackFunnel(FunnelOperation op)
        {
        }

        public void TrackLevelUp(int lvl, AttrDic data = null)
        {
        }

        public void TrackResource(ResourceOperation op)
        {
        }

        public void TrackGameLoaded()
        {
        }
    }
}
