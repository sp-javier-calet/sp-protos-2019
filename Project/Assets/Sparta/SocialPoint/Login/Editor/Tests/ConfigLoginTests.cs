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
        const string kConfigOKResponse = "{\"game\":{},\"globals\":{\"globals\":[{}]},\"map\":{},\"store\":{},\"payment_products\":{},\"bundle_data\":{}}";
        const string kConfigKOResponse = "{\"error\":{\"error\":0,\"message\":\"Product dows not exist\"}}";
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
                resp.OriginalBody = Encoding.UTF8.GetBytes(kConfigOKResponse);
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

        [Test]
        public void Login_error_HttpClient_Send()
        {
            HttpClient.When(x => x.Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>()))
                .Do(x => {
                    HttpResponse resp = new HttpResponse();
                    resp.OriginalBody = Encoding.UTF8.GetBytes(kConfigKOResponse);
                    resp.Error = new Error(400,"HTTP Server responded with error code");
                    x.Arg<HttpResponseDelegate>().Invoke(resp);
                });

            bool error = false;
            ConfigLogin.ErrorEvent += (ErrorType type, Error err, Attr data) => 
            {
                error = true;
            };

            ConfigLogin.Login();

            Assert.IsTrue(error);
        }

        [TearDown]
        public void TearDown()
        {
	        
        }
    }
}
