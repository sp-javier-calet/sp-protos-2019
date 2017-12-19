using System;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.ServerEvents
{
    public delegate void EventDataSetupDelegate(AttrDic data);

    public enum EventTrackerErrorType
    {
        OutOfSync,
        HttpResponse,
        SessionLost,
        Exception
    }

    public delegate void EventTrackerErrorDelegate(EventTrackerErrorType type, Error err);

    public sealed class ResourceOperation
    {
        public string ResourceName;
        public int Amount;
        public string Category;
        public string Subcategory;
        public string ItemId;
        public AttrDic AdditionalData;

        bool _potentialAmountSet;
        int _potentialAmount;

        public int PotentialAmount
        {
            get
            {
                return _potentialAmountSet ? _potentialAmount : Amount;
            }

            set
            {
                _potentialAmountSet = true;
                _potentialAmount = value;
            }
        }

        public bool IsExcessLost;

        public int LostAmount
        {
            get
            {
                return IsExcessLost ? (PotentialAmount - Amount) : 0;
            }
        }
    }

    public sealed class FunnelOperation
    {
        public string Step;
        public string Type;
        public bool AutoCompleted;
        public bool System;
        public AttrDic AdditionalData;
    }

    public interface IEventTracker : IDisposable
    {
        event EventDataSetupDelegate DataSetup;
        event EventTrackerErrorDelegate GeneralError;

        void Start();

        void Stop();

        void Reset();

        bool Send();

        void TrackSystemEvent(string eventName, AttrDic data = null, ErrorDelegate del = null);

        void TrackUrgentSystemEvent(string eventName, AttrDic data = null, ErrorDelegate del = null);

        void TrackEvent(string eventName, AttrDic data = null, ErrorDelegate del = null);

        void TrackFunnel(FunnelOperation op);

        void TrackLevelUp(int lvl, AttrDic data = null);

        void TrackResource(ResourceOperation op);

    }
}
