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

    public class UIScrollRectExtensionInspector : UIViewController
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

        public bool UsePooling = true;
        public bool CenterOnCell;
        public bool UseNavigationButtons;
        public bool UsePaginationButtons;
        public bool DisableDragWhileScrollingAnimation;
        public ShowLastCellPosition LastCellPosition = ShowLastCellPosition.AtEnd;
        public ScrollRect ScrollRect;
        public VerticalLayoutGroup VerticalLayoutGroup;
        public HorizontalLayoutGroup HorizontalLayoutGroup;
        public GridLayoutGroup GridLayoutGroup;
        public GameObject LoadingGroup;
        public GameObject[] BasePrefabs;
        public int BoundsDelta = 50;
        public int InitialIndex;
        public float ScrollAnimationTime = 0.5f;
        public GoEaseType ScrollAnimationEaseType;
        public AnimationCurve ScrollAnimationCurve;
        public float DeltaDragCell = 50f;
        public UIScrollRectPagination Pagination;
        public Canvas MainCanvas;
    }
        
    [RequireComponent(typeof(ScrollRect))]
    public partial class UIScrollRectExtension<TCellData, TCell> : UIScrollRectExtensionInspector, IBeginDragHandler, IDragHandler, IEndDragHandler where TCellData : UIScrollRectCellData where TCell : UIScrollRectCellItem<TCellData>
    {
        public bool Initialized { get; private set; }

        List<TCellData> Data
        {
            get
            {
                return DataSource == null ? null : DataSource.Data;
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
        RectTransform _scrollRectTransform;
        RectTransform _scrollContentRectTransform;

        public bool UsesVerticalLayout
        {
            get
            {
                return VerticalLayoutGroup != null;
            }
        }

        public bool UsesHorizontalLayout
        {
            get
            {
                return HorizontalLayoutGroup != null;
            }
        }

        public bool UsesGridLayout
        {
            get
            {
                return GridLayoutGroup != null;
            }
        }

        void ApplyLayoutRules()
        {

            if(Pagination != null)
            {
                if(CenterOnCell)
                {
                    Pagination.UseNavigationButtons = UseNavigationButtons;
                    Pagination.UsePaginationButtons = UsePaginationButtons;
                    Pagination.gameObject.SetActive(true);
                }
                else
                {
                    Pagination.gameObject.SetActive(false);
                    Pagination = null;
                }
            }
        }

        #region Unity methods

        void Awake()
        {
            if(ScrollRect == null)
            {
                ScrollRect = GetComponent<ScrollRect>();
            }

            _scrollRectTransform = ScrollRect.GetComponent<RectTransform>();
            _scrollContentRectTransform = ScrollRect.content;

            VerticalLayoutGroup = VerticalLayoutGroup ?? _scrollContentRectTransform.GetComponent<VerticalLayoutGroup>();
            HorizontalLayoutGroup = HorizontalLayoutGroup ?? _scrollContentRectTransform.GetComponent<HorizontalLayoutGroup>();
            GridLayoutGroup = GridLayoutGroup ?? _scrollContentRectTransform.GetComponent<GridLayoutGroup>();

            ScrollRect.vertical = UsesVerticalLayout;
            ScrollRect.horizontal = UsesHorizontalLayout;

            _isHorizontal = ScrollRect.horizontal;
            _isVertical = ScrollRect.vertical;

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
            ScrollRect.onValueChanged.AddListener(OnScrollViewValueChanged);
        }

        void OnDisable()
        {
            ScrollRect.onValueChanged.RemoveListener(OnScrollViewValueChanged);
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
            if(LoadingGroup != null)
            {
                LoadingGroup.SetActive(true);
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
            if(LoadingGroup != null)
            {
                LoadingGroup.SetActive(false);
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

            if(Pagination != null)
            {
                Pagination.Init(Data.Count, _currentIndex, ScrollToPreviousCell, ScrollToNextCell, ScrollToCell);
            }
        }

        public void AddData(TCellData data, bool addAtEnd, bool moveToEnd = false)
        {
            if(!CenterOnCell)
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

                    if(Pagination != null)
                    {
                        Pagination.Reload(Data.Count, _currentIndex);
                    }

                    RefreshVisibleCells(true);
                }
            }
        }

        public void RemoveData(int index)
        {
            if(!CenterOnCell)
            {
                if(IndexIsValid(index))
                {
                    Data.RemoveAt(index);

                    SetDataValues(index);

                    if(Pagination != null)
                    {
                        Pagination.Reload(Data.Count, _currentIndex);
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