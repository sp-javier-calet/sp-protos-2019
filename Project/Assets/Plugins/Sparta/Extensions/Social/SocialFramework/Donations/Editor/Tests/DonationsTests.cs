﻿using NSubstitute;
using NUnit.Framework;
using SocialPoint.Attributes;
using SocialPoint.Connection;
using System;
using SocialPoint.Base;
using SocialPoint.WAMP.Caller;

namespace SocialPoint.Social
{
    [TestFixture]
    [Category("SocialPoint.Social")]
    public class DonationsTests
    {
        const long kLocalUserId = 123456789;
        const int kNumRequests = 10;
        const int kNumDonations = 2;
        TimeSpan kEndCooldownTS;

        DonationsManager _manager;
        IConnectionManager _connection;
        Action<DonationsManager.ActionType, AttrDic> _donationsSignal;

        [SetUp]
        public void Setup()
        {
            kEndCooldownTS = TimeSpan.FromSeconds(456);

            _manager = new DonationsManager();
            _connection = Substitute.For<IConnectionManager>();
            _manager.Setup(_connection);

            _donationsSignal = Substitute.For<Action<DonationsManager.ActionType, AttrDic>>();
            _manager.DonationsSignal += _donationsSignal;

            _connection.LoginData.UserId.Returns((ulong)kLocalUserId);
        }

        AttrDic GetDefaultServicesDic()
        {
            return GetServicesDic(0, 0, 0);
        }

        AttrDic GetFakeServicesDic()
        {
            return GetServicesDic(kNumRequests, kNumDonations, (long)kEndCooldownTS.TotalSeconds);
        }

        AttrDic GetServicesDic(int numRequests, int numDonations, long endCooldownTs)
        {
            var statsDic = new AttrDic();
            statsDic.SetValue(DonationsManager.kEndCooldownTs, endCooldownTs);

            var requestsList = new AttrList();
            for(int i = 0; i < numRequests; ++i)
            {
                requestsList.Add(GetDonationRequestDic(numDonations));
            }

            var donationsDic = new AttrDic();
            donationsDic.Set(DonationsManager.kStats, statsDic);
            donationsDic.Set(DonationsManager.kDonationsList, requestsList);

            var servicesDic = new AttrDic();
            servicesDic.Set(DonationsManager.kDonationsService, donationsDic);

            return servicesDic;
        }

        AttrDic GetDonationRequestDic(int numDonations)
        {
            var dic = new AttrDic();
            dic.SetValue(DonationsManager.kRequesterId, "123");
            dic.SetValue(DonationsManager.kRequestUuid, "F-E-D-C");
            dic.SetValue(DonationsManager.kItemId, 321);
            dic.SetValue(DonationsManager.kAmount, 6);
            dic.SetValue(DonationsManager.kType, "type_fake");
            dic.SetValue(DonationsManager.kCreatedAt, 654);
            dic.Set(DonationsManager.kMetadata, new AttrDic());
            var contributionsList = new AttrList();
            for(int i = 0; i < numDonations; ++i)
            {
                var contribution = new AttrDic();
                contribution.SetValue(DonationsManager.kContributorId, "789");
                contribution.SetValue(DonationsManager.kAmountContributed, 2);
                contribution.SetValue(DonationsManager.kAmountCollected, 1);
                contributionsList.Add(contribution);
            }
            dic.Set(DonationsManager.kContributions, contributionsList);
            return dic;
        }

        void LoginManager()
        {
            _connection.OnProcessServices += Raise.Event<Action<AttrDic>>(GetDefaultServicesDic());
        }

        void RegisterSuccessHandlerForRPC(string rpc, Action actionBeforeCompletion)
        {
            _connection.ClearReceivedCalls();
            _connection.When(x => x.Call(rpc, Arg.Any<AttrList>(), Arg.Any<AttrDic>(), Arg.Any<HandlerCall>()))
                .Do(callInfo => {
                if(actionBeforeCompletion != null)
                {
                    actionBeforeCompletion();
                }
                var resultHandler = (HandlerCall)callInfo.Args()[3];
                resultHandler(new Error(), null, null);
            });
        }

        [Test]
        public void ProcessServicesWithData()
        {
            Assert.IsFalse(_manager.IsLoggedIn);
            
            _connection.OnProcessServices += Raise.Event<Action<AttrDic>>(GetFakeServicesDic());

            Assert.IsTrue(_manager.IsLoggedIn);
            Assert.AreEqual(kNumRequests, _manager.NumRequests);
            Assert.AreEqual(kNumDonations * kNumRequests, _manager.NumDonations);
            Assert.AreEqual(kEndCooldownTS, _manager.EndCooldownTs);
        }

