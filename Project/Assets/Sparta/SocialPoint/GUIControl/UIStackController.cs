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

    public class StackNode 
    { 
        public UIViewController Controller; 
        public GameObject GameObject;
        public bool HideControllersBelow; 
        public bool IsDesiredToShow = true;

        public StackNode(UIViewController controller, GameObject gameObject, bool hideControllersBelow)
        {
            Controller = controller;
            GameObject = gameObject;
            HideControllersBelow = hideControllersBelow;
            IsDesiredToShow = true;
        }
    }

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

        /// <summary>
        ///     To be used in Unity Tests to avoid problems with Coroutines and yields.
        /// </summary>
        public ICoroutineRunner CoroutineRunner;

        IList<StackNode> _views = new List<StackNode>();
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
            PushImmediate,
            Replace,
            ReplaceImmediate,
            Pop,
            PopImmediate,
            PopUntilType,
            PopUntilPos,
            PopUntilCheck
        }
            
        public int Count
        {
            get
            {
                return _stack.Count;
            }
        }

        public StackNode Top
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

        IList<StackNode> _stack = new List<StackNode>();
        public IList<StackNode> Stack
        {
            get
            {
                return _stack;
            }
        }

        IDictionary<string,int> _checkpoints = new Dictionary<string,int>();
        IEnumerator _actionCoroutine = null;
        ActionType _action = ActionType.None;

        #region Helper StackNodes

        public bool IsValidStackNode(StackNode stackNode)
        {
            return stackNode != null && stackNode.Controller != null && stackNode.GameObject != null;
        }
            
        StackNode NewStackNode(UIViewController ctrl, bool hideControllersBelow)
        {
            if(ctrl != null)
            {
                return new StackNode(ctrl, ctrl.gameObject, hideControllersBelow);
            }

            return null;
        }

        #endregion

        IEnumerator StartActionCoroutine(IEnumerator enm, ActionType act)
        {
            if(IsPopAction(_action))
            {
                if(Top != null && Top.Controller != null)
                {
                    Top.Controller.HideImmediate(true);
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

        bool SetAnimation(StackNode from, StackNode to, UIViewAnimation anim)
        {
            if(anim != null)
            {
                if(IsValidStackNode(to))
                {
                    to.Controller.Animation = (UIViewAnimation)anim.Clone();
                }

                if(IsValidStackNode(from))
                {
                    from.Controller.Animation = (UIViewAnimation)anim.Clone();
                }

                return true;
            }

            return false;
        }

        bool IsPushAction(ActionType act)
        {
            return act == ActionType.Push || act == ActionType.PushImmediate;
        }

        bool IsPopAction(ActionType act)
        {
            return act == ActionType.Pop || act == ActionType.PopImmediate || act == ActionType.PopUntilCheck || act == ActionType.PopUntilPos || act == ActionType.PopUntilType;
        }

        bool IsReplaceAction(ActionType act)
        {
            return act == ActionType.Replace || act == ActionType.ReplaceImmediate;
        }

        bool IsImmediateAction(ActionType act)
        {
            return act == ActionType.PushImmediate || act == ActionType.PopImmediate || act == ActionType.ReplaceImmediate;
        }
            
        public void CheckStackVisibility(IList<StackNode> views)
        {
            views.Clear();

            bool firstFullScreenFound = false;
            bool hidePreviousPopups = false;

            StackNode top = Top;
            for(int i = _stack.Count - 1; i >= 0; --i)
            {
                var elm = _stack[i];
                if(IsValidStackNode(elm) && IsValidStackNode(top))
                {
                    if(elm == top)
                    {
                        elm.IsDesiredToShow = true;
                        hidePreviousPopups = !elm.Controller.IsFullScreen && elm.HideControllersBelow;
                    }
                    else
                    {
                        if(top.Controller.IsFullScreen)
                        {
                            // Hide all Views behind Top
                            elm.IsDesiredToShow = false;
                        }
                        else 
                        {
                            // Show/Hide all Views behind Top until a screen is found
                            if(!firstFullScreenFound)
                            {
                                if(elm.Controller.IsFullScreen)
                                {
                                    firstFullScreenFound = true;
                                    elm.IsDesiredToShow = true;
                                }
                                else
                                {
                                    if(!hidePreviousPopups)
                                    {
                                        hidePreviousPopups = elm.HideControllersBelow;
                                        elm.IsDesiredToShow = true;
                                    }
                                    else
                                    {
                                        elm.IsDesiredToShow = false;
                                    }
                                }
                            }
                            else
                            {
                                elm.IsDesiredToShow = false;
                            }
                        }
                    }

                    views.Add(elm);
                }
            }
        }

        void UpdateStackVisibility(ActionType act)
        {
            // Always starting from the second element in top, we need to store the views that need 
            // to be shown/hidden and then show or hide them in a reverse way to be sure that the
            // layers sorting order for each views is correct

            CheckStackVisibility(_views);

            if(_views.Count > 0)
            {
                for(int i = _views.Count - 1; i >= 0; --i)
                {
                    var elm = _views[i];
                    if(IsValidStackNode(elm))
                    {
                        if(elm.IsDesiredToShow)
                        {
                            if(IsImmediateAction(act))
                            {
                                elm.Controller.ShowImmediate();
                            }
                            else
                            {
                                elm.Controller.Show();
                            }
                        }
                        else if(!elm.IsDesiredToShow)
                        {
                            if(IsImmediateAction(act))
                            {
                                elm.Controller.HideImmediate();
                            }
                            else
                            {
                                elm.Controller.Hide();
                            }
                        }
                    }
                }
            }
        }
            

        void SetupTransition(StackNode from, StackNode to, ActionType act)
        {
            if(FrontContainer != null && IsValidStackNode(to))
            {
                to.Controller.SetParent(FrontContainer.transform);
            }

            if(BackContainer != null && IsValidStackNode(from))
            {
                from.Controller.SetParent(BackContainer.transform);
            }

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

        IEnumerator DoTransition(StackNode from, StackNode to, ActionType act)
        {            
            if(IsValidStackNode(from) && IsValidStackNode(to) && from.Controller == to.Controller)
            {
                // no need to transition to itself
                yield break;
            }

            SetupTransition(from, to, act);

            DebugLog(string.Format("StartTransition {0} {1} -> {2}", SimultaneousAnimations ? "sim" : "con",
                from == null ? string.Empty : from.GameObject.name,
                to == null ? string.Empty : to.GameObject.name));

            // wait one frame to prevent overlapping transitions. meanwhile we disable the "to" controller to avoid it to update before loading
            if(IsValidStackNode(to))
            {
                to.GameObject.SetActive(false);
            }

            yield return null;

            if(IsValidStackNode(to))
            {
                to.GameObject.SetActive(true);
            }

            if(SimultaneousAnimations)
            {
                if(IsValidStackNode(from) && IsValidStackNode(to) && from.Controller.State == ViewState.Shown)
                {
                    if(IsPopAction(act))
                    {
                        from.Controller.Hide();
                    }
                    else if(IsReplaceAction(act))
                    {
                        from.Controller.Hide();

                        if(_stack.Count > 1)
                        {
                            _stack.RemoveAt(_stack.Count - 2);
                        }
                    }

                    UpdateStackVisibility(act);
                    while(!to.Controller.IsStable || !from.Controller.IsStable)
                    {
                        yield return null;
                    }
                }
                else if(IsValidStackNode(to))
                {
                    Show();
                    to.Controller.Show();
                    while(!to.Controller.IsStable || !IsStable)
                    {
                        yield return null;
                    }
                }
                else if(IsValidStackNode(from))
                {
                    from.Controller.Hide(true);
                    Hide();
                    while(!from.Controller.IsStable || !IsStable)
                    {
                        yield return null;
                    }
                }
            }
            else
            {
                if(IsValidStackNode(from) && IsValidStackNode(to))
                {
                    var enm = from.Controller.HideCoroutine();
                    while(enm.MoveNext())
                    {
                        yield return enm.Current;
                    }
                    enm = to.Controller.ShowCoroutine();
                    while(enm.MoveNext())
                    {
                        yield return enm.Current;
                    }
                }
                else if(IsValidStackNode(to))
                {
                    var enm = ShowCoroutine();
                    while(enm.MoveNext())
                    {
                        yield return enm.Current;
                    }
                    enm = to.Controller.ShowCoroutine();
                    while(enm.MoveNext())
                    {
                        yield return enm.Current;
                    }
                }
                else if(from != null)
                {
                    var enm = from.Controller.HideCoroutine();
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
            var top = Top;
            if(IsValidStackNode(top))
            {
                if(_action == ActionType.None && state == ViewState.Disappearing && top.Controller == ctrl)
                {
                    Pop();
                }
                else if(state == ViewState.Destroying)
                {
                    if(_stack.Count > 0)
                    {
                        if(top.Controller == ctrl)
                        {
                            _stack.RemoveAt(_stack.Count - 1);
                        }
                    }
                }
            }
        }

        #endregion

        #region Push

        public UIViewController Push(GameObject go, bool hideControllersBelow = true)
        {
            var ctrl = go.GetComponent(typeof(UIViewController)) as UIViewController;
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            return Push(ctrl, hideControllersBelow);
        }

        public C Push<C>(bool hideControllersBelow = true) where C : UIViewController
        {
            return Push(typeof(C), hideControllersBelow) as C; 
        }

        public UIViewController Push(Type c, bool hideControllersBelow = true)
        {
            return Push(CreateChild(c), hideControllersBelow);
        }

        public UIViewController Push(UIViewController ctrl, bool hideControllersBelow = true)
        {
            var act = ActionType.Push;
            StartActionCoroutine(DoPushCoroutine(ctrl, act, hideControllersBelow), act);
            return ctrl;
        }

        public IEnumerator PushCoroutine(UIViewController ctrl, bool hideControllersBelow = true)
        {
            var act = ActionType.Push;
            yield return StartActionCoroutine(DoPushCoroutine(ctrl, act, hideControllersBelow), act);
        }

        IEnumerator DoPushCoroutine(UIViewController ctrl, ActionType act, bool hideControllersBelow = true)
        {
            var stackNode = NewStackNode(ctrl, hideControllersBelow);

            var top = Top;
            AddChild(stackNode.GameObject);
            _stack.Add(stackNode);

            var enm = DoTransition(top, stackNode, act);
            while(enm.MoveNext())
            {
                yield return enm;
            }
        }

        public UIViewController PushImmediate(GameObject go, bool hideControllersBelow = true)
        {
            var ctrl = go.GetComponent<UIViewController>();
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            return PushImmediate(ctrl, hideControllersBelow);
        }

        public C PushImmediate<C>(bool hideControllersBelow = true) where C : UIViewController
        {
            return PushImmediate(typeof(C), hideControllersBelow) as C; 
        }

        public UIViewController PushImmediate(Type c, bool hideControllersBelow = true)
        {
            return PushImmediate(CreateChild(c), hideControllersBelow);
        }

        public UIViewController PushImmediate(UIViewController ctrl, bool hideControllersBelow = true)
        {
            var stackNode = NewStackNode(ctrl, hideControllersBelow);
            DebugLog(string.Format("PushImmediate {0}", IsValidStackNode(stackNode) ? stackNode.GameObject.name : string.Empty));

            var top = Top;
            AddChild(stackNode.GameObject);
            _stack.Add(stackNode);

            var act = ActionType.PushImmediate;
            SetupTransition(top, stackNode, act);
            if(IsValidStackNode(top))
            {
                top.Controller.HideImmediate();
            }

            UpdateStackVisibility(act);
            stackNode.Controller.ShowImmediate();

            return stackNode.Controller;
        }

        #endregion

        #region Replace

        public UIViewController Replace(GameObject go, bool hideControllersBelow = true)
        {
            var ctrl = go.GetComponent(typeof(UIViewController)) as UIViewController;
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            return Replace(ctrl, hideControllersBelow);
        }

        public C Replace<C>(bool hideControllersBelow = true) where C : UIViewController
        {
            return Replace(typeof(C), hideControllersBelow) as C; 
        }

        public UIViewController Replace(Type c, bool hideControllersBelow = true)
        {
            return Replace(CreateChild(c), hideControllersBelow);
        }

        public UIViewController Replace(UIViewController ctrl, bool hideControllersBelow = true)
        {
            if(_stack.Count == 0)
            {
                return null;
            }

            var act = ActionType.Replace;
            StartActionCoroutine(DoReplaceCoroutine(ctrl, act, hideControllersBelow), act);
            return ctrl;
        }

        public IEnumerator ReplaceCoroutine(UIViewController ctrl, bool hideControllersBelow = true)
        {
            var act = ActionType.Replace;
            yield return StartActionCoroutine(DoReplaceCoroutine(ctrl, act, hideControllersBelow), act);
        }

        IEnumerator DoReplaceCoroutine(UIViewController ctrl, ActionType act, bool hideControllersBelow = true)
        {
            var top = Top;
            DebugLog(string.Format("Replace {0} with {1}", IsValidStackNode(top) ? top.GameObject.name : string.Empty, ctrl != null ? ctrl.gameObject.name : string.Empty));

            var enm = DoPushCoroutine(ctrl, act, hideControllersBelow);
            while(enm.MoveNext())
            {
                yield return enm.Current;
            }
        }
            
        public UIViewController ReplaceImmediate(GameObject go, bool hideControllersBelow = true)
        {
            var ctrl = go.GetComponent(typeof(UIViewController)) as UIViewController;
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            return ReplaceImmediate(ctrl, hideControllersBelow);
        }

        public C ReplaceImmediate<C>(bool hideControllersBelow = true) where C : UIViewController
        {
            return ReplaceImmediate(typeof(C), hideControllersBelow) as C; 
        }

        public UIViewController ReplaceImmediate(Type c, bool hideControllersBelow = true)
        {
            return ReplaceImmediate(CreateChild(c), hideControllersBelow);
        }

        public UIViewController ReplaceImmediate(UIViewController ctrl, bool hideControllersBelow = true)
        {
            if(_stack.Count == 0)
            {
                return null;
            }

            var stackNode =  NewStackNode(ctrl, hideControllersBelow);
                
            var top = Top;
            DebugLog(string.Format("ReplaceImmediate {0} with {1}", IsValidStackNode(top) ? top.GameObject.name : string.Empty, IsValidStackNode(stackNode) ? stackNode.GameObject.name : string.Empty));

            var act = ActionType.ReplaceImmediate;

            AddChild(stackNode.GameObject);
            _stack.Add(stackNode);
            SetupTransition(top, stackNode, act);

            if(IsValidStackNode(top))
            {
                top.Controller.HideImmediate();
            }
                
            stackNode.Controller.ShowImmediate();

            if(_stack.Count > 1)
            {
                _stack.RemoveAt(_stack.Count - 2);
            }

            UpdateStackVisibility(act);

            return stackNode.Controller;
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
            StackNode top = Top;
            if(IsValidStackNode(top))
            {
                top.Controller.DestroyOnHide = true;
            }

            StackNode stackNode = null;
            if(_stack.Count > 1)
            {
                stackNode = _stack[_stack.Count - 2];
            }

            var act = ActionType.Pop;
            DebugLog(string.Format("{0} from {1} to {2}", act, IsValidStackNode(top) ? top.GameObject.name : string.Empty, IsValidStackNode(stackNode) ? stackNode.GameObject.name : string.Empty));

            var enm = DoTransition(top, stackNode, act);
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

            var top = Top;
            DebugLog(string.Format("PopImmediate {0}", IsValidStackNode(top) ? top.GameObject.name : string.Empty));

            if(IsValidStackNode(top))
            {
                top.Controller.HideImmediate(true);
            }
                
            var stackNode = Top;
            if(IsValidStackNode(stackNode))
            {
                stackNode.Controller.ShowImmediate();
            }
            else
            {
                HideImmediate();
            }

            UpdateStackVisibility(ActionType.PopImmediate);
        }

        private delegate bool PopCondition(UIViewController ctrl);

        IEnumerator DoPopUntilCondition(PopCondition cond, ActionType act)
        {
            StackNode top = Top;
            StackNode stackNode = null;

            if(IsValidStackNode(top))
            {
                top.Controller.DestroyOnHide = true;
            }

            for(var i = _stack.Count - 1; i >= 0; --i)
            {
                var elm = _stack[i];
                if(IsValidStackNode(elm))
                {
                    if(cond(elm.Controller))
                    {
                        stackNode = elm;
                        break;
                    }
                    else if(IsValidStackNode(top) && elm != top)
                    {
                        _stack.RemoveAt(i);
                        elm.Controller.HideImmediate(true);
                    }
                }
            }

            DebugLog(string.Format("{0} {1}", act, IsValidStackNode(stackNode) ? stackNode.GameObject.name : string.Empty));
            if(top != stackNode)
            {   
                var enm = DoTransition(top, stackNode, act);
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
            return DoPopUntilCondition((UIViewController ctrl) => { return ctrl.GetType() == type; }, ActionType.PopUntilType);
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
                return DoPopUntilCondition((UIViewController ctrl) => 
                    { 
                        if(i >= 0)
                        {
                            var elm = _stack.ElementAt(i);
                            if(IsValidStackNode(elm))
                            {
                                return _stack.ElementAt(i).Controller == ctrl; 
                            }
                        }

                        return false;
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
                if(IsValidStackNode(elm))
                {
                    _stack.RemoveAt(i);
                    elm.Controller.HideImmediate(true);
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
    }
}
