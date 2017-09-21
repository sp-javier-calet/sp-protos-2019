using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.AppEvents;
using UnityEngine;
using SocialPoint.Utils;

namespace SocialPoint.GUIControl
{
    public delegate void UIStackControllerDelegate();

    public class UIStackController : UIParentController
    {
        public enum StackVisibility
        {
            ShowLast,
            ShowAllBetweenScreens,
            ShowLastBetweenScreens
        }

        public GameObject Background;
        public GameObject FrontContainer;
        public GameObject BackContainer;
        public GameObject Blocker;
        public bool SimultaneousAnimations = true;

        public UIViewAnimation ChildUpAnimation;
        public UIViewAnimation ChildDownAnimation;
        public UIViewAnimation ChildAnimation;

        /// <summary>
        ///     To be used in Unity Tests to avoid problems with Coroutines and yields.
        /// </summary>
        public ICoroutineRunner CoroutineRunner;

        /// <summary>
        ///     To check if we want to only hide the previous UI View or hide from FullScreen to FullScreen views.
        /// </summary>
        public StackVisibility StackVisibilityMode = StackVisibility.ShowLast;

        List<UIViewController> _views = new List<UIViewController>();
        IAppEvents _appEvents;

        public IAppEvents AppEvents
        {
            set
            {
                if(_appEvents != null)
                {
                    _appEvents.GameWillRestart.Remove(Restart);
                }
                _appEvents = value;
                if(_appEvents != null)
                {
                    _appEvents.GameWillRestart.Add(0, Restart);
                }
            }
        }

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
        public IList<UIViewController> Stack
        {
            get
            {
                return _stack;
            }
        }

        IDictionary<string,int> _checkpoints = new Dictionary<string,int>();
        IEnumerator _actionCoroutine = null;
        ActionType _action = ActionType.None;

        IEnumerator StartActionCoroutine(IEnumerator enm, ActionType act)
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
                if(CoroutineRunner != null)
                {
                    CoroutineRunner.StopCoroutine(_actionCoroutine);
                }
                else
                {
                    StopCoroutine(_actionCoroutine);
                }
                _actionCoroutine = null;
            }
            _action = act;
            if(gameObject.activeInHierarchy)
            {
                if(CoroutineRunner != null)
                {
                    _actionCoroutine = CoroutineRunner.StartCoroutine(DoActionCoroutine(enm));
                }
                else
                {
                    _actionCoroutine = DoActionCoroutine(enm);
                    StartCoroutine(_actionCoroutine);
                }
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

        bool IsPushAction(ActionType act)
        {
            return act == ActionType.Push;
        }

        bool IsPopAction(ActionType act)
        {
            return act == ActionType.Pop || act == ActionType.PopUntilCheck || act == ActionType.PopUntilPos || act == ActionType.PopUntilType;
        }

        bool IsReplaceAction(ActionType act)
        {
            return act == ActionType.Replace;
        }
            
        void ShowStackedUIViews(bool showPopups)
        {
            // Always starting from the second element in top, we need to store the views that need 
            // to be shown/hidden and then show or hide them in a reverse way to be sure that the
            // layers sorting order for each views is correct

            _views.Clear();

            if(_stack.Count > 1)
            {
                for(int i = _stack.Count - 2; i >= 0; i--)
                {
                    var elm = _stack[i];
                    if(elm != null)
                    {
                        _views.Add(elm);

                        if(elm.IsFullScreen || i == 0)
                        {
                            break;
                        }
                    }
                }

                if(_views.Count > 0)
                {
                    for(int i = _views.Count - 1; i >= 0; i--)
                    {
                        var elm = _views[i];
                        if(elm != null)
                        {
                            if(showPopups)
                            {
                                if(StackVisibilityMode == StackVisibility.ShowLastBetweenScreens)
                                {
                                    if(elm.IsFullScreen)
                                    {
                                        elm.ShowImmediate();
                                    }
                                    else if(!elm.IsFullScreen)
                                    {
                                        elm.HideImmediate();
                                    }
                                }
                                else
                                {
                                    elm.ShowImmediate();
                                }
                            }
                            else
                            {
                                elm.HideImmediate();
                            }
                        }
                    }
                }
            }
        }

        void SetupParent(UIViewController from, UIViewController to)
        {
            if(FrontContainer != null && to != null)
            {
                to.SetParent(FrontContainer.transform);
            }

            if(BackContainer != null && from != null)
            {
                from.SetParent(BackContainer.transform);
            }
        }

        void SetupTransition(UIViewController from, UIViewController to, ActionType act)
        {
            SetupParent(from, to);

            if(IsPushAction(act))
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
                if(IsReplaceAction(act))
                {
                    SetAnimation(from, to, ChildAnimation);
                }
            }
        }

