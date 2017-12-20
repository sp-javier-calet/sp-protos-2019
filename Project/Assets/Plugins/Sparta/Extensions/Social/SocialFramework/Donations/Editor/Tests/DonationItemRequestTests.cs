using NUnit.Framework;
using System;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    [TestFixture]
    [Category("SocialPoint.Social")]
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

        [Test]
        public void MultipleContributeAndCollect()
        {
            const int contribution1 = 5;
            const int userId1 = 123;
            const int contribution2 = 10;
            const int userId2 = 456;

            var request = CreateRequest(20);

            Assert.AreEqual(0, request.GetContributedBy(userId1));
            Assert.AreEqual(0, request.GetContributedBy(userId2));
            Assert.AreEqual(0, request.TotalReceivedAmount);
            Assert.AreEqual(0, request.GetCollectedBy(userId1));
            Assert.AreEqual(0, request.GetCollectedBy(userId2));
            Assert.AreEqual(0, request.TotalCollectedAmount);

            request.AddContribution(userId1, contribution1);

            Assert.AreEqual(contribution1, request.GetContributedBy(userId1));
            Assert.AreEqual(0, request.GetContributedBy(userId2));
            Assert.AreEqual(contribution1, request.TotalReceivedAmount);
            Assert.AreEqual(0, request.GetCollectedBy(userId1));
            Assert.AreEqual(0, request.GetCollectedBy(userId2));
            Assert.AreEqual(0, request.TotalCollectedAmount);

            request.AddContribution(userId2, contribution2);

            Assert.AreEqual(contribution1, request.GetContributedBy(userId1));
            Assert.AreEqual(contribution2, request.GetContributedBy(userId2));
            Assert.AreEqual(contribution1 + contribution2, request.TotalReceivedAmount);
            Assert.AreEqual(0, request.GetCollectedBy(userId1));
            Assert.AreEqual(0, request.GetCollectedBy(userId2));
            Assert.AreEqual(0, request.TotalCollectedAmount);

            request.CollectContribution(userId1);

            Assert.AreEqual(contribution1, request.GetContributedBy(userId1));
            Assert.AreEqual(contribution2, request.GetContributedBy(userId2));
            Assert.AreEqual(contribution1 + contribution2, request.TotalReceivedAmount);
            Assert.AreEqual(contribution1, request.GetCollectedBy(userId1));
            Assert.AreEqual(0, request.GetCollectedBy(userId2));
            Assert.AreEqual(contribution1, request.TotalCollectedAmount);

            request.CollectContribution(userId2);

            Assert.AreEqual(contribution1, request.GetContributedBy(userId1));
            Assert.AreEqual(contribution2, request.GetContributedBy(userId2));
            Assert.AreEqual(contribution1 + contribution2, request.TotalReceivedAmount);
            Assert.AreEqual(contribution1, request.GetCollectedBy(userId1));
            Assert.AreEqual(contribution2, request.GetCollectedBy(userId2));
            Assert.AreEqual(contribution1 + contribution2, request.TotalCollectedAmount);
        }
    }
}
