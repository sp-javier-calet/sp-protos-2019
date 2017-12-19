using NUnit.Framework;
using System;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public class DonationItemRequestTests
    {
        [SetUp]
        public void SetUp()
        {

        }

        static ItemRequest CreateRequest(int amount)
        {
            return new ItemRequest(123, "fake_uuid", 456, amount, "fake_type", TimeSpan.Zero, null);
        }

        [Test]
        public void InitialState()
        {
            const int requesterId = 123;
            const string uuid = "fake_uuid";
            const int itemId = 456;
            const int amount = 789;
            const string type = "fake_type";
            var ts = TimeSpan.FromSeconds(987);
            var metadata = new AttrDic();

            var request = new ItemRequest(requesterId, uuid, itemId, amount, type, ts, metadata);

            Assert.AreEqual(requesterId, request.RequesterId);
            Assert.AreEqual(uuid, request.RequestUuid);
            Assert.AreEqual(itemId, request.ItemId);
            Assert.AreEqual(amount, request.Amount);
            Assert.AreEqual(type, request.DonationType);
            Assert.AreEqual(ts, request.Timestamp);
            Assert.AreEqual(metadata, request.Metadata);

            Assert.AreEqual(0, request.TotalReceivedAmount);
            Assert.AreEqual(0, request.TotalCollectedAmount);
        }

        [Test]
        public void AddContribution()
        {
            const int contribution = 2;
            const int userId = 123;

            var request = CreateRequest(5);

            Assert.AreEqual(0, request.TotalReceivedAmount);

            request.AddContribution(userId, contribution);

            Assert.AreEqual(contribution, request.TotalReceivedAmount);
        }

        [Test]
        public void SetCollected()
        {
            const int collected = 1;
            const int userId = 123;

            var request = CreateRequest(3);

            Assert.AreEqual(0, request.TotalCollectedAmount);

            request.AddContribution(userId, collected);
            request.SetCollected(userId, collected);

            Assert.AreEqual(collected, request.TotalCollectedAmount);
        }

        [Test]
        public void CollectSimple()
        {
            const int contributed = 1;
            const int userId = 123;

            var request = CreateRequest(3);
            request.AddContribution(userId, contributed);

            Assert.AreEqual(0, request.TotalCollectedAmount);

            request.CollectContribution(userId);

            Assert.AreEqual(contributed, request.TotalCollectedAmount);
        }

        [Test]
        public void CollectWithInitialCollected()
        {
            const int contributed = 5;
            const int collectedInitial = 3;
            const int userId = 123;

            var request = CreateRequest(10);
            request.AddContribution(userId, contributed);
            request.SetCollected(userId, collectedInitial);

            Assert.AreEqual(contributed, request.TotalReceivedAmount);
            Assert.AreEqual(collectedInitial, request.TotalCollectedAmount);

            request.CollectContribution(userId);

            Assert.AreEqual(contributed, request.TotalCollectedAmount);
        }
    }
}
