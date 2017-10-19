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
                    return Mathf.Abs(_scrollContentRectTransform.anchoredPosition.y);
                }
                else if(UsesHorizontalLayout)
                {
                    return Mathf.Abs(_scrollContentRectTransform.anchoredPosition.x);
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
                    _scrollContentRectTransform.anchoredPosition = new Vector2(0f, -value);
                }
                else if(UsesHorizontalLayout)
                {
                    _scrollContentRectTransform.anchoredPosition = new Vector2(-value, 0f);
                }
                else
                {
                    _scrollContentRectTransform.anchoredPosition = new Vector2(0f, 0f); //_gridLayoutGroup;
                }
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
//                else
//                {
//                    return 0f;//_gridLayoutGroup;
//                }
            }
        }

        int EndPadding
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
                    return 0;//_gridLayoutGroup;
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

        float GetCellSize(int index, bool withSpacing = false)
        {
            float size = 0f;
            if(UsesVerticalLayout)
            {
                size = _data[index].CellHeight;
            }
            else if(UsesHorizontalLayout)
            {
                size = _data[index].CellWidth;
            }
            else
            {
                size = 0f;//_gridLayoutGroup;
            }

            size += (withSpacing ? Spacing : 0);
            return size;
        }

        /// <summary>
        /// Get the maximum scrollable size of the table. scrollPosition property will never be more than this.
        /// </summary>
        public float ScrollableSize
        {
            get
            {
                return UsesVerticalLayout ? _scrollContentRectTransform.rect.height - _scrollRectTransform.rect.height : _scrollContentRectTransform.rect.width - _scrollRectTransform.rect.width;
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
            float endPosition = startPosition + ScrollViewSize + _boundsDelta;

            int startIndex = FindIndexOfElementAtPosition(startPosition, 0, _data.Count - 1);
            int endIndex = FindIndexOfElementAtPosition(endPosition, 0, _data.Count - 1);

            Profiler.EndSample();
            return new Range(startIndex, endIndex - startIndex + 1);
        }
            
        int FindIndexOfElementAtPosition(float position, int startIndex, int endIndex)
        {
            if(startIndex >= endIndex)
            {
                return startIndex;
            }

            int midIndex = (startIndex + endIndex) / 2;
            if(_data[midIndex].CellAccumulatedWidth > position)
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
            return (index > 0 && index < _data.Count);
        }

        float GetAccumulatedSizeForIndex(int index)
        {
            return (index == 0 ? 0f : _data[index - 1].CellAccumulatedWidth);
        }

        void SetInitialPosition()
        {
            ScrollPosition = 0f;
  
            if(!IndexIsValid(_initialIndex))
            {
                _initialIndex = 0;
            }

            if(_centerOnCell)
            {
                _centeredIndex = _initialIndex;
            }

            ScrollPosition += GetAccumulatedSizeForIndex(_initialIndex);
            ScrollPosition += CenterOnCellDeltaDisplacement();
        }

        float CenterOnCellDeltaDisplacement()
        {
            float delta = 0f;
            if(_centerOnCell)
            {
                delta -= ScrollViewSize * 0.5f;
                delta += _data[_centeredIndex].CellWidth * 0.5f;
            }

            return delta;
        }

        void SetInitialVisibleElements()
        {
            Range visibleElements = CalculateCurrentVisibleRange();
            for(int i = visibleElements.from; i < visibleElements.RelativeCount(); ++i)
            {
                AddCell(i, true);
            }
                
            _visibleElementRange = visibleElements;
            UpdatePaddingElements();
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
            
        void SetupCellSizes()
        {
            Profiler.BeginSample("UIScrollRectExtension.SetupCellSizes", this);

            float acumulatedWidth = _defaultStartPadding;
            for(int i = 0; i < _data.Count; ++i)
            {
                var dataValue = _data[i];
                string prefabName = dataValue.PrefabName;

                RectTransform trans;
                GameObject prefab;
                if(!_prefabs.TryGetValue(prefabName, out prefab))
                {
                    prefab = Resources.Load(prefabName) as GameObject;
                    if(prefab != null)
                    { 
                        _prefabs.Add(prefabName, GetCellPrefab(prefab));
                    }
                } 

                if(prefab != null)
                {
                    trans = prefab.transform as RectTransform;
                    dataValue.SetupPrefabSizes(trans.rect.width, trans.rect.height);
                   
                    acumulatedWidth += trans.rect.width;
                    dataValue.SetupAcumulatedPrefabSizes(acumulatedWidth, acumulatedWidth);

                    Debug.Log("index: " + i + " -- accumulated: " + _data[i].CellAccumulatedWidth);

                    if(i < _data.Count - 1)
                    {
                        acumulatedWidth += Spacing;
                    }
                }
            }
                
            Profiler.EndSample();
        }

        GameObject GetCellPrefab(GameObject prefab)
        {
            if(_usePooling)
            {
                UnityObjectPool.CreatePool(prefab, 1);
            }

            return prefab;
        }

        GameObject InstantiateCellPrefabIfNeeded(GameObject prefab)
        {
            return _usePooling ? UnityObjectPool.Spawn(prefab) : UnityEngine.Object.Instantiate(prefab);
        }

        void DestroyCellPrefabIfNeeded(GameObject prefab)
        {
            if(_usePooling)
            {
                UnityObjectPool.Recycle(prefab);
            }
            else
            {
                prefab.DestroyAnyway();
            }
        }

        void SetupRectTransformSize(RectTransform trans, float size, bool disableIfZero = false)
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

        void AddCell(int index, bool insertAtEnd)
        {
            TCell newCell = GetCellForIndexInTableView(index);

            var trans = newCell.transform;
            trans.SetParent(_scrollContentRectTransform, false);
            trans.localScale = Vector3.one;
            trans.localPosition = Vector3.zero;

            newCell.gameObject.name = "cell " + index; // debug mode

            _visibleCells[index] = newCell;

            if(insertAtEnd)
            {
                trans.SetAsLastSibling();
            }
            else
            {
                trans.SetAsFirstSibling(); 
            }

//            if(CellVisibilityChange != null)
//            {
//                CellVisibilityChange(index, true);
//            }
        }

        void RemoceCell(bool removeAtEnd)
        {
            int index = removeAtEnd ? _visibleElementRange.Last() : _visibleElementRange.from;
            TCell removedCell = _visibleCells[index];

            DestroyCellPrefabIfNeeded(removedCell.gameObject);
            _visibleCells.Remove(index);
            _visibleElementRange.count -= 1;
            if(!removeAtEnd)
            {
                _visibleElementRange.from += 1;
            }

//            if(CellVisibilityChange != null)
//            {
//                CellVisibilityChange(element, false);
//            }
        }

        public TCell GetCellForIndexInTableView(int index)
        {
            GameObject prefab = null;
            if(_prefabs.TryGetValue(_data[index].PrefabName, out prefab))
            {
                var go = InstantiateCellPrefabIfNeeded(prefab);
                TCell cell = go.GetComponent<TCell>();
                cell.UpdateData(_data[index]);
                return cell;
            }

            return null;
        }
           
        void ClearAllVisibleCells()
        {
            while(_visibleCells.Count > 0)
            {
                RemoceCell(false);
            }

            _visibleElementRange = new Range(0, 0);
        }

        void RecalculateVisibleCells()
        {
            ClearAllVisibleCells();
            SetInitialVisibleElements();
        }

        void RefreshVisibleElements()
        {
            _requiresRefresh = false;

            Profiler.BeginSample("UIScrollRectExtension.RefreshVisibleElements", this);

            Range newVisibleElements = CalculateCurrentVisibleRange();
            if(_visibleElementRange.Equals(newVisibleElements))
            {
                return;
            }
                
            int oldTo = _visibleElementRange.Last();
            int newTo = newVisibleElements.Last();

            if(newVisibleElements.from > oldTo || newTo < _visibleElementRange.from)
            {
                //We jumped to a completely different segment this frame, destroy all and recreate
                RecalculateVisibleCells();
                UpdatePaddingElements();
                return;
            }

            //Remove elements that disappeared to the start
            for(int i = _visibleElementRange.from; i < newVisibleElements.from; ++i)
            {
                RemoceCell(false);
            }
            //Remove elements that disappeared to the end
            for(int i = newTo; i < oldTo; ++i)
            {
                RemoceCell(true);
            }
            //Add elements that appeared on start
            for(int i = _visibleElementRange.from - 1; i >= newVisibleElements.from; --i)
            {
                AddCell(i, false);
            }
            //Add elements that appeared on end
            for(int i = oldTo + 1; i <= newTo; ++i)
            {
                AddCell(i, true);
            }

            _visibleElementRange = newVisibleElements;
            UpdatePaddingElements();

            Profiler.EndSample();
        }
            
        void UpdatePaddingElements()
        {
            int padding = 0;
            if(_visibleElementRange.from == 0)
            {
                padding = _defaultStartPadding;
            }
            else
            {
                padding += (int)_data[_visibleElementRange.from - 1].CellAccumulatedWidth;
            }

            StartPadding = padding;
        }
            
        void ScrollToCurrentCell()
        {
            ScrollToCell(_centeredIndex);
        }

        void ScrollToClosestCell()
        {
            // Setup the position to the middle of the scrollview to check the desired cell to center
            float position = ScrollPosition;
            position += ScrollViewSize * 0.5f;

            _centeredIndex = FindIndexOfElementAtPosition(position, _visibleElementRange.from, _visibleElementRange.RelativeCount());
            ScrollToCell(_centeredIndex);
        }

        public void ScrollToStartPosition()
        {
            Scroll(0f);
        }

        public void ScrollToFinalPosition()
        {
            Scroll(ScrollViewContentSize - ScrollViewSize);
        }

        public void ScrollToCell(int index)
        {
            if(index <= 0)
            {
                ScrollToStartPosition();
            }
            else if(index > 0 && index < _data.Count)
            {
                float position = GetAccumulatedSizeForIndex(index);
                position += CenterOnCellDeltaDisplacement();

                Scroll(position);
            }
            else
            {
                ScrollToFinalPosition();
            }
        }
            
        public void ScrollToPosition(float finalPosition, bool animate = true)
        {
            Scroll(finalPosition, animate);
        }
            
        void Scroll(float finalPosition, bool animate = true)
        {
            StopScrolling();

            if(animate)
            {
                _smoothScrollCoroutine = ScrollAnimation(finalPosition, _scrollAnimationDuration);
                StartCoroutine(_smoothScrollCoroutine);
            }
            else
            {
                ScrollPosition = finalPosition;
                _requiresRefresh = true;
            }
        }

        Vector2 GetFinalAnchoredPosition(float position)
        {
            if(UsesVerticalLayout)
            {
                return new Vector2(0f, -position);
            }
            else if(UsesHorizontalLayout)
            {
                return new Vector2(-position, 0f);
            }
            else
            {
                return new Vector2(0f, 0f);
            }
        }

        IEnumerator ScrollAnimation(float finalPosition, float time)
        {
            Go.killAllTweensWithTarget(_scrollContentRectTransform);

            if(time < 0.05f)
            {
                yield break;
            }
                
            GoTween tween;
            if(_scrollAnimationEaseType == GoEaseType.AnimationCurve && _scrollAnimationCurve != null)
            {
                tween = Go.to(_scrollContentRectTransform, 0.3f, new GoTweenConfig().anchoredPosition(GetFinalAnchoredPosition(finalPosition)).setEaseType(_scrollAnimationEaseType).setEaseCurve(_scrollAnimationCurve));
            }
            else
            {
                tween = Go.to(_scrollContentRectTransform, 0.3f, new GoTweenConfig().anchoredPosition(GetFinalAnchoredPosition(finalPosition)).setEaseType(_scrollAnimationEaseType));
            }

            yield return tween.waitForCompletion();

            ScrollPosition = finalPosition;
            UpdatePaddingElements();

            if(_disableDragWhileScrollingAnimation)
            {
                EnableScroll();
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

            if(_smoothScrollCoroutine != null)
            {
                StopCoroutine(_smoothScrollCoroutine);
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
//            Debug.Log("start position; " + eventData.position + " anchored: " + _startScrollingPosition);

//            if(_disableDragWhileScrollingAnimation)
//            {
//                StopScrolling();
//            }
        }

        void MyOnEndDrag(PointerEventData eventData)
        {
            Debug.Log("start position; " + eventData.position + " anchored: " + _startScrollingPosition);
            var scrollSize = Mathf.Abs(ScrollPosition - _startScrollingPosition);
            Debug.Log("final displacement " + scrollSize);
            if(_centerOnCell)
            {
                if(_deltaDragCell <= scrollSize)
                {
                    Debug.Log("moving to closest cell!!!");
                    ScrollToClosestCell();
                }
                else
                {
                    Debug.Log("moving to current cell!!!");
                    ScrollToCurrentCell();
                }
            }
        }

        void MyLateUpdate()
        {
            if(_requiresRefresh)
            {
                RefreshVisibleElements();
            }
        }

        #endregion
    }
}