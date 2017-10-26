using System;
using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

namespace SocialPoint.GUIControl
{
    /// <summary>
    /// A reusable table
    /// </summary>
    public abstract class UITableBaseController<T> : UIViewController
    {

        #region Public API

        [SerializeField]
        ScrollRect _scrollRect;
        [SerializeField]
        VerticalLayoutGroup _verticalLayoutGroup;
        [SerializeField]
        HorizontalLayoutGroup _horizontalLayoutGroup;

        /// <summary>
        /// Get the number of elements that a certain table should display
        /// </summary>
        public abstract int GetNumberOfElementsForTableView(UITableBaseController<T> tableView);

        /// <summary>
        /// Get the size of a element of a certain cell in the table view
        /// </summary>
        public abstract Vector2 GetSizeForElement(UITableBaseController<T> tableView, int index);

        /// <summary>
        /// Create a cell for a certain element in a table view.
        /// Callers should use tableView.GetReusableCell to cache objects
        /// </summary>
        public abstract UITableBaseCellController<T> GetCellForIndexInTableView(UITableBaseController<T> tableView, int index);

        /// <summary>
        /// This event will be called when a cell's visibility changes
        /// First param (int) is the element index, second param (bool) is whether or not it is visible
        /// </summary>
        public event Action<int, bool> CellVisibilityChange;

        public float GetSizeForElementInTableView(UITableBaseController<T> tableView, int index)
        {
            return IsVertical ? GetSizeForElement(tableView, index).y : GetSizeForElement(tableView, index).x;
        }

        /// <summary>
        /// Get a cell that is no longer in use for reusing
        /// </summary>
        /// <param name="reuseIdentifier">The identifier for the cell type</param>
        /// <returns>A prepared cell if available, null if none</returns>
        public UITableBaseCellController<T> GetReusableCell(string reuseIdentifier)
        {
            LinkedList<UITableBaseCellController<T>> cells;
            if(!_reusableCells.TryGetValue(reuseIdentifier, out cells))
            {
                return null;
            }
            if(cells.Count == 0)
            {
                return null;
            }
            UITableBaseCellController<T> cell = cells.First.Value;
            cells.RemoveFirst();
            return cell;
        }

        public bool IsEmpty { get; private set; }

        /// <summary>
        /// Reload the table view. Manually call this if the data source changed in a way that alters the basic layout
        /// (number of elements changed, etc)
        /// </summary>
        public void ReloadData()
        {
            Profiler.BeginSample("UITableBaseController.ReloadData for dataSource:" + GetType().Name, this);
            _elementSizes = new float[GetNumberOfElementsForTableView(this)];
            this.IsEmpty = _elementSizes.Length == 0;
            ClearAllElements();
            if(this.IsEmpty)
            {
                return;
            }
            _cumulativeElementSizes = new float[_elementSizes.Length];
            _cleanCumulativeIndex = -1;

            for(int i = 0; i < _elementSizes.Length; i++)
            {
                float elementSize = GetSizeForElementInTableView(this, i);

                if(i > 0)
                {
                    elementSize += Spacing;
                }

                _elementSizes[i] = elementSize;
            }

            if(IsVertical)
            {
                _scrollRect.content.sizeDelta = new Vector2(_scrollRect.content.sizeDelta.x, 
                    GetCumulativeElementSize(_elementSizes.Length - 1) + _verticalLayoutGroup.padding.vertical);
            }
            else
            {
                _scrollRect.content.sizeDelta = new Vector2(GetCumulativeElementSize(_elementSizes.Length - 1) + _horizontalLayoutGroup.padding.horizontal, 
                    _scrollRect.content.sizeDelta.y);
            }


            RecalculateVisibleElementsFromScratch();
            _requiresReload = false;

            Profiler.EndSample();

        }

