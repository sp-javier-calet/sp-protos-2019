using NUnit.Framework;
using NSubstitute;

using SocialPoint.Network;
using SocialPoint.Utils;
using SocialPoint.Base;
using System;
using SocialPoint.Attributes;

namespace SocialPoint.Login
{
	[TestFixture]
	[Category("SocialPoint.Login")]
    public sealed class ConfigLoginTests
	{
        ConfigLogin ConfigLogin;
	    IHttpClient HttpClient;

        const string kConfigUrl = "http://backend.pro.configmanager.sp.laicosp.net/products/mt/envs/env1/download";
        const string kConfigResponse = "{\"game\":{}, \"globals\":{\"globals\":{}}}";
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
                    // Set hardcoded body
                    HttpResponse resp = new HttpResponse();
                    resp.OriginalBody =  StringUtils.GetBytes(kConfigResponse);
                    resp.StatusCode = 200;
                    x.Arg<HttpResponseDelegate>().Invoke(resp);
                });

            bool eventIsCalled = false;
            ConfigLogin.NewUserStreamEvent += (reader) => {
                eventIsCalled = true;

                // Check game_data

                // Check game_data config

                return true;
            };


            ConfigLogin.Login();

            Assert.IsTrue(eventIsCalled);

	    }

	    [TearDown]
	    public void TearDown()
	    {
	        
	    }
	}
}
