using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Base;
using System.Collections;
using System;
using SocialPoint.Dependency;

namespace SocialPoint.GUIControl
{
    [RequireComponent(typeof(ScrollRect))]
    public partial class UIScrollRectExtension<TData, TCell> : MonoBehaviour, IInitializable where TCell : UIScrollRectCellItem<TData>
    {
        public delegate List<TData> UIScrollRectExtensionGetData();

//        [Tooltip("Prefab used as cell base")]
//        [SerializeField]
//        GameObject _cellPrefab;

        [Tooltip("UGUI ScrollRect we will use")]
        [SerializeField]
        ScrollRect _scrollRect;

        LayoutGroup _layoutGroup;

        RectTransform _scrollRectTransform;
        RectTransform _scrollContentRectTransform;
//        RectTransform _scrollViewPortRectTransform;

        [SerializeField]
        VerticalLayoutGroup _verticalLayoutGroup;

        [SerializeField]
        HorizontalLayoutGroup _horizontalLayoutGroup;

        [SerializeField]
        GridLayoutGroup _gridLayoutGroup;
//        ContentSizeFitter _contentSizeFitter;

        public bool Initialized { get; private set; }

//        int _maxVisibleItems = 0;
        private Dictionary<int, TCell> _visibleCells;
        List<TCell> _visibleItems = new List<TCell>();
        List<TData> _data = new List<TData>();

        // TODO setup pooled objects
        protected Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
        GameObject GetCellPrefab(string name)
        {
            GameObject go;
            if(_prefabs.TryGetValue(name, out go))
            {
                return go;
            }

            return null;
        }

        float _scrollPosition;

//        public LayoutGroup Layout
//        {
//            get
//            {
//                if(UsesVerticalLayout)
//                {
//                    return _verticalLayoutGroup;
//                }
//                else if(UsesHorizontalLayout)
//                {
//                    return _horizontalLayoutGroup;
//                }
//                else if(UsesGridLayout)
//                {
//                    return _gridLayoutGroup;
//                }
//                else
//                {
//                    throw new UnityException("Layout not defined");
//                }
//            }
//        }

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

        protected void Awake()
        {
            if(_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }

            _scrollRectTransform = _scrollRect.transform as RectTransform;
            _scrollContentRectTransform = _scrollRect.content;
//            _scrollViewPortRectTransform = _scrollRect.viewport;

            _verticalLayoutGroup = _verticalLayoutGroup ?? _scrollContentRectTransform.GetComponent<VerticalLayoutGroup>();
            _horizontalLayoutGroup = _horizontalLayoutGroup ?? _scrollContentRectTransform.GetComponent<HorizontalLayoutGroup>();
            _gridLayoutGroup = _gridLayoutGroup ?? _scrollContentRectTransform.GetComponent<GridLayoutGroup>();
//            _contentSizeFitter = _scrollContentRectTransform.GetComponent<ContentSizeFitter>();
        
            _scrollRect.vertical = UsesVerticalLayout;
            _scrollRect.horizontal = UsesHorizontalLayout;
        
            _visibleCells = new Dictionary<int, TCell>();
        }

        void Start() 
        { 
            Initialize(); 
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
//            if(_cellPrefab == null)
//            {
//                throw new UnityException("Cell prefab is not set");
//            }
//            Canvas.ForceUpdateCanvases();

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

            SetupContenSize();
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
        }
    }
}