        /// <summary>
        /// Get cell at a specific element (if active). Returns null if not.
        /// </summary>
        public UITableBaseCellController<T> GetCellAtIndex(int index)
        {
            UITableBaseCellController<T> retVal = null;
            _visibleCells.TryGetValue(index, out retVal);
            return retVal;
        }

        /// <summary>
        /// Cells currently displayed
        /// </summary>
        /// <returns>Enumerator with the visible cells</returns>
        protected Dictionary<int, UITableBaseCellController<T>>.Enumerator VisibleCellsWithIndex
        {
            get
            {
                return _visibleCells.GetEnumerator();
            }
        }

        /// <summary>
        /// Get the range of the currently visible elements
        /// </summary>
        public Range VisibleElementRange
        {
            get { return _visibleElementRange; }
        }

        /// <summary>
        /// Determines if a cell at certain index is visible or not
        /// </summary>
        /// <param name="index">The desired element index</param>
        /// <returns>True if the cell is visible</returns>
        public bool IsCellAtIndexVisible(int index)
        {
            return _visibleCells.ContainsKey(index);
        }

        /// <summary>
        /// Notify the table view that one of its elements changed size
        /// </summary>
        public void NotifyCellDimensionsChanged(int index)
        {
            
            float oldSize = _elementSizes[index];
            _elementSizes[index] = GetSizeForElementInTableView(this, index);
            if(index > 0)
            {
                _elementSizes[index] += Spacing;
            }
            _cleanCumulativeIndex = Mathf.Min(_cleanCumulativeIndex, index - 1);
            UITableBaseCellController<T> cell = GetCellAtIndex(index);

            LayoutElement element = cell.GetComponent<LayoutElement>();

            if(cell != null)
            {
                if(IsVertical)
                {
                    element.preferredHeight = _elementSizes[index];
                    if(index > 0)
                    {
                        element.preferredHeight -= Spacing;
                    }
                }
                else
                {
                    element.preferredWidth = _elementSizes[index];
                    if(index > 0)
                    {
                        element.preferredWidth -= Spacing;
                    }
                }
            }

            float sizeDelta = _elementSizes[index] - oldSize;

            if(IsVertical)
            {
                _scrollRect.content.sizeDelta = new Vector2(_scrollRect.content.sizeDelta.x, _scrollRect.content.sizeDelta.y + sizeDelta);
            }
            else
            {
                _scrollRect.content.sizeDelta = new Vector2(_scrollRect.content.sizeDelta.x + sizeDelta, _scrollRect.content.sizeDelta.y);
            }

            _requiresRefresh = true;
        }

        /// <summary>
        /// Get the maximum scrollable size of the table. scrollPosition property will never be more than this.
        /// </summary>
        public float ScrollableSize
        {
            get
            {
                return IsVertical ? _scrollRect.content.rect.height - (this.transform as RectTransform).rect.height : _scrollRect.content.rect.width - (this.transform as RectTransform).rect.width;
            }
        }

        /// <summary>
        /// Get or set the current scrolling position of the table
        /// </summary>
        public float ScrollPosition
        {
            get
            {
                return _scrollPosition;
            }
            set
            {
                if(this.IsEmpty)
                {
                    return;
                }
                value = Mathf.Clamp(value, 0, GetScrollPositionForIndex(_elementSizes.Length - 1, true));
                if(_scrollPosition != value)
                {
                    _scrollPosition = value;
                    _requiresRefresh = true;
                    float relativeScroll = value / this.ScrollableSize;

                    if(IsVertical)
                    {
                        _scrollRect.verticalNormalizedPosition = 1 - relativeScroll;
                    }
                    else
                    {
                        _scrollRect.horizontalNormalizedPosition = relativeScroll;
                    }
                }
            }
        }