        IEnumerator DoTransition(UIViewController from, UIViewController to, ActionType act, bool forceShowLast = false)
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

            // wait one frame to prevent overlapping transitions. meanwhile we disable the "to" controller to avoid it to update before loading
            if(to != null)
            {
                to.gameObject.SetActive(false);
            }

            yield return null;

            if(to != null)
            {
                to.gameObject.SetActive(true);
            }

            if(SimultaneousAnimations)
            {
                if(from != null && to != null && from.State == ViewState.Shown)
                {
                    if(forceShowLast)
                    {
                        // Only allowed Push and Replace
                        if(IsReplaceAction(act))
                        {
                            if(_stack.Count > 1)
                            {
                                _stack.RemoveAt(_stack.Count - 2);
                            }
                        }

                        ShowStackedUIViews(false);
                        from.Hide();
                    }
                    else if(StackVisibilityMode == StackVisibility.ShowLast)
                    {
                        if(IsReplaceAction(act))
                        {
                            if(_stack.Count > 1)
                            {
                                _stack.RemoveAt(_stack.Count - 2);
                            }
                        }

                        from.Hide();
                    }
                    else
                    {
                        if(IsPushAction(act))
                        {
                            if(to.IsFullScreen)
                            {
                                ShowStackedUIViews(false);
                            }
                            else if(!from.IsFullScreen && StackVisibilityMode == StackVisibility.ShowLastBetweenScreens)
                            {
                                from.Hide();
                            }
                        }
                        else if(IsPopAction(act))
                        {
                            ShowStackedUIViews(true);
                            from.Hide();
                        }
                        else if(IsReplaceAction(act))
                        {
                            if(_stack.Count > 1)
                            {
                                _stack.RemoveAt(_stack.Count - 2);
                            }

                            if(from.IsFullScreen && !to.IsFullScreen)
                            {
                                ShowStackedUIViews(true);
                            }
                            else if(!from.IsFullScreen && to.IsFullScreen)
                            {
                                ShowStackedUIViews(false);
                            }
                                
                            from.Hide();
                        }
                    }

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

        [System.Diagnostics.Conditional(DebugFlags.DebugGUIControlFlag)]
        void DebugLog(string msg)
        {
            Log.i(string.Format("UIStackController | {0}", msg));
        }

        public void SetCheckPoint(string name)
        {
            if(_stack.Count > 0)
            {
                _checkpoints[name] = _stack.Count - 1;
            }
        }

        public bool CheckPointExists(string name)
        {
            return _checkpoints.ContainsKey(name);
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
                int topScreenIdx = _stack.Count - 1;
                if (topScreenIdx > -1)
                {
                    if(_stack[topScreenIdx] == ctrl)
                    {
                        _stack.RemoveAt(topScreenIdx);
                    }
                }
            }
        }

        #endregion

        #region Push

        public UIViewController Push(GameObject go, bool forceShowLast = false)
        {
            var ctrl = go.GetComponent(typeof(UIViewController)) as UIViewController;
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            return Push(ctrl, forceShowLast);
        }

        public C Push<C>(bool forceShowLast = false) where C : UIViewController
        {
            return Push(typeof(C), forceShowLast) as C; 
        }

        public UIViewController Push(Type c, bool forceShowLast = false)
        {
            return Push(CreateChild(c), forceShowLast);
        }

        public UIViewController Push(UIViewController ctrl, bool forceShowLast = false)
        {
            var act = ActionType.Push;
            StartActionCoroutine(DoPushCoroutine(ctrl, act, forceShowLast), act);
            return ctrl;
        }

        public IEnumerator PushCoroutine(UIViewController ctrl, bool forceShowLast = false)
        {
            var act = ActionType.Push;
            yield return StartActionCoroutine(DoPushCoroutine(ctrl, act, forceShowLast), act);
        }

        IEnumerator DoPushCoroutine(UIViewController ctrl, ActionType act, bool forceShowLast = false)
        {
            forceShowLast &= !ctrl.IsFullScreen;

            var top = Top;
            AddChild(ctrl);
            _stack.Add(ctrl);

            var enm = DoTransition(top, ctrl, act, forceShowLast);
            while(enm.MoveNext())
            {
                yield return enm;
            }
        }

        public UIViewController PushImmediate(GameObject go, bool forceShowLast = false)
        {
            var ctrl = go.GetComponent<UIViewController>();
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            return PushImmediate(ctrl, forceShowLast);
        }

        public C PushImmediate<C>(bool forceShowLast = false) where C : UIViewController
        {
            return PushImmediate(typeof(C), forceShowLast) as C; 
        }

        public UIViewController PushImmediate(Type c, bool forceShowLast = false)
        {
            return PushImmediate(CreateChild(c), forceShowLast);
        }

        public UIViewController PushImmediate(UIViewController ctrl, bool forceShowLast = false)
        {
            DebugLog(string.Format("PushImmediate {0}", ctrl.gameObject.name));

            forceShowLast &= !ctrl.IsFullScreen;

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

        public UIViewController Replace(GameObject go, bool forceShowLast = false)
        {
            var ctrl = go.GetComponent(typeof(UIViewController)) as UIViewController;
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            return Replace(ctrl, forceShowLast);
        }

        public C Replace<C>(bool forceShowLast = false) where C : UIViewController
        {
            return Replace(typeof(C), forceShowLast) as C; 
        }

        public UIViewController Replace(Type c, bool forceShowLast = false)
        {
            return Replace(CreateChild(c), forceShowLast);
        }

        public UIViewController Replace(UIViewController ctrl, bool forceShowLast = false)
        {
            if(_stack.Count == 0)
            {
                return null;
            }

            var act = ActionType.Replace;
            StartActionCoroutine(DoReplaceCoroutine(ctrl, act, forceShowLast), act);
            return ctrl;
        }

        public IEnumerator ReplaceCoroutine(UIViewController ctrl, bool forceShowLast = false)
        {
            var act = ActionType.Replace;
            yield return StartActionCoroutine(DoReplaceCoroutine(ctrl, act, forceShowLast), act);
        }

        IEnumerator DoReplaceCoroutine(UIViewController ctrl, ActionType act, bool forceShowLast)
        {
            forceShowLast &= !ctrl.IsFullScreen;

            var top = Top;
            if(top != null)
            {
                top.DestroyOnHide = true;
            }
            DebugLog(string.Format("Replace {0} with {1}", top.gameObject.name, ctrl.gameObject.name));

            var enm = DoPushCoroutine(ctrl, act, forceShowLast);
            while(enm.MoveNext())
            {
                yield return enm.Current;
            }
        }

        public UIViewController ReplaceImmediate(GameObject go, bool forceShowLast = false)
        {
            var ctrl = go.GetComponent(typeof(UIViewController)) as UIViewController;
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            return ReplaceImmediate(ctrl, forceShowLast);
        }

        public C ReplaceImmediate<C>(bool forceShowLast = false) where C : UIViewController
        {
            return ReplaceImmediate(typeof(C), forceShowLast) as C; 
        }

        public UIViewController ReplaceImmediate(Type c, bool forceShowLast = false)
        {
            return ReplaceImmediate(CreateChild(c), forceShowLast);
        }

        public UIViewController ReplaceImmediate(UIViewController ctrl, bool forceShowLast)
        {
            if(_stack.Count == 0)
            {
                return null;
            }

            forceShowLast &= !ctrl.IsFullScreen;

            var top = Top;
            DebugLog(string.Format("ReplaceImmediate {0} with {1}", top.gameObject.name, ctrl.gameObject.name));

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

            if(forceShowLast)
            {
                ShowStackedUIViews(false);
            }
            else if(StackVisibilityMode != StackVisibility.ShowLast)
            {
                if(top.IsFullScreen && !ctrl.IsFullScreen)
                {
                    ShowStackedUIViews(true);
                }
                else if(!top.IsFullScreen && ctrl.IsFullScreen)
                {
                    ShowStackedUIViews(false);
                }
            }

            return ctrl;
        }

        #endregion

        #region pop

        public void Clear()
        {
            PopUntil(-1);
        }

        public void Pop()
        {
            if(_stack.Count == 0)
            {
                return;
            }

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

            top = Top;
            if(top != null)
            {
                top.DestroyOnHide = true;
            }
 
            if(_stack.Count > 1)
            {
                ctrl = _stack[_stack.Count - 2];
            }

            var act = ActionType.Pop;
            DebugLog(string.Format("{0} from {1} to {2}", act, top.gameObject.name, ctrl ? ctrl.gameObject.name : "-"));

            var enm = DoTransition(top, ctrl, act);
            while(enm.MoveNext())
            {
                yield return enm.Current;
            }
        }

        public void PopImmediate()
        {
            if(_stack.Count == 0)
            {
                return;
            }

            UIViewController top = null;
            UIViewController ctrl = null;

            if(_stack.Count > 0)
            {
                top = Top;
                top.DestroyOnHide = true;
            }

            if(_stack.Count > 1)
            {
                ctrl = _stack[_stack.Count - 2];
            }

            DebugLog(string.Format("PopImmediate from {0} to {1}", top.gameObject.name, ctrl ? ctrl.gameObject.name : "-"));

            if(_stack.Count > 0)
            {
                if(StackVisibilityMode == StackVisibility.ShowAllBetweenScreens && top.IsFullScreen)
                {
                    ShowStackedUIViews(true);
                }

                _stack.RemoveAt(_stack.Count - 1);
                top.HideImmediate();
            }

            if(ctrl != null)
            {
                ctrl.ShowImmediate();
                SetupParent(top, ctrl);
            }
            else
            {
                HideImmediate();
            }
        }

        private delegate bool PopCondition(UIViewController ctrl);

        IEnumerator DoPopUntilCondition(PopCondition cond, ActionType act)
        {
            UIViewController top = null;
            UIViewController ctrl = null;
            for(var i = _stack.Count - 1; i >= 0; i--)
            {
                var elm = _stack[i];
                if(elm != null)
                {
                    if(top == null)
                    {
                        top = elm;
                        top.DestroyOnHide = true;
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
                    int ctrlIndex = _stack.IndexOf(ctrl);
                    return ctrlIndex <= i;
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
                var enm = DoPopUntilCoroutine(i, ActionType.PopUntilCheck);
                while(enm.MoveNext())
                {
                    yield return enm.Current;
                }
            }
        }

        #endregion

        public void Restart()
        {
            for(int i = _stack.Count - 1; i >= 0; i--)
            {
                var elm = _stack[i];
                if(elm != null)
                {
                    _stack.RemoveAt(i);
                    elm.HideImmediate(true);
                }
            }
            _stack.Clear();

            if(Background != null)
            {
                Background.SetActive(false);
            }

            if(Blocker != null)
            {
                Blocker.SetActive(false);
            }
        }

        public void OnForceCloseUIView(UIViewController ctrl)
        {
            if(ctrl != null)
            {
                Pop();
            }
        }
    }
}
