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
    public partial class UIScrollRectExtension<TCellData, TCell> : UIViewController, IDragHandler where TCellData : UIScrollRectCellData where TCell : UIScrollRectCellItem<TCellData>
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

        [Header("Animations")]
        [SerializeField]
        UIViewAnimation _scrollAnimation;

        [SerializeField]
        float _scrollAnimationDuration = 0.5f;

        [SerializeField]
        bool _disableDragWhileScrollAnimation;

        [Header("Visualization")]
        [SerializeField]
        bool _usePooling;

        [Tooltip("Threshold that we will add to bounds to check if we need to show/hide new cells")]
        [SerializeField]
        int _boundsDelta;

        public bool Initialized { get; private set; }

        Range _visibleElementRange;
        Dictionary<int, TCell> _visibleCells;
        List<TCellData> _data = new List<TCellData>();
        Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();

        IEnumerator _smoothScrollCoroutine;
        int _defaultStartPadding;
        bool _requiresRefresh;

        bool _isHorizontal;
        bool _isVertical;
            
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

        protected virtual void OnDestroy() 
        { 
            Dispose(); 
        }
            
        public void OnDrag(PointerEventData eventData)
        {
            MyOnDrag(eventData);
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

            SetupCellSizes();
            SetupRectTransformSize(_scrollContentRectTransform, GetContentPanelSize());
            SetInitialVisibleElements();
        }

        void Dispose()
        {
            Initialized = false;
            ClearAllVisibleCells();
        }
    }
}