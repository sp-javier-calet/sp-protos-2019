using NUnit.Framework;
using NSubstitute;

using UnityEngine;
using SocialPoint.GUIControl;
using SocialPoint.Utils;
using System;
using System.Collections;
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

            InstantiateGO(ref PopupGO, "popup", false);
            InstantiateGO(ref ScreenGO, "screen", true);
            InstantiateGO(ref ReplacePopupGO, "popup_replace", false);
            InstantiateGO(ref ReplaceScreenGO, "screen_replace", true);
        }
            
        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(GO);
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
                GameObject newGO = UnityEngine.Object.Instantiate(go);
                return newGO;
            }

            return null;
        }

        void Reset()
        {
            UITestStackController.Restart();
        }

        void BasicPush(GameObject gameObject, bool forceShowLast = false)
        {
            GameObject go = CloneGO(gameObject);

            UITestStackController.Push(go, forceShowLast);
            UIViewController top = UITestStackController.Top.Controller;

            Assert.IsNotNull(top);
            Assert.IsTrue(top.gameObject == go);
        }

        void BasicPushImmediate(GameObject gameObject)
        {
            GameObject go = CloneGO(gameObject);

            UITestStackController.PushImmediate(go);
            UIViewController top = UITestStackController.Top.Controller;

            Assert.IsNotNull(top);
            Assert.IsTrue(top.gameObject == go);
        }

        void BasicReplace(GameObject gameObject, GameObject goReplace, bool showOthers = false)
        {
            BasicPush(gameObject);

            GameObject go = CloneGO(goReplace);
            UITestStackController.Replace(go, showOthers);

            UIViewController top = UITestStackController.Top.Controller;

            Assert.IsNotNull(top);
            Assert.IsTrue(top.gameObject == go);
        }

        void BasicReplaceImmediate(GameObject gameObject, GameObject goReplace)
        {
            BasicPush(gameObject);

            GameObject go = CloneGO(goReplace);
            UITestStackController.ReplaceImmediate(go);

            UIViewController top = UITestStackController.Top.Controller;

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
            IList<StackNode> stack = new List<StackNode>();
            UITestStackController.CheckStackVisibility(stack);

            for(int i = 0; i < stack.Count; ++i)
            {
                var elm = stack[i];
                if(UITestStackController.IsValidStackNode(elm))
                {
                    if(elm.IsDesiredToShow)
                    {
                        Assert.IsTrue(elm.GameObject.activeSelf);
                    }
                    else
                    {
                        Assert.IsTrue(!elm.GameObject.activeSelf);
                    }
                }
            }
        }

        void ComplexPushesAndPops(bool addCheckPoint)
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

            UIViewController top = UITestStackController.Top.Controller;
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
            Assert.IsTrue(UITestStackController.Count == 1);
        }

        [Test]
        public void Replace_Popup_By_Popup_After_Push()
        {
            Reset();

            BasicReplace(PopupGO, ReplacePopupGO);
            Assert.IsTrue(UITestStackController.Count == 1);
        }

        [Test]
        public void Replace_Screen_By_Popup_After_Push()
        {
            Reset();

            BasicReplace(ScreenGO, ReplacePopupGO);
            Assert.IsTrue(UITestStackController.Count == 1);
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
            Assert.IsTrue(UITestStackController.Count == 1);
        }

        [Test]
        public void Replace_Immediate_Popup_By_Popup_After_Push()
        {
            Reset();

            BasicReplaceImmediate(PopupGO, ReplacePopupGO);
            Assert.IsTrue(UITestStackController.Count == 1);
        }

        [Test]
        public void Replace_Immediate_Screen_By_Popup_After_Push()
        {
            Reset();

            BasicReplaceImmediate(ScreenGO, ReplacePopupGO);
            Assert.IsTrue(UITestStackController.Count == 1);
        }

        [Test]
        public void Replace_Immediate_Screen_By_Screen_After_Push()
        {
            Reset();

            BasicReplaceImmediate(ScreenGO, ReplaceScreenGO);
            Assert.IsTrue(UITestStackController.Count == 1);
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
            ComplexPushesAndPops(false);

            UITestStackController.Clear();

            Assert.IsTrue(UITestStackController.Count == 0);
        }

        [Test]
        public void PopUntil_Zero()
        {
            Reset();
            ComplexPushesAndPops(false);

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
            ComplexPushesAndPops(true);

            UITestStackController.PopUntilCheckPoint(checkpoint);

            var top = UITestStackController.Top;

            Assert.IsNotNull(top);
            Assert.IsTrue(UITestStackController.Count == 5);
            CheckViewsVisibility();
        }
            
        [Test]
        public void Push_Popup_With_Force_Show_Popup()
        {
            Reset();
            bool showOthers = true;

            ComplexPushesAndPops(false);

            BasicPush(PopupGO, showOthers);

            Assert.IsTrue(UITestStackController.Count == 10);
            CheckViewsVisibility();
        }

        [Test]
        public void Replace_Popup_With_Force_Show_Popup()
        {
            Reset();
            bool showOthers = true;

            ComplexPushesAndPops(false);

            BasicReplace(PopupGO, ReplacePopupGO, showOthers);

            Assert.IsTrue(UITestStackController.Count == 10);
            CheckViewsVisibility();
        }

        [Test]
        public void Close_Top_View_Using_Button()
        {
            Reset();
            ComplexPushesAndPops(false);

            var top = UITestStackController.Top;
            top.Controller.Close();

            Assert.IsTrue(UITestStackController.Count == 8);
            CheckViewsVisibility();
        }
    }
}