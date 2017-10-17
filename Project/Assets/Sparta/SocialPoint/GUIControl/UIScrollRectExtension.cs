using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Dependency;
using UnityEngine.SocialPlatforms;
using System;
using SocialPoint.Pooling;

namespace SocialPoint.GUIControl
{
    [RequireComponent(typeof(ScrollRect))]
    public partial class UIScrollRectExtension<TCellData, TCell> : MonoBehaviour where TCellData : UIScrollRectCellData where TCell : UIScrollRectCellItem<TCellData>
    {
        public delegate List<TCellData> UIScrollRectExtensionGetData();

        [Tooltip("UGUI ScrollRect we will use")]
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

        [SerializeField]
        bool _usePooling;

        [SerializeField]
        UIViewAnimation _scrollAnimation;

        public bool Initialized { get; private set; }

        Dictionary<int, TCell> _visibleCells;
        Range _visibleElementRange;
        List<TCell> _visibleItems = new List<TCell>();
        List<TCellData> _data = new List<TCellData>();

        // TODO setup pooled objects
        protected Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
        GameObject InstantiateCellPrefabIfNeeded(string name)
        {
            GameObject go;
            if(_prefabs.TryGetValue(name, out go))
            {
                return go;
            }

            return null;
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
        
            _defaultStartPadding = StartPadding;

            _visibleCells = new Dictionary<int, TCell>();
        }

        void Start() 
        { 
            Debug.Log("start");
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

            ClearVisibleItems();
            _visibleItems = null;
        }

        void ClearVisibleItems()
        {
            if (_visibleItems != null)
            {
                for (int i = 0; i < _visibleItems.Count; ++i)
                {
                    var item = _visibleItems[i];
                    if(item != null)
                    {
                        item.gameObject.DestroyAnyway();
                    }           
                }

                _visibleItems.Clear();
            }

            _visibleElementRange = new Range(0, 0);
        }
    }
}