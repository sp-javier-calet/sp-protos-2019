using NUnit.Framework;
using NSubstitute;

using SocialPoint.GUIControl;
using UnityEngine;
using SocialPoint.Utils;
using System.Collections.Generic;

namespace SocialPoint.GUIControl
{
    [TestFixture]
    [Category("SocialPoint.GUIControl")]
    public sealed class UIStackTests
    {
        const string checkpoint = "checkpoint";

        GameObject GO;
        GameObject PopupGO;
        GameObject ScreenGO;
        GameObject ReplacePopupGO;
        GameObject ReplaceScreenGO;
        UIStackController UITestStackController;
        ImmediateCoroutineRunner CoroutineRunner;

        [SetUp]
        public void SetUp()
        {
            GO = new GameObject("UIStackController");
            UITestStackController = GO.AddComponent<UIStackController>();
            CoroutineRunner = new ImmediateCoroutineRunner();
            UITestStackController.CoroutineRunner = CoroutineRunner;
            UITestStackController.HideBetweenFullScreenViews = true;

            InstantiateGO(ref PopupGO, "popup", false);
            InstantiateGO(ref ScreenGO, "screen", true);
            InstantiateGO(ref ReplacePopupGO, "popup_replace", false);
            InstantiateGO(ref ReplaceScreenGO, "screen_replace", true);

            Reset();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(GO);
        }

        void InstantiateGO(ref GameObject go, string name, bool isFullScreen)
        {
            if(go == null)
            {
                go = new GameObject(name);
                UIViewController vController = go.AddComponent<UIViewController>();
                vController.IsFullScreen = isFullScreen;
            }
        }

        GameObject CloneGO(GameObject go)
        {
            if(go != null)
            {
                GameObject newGO = Object.Instantiate(go);
                return newGO;
            }

            return null;
        }

        void Reset()
        {
            UITestStackController.Restart();
        }

        void BasicPush(GameObject gameObject)
        {
            GameObject go = CloneGO(gameObject);

            UITestStackController.Push(go);
            UIViewController top = UITestStackController.Top;

            Assert.IsNotNull(top);
            Assert.IsTrue(top.gameObject == go);
        }

        void BasicPushImmediate(GameObject gameObject)
        {
            GameObject go = CloneGO(gameObject);

            UITestStackController.PushImmediate(go);
            UIViewController top = UITestStackController.Top;

            Assert.IsNotNull(top);
            Assert.IsTrue(top.gameObject == go);
        }

        void BasicReplace(GameObject gameObject, GameObject goReplace)
        {
            BasicPush(gameObject);

            GameObject go = CloneGO(goReplace);
            UITestStackController.Replace(go);

            UIViewController top = UITestStackController.Top;

            Assert.IsNotNull(top);
            Assert.IsTrue(top.gameObject == go);
        }

        void BasicReplaceImmediate(GameObject gameObject, GameObject goReplace)
        {
            BasicPush(gameObject);

            GameObject go = CloneGO(goReplace);
            UITestStackController.ReplaceImmediate(go);

            UIViewController top = UITestStackController.Top;

            Assert.IsNotNull(top);
            Assert.IsTrue(top.gameObject == go);
        }

        void BasicSetCheckpoint()
        {
            UITestStackController.SetCheckPoint(checkpoint);

            Assert.IsTrue(UITestStackController.CheckPointExists(checkpoint));
        }

        void CheckViewsVisibility()
        {
            IList<UIViewController> stack = UITestStackController.Stack;
            bool screenFound = false;

            if(stack.Count > 0)
            {
                for(int i = stack.Count - 1; i >= 0; i--)
                {
                    var elm = stack[i];
                    if(elm != null)
                    {
                        if(!screenFound && elm.IsFullScreen)
                        {
                            screenFound = true;
                            Assert.IsTrue(elm.gameObject.activeSelf);
                        }
                        else
                        {
                            if(screenFound)
                            {
                                Assert.IsTrue(!elm.gameObject.activeSelf);
                            }
                            else
                            {
                                Assert.IsTrue(elm.gameObject.activeSelf);
                            }
                        }
                    }
                }
            }
        }

