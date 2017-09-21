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
            return Enum.GetValues(typeof(UIStackController.StackVisibility));
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

        void Reset(UIStackController.StackVisibility stackType)
        {
            UITestStackController.StackVisibilityMode = stackType;
            UITestStackController.Restart();
        }

        void BasicPush(GameObject gameObject, bool forceShowLast = false)
        {
            GameObject go = CloneGO(gameObject);

            UITestStackController.Push(go, forceShowLast);
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

        void BasicReplace(GameObject gameObject, GameObject goReplace, bool forceShowLast = false)
        {
            BasicPush(gameObject);

            GameObject go = CloneGO(goReplace);
            UITestStackController.Replace(go, forceShowLast);

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

        void CheckViewsVisibility(bool forceShowLast = false)
        {
            IList<UIViewController> stack = UITestStackController.Stack;
            bool screenFound = false;
            bool popupFound = false;

            if(stack.Count > 0)
            {
                for(int i = stack.Count - 1; i >= 0; i--)
                {
                    var elm = stack[i];
                    if(elm != null)
                    {
                        if(forceShowLast)
                        {
                            if(i == stack.Count - 1)
                            {
                                Assert.IsTrue(elm.gameObject.activeSelf);
                            }
                            else
                            {
                                Assert.IsTrue(!elm.gameObject.activeSelf);
                            }
                        }
                        else if(UITestStackController.StackVisibilityMode == UIStackController.StackVisibility.ShowAllBetweenScreens)
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
                        else if(UITestStackController.StackVisibilityMode == UIStackController.StackVisibility.ShowLast)
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
                        else if(UITestStackController.StackVisibilityMode == UIStackController.StackVisibility.ShowLastBetweenScreens)
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
                                    if(!popupFound && !elm.IsFullScreen)
                                    {
                                        popupFound = true;
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
        public void Push_Screen([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);
            BasicPush(ScreenGO);
        }

        [Test]
        public void Push_Immediate_Screen([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);
            BasicPushImmediate(ScreenGO);
        }

        [Test]
        public void Push_Popup([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);
            BasicPush(PopupGO);
        }

        [Test]
        public void Push_Immediate_Popup([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);
            BasicPushImmediate(PopupGO);
        }

        [Test]
        public void Pop_After_Push([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);
            BasicPush(PopupGO);
            UITestStackController.Pop();

            Assert.IsTrue(UITestStackController.Count == 0);
        }

        [Test]
        public void Pop_Immediate_After_Push([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);
            BasicPush(PopupGO);
            UITestStackController.PopImmediate();

            Assert.IsTrue(UITestStackController.Count == 0);
        }

        [Test]
        public void Replace_Popup_By_Screen_After_Push([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);

            BasicReplace(PopupGO, ReplaceScreenGO);
            Assert.IsTrue(UITestStackController.Count == 1);
        }

        [Test]
        public void Replace_Popup_By_Popup_After_Push([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);

            BasicReplace(PopupGO, ReplacePopupGO);
            Assert.IsTrue(UITestStackController.Count == 1);
        }

        [Test]
        public void Replace_Screen_By_Popup_After_Push([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);

            BasicReplace(ScreenGO, ReplacePopupGO);
            Assert.IsTrue(UITestStackController.Count == 1);
        }

        [Test]
        public void Replace_Screen_By_Screen_After_Push([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);

            BasicReplace(ScreenGO, ReplaceScreenGO);
        }

        [Test]
        public void Replace_Immediate_Popup_By_Screen_After_Push([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);

            BasicReplaceImmediate(PopupGO, ReplaceScreenGO);
            Assert.IsTrue(UITestStackController.Count == 1);
        }

        [Test]
        public void Replace_Immediate_Popup_By_Popup_After_Push([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);

            BasicReplaceImmediate(PopupGO, ReplacePopupGO);
            Assert.IsTrue(UITestStackController.Count == 1);
        }

        [Test]
        public void Replace_Immediate_Screen_By_Popup_After_Push([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);

            BasicReplaceImmediate(ScreenGO, ReplacePopupGO);
            Assert.IsTrue(UITestStackController.Count == 1);
        }

        [Test]
        public void Replace_Immediate_Screen_By_Screen_After_Push([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);

            BasicReplaceImmediate(ScreenGO, ReplaceScreenGO);
            Assert.IsTrue(UITestStackController.Count == 1);
        }

        [Test]
        public void SetCheckPoint([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);
            BasicPush(ScreenGO);
            BasicSetCheckpoint();
        }

        [Test]
        public void PopUntil_None([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);
            ComplexPushesAndPops(false);

            UITestStackController.Clear();

            Assert.IsTrue(UITestStackController.Count == 0);
        }

        [Test]
        public void PopUntil_Zero([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);
            ComplexPushesAndPops(false);

            UITestStackController.PopUntil(0);

            var top = UITestStackController.Top;

            Assert.IsNotNull(top);
            Assert.IsTrue(UITestStackController.Count == 1);
            CheckViewsVisibility();
        }

        [Test]
        public void PopupUntil_CheckPoint([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);
            ComplexPushesAndPops(true);

            UITestStackController.PopUntilCheckPoint(checkpoint);

            var top = UITestStackController.Top;

            Assert.IsNotNull(top);
            Assert.IsTrue(UITestStackController.Count == 5);
            CheckViewsVisibility();
        }
            
        [Test]
        public void Push_Popup_Force_Hide_All_Behind([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);
            bool forceShowLast = true;

            ComplexPushesAndPops(false);

            BasicPush(PopupGO, forceShowLast);

            Assert.IsTrue(UITestStackController.Count == 10);
            CheckViewsVisibility(forceShowLast);
        }

        [Test]
        public void Replace_Popup_By_Popup_Force_Hide_All_Behind([ValueSource("StackType")] UIStackController.StackVisibility stackType)
        {
            Reset(stackType);
            bool forceShowLast = true;

            ComplexPushesAndPops(false);

            BasicReplace(PopupGO, ReplacePopupGO, forceShowLast);

            Assert.IsTrue(UITestStackController.Count == 10);
            CheckViewsVisibility(forceShowLast);
        }
    }
}