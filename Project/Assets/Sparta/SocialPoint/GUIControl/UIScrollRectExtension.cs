using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Base;
using System.Collections;
using System;
using SocialPoint.Dependency;

namespace SocialPoint.GUIControl
{
    // Add require
    public partial class UIScrollRectExtension<TData, TCell> : MonoBehaviour, IInitializable where TCell : UIScrollRectCellExtension<TCell>
    {
        public delegate List<TData> UIScrollRectExtensionGetData();
        UIScrollRectExtensionGetData _getData;

        [SerializeField]
        TCell _cellPrefab;

        [SerializeField]
        ScrollRect _scrollRect;

        RectTransform _scrollRectTransform;
        RectTransform _scrollContentRectTransform;
        RectTransform _scrollViewPortRectTransform;

        [SerializeField]
        VerticalLayoutGroup _verticalLayoutGroup;

        [SerializeField]
        HorizontalLayoutGroup _horizontalLayoutGroup;

        public bool Initialized { get; private set; }

        List<TCell> _visibleItems = new List<TCell>();
        List<TData> _data = new List<TData>();

        #region Unity methods

        protected virtual void Start() 
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

        public void Define(UIScrollRectExtensionGetData dlg)
        {
            _getData = dlg;
        }

        #region IInitializable implementation

        public void Initialize()
        {
            Canvas.ForceUpdateCanvases();

            if(_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }

            _scrollRectTransform = _scrollRect.transform as RectTransform;
            _scrollContentRectTransform = _scrollRect.content as RectTransform;
            _scrollViewPortRectTransform = _scrollRect.viewport as RectTransform;

            StartCoroutine(FetchDataFromServer(OnReceivedData)); 

            Initialized = true;
        }

        #endregion

        // Method to create items when desired and start showing them
        public void ShowCells()
        {
            
        }

        IEnumerator FetchDataFromServer(Action callback)
        {
            // Simulating server delay
            yield return new WaitForSeconds(5f);

            if(callback != null)
            {
                callback();
            }
        }

        void OnReceivedData()
        {
            _data.Clear();

            if(_getData != null)
            {
                _data = _getData();
            }
            else
            {
                throw new UnityException("Get Data delegate not defined");
            }
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