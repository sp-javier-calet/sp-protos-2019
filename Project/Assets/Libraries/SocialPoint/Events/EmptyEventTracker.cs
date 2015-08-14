
using SocialPoint.Attributes;
using SocialPoint.Utils;

namespace SocialPoint.Events
{	
    public class EmptyEventTracker : IEventTracker
    {
        public event EventDataSetupDelegate DataSetup;
        public event EventTrackerErrorDelegate GeneralError;

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

        public void TrackEvent(string eventName, AttrDic data = null, ErrorDelegate del = null)
        {
        }

        public void TrackFunnel(FunnelOperation op)
        {
        }

        public void TrackPurchaseStart(PurchaseStartOperation op)
        {
        }

        public void TrackLevelUp(int lvl, AttrDic data=null)
        {
        }

        public void TrackResource(ResourceOperation op)
        {
        }
    }
}