        [Test]
        public void ProcessServicesWithoutData()
        {
            Assert.IsFalse(_manager.IsLoggedIn);

            _connection.OnProcessServices += Raise.Event<Action<AttrDic>>(new AttrDic());

            Assert.IsFalse(_manager.IsLoggedIn);
        }

        [Test]
        public void DeferredLogin()
        {
            Assert.IsFalse(_manager.IsLoggedIn);

            _connection.OnProcessServices += Raise.Event<Action<AttrDic>>(new AttrDic());

            Assert.IsFalse(_manager.IsLoggedIn);

            _connection.When(x => x.Call(DonationsManager.kDonationLoginMethod, Arg.Any<AttrList>(), Arg.Any<AttrDic>(), Arg.Any<HandlerCall>()))
                .Do(callInfo => {
                var result = new AttrDic();
                result.Set(DonationsManager.kResultOperation, GetFakeServicesDic());
                var handler = (HandlerCall)callInfo.Args()[3];
                handler(new Error(), null, result);
            });

            bool executed = false;
            _manager.Login(error => {
                executed = true;
                Assert.IsTrue(Error.IsNullOrEmpty(error));
            });

            Assert.IsTrue(executed);
            Assert.IsTrue(_manager.IsLoggedIn);
            Assert.AreEqual(kNumRequests, _manager.NumRequests);
            Assert.AreEqual(kNumDonations * kNumRequests, _manager.NumDonations);
            Assert.AreEqual(kEndCooldownTS, _manager.EndCooldownTs);
        }

        [Test]
        public void RequestItem()
        {
            const int itemId = 10;
            const int amount = 12;
            const string type = "fake_type";
            var metadata = new AttrDic();
            const string key = "test_key";
            const string value = "test_value";
            metadata.SetValue(key, value);

            LoginManager();

            RegisterSuccessHandlerForRPC(DonationsManager.kDonationRequestMethod, null);

            var executed = false;
            Action<Error, ItemRequest> handler = (err, item) => {
                executed = true;
                Assert.IsTrue(Error.IsNullOrEmpty(err));

                Assert.AreEqual(itemId, item.ItemId);
                Assert.AreEqual(amount, item.Amount);
                Assert.AreEqual(type, item.DonationType);

                Assert.IsTrue(item.Metadata.ContainsKey(key));
                Assert.AreEqual(value, item.Metadata.GetValue(key).ToString());
            };

            Assert.AreEqual(0, _manager.NumRequests);

            _manager.RequestItem(itemId, amount, type, metadata, handler);
            Assert.IsTrue(executed);

            Assert.AreEqual(1, _manager.NumRequests);
            var request = _manager.ItemsRequests.First();

            Assert.AreEqual(itemId, request.ItemId);
            Assert.AreEqual(amount, request.Amount);
            Assert.AreEqual(type, request.DonationType);

            Assert.IsTrue(request.Metadata.ContainsKey(key));
            Assert.AreEqual(value, request.Metadata.GetValue(key).ToString());
        }

        void AddItemRequest(long requesterId, string uuid, int itemId, int amount, string type, AttrDic metadata)
        {
            var notificationDic = new AttrDic();
            notificationDic.SetValue(DonationsManager.kUserId, requesterId);
            notificationDic.SetValue(DonationsManager.kRequestUuid, uuid);
            notificationDic.SetValue(DonationsManager.kItemId, itemId);
            notificationDic.SetValue(DonationsManager.kAmount, amount);
            notificationDic.SetValue(DonationsManager.kDonationType, type);
            notificationDic.SetValue(DonationsManager.kTs, 654);
            notificationDic.Set(DonationsManager.kMetadata, metadata);

            _connection.OnNotificationReceived += Raise.Event<NotificationReceivedDelegate>(NotificationType.BroadcastDonationRequest, string.Empty, notificationDic);
        }

        [Test]
        public void ContributeItem()
        {
            const long requesterId = 147;
            const string uuid = "A-B-C-D";
            const int itemId = 10;
            const int amount = 12;
            const string type = "fake_type";

            LoginManager();
            AddItemRequest(requesterId, uuid, itemId, amount, type, new AttrDic());

            RegisterSuccessHandlerForRPC(DonationsManager.kDonationContributeMethod, null);

            const int donateAmount = 6;
            var executed = false;
            Action<Error> handler = err => {
                executed = true;
                Assert.IsTrue(Error.IsNullOrEmpty(err));
            };

            var request = _manager.GetItemRequest(requesterId, uuid);
            Assert.AreEqual(0, request.TotalReceivedAmount);

            _manager.ContributeItem(requesterId, uuid, donateAmount, type, handler);
            Assert.IsTrue(executed);

            Assert.AreEqual(amount, request.Amount);
            Assert.AreEqual(donateAmount, request.TotalReceivedAmount);
        }

