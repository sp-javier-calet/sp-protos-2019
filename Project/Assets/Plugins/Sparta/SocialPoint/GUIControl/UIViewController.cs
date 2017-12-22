using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
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

        /// <summary>
        ///     To check if the UI View is full screen.
        /// </summary>
        public bool IsFullScreen = true;

        public static event Action<UIViewController> AwakeEvent;
        public event Action<UIViewController, ViewState> ViewEvent;
        public event Action<UIViewController, GameObject> InstantiateEvent;

        public UIViewAnimationFactory AppearAnimationFactory;
        public UIViewAnimationFactory DisappearAnimationFactory;

        public UIViewAnimation AppearAnimation;
        public UIViewAnimation DisappearAnimation;

        bool _loaded;
        Coroutine _showCoroutine;
        Coroutine _hideCoroutine;

        [SerializeField]
        ViewState _viewState = ViewState.Initial;

        [HideInInspector]
        public UIViewController ParentController;

        [HideInInspector]
        public static UIViewControllerFactory Factory = new UIViewControllerFactory();

        [HideInInspector]
        public bool DestroyOnHide;

        [HideInInspector]
        public static UILayersController DefaultLayersController;

        [HideInInspector]
        UILayersController _layersController;

        public UILayersController LayersController
        {
            get
            {
                if(_layersController != null)
                {
                    return _layersController;
                }
                if(ParentController != null)
                {
                    return ParentController.LayersController;
                }
                if(DefaultLayersController != null)
                {
                    return DefaultLayersController;
                }
                if(_layersController == null)
                {
                    _layersController = FindObjectOfType<UILayersController>();
                }
                return _layersController;
            }

            set
            {
                _layersController = value;
            }
        }

        [SerializeField]
        List<GameObject> _containers3d = new List<GameObject>();

        IList<Material> Materials3d
        {
            get
            {
                var materials = new List<Material>();
                for(int i = 0, _containers3dCount = _containers3d.Count; i < _containers3dCount; i++)
                {
                    var element = _containers3d[i];
                    var renderer = element.GetComponent<Renderer>();
                    if(renderer != null && renderer.material != null)
                    {
                        materials.Add(renderer.material);
                    }
                }
                return materials;
            }
        }

        public float Alpha
        {
            set
            {
                var group = gameObject.GetComponent<CanvasGroup>();
                if(group != null)
                {
                    group.alpha = value;
                }
                for(int i = 0, Materials3dCount = Materials3d.Count; i < Materials3dCount; i++)
                {
                    var mat = Materials3d[i];
                    mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, value);
                }
            }

            get
            {
                var group = gameObject.GetComponent<CanvasGroup>();
                var alpha = 0.0f;
                if(group != null)
                {
                    alpha = group.alpha;
                }
                for(int i = 0, Materials3dCount = Materials3d.Count; i < Materials3dCount; i++)
                {
                    var mat = Materials3d[i];
                    alpha = Mathf.Max(alpha, mat.color.a);
                }
                return alpha;
            }
        }

        public Vector2 Position
        {
            set
            {
                var canvases = UILayersController.GetCanvasFromElement(gameObject);
                for(int i = 0, canvasesCount = canvases.Count; i < canvasesCount; i++)
                {
                    var canvas = canvases[i];
                    var itr = canvas.transform.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var child = (Transform)itr.Current;
                        child.localPosition = value;
                    }
                }
            }

            get
            {
                var canvases = UILayersController.GetCanvasFromElement(gameObject);
                for(var i = 0; i < canvases.Count; i++)
                {
                    var itr = canvases[i].transform.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var child = (Transform)itr.Current;
                        return child.localPosition;
                    }
                }
                return Vector2.zero;
            }
        }

        public Vector2 Size
        {
            set
            {
                var size = FixSize(value);
                var canvases = UILayersController.GetCanvasFromElement(gameObject);
                for(int i = 0, canvasesCount = canvases.Count; i < canvasesCount; i++)
                {
                    var canvas = canvases[i];
                    var itr = canvas.transform.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var child = (RectTransform)itr.Current;
                        child.sizeDelta = size;
                    }
                }
            }

            get
            {
                var canvases = UILayersController.GetCanvasFromElement(gameObject);
                var size = Vector2.zero;
                if(canvases.Count > 0)
                {
                    var itr = canvases[0].transform.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var child = (RectTransform)itr.Current;
                        size.x = Mathf.Max(size.x, child.sizeDelta.x);
                        size.y = Mathf.Max(size.y, child.sizeDelta.y);
                    }
                }
                return FixSize(size);
            }
        }

        public Vector2 ScreenPosition
        {
            set
            {
                var canvases = UILayersController.GetCanvasFromElement(gameObject);
                for(int i = 0, canvasesCount = canvases.Count; i < canvasesCount; i++)
                {
                    var canvas = canvases[i];
                    var scale = UILayersController.GetCanvasScale(canvas);
                    var itr = canvas.transform.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var child = (Transform)itr.Current;
                        child.localPosition = new Vector2(
                            value.x * scale.x,
                            value.y * scale.y);
                    }
                }
            }

            get
            {
                var canvases = UILayersController.GetCanvasFromElement(gameObject);
                for(var i = 0; i < canvases.Count; i++)
                {
                    var canvas = canvases[i];
                    var scale = UILayersController.GetCanvasScale(canvas);
                    var itr = canvas.transform.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var child = (Transform)itr.Current;
                        var p = child.localPosition;
                        p.x /= scale.x;
                        p.y /= scale.y;
                        return p;
                    }
                }
                return Vector2.zero;
            }
        }

        public Vector2 ScreenSizeFactor
        {
            set
            {
                var canvases = UILayersController.GetCanvasFromElement(gameObject);
                for(int i = 0, canvasesCount = canvases.Count; i < canvasesCount; i++)
                {
                    var canvas = canvases[i];
                    var refres = UILayersController.GetCanvasSize(canvas);
                    var itr = canvas.transform.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var child = (RectTransform)itr.Current;
                        child.sizeDelta = new Vector2(
                            value.x * refres.x,
                            value.y * refres.y);
                    }
                }
            }

            get
            {
                var canvases = UILayersController.GetCanvasFromElement(gameObject);
                var size = Vector2.zero;
                for(int i = 0, canvasesCount = canvases.Count; i < canvasesCount; i++)
                {
                    var canvas = canvases[i];
                    var refres = UILayersController.GetCanvasSize(canvas);
                    var itr = canvas.transform.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var child = (RectTransform)itr.Current;
                        size.x = Mathf.Max(size.x, child.sizeDelta.x / refres.x);
                        size.y = Mathf.Max(size.y, child.sizeDelta.y / refres.y);
                    }
                }
                return size;
            }
        }

        static Vector2 FixSize(Vector2 size)
        {
            if(Math.Abs(size.x) < Single.Epsilon)
            {
                size.x = Screen.width;
            }
            if(Math.Abs(size.y) < Single.Epsilon)
            {
                size.y = Screen.height;
            }
            return size;
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

        bool _worldSpaceFullScreen = true;

        public bool WorldSpaceFullScreen
        {
            get
            {
                return _worldSpaceFullScreen;
            }
            set
            {
                _worldSpaceFullScreen = value;
            }
        }

        void AddLayers()
        {
            if(LayersController != null)
            {
                LayersController.Add(this);

                for(int i = 0, _containers3dCount = _containers3d.Count; i < _containers3dCount; i++)
                {
                    GameObject ui3DContainer = _containers3d[i];
                    LayersController.Add3DContainer(this, ui3DContainer);
                }
            }
            else if(_containers3d.Count > 0)
            {
                throw new Exception("You need to assign a UILayersController");
            }
        }

        public void Add3DContainer(GameObject gameObject)
        {
            if(!_containers3d.Contains(gameObject))
            {
                _containers3d.Add(gameObject);

                var container = gameObject.GetComponent<UI3DContainer>() ?? gameObject.AddComponent<UI3DContainer>();

                container.OnDestroyed += On3dContainerDestroyed;

                LayersController.Add3DContainer(this, gameObject);
            }
        }

        public void On3dContainerDestroyed(GameObject gameObject)
        {
            _containers3d.Remove(gameObject);
            if(LayersController != null)
            {
                LayersController.Remove3DContainer(this, gameObject);
            }
        }

        void RemoveLayers()
        {
            if(LayersController != null)
            {
                LayersController.Remove(this);
            }
        }

        [System.Diagnostics.Conditional(DebugFlags.DebugGUIControlFlag)]
        protected void ShowDebugLogMessage(string msg)
        {
            Log.i(msg);
        }

        [System.Diagnostics.Conditional(DebugFlags.DebugGUIControlFlag)]
        protected virtual void DebugLog(string msg)
        {
            ShowDebugLogMessage(string.Format("UIViewController {0} {1} | {2}", gameObject.name, _viewState, msg));
        }

        public void LoadAppearAnimation(UIViewAnimationFactory defaultAnimationFactory, UIViewAnimation defaultAnimation)
        {
            AppearAnimation = GetAnimation(AppearAnimationFactory, defaultAnimationFactory, defaultAnimation);
            LoadAnimation(AppearAnimation);
        }

        public void LoadDisappearAnimation(UIViewAnimationFactory defaultAnimationFactory, UIViewAnimation defaultAnimation)
        {
            DisappearAnimation = GetAnimation(DisappearAnimationFactory, defaultAnimationFactory, defaultAnimation);
            LoadAnimation(DisappearAnimation);
        }

        void LoadAnimation(UIViewAnimation anim)
        {
            if(anim != null)
            {
                anim.Load(gameObject);
            }
        }

        UIViewAnimation GetAnimation(UIViewAnimationFactory viewAnimationFactory, UIViewAnimationFactory defaultAnimationFactory, UIViewAnimation defaultAnimation)
        {
            if(viewAnimationFactory != null)
            {
                return viewAnimationFactory.Create();
            }
            else if(defaultAnimationFactory != null)
            {
                return (IsFullScreen ? null : defaultAnimationFactory.Create());
            }
            else
            {
                return (IsFullScreen ? null : defaultAnimation);
            }
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

            OnAwake();
        }

        [Obsolete("Use Reset instead")]
        public void ResetState()
        {
            Reset();
        }

        void Start()
        {
            OnStart();
        }

        protected virtual void OnDestroy()
        {
            if(_loaded)
            {
                HideImmediate();
            }
            else
            {
                OnDestroyed();
            }
        }

        virtual protected void OnAwake()
        {
        }

        virtual protected void OnStart()
        {
            if(ParentController == null && gameObject.activeInHierarchy && _showCoroutine == null)
            {
                ShowImmediate();
            }
        }

        virtual protected void OnDestroyed()
        {
        }

        public void Load()
        {
            if(!_loaded)
            {
                _loaded = true;
                OnLoad();
            }

            Setup();
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
            return false;
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

        public void HideImmediate(bool destroy = false)
        {
            DebugLog("HideImmediate");
            Load();
            if(_viewState != ViewState.Disappearing && _viewState != ViewState.Hidden && _viewState != ViewState.Destroyed)
            {
                OnDisappearing();
            }
            Disable();
            if(_viewState != ViewState.Hidden && _viewState != ViewState.Destroyed)
            {
                OnDisappeared();
            }
            CheckDestroyOnHide(destroy);
        }

        public bool Hide()
        {
            DebugLog("Hide");
            Load();
            var enm = DoHideCoroutine();
            if(enm != null)
            {
                StartHideCoroutine(enm);
                return true;
            }
            return false;
        }

        public IEnumerator HideCoroutine()
        {
            DebugLog("HideCoroutine");
            Load();
            yield return StartHideCoroutine(DoHideCoroutine());
        }

        IEnumerator DoHideCoroutine()
        {
            if(_viewState == ViewState.Initial || _viewState == ViewState.Hidden)
            {
                Disable();
            }
            else if(_viewState == ViewState.Disappearing && _hideCoroutine != null)
            {
                yield return _hideCoroutine;
            }
            else if(_viewState != ViewState.Hidden)
            {
                var enm = FullDisappear(DestroyOnHide);
                while(enm.MoveNext())
                {
                    yield return enm.Current;
                }
            }
        }

        void NotifyViewEvent()
        {
            if(ViewEvent != null)
            {
                DebugLog(string.Format("UIVIewController::NotifyViewEvent {0}", _viewState));
                ViewEvent(this, _viewState);
            }
        }

        virtual protected void OnAppearing()
        {
            DebugLog("OnAppearing");
            _viewState = ViewState.Appearing;
            #if !NGUI
            AddLayers();
            #endif
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
            if(AppearAnimation != null)
            {
                var enm = AppearAnimation.Animate();
                while(enm.MoveNext())
                {
                    yield return enm.Current;
                }
            }
        }

        virtual protected void OnAppeared()
        {
            DebugLog("OnAppeared");
            _viewState = ViewState.Shown;
            DestroyOnHide = false;
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
                Factory.Destroy(this);
                _loaded = false;
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
            if(DisappearAnimation != null)
            {
                yield return new WaitForEndOfFrame();
                var enm = DisappearAnimation.Animate();
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

            _viewState = ViewState.Initial;
        }

        virtual protected void Disable()
        {
            DebugLog("Disable");

            if(this != null && gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }

        virtual protected void OnDisappeared()
        {
            DebugLog("OnDisappeared");
            _viewState = ViewState.Hidden;
            RemoveLayers();
            NotifyViewEvent();
        }

        virtual public void OnStackActionEvent(UIViewController ctrl, UIStackController.ActionType action, int count)
        {
        }

        protected GameObject Instantiate(GameObject proto)
        {
            var go = UnityEngine.Object.Instantiate(proto);
            if(InstantiateEvent != null)
            {
                InstantiateEvent(this, go);
            }
            return go;
        }
            
        virtual public bool OnBeforeClose()
        {
            return IsStable; 
        }

        #region public UI button methods

        public void Close()
        {
            Hide();
        }

        #endregion
    }
}
