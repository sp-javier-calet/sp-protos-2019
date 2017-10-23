using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using System.Collections;
using UnityEngine.EventSystems;

namespace SocialPoint.GUIControl
{
    [RequireComponent(typeof(ScrollRect))]
    public partial class UIScrollRectExtension<TCellData, TCell> : UIViewController, IBeginDragHandler, IDragHandler, IEndDragHandler where TCellData : UIScrollRectCellData where TCell : UIScrollRectCellItem<TCellData>
    {
        public enum ShowLastCellPosition
        {
            AtTop,
            AtBottom
        }

        public enum ScrollDirection
        {
            LeftOrTop = -1,
            RightOrBottom = 1
        }

        public delegate List<TCellData> UIScrollRectExtensionGetData();

        [Header("UI Components")]
        [SerializeField]
        ScrollRect _scrollRect;

        RectTransform _scrollRectTransform;
        RectTransform _scrollContentRectTransform;

        [SerializeField]
        VerticalLayoutGroup _verticalLayoutGroup;

        [SerializeField]
        HorizontalLayoutGroup _horizontalLayoutGroup;

        [SerializeField]
        GridLayoutGroup _gridLayoutGroup;

        [Header("ObjectPool")]
        [SerializeField]
        bool _usePooling;

        [Header("Scroll")]
        [Tooltip("Delta that we will add to bounds to check if we need to show/hide new cells")]
        [SerializeField]
        int _boundsDelta;

        [SerializeField]
        int _initialIndex;

        [SerializeField]
        ShowLastCellPosition _showLastCellPosition = ShowLastCellPosition.AtBottom;

        [Header("Animations")]
        [SerializeField]
        GoEaseType _scrollAnimationEaseType;

        [SerializeField]
        AnimationCurve _scrollAnimationCurve;

        [SerializeField]
        float _scrollAnimationDuration = 0.5f;

        [SerializeField]
        bool _disableDragWhileScrollingAnimation;

        [Header("Snapping")]
        [SerializeField]
        bool _centerOnCell;

//        [SerializeField]
//        Vector2 _snapToCellAnchorPoint = new Vector2(0.5f, 0.5f);

        [SerializeField]
        float _deltaDragCell = 50f;

        [Header("Magnify")]
        [SerializeField]
        Vector2 _maginifyMinScale;

        [SerializeField]
        Vector2 _maginifyMaxScale;

        [Header("Pagination")]
        [SerializeField]
        UIScrollRectPagination _pagination;

        [Header("Loading")]
        [SerializeField]
        GameObject _loadingGroup;

        [Header("Debug")]
        [SerializeField]
        Canvas _mainCanvas;

        public bool Initialized { get; private set; }

        Range _visibleElementRange;
        Dictionary<int, TCell> _visibleCells;
        List<TCellData> _data = new List<TCellData>();
        Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();

        IEnumerator _smoothScrollCoroutine;
        int _defaultStartPadding;
        int _deltaStartPadding;
        bool _requiresRefresh;
        float _initialScrollPosition;
        float _initialPadding;
        bool _isHorizontal;
        bool _isVertical;
        float _startScrollingPosition;

        int _currentIndex;
        public int CurrentIndex
        {
            get { return _currentIndex;}
            private set
            {
                Debug.Log("currentIndex: " + value);
                _currentIndex = value;
            }
        }
            
        public bool UsesVerticalLayout
        {
            get
            {
                return _verticalLayoutGroup != null;
            }
        }

        public bool UsesHorizontalLayout
        {
            get
            {
                return _horizontalLayoutGroup != null;
            }
        }

        public bool UsesGridLayout
        {
            get
            {
                return _gridLayoutGroup != null;
            }
        }

        #region Unity methods

        void Awake()
        {
            if(_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }

            _scrollRectTransform = _scrollRect.transform as RectTransform;
            _scrollContentRectTransform = _scrollRect.content;

            _verticalLayoutGroup = _verticalLayoutGroup ?? _scrollContentRectTransform.GetComponent<VerticalLayoutGroup>();
            _horizontalLayoutGroup = _horizontalLayoutGroup ?? _scrollContentRectTransform.GetComponent<HorizontalLayoutGroup>();
            _gridLayoutGroup = _gridLayoutGroup ?? _scrollContentRectTransform.GetComponent<GridLayoutGroup>();

            _scrollRect.vertical = UsesVerticalLayout;
            _scrollRect.horizontal = UsesHorizontalLayout;

            _isHorizontal = _scrollRect.horizontal;
            _isVertical = _scrollRect.vertical;
        
            _visibleCells = new Dictionary<int, TCell>();
        }

        void Start() 
        { 
            Initialize(); 
        }
            
        void LateUpdate()
        {
            MyLateUpdate();
        }

        void OnEnable()
        {
            _scrollRect.onValueChanged.AddListener(OnScrollViewValueChanged);
        }

        void OnDisable()
        {
            _scrollRect.onValueChanged.RemoveListener(OnScrollViewValueChanged);
        }

        protected override void OnDestroy() 
        { 
            Dispose(); 

            base.OnDestroy();
        }

        void OnDrawGizmosSelected()
        {
            MyOnDrawGizmoSelected();
        }

        #endregion

        #region IBeginDragHandler implementation

        public void OnBeginDrag(PointerEventData eventData)
        {
            MyOnBeginDrag(eventData);
        }

        #endregion
            
        #region IDragHandler implementation

        public void OnDrag(PointerEventData eventData)
        {
            MyOnDrag(eventData);
        }

        #endregion

        #region IEndDragHandler implementation

        public void OnEndDrag(PointerEventData eventData)
        {
            MyOnEndDrag(eventData);
        }

        #endregion

        #region IInitializable implementation

        public void Initialize()
        {
            Initialized = true;
        }

        #endregion

        IEnumerator FetchDataFromServer(UIScrollRectExtensionGetData dlg)
        {
            // Simulating server delay
            yield return new WaitForSeconds(2f);
    
            _data = dlg();
            OnEndFetchingDataFromServer();
        }

        public void FetchData(UIScrollRectExtensionGetData dlg)
        {
            if(_loadingGroup != null)
            {
                _loadingGroup.SetActive(true);
            }

            _data.Clear();

            if(dlg != null)
            {
                StartCoroutine(FetchDataFromServer(dlg));
            }
            else
            {
                throw new UnityException("Get Data delegate not defined");
            }
        }

        void OnEndFetchingDataFromServer()
        {
            if(_loadingGroup != null)
            {
                _loadingGroup.SetActive(false);
            }

            if(_data.Count == 0)
            {
                throw new UnityException("Data not loaded!");
            }

            SetInitialPadding();
            SetCellSizes();
            SetRectTransformSize(_scrollContentRectTransform, GetContentPanelSize());
            SetInitialPosition();
            SetInitialVisibleElements();

            if(_pagination != null)
            {
                _pagination.Init(_data.Count, CurrentIndex, ScrollToPreviousCell, ScrollToNextCell, ScrollToSelectedCell);
            }
        }

        void Dispose()
        {
            Initialized = false;
            StopScrolling();
            ClearAllVisibleCells();
        }
    }
}