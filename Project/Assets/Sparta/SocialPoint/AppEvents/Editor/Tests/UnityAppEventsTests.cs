using NUnit.Framework;
using System;
using System.Reflection;
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
    public sealed class UnityAppEventsTests
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
            SendMessage(UnityAppEvent, "OnApplicationFocus", true);
            action.ReceivedWithAnyArgs(1).Invoke();
        }

        [Test]
        public void OnApplicationFocus_False_WasCovered_NotRaised()
        {
            UnityAppEvent.WasCovered += action;
            SendMessage(UnityAppEvent, "OnApplicationFocus", false);
            action.DidNotReceiveWithAnyArgs().Invoke();
        }

        [Test]
        public void OnApplicationPause_True_WillGoBackground_Raised()
        {
            UnityAppEvent.WillGoBackground.Add(0, action);
            SendMessage(UnityAppEvent, "OnApplicationPause", true);
            action.ReceivedWithAnyArgs(1).Invoke();
        }

        [Test]
        public void OnApplicationPause_False_WasOnBackground_Raised()
        {
            UnityAppEvent.WasOnBackground.Add(0, action);
            SendMessage(UnityAppEvent, "OnApplicationPause", false);
            action.ReceivedWithAnyArgs(1).Invoke();
        }

        [Test]
        public void OnApplicationQuit_ApplicationQuit_Raised()
        {
            UnityAppEvent.ApplicationQuit += action;
            SendMessage(UnityAppEvent, "OnApplicationQuit");
            action.ReceivedWithAnyArgs(1).Invoke();
        }

        void SendMessage(object obj, string method)
        {
            var type = typeof(BaseAppEvents);
            var mi = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            mi.Invoke(UnityAppEvent, new object[]{ });
        }

        void SendMessage(object obj, string method, bool param)
        {
            var type = obj.GetType();
            var mi = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            mi.Invoke(UnityAppEvent, new object[]{ param });
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(UnityAppEvent.gameObject);
            UnityEngine.Object.DestroyImmediate(GO);
        }

    }

}
