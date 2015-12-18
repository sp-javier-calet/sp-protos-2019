using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public delegate void UIStackControllerDelegate();

    public class UIStackController : UIParentController
    {
        public GameObject Background;
        public GameObject FrontContainer;
        public GameObject BackContainer;
        public GameObject Blocker;
        public bool SimultaneousAnimations = true;
        public UIViewAnimation ChildUpAnimation;
        public UIViewAnimation ChildDownAnimation;
        public UIViewAnimation ChildAnimation;

        public enum ActionType
        {
            None,
            Push,
            Replace,
            Pop,
            PopUntilType,
            PopUntilPos,
            PopUntilCheck
        }

        protected enum PopActionType
        {
            None,

        };

        public int Count
        {
            get
            {
                return _stack.Count;
            }
        }

        public UIViewController Top
        {
            get
            {
                if(_stack.Count > 0)
                {
                    return _stack[_stack.Count - 1];
                }
                return null;
            }
        }

        IList<UIViewController> _stack = new List<UIViewController>();
        IDictionary<string,int> _checkpoints = new Dictionary<string,int>();
        Coroutine _actionCoroutine = null;
        ActionType _action = ActionType.None;

        Coroutine StartActionCoroutine(IEnumerator enm, ActionType act)
        {
            if(IsPopAction(_action))
            {
                if(Top != null)
                {
                    Top.HideImmediate(true);
                }
                else
                {
                    Hide();
                }
            }
            if(_actionCoroutine != null)
            {
                DebugLog("StopCoroutine");
                StopCoroutine(_actionCoroutine);
                _actionCoroutine = null;
            }
            _action = act;
            if(gameObject.activeInHierarchy)
            {
                _actionCoroutine = StartCoroutine(DoActionCoroutine(enm));
                return _actionCoroutine;
            }
            return null;
        }

        IEnumerator DoActionCoroutine(IEnumerator enm)
        {
            DebugLog("StartProcess");
            if(Blocker != null)
            {
                Blocker.SetActive(true);
            }
            if(enm != null)
            {
                while(enm.MoveNext())
                {
                    yield return enm.Current;
                }
            }
            if(Blocker != null)
            {
                Blocker.SetActive(false);
            }
            _actionCoroutine = null;
            _action = ActionType.None;
            DebugLog("EndProcess");
        }

        bool SetAnimation(UIViewController from, UIViewController to, UIViewAnimation anim)
        {
            if(anim != null)
            {
                if(to != null)
                {
                    to.Animation = (UIViewAnimation)anim.Clone();
                }
                if(from != null)
                {
                    from.Animation = (UIViewAnimation)anim.Clone();
                }
                return true;
            }
            return false;
        }

        bool IsPopAction(ActionType act)
        {
            return act == ActionType.Pop || act == ActionType.PopUntilCheck || act == ActionType.PopUntilPos || act == ActionType.PopUntilType;
        }

        void SetupTransition(UIViewController from, UIViewController to, ActionType act)
        {
            if(FrontContainer != null && to != null)
            {
                to.SetParent(FrontContainer.transform);
            }
            if(BackContainer != null && from != null)
            {
                from.SetParent(BackContainer.transform);
            }
            if(act == ActionType.Push)
            {
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

        IEnumerator DoTransition(UIViewController from, UIViewController to, ActionType act)
        {            
            if(from == to)
            {
                // no need to transition to itself
                yield break;
            }

            SetupTransition(from, to, act);

            DebugLog(string.Format("StartTransition {0} {1} -> {2}", SimultaneousAnimations ? "sim" : "con",
                from == null ? string.Empty : from.gameObject.name,
                to == null ? string.Empty : to.gameObject.name));

            // wait one frame to prevent overlapping transitions
            yield return null;

            if(SimultaneousAnimations)
            {
                if(from != null && to != null && from.State == ViewState.Shown)
                {
                    from.Hide();
                    to.Show();
                    while(!to.IsStable || !from.IsStable)
                    {
                        yield return null;
                    }
                }
                else if(to != null)
                {
                    Show();
                    to.Show();
                    while(!to.IsStable || !IsStable)
                    {
                        yield return null;
                    }
                }
                else if(from != null)
                {
                    from.Hide();
                    Hide();
                    while(!from.IsStable || !IsStable)
                    {
                        yield return null;
                    }
                }
            }
            else
            {
                if(from != null && to != null)
                {
                    var enm = from.HideCoroutine();
                    while(enm.MoveNext())
                    {
                        yield return enm.Current;
                    }
                    enm = to.ShowCoroutine();
                    while(enm.MoveNext())
                    {
                        yield return enm.Current;
                    }
                }
                else if(to != null)
                {
                    var enm = ShowCoroutine();
                    while(enm.MoveNext())
                    {
                        yield return enm.Current;
                    }
                    enm = to.ShowCoroutine();
                    while(enm.MoveNext())
                    {
                        yield return enm.Current;
                    }
                }
                else if(from != null)
                {
                    var enm = from.HideCoroutine();
                    while(enm.MoveNext())
                    {
                        yield return enm.Current;
                    }
                    enm = HideCoroutine();
                    while(enm.MoveNext())
                    {
                        yield return enm.Current;
                    }
                }
            }
            DebugLog("EndTransition");
        }

        [System.Diagnostics.Conditional("DEBUG_SPGUI")]
        void DebugLog(string msg)
        {
            Debug.Log(string.Format("UIStackController | {0}", msg));
        }

        public void SetCheckPoint(string name)
        {
            _checkpoints[name] = _stack.Count;
        }

        #region UIParentController overrides

        
        override protected void OnStart()
        {
            // prevent the stack controller
            // from appearing when the scene is loaded
        }

        override protected void OnAppearing()
        {
            base.OnAppearing();
            if(Background != null)
            {
                Background.SetActive(true);
            }
        }

        override protected void OnDisappeared()
        {
            if(Background != null)
            {
                Background.SetActive(false);
            }
            base.OnDisappeared();
        }

        override protected void OnChildViewStateChanged(UIViewController ctrl, ViewState state)
        {
            if(_action == ActionType.None && state == ViewState.Disappearing && ctrl == Top)
            {
                Pop();
            }
            else if(state == ViewState.Destroying)
            {
                for(int i = _stack.Count - 1; i > -1; i--)
                {
                    if(_stack[i] == ctrl)
                    {
                        _stack.RemoveAt(i);
                    }
                }
            }
        }

        #endregion

        
        #region Push

        public UIViewController Push(GameObject go)
        {
            var ctrl = go.GetComponent(typeof(UIViewController)) as UIViewController;
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            return Push(ctrl);
        }

        public C Push<C>() where C : UIViewController
        {
            return Push(typeof(C)) as C; 
        }

        public UIViewController Push(Type c)
        {
            return Push(CreateChild(c));
        }

        public UIViewController Push(UIViewController ctrl)
        {
            var act = ActionType.Push;
            StartActionCoroutine(DoPushCoroutine(ctrl, act), act);
            return ctrl;
        }

        public IEnumerator PushCoroutine(UIViewController ctrl)
        {
            DebugLog(string.Format("Push {0}", ctrl.gameObject.name));
            var act = ActionType.Push;
            yield return StartActionCoroutine(DoPushCoroutine(ctrl, act), act);
        }

        IEnumerator DoPushCoroutine(UIViewController ctrl, ActionType act)
        {
            var top = Top;
            AddChild(ctrl);
            _stack.Add(ctrl);
            var enm = DoTransition(top, ctrl, act);
            while(enm.MoveNext())
            {
                yield return enm;
            }
        }

        public UIViewController PushImmediate(GameObject go)
        {
            var ctrl = go.GetComponent<UIViewController>();
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            return PushImmediate(ctrl);
        }

        public C PushImmediate<C>() where C : UIViewController
        {
            return PushImmediate(typeof(C)) as C; 
        }

        public UIViewController PushImmediate(Type c)
        {
            return PushImmediate(CreateChild(c));
        }

        public UIViewController PushImmediate(UIViewController ctrl)
        {
            DebugLog(string.Format("PushImmediate {0}", ctrl.gameObject.name));
            var top = Top;
            AddChild(ctrl);
            _stack.Add(ctrl);
            SetupTransition(top, ctrl, ActionType.Push);
            if(top != null)
            {
                top.HideImmediate();
            }
            ctrl.ShowImmediate();
            return ctrl;
        }

        #endregion

        #region PushBehind

        public UIViewController PushBehind(GameObject go)
        {
            var ctrl = go.GetComponent(typeof(UIViewController)) as UIViewController;
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            return PushBehind(ctrl);
        }

        public C PushBehind<C>() where C : UIViewController
        {
            return PushBehind(typeof(C)) as C; 
        }

        public UIViewController PushBehind(Type c)
        {
            return PushBehind(CreateChild(c));
        }

        public UIViewController PushBehind(UIViewController ctrl)
        {
            if(_stack.Count == 0)
            {
                return Push(ctrl);
            }
            DebugLog(string.Format("PushBehind {0}", ctrl.gameObject.name));
            AddChild(ctrl);
            if(BackContainer != null)
            {
                ctrl.transform.parent = BackContainer.transform;
            }
            ctrl.HideImmediate();
            _stack.Insert(0, ctrl);
            return ctrl;
        }

        #endregion

        #region Replace

        public UIViewController Replace(GameObject go, ActionType act = ActionType.Replace)
        {
            var ctrl = go.GetComponent(typeof(UIViewController)) as UIViewController;
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            return Replace(ctrl, act);
        }

        public C Replace<C>(ActionType act = ActionType.Replace) where C : UIViewController
        {
            return Replace(typeof(C), act) as C; 
        }

        public UIViewController Replace(Type c, ActionType act = ActionType.Replace)
        {
            return Replace(CreateChild(c), act);
        }

        public UIViewController Replace(UIViewController ctrl, ActionType act = ActionType.Replace)
        {
            StartActionCoroutine(DoReplaceCoroutine(ctrl, act), act);
            return ctrl;
        }

        public IEnumerator ReplaceCoroutine(UIViewController ctrl, ActionType act = ActionType.Replace)
        {
            yield return StartActionCoroutine(DoReplaceCoroutine(ctrl, act), act);
        }

        IEnumerator DoReplaceCoroutine(UIViewController ctrl, ActionType act)
        {
            DebugLog(string.Format("Replace {0}", ctrl.gameObject.name));
            var enm = DoPushCoroutine(ctrl, act);
            while(enm.MoveNext())
            {
                yield return enm.Current;
            }
            if(_stack.Count > 1)
            {
                _stack.RemoveAt(_stack.Count - 2);
            }
        }

        
        public UIViewController ReplaceImmediate(GameObject go)
        {
            var ctrl = go.GetComponent(typeof(UIViewController)) as UIViewController;
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            return ReplaceImmediate(ctrl);
        }

        public C ReplaceImmediate<C>() where C : UIViewController
        {
            return ReplaceImmediate(typeof(C)) as C; 
        }

        public UIViewController ReplaceImmediate(Type c)
        {
            return ReplaceImmediate(CreateChild(c));
        }

        public UIViewController ReplaceImmediate(UIViewController ctrl)
        {
            DebugLog(string.Format("ReplaceImmediate {0}", ctrl.gameObject.name));
            var top = Top;
            AddChild(ctrl);
            _stack.Add(ctrl);
            SetupTransition(top, ctrl, ActionType.Replace);
            if(top != null)
            {
                top.HideImmediate();
            }
            ctrl.ShowImmediate();
            if(_stack.Count > 1)
            {
                _stack.RemoveAt(_stack.Count - 2);
            }
            return ctrl;
        }

        #endregion

        #region pop

        public void Clear()
        {
            PopUntil(0);
        }

        public void Pop()
        {
            StartActionCoroutine(DoPopCoroutine(), ActionType.Pop);
        }

        public IEnumerator PopCoroutine()
        {
            yield return StartActionCoroutine(DoPopCoroutine(), ActionType.Pop);
        }

        IEnumerator DoPopCoroutine()
        {
            UIViewController top = null;
            UIViewController ctrl = null;
            if(_stack.Count > 0)
            {
                top = _stack[_stack.Count - 1];
                top.DestroyOnHide = true;
            }
            if(_stack.Count > 1)
            {
                ctrl = _stack[_stack.Count - 2];
            }
            var act = ActionType.Pop;
            DebugLog(string.Format("{0} {1}", act, ctrl ? ctrl.gameObject.name : string.Empty));
            var enm = DoTransition(top, ctrl, act);
            while(enm.MoveNext())
            {
                yield return enm.Current;
            }
        }

        public void PopImmediate()
        {
            if(_stack.Count > 0)
            {
                Top.HideImmediate();
                _stack.RemoveAt(_stack.Count - 1);
            }
            var ctrl = Top;
            DebugLog(string.Format("PopImmediate {0}", ctrl ? ctrl.gameObject.name : string.Empty));
            if(ctrl)
            {
                ctrl.ShowImmediate();
            }
            else
            {
                HideImmediate();
            }
        }

        private delegate bool PopCondition(UIViewController ctrl);

        IEnumerator DoPopUntilCondition(PopCondition cond, ActionType act, bool checkTop = false)
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
                else if(elm != top)
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
        }

        public void PopUntil<C>() where C : UIViewController
        {
            PopUntil(typeof(C));
        }

        public IEnumerator PopUntilCoroutine<C>() where C : UIViewController
        {
            return PopUntilCoroutine(typeof(C));
        }

        public void PopUntil(Type type)
        {
            StartActionCoroutine(DoPopUntilCoroutine(type), ActionType.PopUntilType);
        }

        public IEnumerator PopUntilCoroutine(Type type)
        {
            yield return StartActionCoroutine(DoPopUntilCoroutine(type), ActionType.PopUntilType);
        }

        IEnumerator DoPopUntilCoroutine(Type type)
        {
            return DoPopUntilCondition((UIViewController ctrl) => {
                return ctrl.GetType() == type;
            }, ActionType.PopUntilType);
        }

        public void PopUntil(int i)
        {
            StartActionCoroutine(DoPopUntilCoroutine(i), ActionType.PopUntilPos);
        }

        public IEnumerator PopUntilCoroutine(int i)
        {
            yield return StartActionCoroutine(DoPopUntilCoroutine(i), ActionType.PopUntilPos);
        }

        IEnumerator DoPopUntilCoroutine(int i)
        {
            return DoPopUntilCoroutine(i, ActionType.PopUntilPos);
        }

        IEnumerator DoPopUntilCoroutine(int i, ActionType act)
        {
            if(_stack.Count > i)
            {           
                return DoPopUntilCondition((UIViewController ctrl) => {
                    return _stack.Count <= i;
                }, act);
            }
            return null;
        }

        public void PopUntilCheckPoint(string name)
        {
            StartActionCoroutine(DoPopUntilCheckPointCoroutine(name), ActionType.PopUntilCheck);
        }

        public IEnumerator PopUntilCheckPointCoroutine(string name)
        {
            yield return StartActionCoroutine(DoPopUntilCheckPointCoroutine(name), ActionType.PopUntilCheck);
        }

        IEnumerator DoPopUntilCheckPointCoroutine(string name)
        {
            int i;
            if(!_checkpoints.TryGetValue(name, out i))
            {
                throw new Exception(string.Format("Could not find checkpoint '{0}'.", name));
            }
            else
            {
                var act = ActionType.PopUntilCheck;
                var enm = DoPopUntilCoroutine(i, act);
                while(enm.MoveNext())
                {
                    yield return enm.Current;
                }
            }
        }

        #endregion
    }
}
