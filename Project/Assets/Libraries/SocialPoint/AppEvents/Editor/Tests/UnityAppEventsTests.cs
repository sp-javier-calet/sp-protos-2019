using NUnit.Framework;
using System;
using NSubstitute;
using UnityEngine;

namespace SocialPoint.AppEvents
{
    /// <summary>
    /// Unity app events tests.
    /// IAppEvents has 5 event to dispatch when certain circunstances happen
    ///  willGoBackground
    ///  WasOnBackground
    ///  WasCovered
    ///  ApplicationQuit
    ///  ReceivedMemoryWarning
    /// </summary>
    [TestFixture]
    [Category("SocialPoint.AppEvents")]
    public class UnityAppEventsTests
    {
        GameObject GO;
        UnityAppEvents UnityAppEvent;
        Action action;

        [SetUp]
        public void SetUp()
        {
            GO = new GameObject();
            UnityAppEvent = GO.AddComponent<UnityAppEvents>();
            action = Substitute.For<Action>();
        }

        [Test]
        public void OnApplicationFocus_True_WasCovered_Raised()
        {
            UnityAppEvent.WasCovered += action;
            UnityAppEvent.SendMessage("OnApplicationFocus", true);
            action.ReceivedWithAnyArgs(1).Invoke();

        }

        [Test]
        public void OnApplicationFocus_False_WasCovered_NotRaised()
        {
            UnityAppEvent.WasCovered += action;
            UnityAppEvent.SendMessage("OnApplicationFocus", false);
            action.DidNotReceiveWithAnyArgs().Invoke();
        }

        [Test]
        public void OnApplicationPause_True_WillGoBackground_Raised()
        {
            UnityAppEvent.WillGoBackground += action;
            UnityAppEvent.SendMessage("OnApplicationPause", true);
            action.ReceivedWithAnyArgs(1).Invoke();
        }

        [Test]
        public void OnApplicationPause_False_WasOnBackground_Raised()
        {
            UnityAppEvent.WasOnBackground += action;
            UnityAppEvent.SendMessage("OnApplicationPause", false);
            action.ReceivedWithAnyArgs(1).Invoke();
        }

        [Test]
        public void OnApplicationQuit_ApplicationQuit_Raised()
        {
            UnityAppEvent.ApplicationQuit += action;
            UnityAppEvent.SendMessage("OnApplicationQuit");
            action.ReceivedWithAnyArgs(1).Invoke();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(UnityAppEvent.gameObject);
            UnityEngine.Object.DestroyImmediate(GO);
        }

    }

}
