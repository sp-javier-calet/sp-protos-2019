
using SocialPoint.Attributes;
using SocialPoint.Utils;
using System;

namespace SocialPoint.Events
{
    public delegate void EventDataSetupDelegate(AttrDic data);

    public enum EventTrackerErrorType
    {
        OutOfSync,
        HttpResponse,
        SessionLost
    };

    public delegate void EventTrackerErrorDelegate(EventTrackerErrorType type, Error err);

    public class ResourceOperation
    {
        public string Resource;
        public int Amount;
        public string Category;
        public string Subcategory;
        public AttrDic AdditionalData;

        bool _potentialAmountSet = false;
        int _potentialAmount = 0;
        public int PotentialAmount
        {
            get
            {
                if(_potentialAmountSet)
                {
                    return _potentialAmount;
                }
                else
                {
                    return Amount;
                }
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

    public class FunnelOperation
    {
        public string Step;
        public string Type;
        public bool AutoCompleted = false;
        public bool System = false;
        public AttrDic AdditionalData;
    }

    public class PurchaseStartOperation
    {
        public string TransactionId;
        public string ProductId;
        public string PaymentProvider;
        public float AmountGross;
        public PurchaseGameInfo Info;
    }

    public class PurchaseGameInfo
    {
        public string OfferName;
        public string ResourceName;
        public int ResourceAmount;
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
        void TrackEvent(string eventName, AttrDic data = null, ErrorDelegate del = null);
        void TrackFunnel(FunnelOperation op);
        void TrackPurchaseStart(PurchaseStartOperation op);
        void TrackLevelUp(int lvl, AttrDic data = null);
        void TrackResource(ResourceOperation op);
    }
}
