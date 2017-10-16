using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

namespace SocialPoint.GUIControl
{
    public partial class UIScrollRectExtension<TData, TCell> where TCell : UIScrollRectCellItem<TData>
    {
        float _cumulativeSize;
//        float[] _elementSizes;
//        float[] _cumulativeElementSizes;
//        int _cleanCumulativeIndex;
//        public event Action<int, bool> CellVisibilityChange;

//        Range _visibleElementRange;

        float StartPadding
        {
            get
            {
                if(UsesVerticalLayout)
                {
                    return _verticalLayoutGroup.padding.top;
                }
                else if(UsesHorizontalLayout)
                {
                    return _horizontalLayoutGroup.padding.left;
                }
                else
                {
                    return 0f;//_gridLayoutGroup;
                }
            }
        }

        float EndPadding
        {
            get
            {
                if(UsesVerticalLayout)
                {
                    return _verticalLayoutGroup.padding.bottom;
                }
                else if(UsesHorizontalLayout)
                {
                    return _horizontalLayoutGroup.padding.right;
                }
                else
                {
                    return 0f;//_gridLayoutGroup;
                }
            }
        }

        float ScrollViewSize
        {
            get
            {
                if(UsesVerticalLayout)
                {
                    return _scrollRectTransform.rect.height;
                }
                else if(UsesHorizontalLayout)
                {
                    return _scrollRectTransform.rect.width;
                }
                else
                {
                    return 0f;//_gridLayoutGroup;
                }
            }
        }

        float Spacing
        {
            get
            {
                if(UsesVerticalLayout)
                {
                    return _verticalLayoutGroup.spacing;
                }
                else if(UsesHorizontalLayout)
                {
                    return _horizontalLayoutGroup.spacing;
                }
                else
                {
                    return 0f;//_gridLayoutGroup;
                }
            }
        }

        float GetCellSize(RectTransform trans, bool withSpacing = false)
        {
            float size = 0f;
            if(UsesVerticalLayout)
            {
                size = trans.rect.height;
            }
            else if(UsesHorizontalLayout)
            {
                size = trans.rect.width;
            }
            else
            {
                size = 0f;//_gridLayoutGroup;
            }

            size += (withSpacing ? Spacing : 0);
            return size;
        }

        public float ScrollPosition
        {
            get
            {
                return _scrollPosition;
            }
            set
            {
                _scrollPosition = value;
//                if(this.IsEmpty)
//                {
//                    return;
//                }
//                value = Mathf.Clamp(value, 0, GetScrollPositionForIndex(_elementSizes.Length - 1, true));
//                if(_scrollPosition != value)
//                {
//                    _scrollPosition = value;
//                    _requiresRefresh = true;
//                    float relativeScroll = value / this.ScrollableSize;
//
//                    if(IsVertical)
//                    {
//                        _scrollRect.verticalNormalizedPosition = 1 - relativeScroll;
//                    }
//                    else
//                    {
//                        _scrollRect.horizontalNormalizedPosition = relativeScroll;
//                    }
//                }
            }
        }

        void OnScrollViewValueChanged(Vector2 newScrollValue)
        {
            float relativeScroll = UsesVerticalLayout ? 1 - newScrollValue.y : newScrollValue.x;
            _scrollPosition = relativeScroll * ScrollViewSize;
//            _requiresRefresh = true;
        }
            
        void SetInitialVisibleElements()
        {
            _cumulativeSize = StartPadding;

            Debug.Log("initial cumulative size: " + _cumulativeSize);

            int index = 0;
            Debug.Log("total data values: " + _data.Count);
            while(_cumulativeSize < ScrollViewSize && index < _data.Count)
            {
                var size = AddCell(index, true);
                index++;

                _cumulativeSize += size;
                _cumulativeSize += Spacing;

                Debug.Log("initial cumulative size: " + _cumulativeSize);


//                _cleanCumulativeIndex++;
//                _cumulativeElementSizes[_cleanCumulativeIndex] = _elementSizes[_cleanCumulativeIndex];
//                if(_cleanCumulativeIndex > 0)
//                {
//                    _cumulativeElementSizes[_cleanCumulativeIndex] += _cumulativeElementSizes[_cleanCumulativeIndex - 1];
//                } 
            }


//            Range visibleElements = CalculateCurrentVisibleRange();
//            for(int i = 0; i < visibleElements.count; ++i)
//            {
//                AddCell(visibleElements.from + i, true);
//            }
//            _visibleElementRange = visibleElements;
//            UpdatePaddingElements();
        }