        /// <summary>
        /// Get the position that the table would need to scroll to to have a certain element at the start
        /// </summary>
        /// <param name="index">The desired element index</param>
        /// <param name="above">Should the start of the table be above the element or below the element?</param>
        /// <returns>The position to scroll to, can be used with scrollPosition property</returns>
        public float GetScrollPositionForIndex(int index, bool above)
        {
            float retVal = GetCumulativeElementSize(index);
            retVal += StartPadding;
            if(above)
            {
                retVal -= _elementSizes[index];
            }
            return retVal;
        }

        #endregion

        #region Private implementation

        protected bool _requiresReload;

        private LayoutElement _startContentPlaceHolder;
        private LayoutElement _endContentPlaceholder;

        private float[] _elementSizes;
        private float[] _cumulativeElementSizes;
        private int _cleanCumulativeIndex;

        private Dictionary<int, UITableBaseCellController<T>> _visibleCells;
        private Range _visibleElementRange;

        private RectTransform _reusableCellContainer;
        private Dictionary<string, LinkedList<UITableBaseCellController<T>>> _reusableCells;

        private float _scrollPosition;

        private bool _requiresRefresh;

        private void ScrollViewValueChanged(Vector2 newScrollValue)
        {
            float relativeScroll = IsVertical ? 1 - newScrollValue.y : newScrollValue.x;
            _scrollPosition = relativeScroll * ScrollableSize;
            _requiresRefresh = true;
        }

        private void RecalculateVisibleElementsFromScratch()
        {
            ClearAllElements();
            SetInitialVisibleElements();
        }

        private void ClearAllElements()
        {
            while(_visibleCells.Count > 0)
            {
                HideElement(false);
            }
            _visibleElementRange = new Range(0, 0);
        }

        override protected void OnAwake()
        {
            base.OnAwake();

            IsEmpty = true;

            _verticalLayoutGroup = _verticalLayoutGroup ?? GetComponentInChildren<VerticalLayoutGroup>();
            _horizontalLayoutGroup = _horizontalLayoutGroup ?? GetComponentInChildren<HorizontalLayoutGroup>();

            DebugUtils.Assert(_verticalLayoutGroup != null || _horizontalLayoutGroup != null, "Vertical or Horizontal layout not found");

            _scrollRect.vertical = IsVertical;
            _scrollRect.horizontal = !IsVertical;

            _startContentPlaceHolder = CreateEmptyContentPlaceHolderElement("TopContentPlaceHolder");
            _startContentPlaceHolder.transform.SetParent(_scrollRect.content, false);
            _endContentPlaceholder = CreateEmptyContentPlaceHolderElement("BottomContentPlaceHolder");
            _endContentPlaceholder.transform.SetParent(_scrollRect.content, false);
            _visibleCells = new Dictionary<int, UITableBaseCellController<T>>();

            _reusableCellContainer = new GameObject("ReusableCells", typeof(RectTransform)).GetComponent<RectTransform>();
            _reusableCellContainer.SetParent(this.transform, false);
            _reusableCellContainer.gameObject.SetActive(false);
            _reusableCells = new Dictionary<string, LinkedList<UITableBaseCellController<T>>>();
        }

        void Update()
        {
            if(_requiresReload)
            {
                ReloadData();
            }
        }

        void LateUpdate()
        {
            if(_requiresRefresh)
            {
                RefreshVisibleElements();
            }
        }

        void OnEnable()
        {
            _scrollRect.onValueChanged.AddListener(ScrollViewValueChanged);
        }

        void OnDisable()
        {
            _scrollRect.onValueChanged.RemoveListener(ScrollViewValueChanged);
        }

        private bool IsVertical
        {
            get
            {
                return _verticalLayoutGroup != null;
            }
        }

        private float StartPadding
        {
            get
            {
                return IsVertical ? _verticalLayoutGroup.padding.top : _horizontalLayoutGroup.padding.left;
            }
        }

        private float EndPadding
        {
            get
            {
                return IsVertical ? _verticalLayoutGroup.padding.bottom : _horizontalLayoutGroup.padding.right;
            }
        }