        void AddItemContribution(long userId, string uuid, int amount)
        {
            var notificationDic = new AttrDic();
            notificationDic.SetValue(DonationsManager.kUserId, userId);
            notificationDic.SetValue(DonationsManager.kRequesterId, kLocalUserId);
            notificationDic.SetValue(DonationsManager.kRequestUuid, uuid);
            notificationDic.SetValue(DonationsManager.kAmount, amount);

            _connection.OnNotificationReceived += Raise.Event<NotificationReceivedDelegate>(NotificationType.BroadcastDonationContribute, string.Empty, notificationDic);
        }

        [Test]
        public void CollectItem()
        {
            const string uuid = "A-B-C-D";
            const int itemId = 10;
            const int amount = 12;
            const string type = "fake_type";

            LoginManager();
            RegisterSuccessHandlerForRPC(DonationsManager.kDonationCollectMethod, null);

            var numsHandlerExecuted = 0;
            Action<Error> handler = err => {
                numsHandlerExecuted++;
                Assert.IsTrue(Error.IsNullOrEmpty(err));
            };

            AddItemRequest(kLocalUserId, uuid, itemId, amount, type, new AttrDic());
            var request = _manager.GetItemRequest(kLocalUserId, uuid);
            Assert.AreEqual(0, request.TotalReceivedAmount);

            const long contributorId1 = 852;
            const int contributionAmount1 = 4;
            AddItemContribution(contributorId1, uuid, contributionAmount1);

            _manager.CollectItem(contributorId1, uuid, type, handler);
            Assert.AreEqual(1, numsHandlerExecuted);
            Assert.AreEqual(contributionAmount1, request.TotalReceivedAmount);
            Assert.AreEqual(contributionAmount1, request.TotalCollectedAmount);
        }

        [Test]
        public void CollectItemWhileReceivingNewContribution()
        {
            const string uuid = "A-B-C-D";
            const int itemId = 10;
            const int amount = 12;
            const string type = "fake_type";

            LoginManager();

            var numsHandlerExecuted = 0;
            Action<Error> handler = err => {
                numsHandlerExecuted++;
                Assert.IsTrue(Error.IsNullOrEmpty(err));
            };

            AddItemRequest(kLocalUserId, uuid, itemId, amount, type, new AttrDic());
            var request = _manager.GetItemRequest(kLocalUserId, uuid);
            Assert.AreEqual(0, request.TotalReceivedAmount);

            const long contributorId1 = 852;
            const int contributionAmount1 = 4;
            const int contributionAmount2 = 6;
            RegisterSuccessHandlerForRPC(DonationsManager.kDonationCollectMethod, () => AddItemContribution(contributorId1, uuid, contributionAmount2));
            AddItemContribution(contributorId1, uuid, contributionAmount1);

            _manager.CollectItem(contributorId1, uuid, type, handler);

            Assert.AreEqual(1, numsHandlerExecuted);
            Assert.AreEqual(contributionAmount1 + contributionAmount2 * numsHandlerExecuted, request.TotalReceivedAmount);
            Assert.AreEqual(contributionAmount1 + contributionAmount2 * (numsHandlerExecuted - 1), request.TotalCollectedAmount);

            _manager.CollectItem(contributorId1, uuid, type, handler);

            Assert.AreEqual(2, numsHandlerExecuted);
            Assert.AreEqual(contributionAmount1 + contributionAmount2 * numsHandlerExecuted, request.TotalReceivedAmount);
            Assert.AreEqual(contributionAmount1 + contributionAmount2 * (numsHandlerExecuted - 1), request.TotalCollectedAmount);
        }

        [Test]
        public void CollectItemMultipleContributors()
        {
            const string uuid = "A-B-C-D";
            const int itemId = 10;
            const int amount = 12;
            const string type = "fake_type";

            LoginManager();
            RegisterSuccessHandlerForRPC(DonationsManager.kDonationCollectMethod, null);

            var numsHandlerExecuted = 0;
            Action<Error> handler = err => {
                numsHandlerExecuted++;
                Assert.IsTrue(Error.IsNullOrEmpty(err));
            };

            AddItemRequest(kLocalUserId, uuid, itemId, amount, type, new AttrDic());
            var request = _manager.GetItemRequest(kLocalUserId, uuid);
            Assert.AreEqual(0, request.TotalReceivedAmount);

            const long contributorId1 = 852;
            const long contributorId2 = 963;
            const int contributionAmount1 = 4;
            const int contributionAmount2 = 3;
            AddItemContribution(contributorId1, uuid, contributionAmount1);
            AddItemContribution(contributorId2, uuid, contributionAmount2);

            _manager.CollectItem(contributorId2, uuid, type, handler);
            Assert.AreEqual(1, numsHandlerExecuted);
            Assert.AreEqual(contributionAmount1 + contributionAmount2, request.TotalReceivedAmount);
            Assert.AreEqual(contributionAmount2, request.TotalCollectedAmount);

            _manager.CollectItem(contributorId1, uuid, type, handler);
            Assert.AreEqual(2, numsHandlerExecuted);
            Assert.AreEqual(contributionAmount1 + contributionAmount2, request.TotalReceivedAmount);
            Assert.AreEqual(contributionAmount1 + contributionAmount2, request.TotalCollectedAmount);
        }

