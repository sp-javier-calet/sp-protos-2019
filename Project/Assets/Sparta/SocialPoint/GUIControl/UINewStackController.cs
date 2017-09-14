using System.Collections;
using UnityEngine;
using System;

namespace SocialPoint.GUIControl
{
    public class UINewStackController : UIStackController
    {
        public GameObject ViewsContainer;

        #region Helpers

        void ShowPopupsBetweenScreens(UIViewController top, UIViewController newTop, ActionType act)
        {
            switch(act)
            {
            case ActionType.Push:
                if(newTop.ViewType == ViewCtrlType.Screen)
                {
                    ShowStackedUIViews(false);
                }

                break;

            case ActionType.Pop:
                if(top.ViewType == ViewCtrlType.Screen)
                {
                    ShowStackedUIViews(true);
                }

                break;

            case ActionType.PopUntilPos:
            case ActionType.PopUntilType:
            case ActionType.PopUntilCheck:

                if(newTop.ViewType == ViewCtrlType.Popup)
                {
                    ShowStackedUIViews(true);
                }

                break;

            case ActionType.Replace:
                if(top == null || newTop == null)
                {
                    return;
                }

                if(top.ViewType == ViewCtrlType.Screen && newTop.ViewType == ViewCtrlType.Popup)
                {
                    ShowStackedUIViews(true);
                }
                else if(top.ViewType == ViewCtrlType.Popup && newTop.ViewType == ViewCtrlType.Screen)
                {
                    ShowStackedUIViews(false);
                }

                break;
            }
        }

        void ShowStackedUIViews(bool showPopups)
        {
            if(_stack.Count > 1)
            {
                for(int i = _stack.Count - 2; i >= 0; i--)
                {
                    var elm = _stack[i];

                    if(showPopups)
                    {
                        elm.ShowImmediate();
                    }
                    else
                    {
                        elm.HideImmediate();
                    }

                    if(elm.ViewType == ViewCtrlType.Screen)
                    {
                        break;
                    }
                }
            }
        }

        protected void SetupParent(UIViewController ctrl)
        {
            if(ViewsContainer != null && ctrl != null)
            {
                ctrl.SetParent(ViewsContainer.transform);
            }
        }

        protected override void SetupTransition(UIViewController from, UIViewController to, ActionType act)
        {
            if(IsPushAction(act))
            {
                SetupParent(to);

                if(!SetAnimation(from, to, ChildUpAnimation))
                {
                    SetAnimation(from, to, ChildAnimation);
                }
            }
            else if(IsPopAction(act))
            {
                if(!SetAnimation(from, to, ChildDownAnimation))
                {
                    SetAnimation(from, to, ChildAnimation);
                }
            }
            else
            {
                if(act == ActionType.Replace)
                {
                    SetAnimation(from, to, ChildAnimation);
                }
            }
        }

        #endregion

        #region Push
            
        protected override IEnumerator DoPushCoroutine(UIViewController ctrl, ActionType act)
        {
            var top = Top;
            AddChild(ctrl);
            _stack.Add(ctrl);

            DebugLog(string.Format("{0} on {1}", act, ctrl ? ctrl.gameObject.name : string.Empty));

            var enm = DoTransition(null, ctrl, act);
            while(enm.MoveNext())
            {
                yield return enm;
            }

            ShowPopupsBetweenScreens(top, ctrl, act);
        }

        public override UIViewController PushImmediate(UIViewController ctrl)
        {
            DebugLog(string.Format("PushImmediate {0}", ctrl.gameObject.name));
            var top = Top;
            AddChild(ctrl);
            _stack.Add(ctrl);

            SetupParent(ctrl);
            ctrl.ShowImmediate();

            ShowPopupsBetweenScreens(top, ctrl, ActionType.Push);

            return ctrl;
        }

        #endregion

        #region Replace

        public override UIViewController Replace(UIViewController ctrl, ActionType act = ActionType.Replace)
        {
            if(_stack.Count == 0)
            {
                return null;
            }

            var top = Top;
            DebugLog(string.Format("Replace {0} with {1}", Top.gameObject.name, ctrl.gameObject.name));

            ShowPopupsBetweenScreens(top, ctrl, act);

            AddChild(ctrl);
            _stack.Add(ctrl);

            SetupParent(ctrl);

            ctrl.Show();
            top.Hide(true);

            if(_stack.Count > 1)
            {
                _stack.RemoveAt(_stack.Count - 2);
            }
                
            return ctrl;
        }

        public override UIViewController ReplaceImmediate(UIViewController ctrl)
        {
            if(_stack.Count == 0)
            {
                return null;
            }

            var top = Top;
            DebugLog(string.Format("ReplaceImmediate {0} with {1}", top.gameObject.name, ctrl.gameObject.name));

            ShowPopupsBetweenScreens(top, ctrl, ActionType.Replace);

            AddChild(ctrl);
            _stack.Add(ctrl);

            SetupParent(ctrl);

            ctrl.ShowImmediate();
            top.HideImmediate(true);

            if(_stack.Count > 1)
            {
                _stack.RemoveAt(_stack.Count - 2);
            }

            return ctrl;
        }

        #endregion

        #region PopupUntil

        protected override IEnumerator DoPopUntilCheckPointCoroutine(string name)
        {
            int i;
            if(!_checkpoints.TryGetValue(name, out i))
            {
                throw new Exception(string.Format("Could not find checkpoint '{0}'.", name));
            }
            else
            {
                var enm = DoPopUntilCoroutine(i, ActionType.PopUntilCheck);
                while(enm.MoveNext())
                {
                    yield return enm.Current;
                }
            }
        }

        protected override IEnumerator DoPopUntilCondition(PopCondition cond, ActionType act)
        {
            UIViewController top = null;
            UIViewController ctrl = null;
            for(var i = _stack.Count - 1; i >= 0; i--)
            {
                var elm = _stack[i];
                if(top == null)
                {
                    top = elm;
                }
                if(cond(elm))
                {
                    ctrl = elm;
                    break;
                }
                else
                {
                    _stack.RemoveAt(i);
                    elm.HideImmediate(true);
                }
            }
            DebugLog(string.Format("{0} {1}", act, ctrl ? ctrl.gameObject.name : string.Empty));
            if(top != ctrl)
            {
                var enm = DoTransition(top, ctrl, act);
                while(enm.MoveNext())
                {
                    yield return enm.Current;
                }
            }
            ShowPopupsBetweenScreens(null, ctrl, ActionType.PopUntilType);
        }

        #endregion

        #region pop

        protected override IEnumerator DoPopCoroutine()
        {
            var top = Top;
            var ctrl = Second;
            DebugLog(string.Format("Pop {0}", top ? top.gameObject.name : string.Empty));

            ShowPopupsBetweenScreens(top, ctrl, ActionType.Pop);
            top.Hide(true);

            yield return null;
        }

        public override void PopImmediate()
        {
            if(_stack.Count == 0)
            {
                return;
            }

            var top = Top;
            var ctrl = Second;

            DebugLog(string.Format("PopImmediate {0}", top ? top.gameObject.name : string.Empty));

            _stack.RemoveAt(_stack.Count - 1);
            top.HideImmediate(true);

            ctrl.ShowImmediate();

            ShowPopupsBetweenScreens(top, ctrl,  ActionType.Pop);
        }

        #endregion
    }
}
