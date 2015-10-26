
using SocialPoint.Attributes;
using SocialPoint.Base;
using System;

namespace SocialPoint.Events
{	
    public class EmptyEventTracker : IEventTracker
    {
        public event EventTrackerErrorDelegate GeneralError
        {
            add { throw new NotSupportedException(); }
            remove { }
        }

        public event EventDataSetupDelegate DataSetup
        {
            add { throw new NotSupportedException(); }
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

        public void TrackEvent(string eventName, AttrDic data = null, ErrorDelegate del = null)
        {
        }

        public void TrackFunnel(FunnelOperation op)
        {
        }

        public void TrackLevelUp(int lvl, AttrDic data=null)
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
