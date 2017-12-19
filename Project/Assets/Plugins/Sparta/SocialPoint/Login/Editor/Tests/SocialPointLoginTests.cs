using NUnit.Framework;
using NSubstitute;

using SocialPoint.Network;
using SocialPoint.Attributes;
using SocialPoint.Hardware;
using SocialPoint.Base;
using SocialPoint.AppEvents;

namespace SocialPoint.Login
{
	[TestFixture]
	[Category("SocialPoint.Login")]
	public sealed class SocialPointLoginTests
	{

	    SocialPointLogin SocialPointLogin;
	    IHttpClient HttpClient;

	    [SetUp]
	    public void SetUp()
	    {
	        HttpClient = Substitute.For<IHttpClient>();
	        SocialPointLogin = new SocialPointLogin(HttpClient,
                                                    new SocialPointLogin.LoginConfig {
                                                        BaseUrl = "http://int-ds.socialpointgames.com/ds_tech/web/index_dev.php/api/v3/",
                                                        SecurityTokenErrors = SocialPointLogin.DefaultMaxSecurityTokenErrorRetries,
                                                        ConnectivityErrors = SocialPointLogin.DefaultMaxConnectivityErrorRetries,
                                                        EnableOnLinkConfirm = SocialPointLogin.DefaultEnableLinkConfirmRetries });
	        SocialPointLogin.Storage = Substitute.For<IAttrStorage>();
	        SocialPointLogin.DeviceInfo = Substitute.For<IDeviceInfo>();
	        SocialPointLogin.TrackEvent = Substitute.For<TrackEventDelegate>();
	    }
	    
	    [Test]
	    public void Login_calls_TrackEvent()
	    {
	        var TrackEvent = Substitute.For<TrackEventDelegate>();
	        SocialPointLogin.TrackEvent = TrackEvent;
	        SocialPointLogin.Login();
	        TrackEvent.ReceivedWithAnyArgs(1).Invoke(Arg.Any<string>(),Arg.Any<AttrDic>(),Arg.Any<ErrorDelegate>());
	    }

	    [Test]
	    public void Login_calls_HttpClient_Send()
	    {
	        SocialPointLogin.Login();
	        HttpClient.Received(1).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
	    }

		[Test]
		public void Login_sets_AppSource()
		{
			var appEvents = Substitute.For<IAppEvents>();
			appEvents.Source.Returns(new AppSource("lala://load-external-user?privilegedToken=AA123&userId=1&envurl=http%3A%2F%2Flololo.com"));
			SocialPointLogin.AppEvents = appEvents;
			SocialPointLogin.Login();

			Assert.That(SocialPointLogin.ImpersonatedUserId == 1);
			Assert.That(SocialPointLogin.PrivilegeToken == "AA123");
			Assert.That(SocialPointLogin.BaseUrl == "http://lololo.com/");
		}

	    /*
	     * Login.AddLink(
	     * login.Login(OnLogin);
	     */

	    [TearDown]
	    public void TearDown()
	    {
	        
	    }
	}
}
