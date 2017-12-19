using NUnit.Framework;
using NSubstitute;
using SocialPoint.Attributes;
using SocialPoint.Utils;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    [TestFixture]
    [Category("SocialPoint.Network")]
    public sealed class HttpRequestTests
    {
        [Test]
        public void HttpRequest_parses_params()
        {
            var req = new HttpRequest("http://www.google.com/?test=value");
            //Assert.AreEqual(new AttrString("value"), req.Params["test"]);

            req.Url = new System.Uri("http://www.google.com/?test[0]=value&test[1]=value2");
            Assert.AreEqual(new AttrList(new List<string>{"value", "value2" }), req.Params["test"]);

            req.Method = HttpRequest.MethodType.POST;
            req.Body = System.Text.Encoding.UTF8.GetBytes("aa=bb&cc[dd]=ee");
            req.Headers[HttpRequest.ContentTypeHeader] = HttpRequest.ContentTypeUrlencoded;
            Assert.AreEqual("ee", req.Params["cc"].AsDic["dd"].ToString());
        }

        [Test]
        public void HttpRequest_AddParam()
        {
            var req = new HttpRequest("http://www.google.com/", HttpRequest.MethodType.POST);
            req.AddParam("test", "aa");
            Assert.AreEqual("test=aa", System.Text.Encoding.UTF8.GetString (req.Body));
        }

    }
}