        [Test]
        public void DonationsSignal()
        {
            LoginManager();

            const string uuid = "ABCD";
            AddItemRequest(kLocalUserId, uuid, 0, 0, "", new AttrDic());
            _donationsSignal.Received(1).Invoke(DonationsManager.ActionType.OnDonationRequestReceived, Arg.Any<AttrDic>());

            AddItemContribution(963, uuid, 0);
            _donationsSignal.Received(1).Invoke(DonationsManager.ActionType.OnDonationContributeReceived, Arg.Any<AttrDic>());

            {
                var notificationDic = new AttrDic();
                notificationDic.SetValue(DonationsManager.kUserId, kLocalUserId);
                notificationDic.SetValue(DonationsManager.kRequestUuid, uuid);
                _connection.OnNotificationReceived += Raise.Event<NotificationReceivedDelegate>(NotificationType.BroadcastDonationRemove, string.Empty, notificationDic);
            }
            _donationsSignal.Received(1).Invoke(DonationsManager.ActionType.OnDonationRequestRemoved, Arg.Any<AttrDic>());
        }

        [Test]
        public void DonationsSignalRemoveOwnUser()
        {
            LoginManager();

            const string uuid = "ABCD";
            const string donationType1 = "type1";
            const string donationType2 = "type2";
            const long anotherUser = 159;

            AddItemRequest(kLocalUserId, uuid, 0, 0, donationType1, new AttrDic());
            AddItemRequest(kLocalUserId, uuid, 0, 0, donationType2, new AttrDic());
            AddItemRequest(anotherUser, uuid, 0, 0, donationType1, new AttrDic());

            Assert.AreEqual(3, _manager.NumRequests);

            {
                var notificationDic = new AttrDic();
                notificationDic.SetValue(DonationsManager.kUserId, kLocalUserId);
                notificationDic.SetValue(DonationsManager.kDonationType, donationType1);
                _connection.OnNotificationReceived += Raise.Event<NotificationReceivedDelegate>(NotificationType.BroadcastDonationUserRemove, string.Empty, notificationDic);
            }
            _donationsSignal.Received(1).Invoke(DonationsManager.ActionType.OnDonationRequestUserRemoved, Arg.Any<AttrDic>());

            Assert.AreEqual(1, _manager.NumRequests);
            Assert.AreEqual(donationType2, _manager.ItemsRequests.First().DonationType);
            Assert.AreEqual(kLocalUserId, _manager.ItemsRequests.First().RequesterId);
        }

        [Test]
        public void DonationsSignalRemoveOtherUser()
        {
            LoginManager();

            const string uuid = "ABCD";
            const string donationType1 = "type1";
            const string donationType2 = "type2";
            const long anotherUser = 159;

            AddItemRequest(kLocalUserId, uuid, 0, 0, donationType1, new AttrDic());
            AddItemRequest(kLocalUserId, uuid, 0, 0, donationType2, new AttrDic());
            AddItemRequest(anotherUser, uuid, 0, 0, donationType1, new AttrDic());

            Assert.AreEqual(3, _manager.NumRequests);

            {
                var notificationDic = new AttrDic();
                notificationDic.SetValue(DonationsManager.kUserId, anotherUser);
                notificationDic.SetValue(DonationsManager.kDonationType, donationType1);
                _connection.OnNotificationReceived += Raise.Event<NotificationReceivedDelegate>(NotificationType.BroadcastDonationUserRemove, string.Empty, notificationDic);
            }
            _donationsSignal.Received(1).Invoke(DonationsManager.ActionType.OnDonationRequestUserRemoved, Arg.Any<AttrDic>());

            Assert.AreEqual(2, _manager.NumRequests);
            foreach(var request in _manager.ItemsRequests)
            {
                Assert.AreEqual(kLocalUserId, request.RequesterId);
            }
        }
    }
}
