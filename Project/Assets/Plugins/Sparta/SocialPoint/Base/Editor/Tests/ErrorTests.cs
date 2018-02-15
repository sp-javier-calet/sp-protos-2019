using NUnit.Framework;

namespace SocialPoint.Base
{
    [TestFixture]
    [Category("SocialPoint.Base")]
    public class ErrorTests
    {
        [Test]
        public void ErrorFromString()
        {
            var error = new Error(0, "msg");
            var errorFromString = Error.FromString(error.ToString());
            Assert.AreEqual(error.Code, errorFromString.Code);
            Assert.AreEqual(error.Msg, errorFromString.Msg);
        }

        [Test]
        public void ErrorWithDetailFromString()
        {
            var error0 = new Error(0, "msg", "detail");
            var error1 = new Error(1, "msg");
            var error0FromString = Error.FromString(error0.ToString());
            var error1FromString = Error.FromString(error1.ToString());
            Assert.That((error0.Code == error0FromString.Code) &&
            (error0.Msg == error0FromString.Msg) &&
            (error0.Detail == error0FromString.Detail));
            Assert.That((error1.Code == error1FromString.Code) &&
            (error1.Msg == error1FromString.Msg));
            Assert.That(error0FromString.Code != error1FromString.Code);
            Assert.That(error1FromString.Detail == string.Empty);
        }
    }
}