        void ComplexPushPops(bool addCheckPoint)
        {
            BasicPush(ScreenGO);
            Assert.IsTrue(UITestStackController.Count == 1);
            CheckViewsVisibility();

            BasicPush(PopupGO);
            Assert.IsTrue(UITestStackController.Count == 2);
            CheckViewsVisibility();

            BasicPush(PopupGO);
            Assert.IsTrue(UITestStackController.Count == 3);
            CheckViewsVisibility();

            BasicPush(ReplacePopupGO);
            Assert.IsTrue(UITestStackController.Count == 4);
            CheckViewsVisibility();

            BasicPush(ScreenGO);
            Assert.IsTrue(UITestStackController.Count == 5);
            CheckViewsVisibility();

            if(addCheckPoint)
            {
                BasicSetCheckpoint();
            }

            UITestStackController.Pop();

            UIViewController top = UITestStackController.Top;
            Assert.IsNotNull(top);
            Assert.IsTrue(UITestStackController.Count == 4);
            CheckViewsVisibility();

            BasicPush(ReplaceScreenGO);
            Assert.IsTrue(UITestStackController.Count == 5);
            CheckViewsVisibility();

            BasicPush(PopupGO);
            Assert.IsTrue(UITestStackController.Count == 6);
            CheckViewsVisibility();

            BasicPush(PopupGO);
            Assert.IsTrue(UITestStackController.Count == 7);
            CheckViewsVisibility();

            BasicPush(ScreenGO);
            Assert.IsTrue(UITestStackController.Count == 8);
            CheckViewsVisibility();

            BasicPush(PopupGO);
            Assert.IsTrue(UITestStackController.Count == 9);
            CheckViewsVisibility();

        }

        [Test]
        public void Push_Screen()
        {
            Reset();
            BasicPush(ScreenGO);
        }

        [Test]
        public void Push_Immediate_Screen()
        {
            Reset();
            BasicPushImmediate(ScreenGO);
        }

        [Test]
        public void Push_Popup()
        {
            Reset();
            BasicPush(PopupGO);
        }

        [Test]
        public void Push_Immediate_Popup()
        {
            Reset();
            BasicPushImmediate(PopupGO);
        }

        [Test]
        public void Pop_After_Push()
        {
            Reset();
            BasicPush(PopupGO);
            UITestStackController.Pop();

            Assert.IsTrue(UITestStackController.Count == 0);
        }

        [Test]
        public void Pop_Immediate_After_Push()
        {
            Reset();
            BasicPush(PopupGO);
            UITestStackController.PopImmediate();

            Assert.IsTrue(UITestStackController.Count == 0);
        }

        [Test]
        public void Replace_Popup_By_Screen_After_Push()
        {
            Reset();

            BasicReplace(PopupGO, ReplaceScreenGO);
        }

        [Test]
        public void Replace_Popup_By_Popup_After_Push()
        {
            Reset();

            BasicReplace(PopupGO, ReplacePopupGO);
        }

        [Test]
        public void Replace_Screen_By_Popup_After_Push()
        {
            Reset();

            BasicReplace(ScreenGO, ReplacePopupGO);
        }

        [Test]
        public void Replace_Screen_By_Screen_After_Push()
        {
            Reset();

            BasicReplace(ScreenGO, ReplaceScreenGO);
        }

        [Test]
        public void Replace_Immediate_Popup_By_Screen_After_Push()
        {
            Reset();

            BasicReplaceImmediate(PopupGO, ReplaceScreenGO);
        }

        [Test]
        public void Replace_Immediate_Popup_By_Popup_After_Push()
        {
            Reset();

            BasicReplaceImmediate(PopupGO, ReplacePopupGO);
        }

        [Test]
        public void Replace_Immediate_Screen_By_Popup_After_Push()
        {
            Reset();

            BasicReplaceImmediate(ScreenGO, ReplacePopupGO);
        }

        [Test]
        public void Replace_Immediate_Screen_By_Screen_After_Push()
        {
            Reset();

            BasicReplaceImmediate(ScreenGO, ReplaceScreenGO);
        }

        [Test]
        public void SetCheckPoint()
        {
            Reset();
            BasicPush(ScreenGO);
            BasicSetCheckpoint();
        }

        [Test]
        public void PopUntil_None()
        {
            Reset();
            ComplexPushPops(false);

            UITestStackController.Clear();

            Assert.IsTrue(UITestStackController.Count == 0);
        }

        [Test]
        public void PopUntil_Zero()
        {
            Reset();
            ComplexPushPops(false);

            UITestStackController.PopUntil(0);

            var top = UITestStackController.Top;

            Assert.IsNotNull(top);
            Assert.IsTrue(UITestStackController.Count == 1);
            CheckViewsVisibility();
        }

        [Test]
        public void PopupUntil_CheckPoint()
        {
            Reset();
            ComplexPushPops(true);

            UITestStackController.PopUntilCheckPoint(checkpoint);

            var top = UITestStackController.Top;

            Assert.IsNotNull(top);
//            Assert.IsTrue(UITestStackController.Count == 5);
            CheckViewsVisibility();
        }
    }
}