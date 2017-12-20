using NSubstitute;
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
        const ulong kLocalUserId = 123456789;
        const int kNumRequests = 10;
        const int kNumDonations = 10;
        TimeSpan kEndCooldownTS;

        DonationsManager _manager;
        IConnectionManager _connection;

        [SetUp]
        public void Setup()
        {
            kEndCooldownTS = TimeSpan.FromSeconds(456);

            _manager = new DonationsManager();
            _connection = Substitute.For<IConnectionManager>();
            _manager.Setup(_connection);

            _connection.LoginData.UserId.Returns(kLocalUserId);
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
            statsDic.SetValue(DonationsManager.kNbRequest, numRequests);
            statsDic.SetValue(DonationsManager.kNbDonation, numDonations);
            statsDic.SetValue(DonationsManager.kEndCooldownTs, endCooldownTs);

            var donationsDic = new AttrDic();
            donationsDic.Set(DonationsManager.kStats, statsDic);

            var servicesDic = new AttrDic();
            servicesDic.Set(DonationsManager.kDonations, donationsDic);

            return servicesDic;
        }

        void LoginManager()
        {
            _connection.OnProcessServices += Raise.Event<Action<AttrDic>>(GetDefaultServicesDic());
        }

        [Test]
        public void ProcessServicesWithData()
        {
            Assert.IsFalse(_manager.IsLoggedIn);
            
            _connection.OnProcessServices += Raise.Event<Action<AttrDic>>(GetFakeServicesDic());

            Assert.IsTrue(_manager.IsLoggedIn);
            Assert.AreEqual(kNumRequests, _manager.NumRequests);
            Assert.AreEqual(kNumDonations, _manager.NumDonations);
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
                .Do(callInfo => 
                    {
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
            Assert.AreEqual(kNumDonations, _manager.NumDonations);
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

            _connection.When(x => x.Call(DonationsManager.kDonationRequestMethod, Arg.Any<AttrList>(), Arg.Any<AttrDic>(), Arg.Any<HandlerCall>()))
                .Do(callInfo => 
                    {
                        var resultHandler = (HandlerCall)callInfo.Args()[3];
                        resultHandler(new Error(), null, null);
                    });

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

        void AddItemRequest(long userId, string uuid, int itemId, int amount, string type, AttrDic metadata)
        {
            var notificationDic = new AttrDic();
            notificationDic.SetValue(DonationsManager.kUserId, userId);
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
            const long userId = 147;
            const string uuid = "A-B-C-D";
            const int itemId = 10;
            const int amount = 12;
            const string type = "fake_type";

            LoginManager();
            AddItemRequest(userId, uuid, itemId, amount, type, new AttrDic());

            _connection.When(x => x.Call(DonationsManager.kDonationContributeMethod, Arg.Any<AttrList>(), Arg.Any<AttrDic>(), Arg.Any<HandlerCall>()))
                .Do(callInfo => 
                    {
                        var resultHandler = (HandlerCall)callInfo.Args()[3];
                        resultHandler(new Error(), null, null);
                    });

            const int donateAmount = 6;
            var executed = false;
            Action<Error> handler = err => {
                executed = true;
                Assert.IsTrue(Error.IsNullOrEmpty(err));
            };

            var request = _manager.GetItemRequest(userId, uuid);
            Assert.AreEqual(0, request.TotalReceivedAmount);

            _manager.ContributeItem(userId, uuid, donateAmount, type, handler);
            Assert.IsTrue(executed);

            Assert.AreEqual(amount, request.Amount);
            Assert.AreEqual(donateAmount, request.TotalReceivedAmount);
        }
    }
}
