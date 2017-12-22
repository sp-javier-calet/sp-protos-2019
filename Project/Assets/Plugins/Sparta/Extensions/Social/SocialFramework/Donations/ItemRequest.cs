using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public sealed class ItemRequest
    {

        readonly long _requesterId;

        public long RequesterId
        {
            get
            {
                return _requesterId;
            }
        }

        readonly string _requestUuid;

        public string RequestUuid
        {
            get
            {
                return _requestUuid;
            }
        }

        readonly int _itemId;

        public int ItemId
        {
            get
            {
                return _itemId;
            }
        }

        readonly int _amount;

        public int Amount
        {
            get
            {
                return _amount; 
            }
        }

        public int TotalReceivedAmount { get; private set; }

        public int TotalCollectedAmount{ get; private set; }

        readonly string _donationType;

        public string DonationType
        {
            get
            {
                return _donationType;
            }
        }

        System.TimeSpan _timestamp;

        public System.TimeSpan Timestamp
        {
            get
            {
                return _timestamp;
            }
        }

        readonly AttrDic _metadata;

        public AttrDic Metadata
        {
            get
            {
                return _metadata;
            }
        }

        readonly Dictionary<long, int> _receivedMap;

        public IEnumerator<KeyValuePair<long, int>> ReceivedMapEnumerator
        {
            get
            {
                return _receivedMap.GetEnumerator();
            }
        }

        readonly Dictionary<long, int> _collectedMap;

        public IEnumerator<KeyValuePair<long, int>> CollectedMapEnumerator
        {
            get
            {
                return _collectedMap.GetEnumerator();
            }
        }

        public ItemRequest(long requesterId, string requestUuid, int itemId, int amount, string donationType, System.TimeSpan timestamp, AttrDic metadata)
        {
            _requesterId = requesterId;
            _requestUuid = requestUuid;
            _itemId = itemId;
            _amount = amount;
            _donationType = donationType;
            _timestamp = timestamp;
            _metadata = metadata;

            TotalReceivedAmount = 0;
            TotalCollectedAmount = 0;

            _receivedMap = new Dictionary<long, int>();
            _collectedMap = new Dictionary<long, int>();
        }

        public void SetCollected(long contributorId, int amount)
        {
            if(_collectedMap.ContainsKey(contributorId))
            {
                _collectedMap[contributorId] = amount;
            }
            else
            {
                _collectedMap.Add(contributorId, amount);
            }
            TotalCollectedAmount += amount;
        }

        public void AddContribution(long contributorId, int amount)
        {
            if(_receivedMap.ContainsKey(contributorId))
            {
                _receivedMap[contributorId] += amount;
            }
            else
            {
                _receivedMap.Add(contributorId, amount);
            }
            TotalReceivedAmount += amount;
        }

        public void CollectContribution(long contributorId, int amount)
        {
            int received;
            int collected;
            _receivedMap.TryGetValue(contributorId, out received);
            _collectedMap.TryGetValue(contributorId, out collected);

            int diff = received - collected;
            if(amount > diff)
            {
                amount = diff;
            }

            if(_collectedMap.ContainsKey(contributorId))
            {
                _collectedMap[contributorId] += amount;
            }
            else
            {
                _collectedMap.Add(contributorId, amount);
            }
            TotalCollectedAmount += amount;
        }

        public int GetContributedBy(long contributorId)
        {
            int amount;
            _receivedMap.TryGetValue(contributorId, out amount);
            return amount;
        }

        public int GetCollectedBy(long collectorId)
        {
            int amount;
            _collectedMap.TryGetValue(collectorId, out amount);
            return amount;
        }

        public override string ToString()
        {
            const string message = "RequesterId:{0}\nRequestUuid:{1}\nItemId:{2} - Amount:{3} - Amount Received:{4} - Amount Collected:{5}";
            return string.Format(message, RequesterId, RequestUuid, ItemId, Amount, TotalReceivedAmount, TotalCollectedAmount);
        }

        public string ToStringExtended()
        {
            const string message = "RequesterId:{0}\nRequestUuid:{1}\nItemId:{2} - Amount:{3} - Amount Received:{4} - Amount Collected:{5}\\n Type: {6}\\n Timestamp: {7}\\n Metadata: {8}";
            return string.Format(message, RequesterId, RequestUuid, ItemId, Amount, TotalReceivedAmount, TotalCollectedAmount, DonationType, Timestamp, Metadata);
        }
    }
}