        private float TableSize
        {
            get
            {
                return IsVertical ? (this.transform as RectTransform).rect.height : (this.transform as RectTransform).rect.width; 
            }
        }

        private float Spacing
        {
            get
            {
                return IsVertical ? _verticalLayoutGroup.spacing : _horizontalLayoutGroup.spacing;
            }
        }

        private Range CalculateCurrentVisibleRange()
        {
            float startPosition = Math.Max(_scrollPosition - StartPadding, 0);

            var visibleStartPadding = Math.Max(StartPadding - _scrollPosition, 0);
            float endPosition = startPosition + TableSize - visibleStartPadding;

            int startIndex = FindIndexOfElementAtPosition(startPosition);
            int endIndex = FindIndexOfElementAtPosition(endPosition);

            return new Range(startIndex, endIndex - startIndex + 1);
        }

        private void SetInitialVisibleElements()
        {
            Range visibleElements = CalculateCurrentVisibleRange();
            for(int i = 0; i < visibleElements.count; i++)
            {
                AddElement(visibleElements.from + i, true);
            }
            _visibleElementRange = visibleElements;
            UpdatePaddingElements();
        }

        private void AddElement(int index, bool atEnd)
        {
            UITableBaseCellController<T> newCell = GetCellForIndexInTableView(this, index);
            newCell.transform.SetParent(_scrollRect.content, false);

            LayoutElement layoutElement = newCell.GetComponent<LayoutElement>();
            if(layoutElement == null)
            {
                layoutElement = newCell.gameObject.AddComponent<LayoutElement>();
            }

            if(IsVertical)
            {
                layoutElement.preferredHeight = _elementSizes[index];
                if(index > 0)
                {
                    layoutElement.preferredHeight -= Spacing;
                }
            }
            else
            {
                layoutElement.preferredWidth = _elementSizes[index];
                if(index > 0)
                {
                    layoutElement.preferredWidth -= Spacing;
                }
            }
            
            _visibleCells[index] = newCell;
            if(atEnd)
            {
                newCell.transform.SetSiblingIndex(_scrollRect.content.childCount - 2); //One before end padding
            }
            else
            {
                newCell.transform.SetSiblingIndex(1); //One after the start padding
            }

            if(CellVisibilityChange != null)
            {
                CellVisibilityChange(index, true);
            }
        }

        private void RefreshVisibleElements()
        {
            _requiresRefresh = false;

            if(this.IsEmpty)
            {
                return;
            }

            Range newVisibleElements = CalculateCurrentVisibleRange();
            int oldTo = _visibleElementRange.Last();
            int newTo = newVisibleElements.Last();

            if(newVisibleElements.from > oldTo || newTo < _visibleElementRange.from)
            {
                //We jumped to a completely different segment this frame, destroy all and recreate
                RecalculateVisibleElementsFromScratch();
                return;
            }

            //Remove elements that disappeared to the start
            for(int i = _visibleElementRange.from; i < newVisibleElements.from; i++)
            {
                HideElement(false);
            }
            //Remove elements that disappeared to the end
            for(int i = newTo; i < oldTo; i++)
            {
                HideElement(true);
            }
            //Add elements that appeared on start
            for(int i = _visibleElementRange.from - 1; i >= newVisibleElements.from; i--)
            {
                AddElement(i, false);
            }
            //Add elements that appeared on end
            for(int i = oldTo + 1; i <= newTo; i++)
            {
                AddElement(i, true);
            }
            _visibleElementRange = newVisibleElements;
            UpdatePaddingElements();
        }

