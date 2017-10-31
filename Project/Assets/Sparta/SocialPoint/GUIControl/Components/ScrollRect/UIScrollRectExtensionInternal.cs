using UnityEngine;
using System;
using UnityEngine.SocialPlatforms;
using SocialPoint.Pooling;
using SocialPoint.Base;
using UnityEngine.Profiling;
using System.Collections;
using UnityEngine.EventSystems;

namespace SocialPoint.GUIControl
{
    public partial class UIScrollRectExtension<TCellData, TCell> where TCellData : UIScrollRectCellData where TCell : UIScrollRectCellItem<TCellData>
    {
        float ScrollPosition
        {
            get
            {
                if(UsesVerticalLayout)
                {
                    return Math.Abs(_scrollContentRectTransform.anchoredPosition.y);
                }
                else if(UsesHorizontalLayout)
                {
                    return Math.Abs(_scrollContentRectTransform.anchoredPosition.x);
                }
                else
                {
                    return 0;//_gridLayoutGroup;
                }
            }
            set
            {
                _scrollContentRectTransform.anchoredPosition = GetFinalScrollPosition(value);
            }
        }

        int StartPadding
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
                    return 0;//_gridLayoutGroup;
                }
            }
            set
            {
                if(UsesVerticalLayout)
                {
                    _verticalLayoutGroup.padding.top = value;
                }
                else if(UsesHorizontalLayout)
                {
                    _horizontalLayoutGroup.padding.left = value;
                }
                else
                {
//                    return 0f;//_gridLayoutGroup;
                }
            }
        }

        int EndPadding
        {
            get
            {
                if(_centerOnCell && _data.Count > 0)
                {
                    return (int)((ScrollViewSize * 0.5f) + (GetCellSize(_data.Count - 1) * 0.5f));
                }
                else
                {
                    int padding = 0;
                    if(UsesVerticalLayout)
                    {
                        padding = _verticalLayoutGroup.padding.bottom;
                    }
                    else if(UsesHorizontalLayout)
                    {
                        padding = _horizontalLayoutGroup.padding.right;
                    }
                    else
                    {
                        padding = 0;//_gridLayoutGroup;
                    }

                    if(_data.Count > 0 && _showLastCellPosition == ShowLastCellPosition.AtStart)
                    {
                        padding += (int)(ScrollViewSize);
                        padding -= (int)GetCellSize(_data.Count - 1);
                    }

                    return padding;
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

        float ScrollViewContentSize
        {
            get
            {
                if(UsesVerticalLayout)
                {
                    return _scrollContentRectTransform.rect.height;
                }
                else if(UsesHorizontalLayout)
                {
                    return _scrollContentRectTransform.rect.width;
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

        float GetCellAccumulatedSize(int index)
        {
            if(index < 0)
            {
                return 0f;
            }

            if(UsesVerticalLayout)
            {
                return _data[index].AccumulatedSize.y;
            }
            else if(UsesHorizontalLayout)
            {
                return _data[index].AccumulatedSize.x;
            }
            else
            {
                return 0f;//_gridLayoutGroup;
            }
        }

        float GetCellSize(int index)
        {
            if(UsesVerticalLayout)
            {
                return _data[index].Size.y;
            }
            else if(UsesHorizontalLayout)
            {
                return _data[index].Size.x;
            }
            else
            {
                return 0f;//_gridLayoutGroup;
            }
        }
            
        void OnScrollViewValueChanged(Vector2 newScrollValue)
        {
            _requiresRefresh = true;
        }      
            
        Range CalculateCurrentVisibleRange()
        {
            Profiler.BeginSample("UIScrollRectExtension.CalculateCurrentVisibleRange", this);

            float startPosition = ScrollPosition - _boundsDelta;
            float endPosition = ScrollPosition + ScrollViewSize + _boundsDelta;

            int startIndex = FindIndexOfElementAtPosition(startPosition, 0, _data.Count - 1);
            int endIndex = FindIndexOfElementAtPosition(endPosition, 0, _data.Count - 1);

            Profiler.EndSample();
            return new Range(startIndex, endIndex - startIndex + 1);
        }
            
        int FindIndexOfElementAtPosition(float position)
        {
            return FindIndexOfElementAtPosition(position, _visibleElementRange.from, _visibleElementRange.RelativeCount());
        }

        int FindIndexOfElementAtPosition(float position, int startIndex, int endIndex)
        {
            if(startIndex >= endIndex)
            {
                return startIndex;
            }

            int midIndex = (startIndex + endIndex) / 2;
            if(GetCellAccumulatedSize(midIndex) > position)
            {
                return FindIndexOfElementAtPosition(position, startIndex, midIndex);
            }
            else
            {
                return FindIndexOfElementAtPosition(position, midIndex + 1, endIndex);
            }
        }
            
        bool IndexIsValid(int index)
        {
            return (index >= 0 && index < _data.Count);
        }
            
        void SetInitialPosition()
        {
            ScrollPosition = 0f;

            if(!IndexIsValid(_initialIndex))
            {
                _initialIndex = 0;
            }

            _currentIndex = _initialIndex;
                
            ScrollPosition = GetCellAccumulatedSize(_currentIndex - 1);
        }

        void SetInitialVisibleElements()
        {
            Range visibleElements = CalculateCurrentVisibleRange();
            for(int i = visibleElements.from; i < visibleElements.RelativeCount(); ++i)
            {
                ShowCell(i, true);
            }
                
            _visibleElementRange = visibleElements;
            UpdatePaddingElements();
            UpdateScrollState();
        }

        void SetDataValues(int beginIndex = 0)
        {
            Profiler.BeginSample("UIScrollRectExtension.SetupCellSizes", this);

            var acumulatedWidth = 0f;
            var acumulatedHeight = 0f;

            if(beginIndex == 0)
            {
                if(UsesVerticalLayout)
                {
                    acumulatedHeight += _defaultStartPadding;
                }
                else if(UsesHorizontalLayout)
                {
                    acumulatedWidth += _defaultStartPadding;
                }
            }
            else
            {
                if(UsesVerticalLayout)
                {
                    acumulatedHeight += GetCellAccumulatedSize(beginIndex - 1);
                }
                else if(UsesHorizontalLayout)
                {
                    acumulatedWidth += GetCellAccumulatedSize(beginIndex - 1);
                }
            }

            for(int i = beginIndex; i < _data.Count; ++i)
            {
                var dataValue = _data[i];

                var prefab = _prefabs[dataValue.PrefabIndex];
                if(prefab != null)
                {
                    var trans = prefab.transform as RectTransform;
                    dataValue.Size = NewVector2(trans.rect.width, trans.rect.height);

                    if(UsesVerticalLayout)
                    {
                        acumulatedHeight += trans.rect.height;
                        if(i < _data.Count - 1)
                        {
                            acumulatedHeight += Spacing;
                        }
                    }
                    else if(UsesHorizontalLayout)
                    {
                        acumulatedWidth += trans.rect.width;
                        if(i < _data.Count - 1)
                        {
                            acumulatedWidth += Spacing;
                        }
                    }

                    dataValue.AccumulatedSize = NewVector2(acumulatedWidth, acumulatedHeight);
                }
            }

            Profiler.EndSample();
        }

        float GetContentPanelSize()
        {
            float size = _defaultStartPadding;
            for(int i = 0; i < _data.Count; ++i)
            {
                size += GetCellSize(i);

                if(i < _data.Count - 1)
                {
                    size += Spacing;
                }
            }

            size += EndPadding;

            return size;
        }

        void SetInitialPadding()
        {
            _defaultStartPadding = StartPadding;
        }

        Vector2 NewVector2(float x, float y)
        {
            _tempVector2 = Vector3.zero;
            _tempVector2.x = x;
            _tempVector2.y = y;

            return _tempVector2;
        }

        GameObject GetCellPrefab(GameObject prefab)
        {
            if(_usePooling)
            {
                UnityObjectPool.CreatePool(prefab, 1);
            }

            return prefab;
        }

        void SetRectTransformSize(RectTransform trans, float size, bool disableIfZero = false)
        {
            if(UsesVerticalLayout)
            {
                if(_scrollContentRectTransform.rect.height != size)
                {
                    trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
                }
                    
                if(disableIfZero)
                {
                    _scrollContentRectTransform.gameObject.SetActive(_scrollContentRectTransform.rect.height > 0);
                }
            }
            else if(UsesHorizontalLayout)
            {
                if(_scrollContentRectTransform.rect.width != size)
                {
                    trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                }

                if(disableIfZero)
                {
                    _scrollContentRectTransform.gameObject.SetActive(_scrollContentRectTransform.rect.width > 0);
                }
            }
            else if(UsesGridLayout)
            {
                // TODO
            }
        }
 
        void UpdatePaddingElements()
        {
            int currentIndex = _currentIndex;
            float padding = 0f;
            if(_visibleElementRange.from == 0)
            {
                if(currentIndex == 0)
                {
                    padding += _defaultStartPadding;
                }
            }
            else
            {
                padding += GetCellAccumulatedSize(_visibleElementRange.from - 1);
            }

            if(_centerOnCell)
            {
                padding += (ScrollViewSize * 0.5f);
                padding -= (GetCellSize(_currentIndex) * 0.5f);
            }

            StartPadding = (int)padding;
        }
            
        public void ScrollToPreviousCell()
        {
            ScrollToCell(_currentIndex - 1);
        }

        public void ScrollToCurrentCell()
        {
            ScrollToCell(_currentIndex);
        }

        public void ScrollToNextCell()
        {
            ScrollToCell(_currentIndex + 1);
        }

        public void ScrollToSelectedCell(int index)
        {
            ScrollToCell(index);
        }

        void ScrollToClosestCell(ScrollDirection scrollDirection)
        {
            // Setup the position to the middle of the scrollview to check the desired cell to center
//            float closestPosition = ScrollPosition;
//            closestPosition += ScrollViewSize * 0.5f;
//
//            int closestIndex = FindIndexOfElementAtPosition(closestPosition);
//            if(closestIndex == _currentIndex)
//            {
//                closestIndex += (int)scrollDirection;
//            }
//
            _currentIndex += (int)scrollDirection;
//            ScrollToCell(closestIndex);
            ScrollToCurrentCell();
        }

        public void ScrollToStartPosition()
        {
            ScrollToCell(0);
        }

        public void ScrollToFinalPosition()
        {
            if(_data.Count > 0)
            {
                int index = _data.Count - 1;

                if(_centerOnCell)
                {
                    if(_pagination != null)
                    {
                        _pagination.SetSelectedButton(index);
                    }

                    ScrollToCell(index);
                }
                else
                {
                    if(_showLastCellPosition == ShowLastCellPosition.AtStart)
                    {
                        ScrollToCell(index);
                    }
                    else
                    {
                        ScrollToPosition(ScrollViewContentSize - ScrollViewSize);
                    }
                }
            }
        }

        public void ScrollToCell(int index)
        {
            if(index >= 0 && index < _data.Count)
            {
                if(_centerOnCell && index != _currentIndex)
                {
                    _currentIndex = index;

                    if(_pagination != null)
                    {
                        _pagination.SetSelectedButton(index);
                    }
                }

                ScrollToPosition(GetCellAccumulatedSize(index - 1));
            }
        }
            
        public void ScrollToPosition(float finalPosition, bool animate = true)
        {
            StopScrolling();

            _scrollCoroutine = Scroll(finalPosition, animate);
            StartCoroutine(_scrollCoroutine);
        }
            
        IEnumerator Scroll(float finalPosition, bool animate = true)
        {
            if(animate)
            {
                _scrollAnimation = new AnchoredPositionAnimation(_scrollAnimationTime, GetFinalScrollPosition(finalPosition), _scrollAnimationEaseType, _scrollAnimationCurve);
                if(_scrollAnimation != null)
                {
                    _scrollAnimation.Load(_scrollContentRectTransform.gameObject);
                    var enm = _scrollAnimation.Animate();
                    while(enm.MoveNext())
                    {
                        yield return enm.Current;
                    }
                }
                else
                {
                    ScrollPosition = finalPosition;
                    _requiresRefresh = true;
                }
            }
            else
            {
                ScrollPosition = finalPosition;
                _requiresRefresh = true;
            }
        }

        Vector2 GetFinalScrollPosition(float position)
        {
            if(UsesVerticalLayout)
            {
                return NewVector2(0f, -position);
            }
            else if(UsesHorizontalLayout)
            {
                return NewVector2(-position, 0f);
            }
            else
            {
                return NewVector2(0f, 0f);
            }
        }
            
        void EnableScroll()
        {
            _scrollRect.horizontal = _isHorizontal;
            _scrollRect.vertical = _isVertical;
        }

        void DisableScroll()
        {
            _scrollRect.horizontal = false;
            _scrollRect.vertical = false;
        }

        void StopScrolling()
        {
            _scrollRect.StopMovement();
            Canvas.ForceUpdateCanvases();

            if(_scrollCoroutine != null)
            {
                StopCoroutine(_scrollCoroutine);
            }

            if(_disableDragWhileScrollingAnimation)
            {
                DisableScroll();
            }
        }

        #region Unity methods

        void MyOnBeginDrag(PointerEventData eventData)
        {
            _startScrollingPosition = ScrollPosition;
        }

        void MyOnDrag(PointerEventData eventData)
        {
            if(_disableDragWhileScrollingAnimation)
            {
                StopScrolling();
            }
        }

        void MyOnEndDrag(PointerEventData eventData)
        {
            var scrollSize = Mathf.Abs(ScrollPosition - _startScrollingPosition);
            ScrollDirection scrollDirection = ScrollPosition - _startScrollingPosition < 0f ? ScrollDirection.LeftOrTop : ScrollDirection.RightOrBottom; 

            if(_centerOnCell)
            {
                if(_deltaDragCell <= scrollSize)
                {
                    if(scrollDirection == ScrollDirection.LeftOrTop && _currentIndex == 0)
                    {
                        ScrollToCurrentCell();
                    }
                    else if(scrollDirection == ScrollDirection.RightOrBottom && _currentIndex == _data.Count - 1)
                    {
                        ScrollToCurrentCell();
                    }
                    else
                    { 
                        ScrollToClosestCell(scrollDirection);
                    }
                }
                else
                {
                    ScrollToCurrentCell();
                }
            }
        }

        void MyLateUpdate()
        {
            if(_requiresRefresh)
            {
                RefreshVisibleCells(false);
            }
        }

        void MyOnDrawGizmoSelected()
        {
            if(_mainCanvas != null && _scrollRect != null)
            {
                Gizmos.color = Color.red;

                var trans = transform;
                var rectTrans = trans as RectTransform;
                float posXtop = 0f;
                float posXbottom = 0f;
                float posYtop = 0f;
                float posYbottom = 0f;

                if(_scrollRect.vertical)
                {
                    posXtop = trans.position.x + (rectTrans.rect.xMax * _mainCanvas.transform.localScale.x);
                    posXbottom = trans.position.x + (rectTrans.rect.xMin * _mainCanvas.transform.localScale.x);
                    posYtop = trans.position.y + ((rectTrans.rect.yMax * 0.5f) * _mainCanvas.transform.localScale.y);
                    posYbottom = trans.position.y + ((rectTrans.rect.yMax * 0.5f) * _mainCanvas.transform.localScale.y);
                }
                else if(_scrollRect.horizontal)
                {
                    posXtop = trans.position.x + ((rectTrans.rect.xMax * 0.5f) * _mainCanvas.transform.localScale.x);
                    posXbottom = trans.position.x + ((rectTrans.rect.xMax * 0.5f) * _mainCanvas.transform.localScale.x);
                    posYtop = trans.position.y + (rectTrans.rect.yMax * _mainCanvas.transform.localScale.y);
                    posYbottom = trans.position.y + (rectTrans.rect.yMin * _mainCanvas.transform.localScale.y);
                }

                Gizmos.DrawLine(new Vector3(posXtop, posYtop, 0f), new Vector3(posXbottom, posYbottom, 0f));

                Gizmos.color = Color.blue;

                if(_scrollRect.vertical)
                {
                    posXtop = trans.position.x + (rectTrans.rect.xMax * _mainCanvas.transform.localScale.x);
                    posXbottom = trans.position.x + (rectTrans.rect.xMin * _mainCanvas.transform.localScale.x);
                    posYtop = trans.position.y + ((rectTrans.rect.yMax * 0.5f) * _mainCanvas.transform.localScale.y);
                    posYbottom = trans.position.y + ((rectTrans.rect.yMax * 0.5f) * _mainCanvas.transform.localScale.y);
                }
                else if(_scrollRect.horizontal)
                {
                    posXtop = trans.position.x + ((rectTrans.rect.xMin - _boundsDelta) * _mainCanvas.transform.localScale.x);
                    posXbottom = trans.position.x + ((rectTrans.rect.xMin - _boundsDelta) * _mainCanvas.transform.localScale.x);
                    posYtop = trans.position.y + (rectTrans.rect.yMax * _mainCanvas.transform.localScale.y);
                    posYbottom = trans.position.y + (rectTrans.rect.yMin * _mainCanvas.transform.localScale.y);
                }

                Gizmos.DrawLine(new Vector3(posXtop, posYtop, 0f), new Vector3(posXbottom, posYbottom, 0f));

                if(_scrollRect.vertical)
                {
                    posXtop = trans.position.x + (rectTrans.rect.xMax * _mainCanvas.transform.localScale.x);
                    posXbottom = trans.position.x + (rectTrans.rect.xMin * _mainCanvas.transform.localScale.x);
                    posYtop = trans.position.y + ((rectTrans.rect.yMax * 0.5f) * _mainCanvas.transform.localScale.y);
                    posYbottom = trans.position.y + ((rectTrans.rect.yMax * 0.5f) * _mainCanvas.transform.localScale.y);
                }
                else if(_scrollRect.horizontal)
                {
                    posXtop = trans.position.x + ((_boundsDelta + rectTrans.rect.xMax) * _mainCanvas.transform.localScale.x);
                    posXbottom = trans.position.x + ((_boundsDelta + rectTrans.rect.xMax) * _mainCanvas.transform.localScale.x);
                    posYtop = trans.position.y + (rectTrans.rect.yMax * _mainCanvas.transform.localScale.y);
                    posYbottom = trans.position.y + (rectTrans.rect.yMin * _mainCanvas.transform.localScale.y);
                }

                Gizmos.DrawLine(new Vector3(posXtop, posYtop, 0f), new Vector3(posXbottom, posYbottom, 0f));
            }
        }

        #endregion
    }
}