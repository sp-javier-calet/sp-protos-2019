using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using SocialPoint.Base;

namespace SocialPoint.GUIControl
{
    [RequireComponent(typeof(ScrollRect))]
    public partial class UIScrollRectExtension<TCellData, TCell> : UIViewController, IBeginDragHandler, IDragHandler, IEndDragHandler where TCellData : UIScrollRectCellData where TCell : UIScrollRectCellItem<TCellData>
    {
        public enum ShowLastCellPosition
        {
            AtStart,
            AtEnd
        }

        public enum ScrollDirection
        {
            LeftOrTop = -1,
            RightOrBottom = 1
        }

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

        [Header("System")]
        [SerializeField]
        GameObject _loadingGroup;

        [SerializeField]
        protected GameObject[] _prefabs;

        [SerializeField]
        bool _usePooling;

        [Tooltip("Delta that we will add to bounds to check if we need to show/hide new cells")]
        [SerializeField]
        int _boundsDelta;

        [SerializeField]
        int _initialIndex;

        [SerializeField]
        ShowLastCellPosition _showLastCellPosition = ShowLastCellPosition.AtEnd;

        [Header("Animations")]
        [SerializeField]
        float _scrollAnimationTime = 0.5f;

        [SerializeField]
        GoEaseType _scrollAnimationEaseType;

        [SerializeField]
        AnimationCurve _scrollAnimationCurve;

        [SerializeField]
        bool _disableDragWhileScrollingAnimation;

        [Header("Snapping")]
        [SerializeField]
        bool _centerOnCell;

        // TODO IMPROVEMENT
//        [SerializeField] 
//        Vector2 _snapToCellAnchorPoint = new Vector2(0.5f, 0.5f);

        [SerializeField]
        float _deltaDragCell = 50f;

        // TODO IMPROVEMENT
//        [Header("Magnify")]
//        [SerializeField]
//        bool _magnifyOnCenteredCell;

//        [SerializeField]
//        Vector2 _maginifyMinScale;

        // TODO IMPROVEMENT
//        [SerializeField]
//        Vector2 _maginifyMaxScale;

        [Header("Pagination")]
        [SerializeField]
        UIScrollRectPagination _pagination;

        [SerializeField]
        bool _useNavigationButtons;

        [SerializeField]
        bool _usePaginationButtons;

        [Header("Debug")]
        [SerializeField]
        Canvas _mainCanvas;

        public bool Initialized { get; private set; }

        List<TCellData> _data = new List<TCellData>();
        IEnumerator _scrollCoroutine;
        int _defaultStartPadding;
        int _deltaStartPadding;
        bool _requiresRefresh;
        float _initialScrollPosition;
        float _initialPadding;
        bool _isHorizontal;
        bool _isVertical;
        float _startScrollingPosition;
        Vector2 _tempVector2 = Vector3.zero;
        int _currentIndex;
        UIViewAnimation _scrollAnimation;

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

        void ApplyLayoutRules()
        {
            if(_centerOnCell)
            {
                if(_pagination != null)
                {
                    _pagination.UseNavigationButtons = _useNavigationButtons;
                    _pagination.UsePaginationButtons = _usePaginationButtons;
                    _pagination.gameObject.SetActive(true);
                }
            }
            else
            {
                _pagination.gameObject.SetActive(false);
                _pagination = null;
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

            ApplyLayoutRules();
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

        protected virtual List<TCellData> GetData() { return null; }
        protected virtual TCellData AddData() { return null; }

        IEnumerator FetchDataFromServer()
        {
            // Simulating server delay to show loading spinner (only for testing)
            yield return new WaitForSeconds(2f);

            _data.Clear();
            _data = GetData();
            OnEndFetchingDataFromServer();

            yield return null;
        }

        public void FetchData()
        {
            if(_loadingGroup != null)
            {
                _loadingGroup.SetActive(true);
            }

            StartCoroutine(FetchDataFromServer());
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
            CreatePoolObjectsIfNeeded();
            SetDataValues();
            SetRectTransformSize(_scrollContentRectTransform, GetContentPanelSize());
            SetInitialPosition();
            SetInitialVisibleElements();

            if(_pagination != null)
            {
                _pagination.Init(_data.Count, _currentIndex, ScrollToPreviousCell, ScrollToNextCell, ScrollToCell);
            }
        }

        public void AddData(bool addAtEnd = true, bool moveToEnd = false)
        {
            if(!_centerOnCell)
            {
                var data = AddData();
                if(data != null)
                {
                    if(addAtEnd)
                    {
                        _data.Add(data);
                        SetDataValues(_data.Count - 1);
                    }
                    else
                    {
                        _data.Insert(0, data);
                        SetDataValues();
                    }

                    SetRectTransformSize(_scrollContentRectTransform, GetContentPanelSize());

                    if(_pagination != null)
                    {
                        _pagination.Reload(_data.Count, _currentIndex);
                    }

                    RefreshVisibleCells(true);
                }
            }
        }

        public void RemoveData(int index)
        {
            if(!_centerOnCell)
            {
                if(IndexIsValid(index))
                {
                    _data.RemoveAt(index);

                    SetDataValues(index);

                    if(_pagination != null)
                    {
                        _pagination.Reload(_data.Count, _currentIndex);
                    }

                    HideCell(index, true, FinishRemovingData);
                }
            }
        }

        void FinishRemovingData()
        {
            SetRectTransformSize(_scrollContentRectTransform, GetContentPanelSize());
            RefreshVisibleCells(true);
        }

        void Dispose()
        {
            Initialized = false;
            StopScrolling();
            ClearAllVisibleCells();
        }
    }
}