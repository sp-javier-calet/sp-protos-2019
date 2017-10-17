using NUnit.Framework;
using NSubstitute;

using SocialPoint.Network;
using SocialPoint.Utils;
using SocialPoint.Base;
using System;
using SocialPoint.Attributes;
using System.Text;

namespace SocialPoint.Login
{
    [TestFixture]
    [Category("SocialPoint.Login")]
    public sealed class ConfigLoginTests
    {
        ConfigLogin ConfigLogin;
        IHttpClient HttpClient;

        const string kConfigUrl = "http://backend.pro.configmanager.sp.laicosp.net/products/mt/envs/env1/download";
        const string kConfigResponse = "{\"game\":{},\"globals\":{\"globals\":[{}]},\"map\":{},\"store\":{},\"payment_products\":{},\"bundle_data\":{}}";
        const string kConfig = "config";

        [SetUp]
        public void SetUp()
        {
            HttpClient = Substitute.For<IHttpClient>();
            ConfigLogin = new ConfigLogin(HttpClient, kConfigUrl);
        }


        [Test]
        public void Login_calls_HttpClient_Send()
        {
            HttpClient.When(x => x.Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>()))
                .Do(x => {
                HttpResponse resp = new HttpResponse();
                resp.OriginalBody = Encoding.UTF8.GetBytes(kConfigResponse);
                resp.StatusCode = 200;
                x.Arg<HttpResponseDelegate>().Invoke(resp);
            });

            bool isParsedOk = false;
            ConfigLogin.NewUserStreamEvent += (reader) => {

                var data = reader.ParseElement();
                if(data.AsDic.ContainsKey(kConfig))
                {
                    isParsedOk = true;
                }
                return true;
            };

            ConfigLogin.Login();

            Assert.IsTrue(isParsedOk);
        }

        [TearDown]
        public void TearDown()
        {
	        
        }
    }
}