        private void UpdatePaddingElements()
        {
            float hiddenElementsSizeSum = 0;
            
            for(int i = 0; i < _visibleElementRange.from; i++)
            {
                hiddenElementsSizeSum += _elementSizes[i];
            }
            var startContentPlaceHolderSize = hiddenElementsSizeSum;

            if(IsVertical)
            {
                _startContentPlaceHolder.preferredHeight = startContentPlaceHolderSize;
            }
            else
            {
                _startContentPlaceHolder.preferredWidth = startContentPlaceHolderSize;
            }

            bool isPreferredSize = IsVertical ? _startContentPlaceHolder.preferredHeight > 0 : _startContentPlaceHolder.preferredWidth > 0;
            _startContentPlaceHolder.gameObject.SetActive(isPreferredSize);

            for(int i = _visibleElementRange.from; i <= _visibleElementRange.Last(); i++)
            {
                hiddenElementsSizeSum += _elementSizes[i];
            }

            float scrollRectContentSize = IsVertical ? _scrollRect.content.rect.height : _scrollRect.content.rect.width;

            float endContentPlaceHolderSize = scrollRectContentSize - hiddenElementsSizeSum;
            endContentPlaceHolderSize -= StartPadding;
            endContentPlaceHolderSize -= EndPadding;

            if(IsVertical)
            {
                _endContentPlaceholder.preferredHeight = endContentPlaceHolderSize - Spacing;
            }
            else
            {
                _endContentPlaceholder.preferredWidth = endContentPlaceHolderSize - Spacing;
            }
                
            isPreferredSize = IsVertical ? _endContentPlaceholder.preferredHeight > 0 : _endContentPlaceholder.preferredWidth > 0;
            _endContentPlaceholder.gameObject.SetActive(isPreferredSize);
        }

        private void HideElement(bool last)
        {
            int element = last ? _visibleElementRange.Last() : _visibleElementRange.from;
            UITableBaseCellController<T> removedCell = _visibleCells[element];
            StoreCellForReuse(removedCell);
            _visibleCells.Remove(element);
            _visibleElementRange.count -= 1;
            if(!last)
            {
                _visibleElementRange.from += 1;
            }

            if(CellVisibilityChange != null)
            {
                CellVisibilityChange(element, false);
            }
        }

        private LayoutElement CreateEmptyContentPlaceHolderElement(string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            LayoutElement le = go.GetComponent<LayoutElement>();
            return le;
        }

        private int FindIndexOfElementAtPosition(float position)
        {
            return FindIndexOfElementAtPosition(position, 0, _cumulativeElementSizes.Length - 1);
        }

        private int FindIndexOfElementAtPosition(float position, int startIndex, int endIndex)
        {
            if(startIndex >= endIndex)
            {
                return startIndex;
            }
            int midIndex = (startIndex + endIndex) / 2;
            if(GetCumulativeElementSize(midIndex) >= position)
            {
                return FindIndexOfElementAtPosition(position, startIndex, midIndex);
            }
            else
            {
                return FindIndexOfElementAtPosition(position, midIndex + 1, endIndex);
            }
        }

        private float GetCumulativeElementSize(int index)
        {
            while(_cleanCumulativeIndex < index)
            {
                _cleanCumulativeIndex++;
                _cumulativeElementSizes[_cleanCumulativeIndex] = _elementSizes[_cleanCumulativeIndex];
                if(_cleanCumulativeIndex > 0)
                {
                    _cumulativeElementSizes[_cleanCumulativeIndex] += _cumulativeElementSizes[_cleanCumulativeIndex - 1];
                } 
            }
            return _cumulativeElementSizes[index];
        }

        private void StoreCellForReuse(UITableBaseCellController<T> cell)
        {
            string reuseIdentifier = cell.ReuseIdentifier;
            
            if(string.IsNullOrEmpty(reuseIdentifier))
            {
                GameObject.Destroy(cell.gameObject);
                return;
            }

            if(!_reusableCells.ContainsKey(reuseIdentifier))
            {
                _reusableCells.Add(reuseIdentifier, new LinkedList<UITableBaseCellController<T>>());
            }
            _reusableCells[reuseIdentifier].AddLast(cell);
            cell.transform.SetParent(_reusableCellContainer, false);
        }

        #endregion
    }
}