        float GetContentPanelSize()
        {
//            float size = StartPadding;
//            for(int i = 0; i < _data.Count; ++i)
//            {
//                size += GetCellSize(i);
//                size += Spacing; // TODO check that last item has no space
//            }
//
//            size += EndPadding;

//            return size;

            return 1024f;
        }

        void SetupContenSize()
        {
            float size = GetContentPanelSize();
            if(UsesVerticalLayout)
            {
                if(_scrollContentRectTransform.rect.height != size)
                {
                    _scrollContentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
                }
            }
            else if(UsesHorizontalLayout)
            {
                if(_scrollContentRectTransform.rect.width != size)
                {
                    _scrollContentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                }
            }
            else if(UsesGridLayout)
            {
                // TODO
            }
        }

//        Range CalculateCurrentVisibleRange()
//        {
//            var startPosition = Math.Max(_scrollPosition - StartPadding, 0);
//
//            var visibleStartPadding = Math.Max(StartPadding - _scrollPosition, 0);
//            var endPosition = startPosition + ScrollViewSize - visibleStartPadding;
//
//            int startIndex = FindIndexOfElementAtPosition(startPosition);
//            int endIndex = FindIndexOfElementAtPosition(endPosition);
//
//            return new Range(startIndex, endIndex - startIndex + 1);
//        }

//        int FindIndexOfElementAtPosition(float position)
//        {
//            return FindIndexOfElementAtPosition(position, 0, _cumulativeElementSizes.Length - 1);
//        }
//
//        private int FindIndexOfElementAtPosition(float position, int startIndex, int endIndex)
//        {
//            if(startIndex >= endIndex)
//            {
//                return startIndex;
//            }
//
//            int midIndex = (startIndex + endIndex) / 2;
//            if(GetCumulativeElementSize(midIndex) >= position)
//            {
//                return FindIndexOfElementAtPosition(position, startIndex, midIndex);
//            }
//            else
//            {
//                return FindIndexOfElementAtPosition(position, midIndex + 1, endIndex);
//            }
//        }
//
//        float GetCumulativeElementSize(int index)
//        {
//            while(_cleanCumulativeIndex < index)
//            {
//                _cleanCumulativeIndex++;
//                _cumulativeElementSizes[_cleanCumulativeIndex] = _elementSizes[_cleanCumulativeIndex];
//                if(_cleanCumulativeIndex > 0)
//                {
//                    _cumulativeElementSizes[_cleanCumulativeIndex] += _cumulativeElementSizes[_cleanCumulativeIndex - 1];
//                } 
//            }
//
//            return _cumulativeElementSizes[index];
//        }

        float AddCell(int index, bool insertAtEnd)
        {
//            UIScrollRectCellExtension<TData> newCell = GetCellForIndexInScrollView(this, index);

            // Get from pool
            var go = GameObject.Instantiate(GetCellPrefab(_data[index].PrefabName));

            var newCell = go.GetComponent<TCell>();

            var trans = newCell.transform;
            trans.SetParent(_scrollContentRectTransform, false);
            trans.localScale = Vector3.one;
            trans.localPosition = Vector3.zero;

//            LayoutElement layoutElement = newCell.GetComponent<LayoutElement>();
//            if(layoutElement == null)
//            {
//                layoutElement = newCell.gameObject.AddComponent<LayoutElement>();
//            }
//
//            if(IsVertical)
//            {
//                layoutElement.preferredHeight = _elementSizes[index];
//                if(index > 0)
//                {
//                    layoutElement.preferredHeight -= Spacing;
//                }
//            }
//            else
//            {
//                layoutElement.preferredWidth = _elementSizes[index];
//                if(index > 0)
//                {
//                    layoutElement.preferredWidth -= Spacing;
//                }
//            }

            _visibleCells.Add(index, newCell);

            Debug.Log("created new cell with index : " + index);
            if(insertAtEnd)
            {
                trans.SetSiblingIndex(_scrollContentRectTransform.childCount - 1); //One before end padding
            }
            else
            {
                trans.SetSiblingIndex(0); //One after the start padding
            }

//            if(CellVisibilityChange != null)
//            {
//                CellVisibilityChange(index, true);
//            }

//            var layoutElement = newCell.GetComponent<LayoutElement>();
            return GetCellSize(trans as RectTransform);
        }

        TCell GetCellForIndexInScrollView(UIScrollRectExtension<TData, TCell> scrollView, int index)
        {
//            TCell cell = tableView.GetReusableCell(_cellPrefab.ReuseIdentifier) as T;
//            if(cell == null)
//            {
            TCell cell = GameObject.Instantiate(_cellPrefab) as TCell;
//            cell.name = _cellPrefab.name + (++_numInstancesCreated).ToString();
//            }
//            cell.UpdateData(index, _data[index]);
            return cell;
        }
    }
}