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

        static IEnumerable StackType()
        {
            return Enum.GetValues(typeof(UIStackController.StackShowType));
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

        void Reset(UIStackController.StackShowType stackType)
        {
            UITestStackController.StackType = stackType;
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
            Assert.IsTrue(UITestStackController.Count == 1);
        }

        void BasicReplaceImmediate(GameObject gameObject, GameObject goReplace)
        {
            BasicPush(gameObject);

            GameObject go = CloneGO(goReplace);
            UITestStackController.ReplaceImmediate(go);

            UIViewController top = UITestStackController.Top;

            Assert.IsNotNull(top);
            Assert.IsTrue(top.gameObject == go);
            Assert.IsTrue(UITestStackController.Count == 1);
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
                        if(UITestStackController.StackType == UIStackController.StackShowType.ShowAndHidePreviousUntilScreen)
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
                    else if(UITestStackController.StackType == UIStackController.StackShowType.ShowAndHidePrevious)
                    {
                        if(elm == UITestStackController.Top)
                        {
                            Assert.IsTrue(elm.gameObject.activeSelf);
                        }
                        else
                        {
                            Assert.IsTrue(!elm.gameObject.activeSelf);
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
        public void Push_Screen([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);
            BasicPush(ScreenGO);
        }

        [Test]
        public void Push_Immediate_Screen([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);
            BasicPushImmediate(ScreenGO);
        }

        [Test]
        public void Push_Popup([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);
            BasicPush(PopupGO);
        }

        [Test]
        public void Push_Immediate_Popup([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);
            BasicPushImmediate(PopupGO);
        }

        [Test]
        public void Pop_After_Push([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);
            BasicPush(PopupGO);
            UITestStackController.Pop();

            Assert.IsTrue(UITestStackController.Count == 0);
        }

        [Test]
        public void Pop_Immediate_After_Push([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);
            BasicPush(PopupGO);
            UITestStackController.PopImmediate();

            Assert.IsTrue(UITestStackController.Count == 0);
        }

        [Test]
        public void Replace_Popup_By_Screen_After_Push([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);

            BasicReplace(PopupGO, ReplaceScreenGO);
        }

        [Test]
        public void Replace_Popup_By_Popup_After_Push([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);

            BasicReplace(PopupGO, ReplacePopupGO);
        }

        [Test]
        public void Replace_Screen_By_Popup_After_Push([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);

            BasicReplace(ScreenGO, ReplacePopupGO);
        }

        [Test]
        public void Replace_Screen_By_Screen_After_Push([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);

            BasicReplace(ScreenGO, ReplaceScreenGO);
        }

        [Test]
        public void Replace_Immediate_Popup_By_Screen_After_Push([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);

            BasicReplaceImmediate(PopupGO, ReplaceScreenGO);
        }

        [Test]
        public void Replace_Immediate_Popup_By_Popup_After_Push([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);

            BasicReplaceImmediate(PopupGO, ReplacePopupGO);
        }

        [Test]
        public void Replace_Immediate_Screen_By_Popup_After_Push([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);

            BasicReplaceImmediate(ScreenGO, ReplacePopupGO);
        }

        [Test]
        public void Replace_Immediate_Screen_By_Screen_After_Push([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);

            BasicReplaceImmediate(ScreenGO, ReplaceScreenGO);
        }

        [Test]
        public void SetCheckPoint([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);
            BasicPush(ScreenGO);
            BasicSetCheckpoint();
        }

        [Test]
        public void PopUntil_None([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);
            ComplexPushPops(false);

            UITestStackController.Clear();

            Assert.IsTrue(UITestStackController.Count == 0);
        }

        [Test]
        public void PopUntil_Zero([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);
            ComplexPushPops(false);

            UITestStackController.PopUntil(0);

            var top = UITestStackController.Top;

            Assert.IsNotNull(top);
            Assert.IsTrue(UITestStackController.Count == 1);
            CheckViewsVisibility();
        }

        [Test]
        public void PopupUntil_CheckPoint([ValueSource("StackType")] UIStackController.StackShowType stackType)
        {
            Reset(stackType);
            ComplexPushPops(true);

            UITestStackController.PopUntilCheckPoint(checkpoint);

            var top = UITestStackController.Top;

            Assert.IsNotNull(top);
            Assert.IsTrue(UITestStackController.Count == 5);
            CheckViewsVisibility();
        }
    }
}