using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Dependency;
using UnityEngine.SocialPlatforms;
using System;
using SocialPoint.Pooling;
using System.Collections;
using UnityEngine.EventSystems;

namespace SocialPoint.GUIControl
{
    [RequireComponent(typeof(ScrollRect))]
    public partial class UIScrollRectExtension<TCellData, TCell> : UIViewController, IBeginDragHandler, IDragHandler, IEndDragHandler where TCellData : UIScrollRectCellData where TCell : UIScrollRectCellItem<TCellData>
    {
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

        [Header("System")]
        [SerializeField]
        bool _usePooling;

        [Tooltip("Delta that we will add to bounds to check if we need to show/hide new cells")]
        [SerializeField]
        int _boundsDelta;

        [SerializeField]
        int _initialIndex;

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

        public bool Initialized { get; private set; }

        Range _visibleElementRange;
        Dictionary<int, TCell> _visibleCells;
        List<TCellData> _data = new List<TCellData>();
        Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();

        IEnumerator _smoothScrollCoroutine;
        int _defaultStartPadding;
        bool _requiresRefresh;
        float _initialScrollPosition;
        float _initialPadding;
        bool _isHorizontal;
        bool _isVertical;
        float _startScrollingPosition;
        int _centeredIndex;
            
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
        
            _defaultStartPadding = StartPadding;

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

//        void OnDrawGizmosSelected()
//        {
//            Gizmos.color = Color.red;
//
//            var trans = transform;
//            var rectTrans = trans as RectTransform;
//
//            Gizmos.DrawLine(new Vector3(trans.position.x + (rectTrans.rect.xMax * 0.5f), transform.position.y, 0f), new Vector3(trans.position.x + (rectTrans.rect.width * 0.5f), transform.position.y + rectTrans.rect.height, 0f));
//        }
//
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

        public void FetchData(UIScrollRectExtensionGetData dlg)
        {
            // show loading spinner

            _data.Clear();

            if(dlg != null)
            {
                _data = dlg();
            }
            else
            {
                throw new UnityException("Get Data delegate not defined");
            }

            // hide loading spinner

            if(_data.Count == 0)
            {
                throw new UnityException("Data not loaded!");
            }

            SetupCellSizes();
            SetupRectTransformSize(_scrollContentRectTransform, GetContentPanelSize());
            SetInitialPosition();
            SetInitialVisibleElements();
        }

        void Dispose()
        {
            Initialized = false;
            ClearAllVisibleCells();
        }
    }
}