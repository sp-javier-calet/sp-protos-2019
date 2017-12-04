using NUnit.Framework;
using NSubstitute;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    [TestFixture]
    [Category("SocialPoint.Social.MessagingSystem")]
    class MessageOriginTests
    {
        [Test]
        public void SystemOrigin()
        {
            var factory = new MessageOriginSystemFactory();
            var origin = factory.CreateOrigin(Attr.InvalidDic);

            Assert.IsInstanceOf<MessageOriginSystem>(origin);
        }

        [Test]
        public void UserOrigin()
        {
            const string kName = "TestName";
            const string kUid = "123456789";

            var playerFactory = new SocialPlayerFactory();
            var basicDataFactory = Substitute.For<SocialPlayerFactory.IFactory>();
            basicDataFactory.CreateElement(Arg.Any<AttrDic>()).ReturnsForAnyArgs(x => {
                var component = new SocialPlayer.BasicData();
                component.Name = kName;
                component.Uid = kUid;
                return component;
            });
            playerFactory.AddFactory(basicDataFactory);

            var factory = new MessageOriginUserFactory(playerFactory);
            var origin = factory.CreateOrigin(Attr.InvalidDic);

            Assert.IsInstanceOf<MessageOriginUser>(origin);
            var userOrigin = origin as MessageOriginUser;
            Assert.AreEqual(userOrigin.Player.Name, kName);
            Assert.AreEqual(userOrigin.Player.Uid, kUid);
        }

        [Test]
        public void AllianceOrigin()
        {
            const string kName = "TestName";
            const string kUid = "123456789";
            const int kScore = 999;

            var dataDic = new AttrDic();
            dataDic.SetValue("id", kUid);
            dataDic.SetValue("name", kName);
            dataDic.SetValue("score", kScore);

            var allianceFactory = new AllianceDataFactory();

            var factory = new MessageOriginAllianceFactory(allianceFactory);
            var origin = factory.CreateOrigin(dataDic);

            Assert.IsInstanceOf<MessageOriginAlliance>(origin);
            var allianceOrigin = origin as MessageOriginAlliance;
            Assert.AreEqual(allianceOrigin.Alliance.Name, kName);
            Assert.AreEqual(allianceOrigin.Alliance.Id, kUid);
            Assert.AreEqual(allianceOrigin.Alliance.Score, kScore);
        }
    }
}
