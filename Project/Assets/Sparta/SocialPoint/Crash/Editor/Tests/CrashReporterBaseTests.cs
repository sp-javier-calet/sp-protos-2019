using NSubstitute;
using NUnit.Framework;
using SocialPoint.Attributes;
using SocialPoint.Hardware;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Utils;
using UnityEngine;


namespace SocialPoint.Crash
{

    [TestFixture]
    [Category("SocialPoint.Crash")]
    /// <summary>
    /// Crash reporter base tests.
    /// 
    /// </summary>
    class CrashReporterBaseTests
    {
        
        BaseCrashReporter CrashReporterBase;
        IHttpClient HttpClient;
        GameObject GO;

        [SetUp]
        public void SetUp()
        {
            GO = new GameObject();
            var runner = GO.AddComponent<UnityUpdateRunner>();
            PathsManager.Init();

            HttpClient = Substitute.For<IHttpClient>();
            var DeviceInfo = Substitute.For<UnityDeviceInfo>();
            CrashReporterBase = new BaseCrashReporter(runner, HttpClient, DeviceInfo);
            CrashReporterBase.LoginData = Substitute.For<Login.ILoginData>();
        }

        [Test]
        public void Enable()
        {
            CrashReporterBase.Enable();
            Assert.Pass();
        }

        [Test]
        public void Disable()
        {
            CrashReporterBase.Disable();
            Assert.Pass();
        }

        [Test]
        public void TrackEvent_Called_If_Exception()
        {
            CrashReporterBase.Enable();
            CrashReporterBase.TrackEvent = Substitute.For<BaseCrashReporter.TrackEventDelegate>();
            Debug.LogException(new System.Exception("test exception"));
            CrashReporterBase.TrackEvent.ReceivedWithAnyArgs(1);
        }

        [Test]
        public void SendExceptions()
        {
            const string uuid = "testException";
            var _exceptionStorage = new FileAttrStorage(string.Format("{0}/{1}", PathsManager.AppPersistentDataPath, "logs/exceptions"));
            _exceptionStorage.Save(uuid, new AttrDic());
            CrashReporterBase.Enable();
            CrashReporterBase.Update();
            HttpClient.Received().Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
            _exceptionStorage.Remove(uuid);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(GO);
        }

    }
}
