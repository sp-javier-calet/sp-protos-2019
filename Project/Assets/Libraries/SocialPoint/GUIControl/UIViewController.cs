using System;
using System.Collections;
using System.Collections.Generic;
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
        public event Action<UIViewController, ViewState> ViewEvent;
        public event Action<UIViewController, GameObject> InstantiateEvent;

        private bool _loaded = false;
        private ViewState _viewState = ViewState.Initial;
        private Coroutine _showCoroutine;
        private Coroutine _hideCoroutine;
        private UIViewAnimation _animation;

        [HideInInspector]
        public UIViewController ParentController;

        [HideInInspector]
        public static UIViewControllerFactory Factory = new UIViewControllerFactory();

        [HideInInspector]
        public bool DestroyOnHide = false;

        [HideInInspector]
        public static UILayersController LayersController;

        [SerializeField]
        private List<GameObject> _containers3d = new List<GameObject>();

        IList<Material> Materials3d
        {
            get
            {
                var materials = new List<Material>();
                foreach(var element in _containers3d)
                {
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
                foreach(var mat in Materials3d)
                {
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
                foreach(var mat in Materials3d)
                {
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
                foreach(var canvas in canvases)
                {
                    foreach(RectTransform child in canvas.transform)
                    {
                        child.localPosition = value;
                    }
                }
            }

            get
            {
                var canvases = UILayersController.GetCanvasFromElement(gameObject);
                if(canvases.Count > 0)
                {
                    foreach(RectTransform child in canvases[0].transform)
                    {
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
                foreach(var canvas in canvases)
                {
                    foreach(RectTransform child in canvas.transform)
                    {
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
                    foreach(RectTransform child in canvases[0].transform)
                    {
                        size.x = Mathf.Max(size.x, child.sizeDelta.x);
                        size.y = Mathf.Max(size.y, child.sizeDelta.y);
                    }
                }
                return FixSize(size);
            }
        }

        Vector2 FixSize(Vector2 size)
        {
            if(size.x == 0)
            {
                size.x = Screen.width;
            }
            if(size.y == 0)
            {
                size.y = Screen.height;
            }
            return size;
        }

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

        void AddLayers()
        {
            if(LayersController != null)
            {
                LayersController.Add(this);

                foreach(GameObject ui3DContainer in _containers3d)
                {
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

                var container = gameObject.GetComponent<UI3DContainer>();

                if(container == null)
                {
                    container = gameObject.AddComponent<UI3DContainer>();
                }

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
            OnDestroyed();
        }

        virtual protected void OnStart()
        {
            if(ParentController == null && isActiveAndEnabled && transform.parent != null && _showCoroutine == null)
            {
                ShowImmediate();
            }
        }

        virtual protected void OnDestroyed()
        {
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

        public bool Hide(bool destroy = false)
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

        public IEnumerator HideCoroutine(bool destroy = false)
        {
            DebugLog("HideCoroutine");
            Load();
            yield return StartHideCoroutine(DoHideCoroutine(destroy));
        }

        IEnumerator DoHideCoroutine(bool destroy)
        {
            if(_viewState == ViewState.Initial || _viewState == ViewState.Hidden)
            {
                Disable();
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
            AddLayers();
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
                var enm = Animation.Appear();
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
                yield return new WaitForEndOfFrame();
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
            RemoveLayers();
            NotifyViewEvent();
        }

        protected GameObject Instantiate(GameObject proto)
        {
            var go = GameObject.Instantiate(proto);
            if(InstantiateEvent != null)
            {
                InstantiateEvent(this, go);
            }
            return go;
        }
    }
}