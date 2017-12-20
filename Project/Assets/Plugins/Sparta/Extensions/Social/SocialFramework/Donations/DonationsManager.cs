using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Connection;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public sealed class DonationsManager : IDisposable
    {
        public enum ActionType
        {
            OnDonationRequestReceived,
            OnDonationContributeReceived,
            OnDonationRequestRemoved,
            OnDonationRequestUserRemoved
        }

        #region String Keys

        public const string kUserId = "user_id";
        public const string kItemId = "item_id";
        public const string kAmount = "amount";
        public const string kRequesterId = "requester_id";
        public const string kRequestUuid = "donation_id";
        public const string kContributorId = "contributor_id";
        public const string kType = "type";
        public const string kDonationType = "donation_type";
        public const string kCreatedAt = "created_at";
        public const string kTs = "ts";
        public const string kMetadata = "metadata";
         
        public const string kResultOperation = "result";
        public const string kDonations = "donations";
        public const string kStats = "stats";
        public const string kNbRequest = "nb_request";
        public const string kNbDonation = "nb_donation";
        public const string kEndCooldownTs = "end_cooldown_ts";
        public const string kContributions = "contributions";
        public const string kAmountContributed = "amount_contributed";
        public const string kAmountCollected = "amount_collected";
         
        public const string kDonationLoginMethod = "donation.login";
        public const string kDonationRequestMethod = "donation.request";
        public const string kDonationContributeMethod = "donation.contribute";
        public const string kDonationCollectMethod = "donation.collect";
        public const string kDonationRemoveMethod = "donation.remove";
         
        #endregion

        public event Action<ActionType, AttrDic> DonationsSignal;

        public bool IsLoggedIn { get; private set; }

        public int NumRequests{ get; private set; }

        public int NumDonations{ get; private set; }

        public TimeSpan EndCooldownTs{ get; private set; }

        IConnectionManager _connectionManager;
        List<ItemRequest> _itemsRequests;

        public ReadOnlyCollection<ItemRequest> ItemsRequests
        {
            get
            {
                return _itemsRequests.AsReadOnly();
            }
        }

        public DonationsManager()
        {
            _itemsRequests = new List<ItemRequest>();
        }

        public void Setup(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;

            _connectionManager.OnNotificationReceived += OnNotificationReceived;
            _connectionManager.OnProcessServices += OnProcessServices;
        }

        public void Dispose()
        {
            _connectionManager.OnNotificationReceived -= OnNotificationReceived;
            _connectionManager.OnProcessServices -= OnProcessServices;

            Clear();
        }

        public void Login(Action<Error> completionCallback)
        {
            if(IsLoggedIn)
            {
                completionCallback(new Error());
                return;
            }

            var dict = new AttrDic();
            dict.SetValue(kUserId, _connectionManager.LoginData.UserId.ToString());

            SocialPoint.WAMP.Caller.HandlerCall handler = (error, args, kwargs) => {
                if(!Error.IsNullOrEmpty(error))
                {
                    completionCallback(error);
                    return;
                }

                OnProcessServices(kwargs.Get(kResultOperation).AsDic);

                if(!IsLoggedIn)
                {
                    completionCallback(new Error("[DonationsManager] Failed to process services while Login"));
                }
                else
                {
                    completionCallback(new Error());
                }
            };
            _connectionManager.Call(kDonationLoginMethod, null, dict, handler);
        }

        void Clear()
        {
            IsLoggedIn = false;
            NumRequests = 0;
            NumDonations = 0;
            EndCooldownTs = TimeSpan.Zero;
            _itemsRequests.Clear();
        }

        public void RequestItem(int itemId, int amount, string type, AttrDic metadata, Action<Error, ItemRequest> completionCallback)
        {
            if(!IsLoggedIn)
            {
                const string msg = "[DonationsManager] Trying to request an item when not logged in";
                Log.e(msg);
                completionCallback(new Error(msg), null);
                return;
            }

            var userId = (long)_connectionManager.LoginData.UserId;
            var requestUuid = RandomUtils.GetUuid();

            var dict = new AttrDic();
            dict.SetValue(kUserId, userId);
            dict.SetValue(kItemId, itemId);
            dict.SetValue(kAmount, amount);
            dict.SetValue(kRequestUuid, requestUuid);
            dict.SetValue(kType, type);
            dict.Set(kMetadata, metadata);


            SocialPoint.WAMP.Caller.HandlerCall handler = (error, args, kwargs) => {
                if(!Error.IsNullOrEmpty(error))
                {
                    completionCallback(error, null);
                    return;
                }

                var itemRequest = CreateItemRequest(userId, requestUuid, itemId, amount, type, TimeSpan.FromSeconds(TimeUtils.Timestamp), metadata);

                completionCallback(new Error(), itemRequest);
            };
            _connectionManager.Call(kDonationRequestMethod, null, dict, handler);
        }

        public void ContributeItem(long requesterId, string requestUuid, int amount, string type, Action<Error> completionCallback)
        {
            if(!IsLoggedIn)
            {
                const string msg = "[DonationsManager] Trying to contribute to an item when not logged in";
                Log.e(msg);
                completionCallback(new Error(msg));
                return;
            }

            long userId = (long)_connectionManager.LoginData.UserId;

            var dict = new AttrDic();
            dict.SetValue(kUserId, userId.ToString());
            dict.SetValue(kRequesterId, requesterId);
            dict.SetValue(kRequestUuid, requestUuid);
            dict.SetValue(kAmount, amount);
            dict.SetValue(kType, type);

            SocialPoint.WAMP.Caller.HandlerCall handler = (error, args, kwargs) => {
                if(!Error.IsNullOrEmpty(error))
                {
                    completionCallback(error);
                    return;
                }

                ItemRequest itemRequest = FindItemRequest(requesterId, requestUuid);
                AddContribution(itemRequest, userId, amount);

                completionCallback(new Error());
            };
            _connectionManager.Call(kDonationContributeMethod, null, dict, handler);
        }

        public void CollectItem(long contributorId, string requestUuid, int amount, string type, Action<Error> completionCallback)
        {
            if(!IsLoggedIn)
            {
                const string msg = "[DonationsManager] Trying to collect an item when not logged in";
                Log.e(msg);
                completionCallback(new Error(msg));
                return;
            }

            long userId = (long)_connectionManager.LoginData.UserId;

            var dict = new AttrDic();
            dict.SetValue(kUserId, userId.ToString());
            dict.SetValue(kContributorId, contributorId);
            dict.SetValue(kRequestUuid, requestUuid);
            dict.SetValue(kAmount, amount);
            dict.SetValue(kType, type);

            SocialPoint.WAMP.Caller.HandlerCall handler = (error, args, kwargs) => {
                if(!Error.IsNullOrEmpty(error))
                {
                    completionCallback(error);
                    return;
                }

                ItemRequest itemRequest = FindItemRequest(userId, requestUuid);
                CollectContribution(itemRequest, contributorId);

                completionCallback(new Error());
            };
            _connectionManager.Call(kDonationCollectMethod, null, dict, handler);
        }

        public void RemoveRequest(string requestUuid, string type, Action<Error> completionCallback)
        {
            if(!IsLoggedIn)
            {
                const string msg = "[DonationsManager] Trying to remove an item when not logged in";
                Log.e(msg);
                completionCallback(new Error(msg));
                return;
            }

            long userId = (long)_connectionManager.LoginData.UserId;

            var dict = new AttrDic();
            dict.SetValue(kUserId, userId.ToString());
            dict.SetValue(kRequestUuid, requestUuid);
            dict.SetValue(kType, type);

            SocialPoint.WAMP.Caller.HandlerCall handler = (error, args, kwargs) => {
                if(!Error.IsNullOrEmpty(error))
                {
                    completionCallback(error);
                    return;
                }

                RemoveItemRequest(userId, requestUuid);

                completionCallback(new Error());
            };
            _connectionManager.Call(kDonationRemoveMethod, null, dict, handler);
        }

        public ItemRequest GetItemRequest(long requesterId, string requestUuid)
        {
            if(!IsLoggedIn)
            {
                const string msg = "[DonationsManager] Trying to get an item when not logged in";
                Log.e(msg);
                return null;
            }

            return FindItemRequest(requesterId, requestUuid);
        }

        void OnNotificationReceived(int type, string topicType, AttrDic data)
        {
            if(!IsLoggedIn)
            {
                return;
            }
                
            switch(type)
            {
            case NotificationType.BroadcastDonationRequest:
                {
                    OnDonationRequestReceived(data);
                    break;
                }
            case NotificationType.BroadcastDonationContribute:
                {
                    OnDonationContributeReceived(data);
                    break;
                }

            case NotificationType.BroadcastDonationRemove:
                {
                    OnDonationRequestRemoved(data);
                    break;
                }

            case NotificationType.BroadcastDonationUserRemove:
                {
                    OnDonationUserRemoved(data);
                    break;
                }
            }
        }

        void OnProcessServices(AttrDic servicesDic)
        {
            Clear();

            // backend enabled deferred login
            if(!servicesDic.ContainsKey(kDonations))
            {
                return;
            }

            var donationsDict = servicesDic.Get(kDonations).AsDic;

            var stats = donationsDict.Get(kStats).AsDic;
            NumRequests = stats.GetValue(kNbRequest).ToInt();
            NumDonations = stats.GetValue(kNbDonation).ToInt();
            EndCooldownTs = TimeSpan.FromSeconds(stats.GetValue(kEndCooldownTs).ToLong());

            var donationsList = donationsDict.Get(kDonations).AsList;

            foreach(var donation in donationsList)
            {
                var donationDict = donation.AsDic;

                var userId = donationDict.GetValue(kRequesterId).ToLong();

                var requestUuid = donationDict.GetValue(kRequestUuid).ToString();
                var itemId = donationDict.GetValue(kItemId).ToInt();
                var amount = donationDict.GetValue(kAmount).ToInt();

                var donationType = donationDict.GetValue(kType).ToString();
                var timestamp = TimeSpan.FromSeconds(donationDict.GetValue(kCreatedAt).ToLong());
                var metadata = donationDict.Get(kMetadata).AsDic;

                var itemRequest = CreateItemRequest(userId, requestUuid, itemId, amount, donationType, timestamp, metadata);

                var contributionsList = donationDict.Get(kContributions).AsList;
                foreach(var contribution in contributionsList)
                {
                    var contributionDict = contribution.AsDic;
                    var contributorId = contributionDict.GetValue(kContributorId).ToLong();
                    var amountContributed = contributionDict.GetValue(kAmountContributed).ToInt();
                    var amountCollected = contributionDict.GetValue(kAmountCollected).ToInt();

                    itemRequest.AddContribution(contributorId, amountContributed);
                    itemRequest.SetCollected(contributorId, amountCollected);
                }
            }

            IsLoggedIn = true;
        }

        void OnDonationRequestReceived(AttrDic dictNotificationInfo)
        {
            var userId = dictNotificationInfo.GetValue(kUserId).ToLong();

            var requestUuid = dictNotificationInfo.GetValue(kRequestUuid).ToString();

            var itemId = dictNotificationInfo.GetValue(kItemId).ToInt();
            var amount = dictNotificationInfo.GetValue(kAmount).ToInt();

            var donationType = dictNotificationInfo.GetValue(kDonationType).ToString();
            var timestamp = TimeSpan.FromSeconds(dictNotificationInfo.GetValue(kTs).ToLong());
            var metadata = dictNotificationInfo.Get(kMetadata).AsDic;

            CreateItemRequest(userId, requestUuid, itemId, amount, donationType, timestamp, metadata);

            if(DonationsSignal != null)
            {
                DonationsSignal(ActionType.OnDonationRequestReceived, dictNotificationInfo);
            }
        }

        void OnDonationContributeReceived(AttrDic dictNotificationInfo)
        {
            var userId = dictNotificationInfo.GetValue(kUserId).ToLong();

            var requesterId = dictNotificationInfo.GetValue(kRequesterId).ToLong();

            var requestUuid = dictNotificationInfo.GetValue(kRequestUuid).ToString();

            var amount = dictNotificationInfo.GetValue(kAmount).ToInt();

            var itemRequest = FindItemRequest(requesterId, requestUuid);
            AddContribution(itemRequest, userId, amount);

            if(DonationsSignal != null)
            {
                DonationsSignal(ActionType.OnDonationContributeReceived, dictNotificationInfo);
            }

            // Other params included in dictNotificationInfo:
            // "ts" timestamp
            // "donation_type" string
        }

        void OnDonationRequestRemoved(AttrDic dictNotificationInfo)
        {
            var userId = dictNotificationInfo.GetValue(kUserId).ToLong();
            var requestUuid = dictNotificationInfo.GetValue(kRequestUuid).ToString();

            RemoveItemRequest(userId, requestUuid);

            if(DonationsSignal != null)
            {
                DonationsSignal(ActionType.OnDonationRequestRemoved, dictNotificationInfo);
            }
        }

        void OnDonationUserRemoved(AttrDic dictNotificationInfo)
        {
            var myUserId = (long)_connectionManager.LoginData.UserId;

            var userId = dictNotificationInfo.GetValue(kUserId).ToLong();
            var donationType = dictNotificationInfo.GetValue(kDonationType).ToString();

            if(myUserId == userId)
            {
                RemoveUserRequests(donationType);
            }
            else
            {
                RemoveUserRequests(userId, donationType);
            }

            if(DonationsSignal != null)
            {
                DonationsSignal(ActionType.OnDonationRequestUserRemoved, dictNotificationInfo);
            }
        }

        ItemRequest FindItemRequest(long requesterId, string requestUuid)
        {
            var item = _itemsRequests.Find(element => element.RequesterId == requesterId && element.RequestUuid == requestUuid);

            if(item == null)
            {
                Log.e(string.Format("[DonationsManager] ItemRequest with uuid {0} from user {1} not found", requestUuid, requesterId));
            }

            return item;
        }

        ItemRequest CreateItemRequest(long requesterId, string requestUuid, int itemId, int amount, string donationType, TimeSpan timestamp, AttrDic metadata)
        {
            var itemRequest = new ItemRequest(requesterId, requestUuid, itemId, amount, donationType, timestamp, metadata);
            _itemsRequests.Add(itemRequest);
            NumRequests++;
            return itemRequest;
        }

        void RemoveItemRequest(long requesterId, string requestUuid)
        {
            var removed = _itemsRequests.RemoveAll(element => element.RequesterId == requesterId && element.RequestUuid == requestUuid);
            NumRequests -= removed;
        }

        void RemoveUserRequests(string type)
        {
            var removed = _itemsRequests.RemoveAll(element => element.DonationType == type);
            NumRequests -= removed;
        }

        void RemoveUserRequests(long requesterId, string type)
        {
            var removed = _itemsRequests.RemoveAll(element => element.DonationType == type && element.RequesterId == requesterId);
            NumRequests -= removed;
        }

        void AddContribution(ItemRequest itemRequest, long contributorId, int amount)
        {
            itemRequest.AddContribution(contributorId, amount);
            NumDonations++;
        }

        void CollectContribution(ItemRequest itemRequest, long contributorId)
        {
            itemRequest.CollectContribution(contributorId);
        }
    }
}
