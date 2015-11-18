using System;
using System.Collections;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public class UIViewController : MonoBehaviour
    {
        public enum ViewState
        {
            Initial,
            Shown,
            Hidden,
            Appearing,
            Disappearing,
            Destroying,
            Destroyed
        }

        public static event Action<UIViewController> AwakeEvent;

        public delegate UIViewController CreationDelegate();
        public delegate UIViewController DefaultCreationDelegate(Type t);
        public delegate string StringCreationDelegate();
        public delegate string StringDefaultCreationDelegate(Type t);

        private bool _loaded = false;
        private ViewState _viewState = ViewState.Initial;
        private Coroutine _showCoroutine;
        private Coroutine _hideCoroutine;
        private UIViewAnimation _animation;

        public event Action<UIViewController, ViewState> ViewEvent;

        [HideInInspector]
        public UIViewController ParentController;

        [HideInInspector]
        public static UIViewControllerFactory Factory = new UIViewControllerFactory();

        [HideInInspector]
        public bool DestroyOnHide = false;

        public UIViewAnimation Animation
        {
            set
            {
                if(_animation != null)
                {
                    _animation.Reset();
                }
                if(value != null)
                {
                    value.Load(this);
                }
                _animation = value;
            }

            get
            {
                return _animation;
            }
        }

        public bool IsStable
        {
            get
            {
                return _viewState != ViewState.Appearing && _viewState != ViewState.Disappearing && _viewState != ViewState.Destroying;
            }
        }

        public ViewState State
        {
            get
            {
                return _viewState;
            }
        }

        [System.Diagnostics.Conditional("DEBUG_SPGUI")]
        void DebugLog(string msg)
        {
            Debug.Log(string.Format("UIViewController {0} {1} | {2}", gameObject.name, _viewState, msg));
        }

        public void SetParent(Transform parent)
        {
            gameObject.transform.SetParent(parent, false);
        }

        void Awake()
        {
            if(AwakeEvent != null)
            {
                AwakeEvent(this);
            }
        }

        void Start()
        {
            OnStart();
        }

        void OnDestroy()
        {
            Reset();
            HideImmediate();
        }

        virtual protected void OnStart()
        {
            if(isActiveAndEnabled && transform.parent != null)
            {
                ShowImmediate();
            }
        }

        public bool Load()
        {
            bool loaded = false;
            if(!_loaded)
            {
                _loaded = true;
                loaded = true;
                OnLoad();
            }
            Setup();
            Reset();
            return loaded;
        }

        void Setup()
        {
            if(ParentController == null)
            {
                ParentController = FindParentController();
            }
            if(transform.parent == null)
            {
                if(ParentController != null)
                {
                    SetParent(ParentController.transform);
                }
                if(Canvas != null)
                {
                    SetParent(Canvas.transform);
                }
            }
        }

        UIViewController FindParentController()
        {
            if(transform.parent == null)
            {
                return null;
            }
            GameObject parent = transform.parent.gameObject;
            while(parent != null)
            {
                var ctrl = parent.GetComponent(typeof(UIViewController)) as UIViewController;
                if(ctrl != null)
                {
                    return ctrl;
                }
                if(parent.transform.parent == null)
                {
                    break;
                }
                parent = parent.transform.parent.gameObject;
            }
            return null;
        }
        
        virtual protected void OnLoad()
        {
        }

        Coroutine StartShowCoroutine(IEnumerator enm)
        {
            gameObject.SetActive(true);
            _showCoroutine = StartCoroutine(enm);
            return _showCoroutine;
        }
        
        Coroutine StartHideCoroutine(IEnumerator enm)
        {
            gameObject.SetActive(true);
            _hideCoroutine = StartCoroutine(enm);
            return _hideCoroutine;
        }

        public void ShowImmediate()
        {
            DebugLog("ShowImmediate");
            Load();
            if(_viewState != ViewState.Appearing && _viewState != ViewState.Shown)
            {
                OnAppearing();
            }
            gameObject.SetActive(true);
            if(_viewState != ViewState.Shown)
            {
                OnAppeared();
            }
        }

        public bool Show()
        {
            DebugLog("Show");
            Load();
            var enm = DoShowCoroutine();
            if(enm != null)
            {
                StartShowCoroutine(enm);
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerator ShowCoroutine()
        {
            DebugLog("ShowCoroutine");
            Load();
            yield return StartShowCoroutine(DoShowCoroutine());
        }

        IEnumerator DoShowCoroutine()
        {
            if(_viewState == ViewState.Appearing && _showCoroutine != null)
            {
                yield return _showCoroutine;
            }
            else
            {
                Load();
                if(_viewState != ViewState.Shown)
                {
                    var enm = FullAppear();
                    while(enm.MoveNext())
                    {
                        yield return enm.Current;
                    }
                }
            }
        }

        public void HideImmediate(bool destroy=false)
        {
            DebugLog("HideImmediate");
            Load();
            if(_viewState != ViewState.Disappearing && _viewState != ViewState.Hidden)
            {
                OnDisappearing();
            }
            Disable();
            if(_viewState != ViewState.Hidden)
            {
                OnDisappeared();
            }
            CheckDestroyOnHide(destroy);
        }
        
        public bool Hide(bool destroy=false)
        {
            DebugLog("Hide");
            Load();
            var enm = DoHideCoroutine(destroy);
            if(enm != null)
            {
                StartHideCoroutine(enm);
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerator HideCoroutine(bool destroy=false)
        {
            DebugLog("HideCoroutine");
            Load();
            yield return StartHideCoroutine(DoHideCoroutine(destroy));
        }

        IEnumerator DoHideCoroutine(bool destroy)
        {
            if(_viewState == ViewState.Initial)
            {
                HideImmediate();
            }
            else if(_viewState == ViewState.Disappearing && _hideCoroutine != null)
            {
                if(destroy)
                {
                    DestroyOnHide = true;
                }
                yield return _hideCoroutine;
            }
            else if(_viewState != ViewState.Hidden)
            {
                var enm = FullDisappear(destroy);
                while(enm.MoveNext())
                {
                    yield return enm.Current;
                }
            }
        }

        void NotifyViewEvent()
        {
            DebugLog(string.Format("NotifyViewEvent {0}", _viewState));
            if(ViewEvent != null)
            {
                ViewEvent(this, _viewState);
            }
        }

        virtual protected void OnAppearing()
        {
            DebugLog("OnAppearing");
            _viewState = ViewState.Appearing;
            NotifyViewEvent();
        }

        IEnumerator FullAppear()
        {
            OnAppearing();
            var enm = Appear();
            while(enm.MoveNext())
            {
                yield return enm.Current;
            }
            OnAppeared();
            _showCoroutine = null;
        }
        
        virtual protected IEnumerator Appear()
        {
            if(Animation != null)
            {
                _showCoroutine = StartCoroutine(Animation.Appear());
                yield return _showCoroutine;
            }
        }

        virtual protected void OnAppeared()
        {
            DebugLog("OnAppeared");
            _viewState = ViewState.Shown;
            NotifyViewEvent();
        }

        IEnumerator FullDisappear(bool destroy)
        {
            OnDisappearing();
            var enm = Disappear();
            while(enm.MoveNext())
            {
                yield return enm.Current;
            }
            Disable();
            OnDisappeared();
            _hideCoroutine = null;
            CheckDestroyOnHide(destroy);
        }

        void CheckDestroyOnHide(bool force)
        {
            if(DestroyOnHide || force)
            {
                _viewState = ViewState.Destroying;
                NotifyViewEvent();
                GameObject.Destroy(gameObject);
                _viewState = ViewState.Destroyed;
                NotifyViewEvent();
            }
        }

        virtual protected void OnDisappearing()
        {
            DebugLog("OnDisappearing");
            _viewState = ViewState.Disappearing;
            NotifyViewEvent();
        }
        
        virtual protected IEnumerator Disappear()
        {
            if(Animation != null)
            {
                var enm = Animation.Disappear();
                while(enm.MoveNext())
                {
                    yield return enm.Current;
                }
            }
        }

        virtual protected void Reset()
        {
            DebugLog("Reset");
            if(_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
                _showCoroutine = null;
            }
            if(_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }
            if(Animation != null)
            {
                Animation.Reset();
            }
        }

        virtual protected void Disable()
        {
            DebugLog("Disable");
            gameObject.SetActive(false);
        }

        virtual protected void OnDisappeared()
        {
            DebugLog("OnDisappeared");
            _viewState = ViewState.Hidden;
            NotifyViewEvent();
        }
        
        public static Camera GetLayerCamera(int layer)
        {
            var cams = GameObject.FindObjectsOfType<Camera>();
            int layerMask = (1 << layer);
            for(int i = 0; i < cams.Length; i++)
            {
                if(cams[i].cullingMask == layerMask)
                {
                    return cams[i];
                }
            }
            return null;
        }
        
        public static void SetViewportRect(Rect viewport, int layer)
        {
            Camera cam = GetLayerCamera(layer);
            if(cam == null)
            {
                throw new Exception(string.Format("Could not find camera in layer '{0}'.", LayerMask.LayerToName(layer)));
            }
            else
            {
                cam.rect = viewport;
            }
        }

        static Canvas _canvas;
        public static Canvas Canvas
        {
            get
            {
                if(_canvas == null)
                {
                    _canvas = FindObjectOfType<Canvas>();
                }
                return _canvas;
            }
        }
    }
}
