using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SocialPoint.GUIControl
{
    public abstract class UIScrollRectBaseDataSource<TCellData> : MonoBehaviour where TCellData : UIScrollRectCellData
    {
        public List<TCellData> Data { get; set; } 

        public abstract IEnumerator Load();
        public abstract TCellData CreateCellData();
    }

    public class UIScrollRectDataSource<TCellData> : UIScrollRectBaseDataSource<TCellData> where TCellData : UIScrollRectCellData
    {
        public UIScrollRectDataSource(List<TCellData> data)
        {
            Data = data;
        }

        public override IEnumerator Load()
        {
            return null;
        }

        public override TCellData CreateCellData()
        {
            return null;
        }
    }

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

        public GameObject[] BasePrefabs;

        [SerializeField]
        bool _usePooling = true;

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

        List<TCellData> Data
        {
            get
            {
                if(DataSource == null)
                {
                    return null;
                }
                return DataSource.Data;
            }
        }

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

            if(_pagination != null)
            {
                if(_centerOnCell)
                {
                    _pagination.UseNavigationButtons = _useNavigationButtons;
                    _pagination.UsePaginationButtons = _usePaginationButtons;
                    _pagination.gameObject.SetActive(true);
                }
                else
                {
                    _pagination.gameObject.SetActive(false);
                    _pagination = null;
                }
            }
        }

        #region Unity methods

        void Awake()
        {
            if(_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }

            _scrollRectTransform = _scrollRect.GetComponent<RectTransform>();
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
            InternalLateUpdate();
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
            InternalOnDrawGizmoSelected();
        }

        #endregion

        #region IBeginDragHandler implementation

        public void OnBeginDrag(PointerEventData eventData)
        {
            InternalOnBeginDrag(eventData);
        }

        #endregion

        #region IDragHandler implementation

        public void OnDrag(PointerEventData eventData)
        {
            InternalOnDrag(eventData);
        }

        #endregion

        #region IEndDragHandler implementation

        public void OnEndDrag(PointerEventData eventData)
        {
            InternalOnEndDrag(eventData);
        }

        #endregion

        #region IInitializable implementation

        public void Initialize()
        {
            Initialized = true;
        }

        #endregion

        public UIScrollRectBaseDataSource<TCellData> DataSource;

        // This is the main method you need to call to start working with ScrollRectExtension
        public void LoadData()
        {
            if(_loadingGroup != null)
            {
                _loadingGroup.SetActive(true);
            }

            StartCoroutine(LoadDataCoroutine());
        }

        IEnumerator LoadDataCoroutine()
        {
            yield return StartCoroutine(DataSource.Load());
            OnDataLoaded();
        }

        void OnDataLoaded()
        {
            if(_loadingGroup != null)
            {
                _loadingGroup.SetActive(false);
            }

            if(Data.Count == 0)
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
                _pagination.Init(Data.Count, _currentIndex, ScrollToPreviousCell, ScrollToNextCell, ScrollToCell);
            }
        }

        public void AddData(TCellData data, bool addAtEnd, bool moveToEnd = false)
        {
            if(!_centerOnCell)
            {
                if(data != null)
                {
                    if(addAtEnd)
                    {
                        Data.Add(data);
                        SetDataValues(Data.Count - 1);
                    }
                    else
                    {
                        Data.Insert(0, data);
                        SetDataValues();
                    }

                    SetRectTransformSize(_scrollContentRectTransform, GetContentPanelSize());

                    if(_pagination != null)
                    {
                        _pagination.Reload(Data.Count, _currentIndex);
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
                    Data.RemoveAt(index);

                    SetDataValues(index);

                    if(_pagination != null)
                    {
                        _pagination.Reload(Data.Count, _currentIndex);
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