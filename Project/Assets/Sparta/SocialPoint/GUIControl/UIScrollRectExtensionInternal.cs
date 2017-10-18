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
        float _scrollPosition;

        Vector2 ScrollPositionNormalized
        {
            get
            {
                return _scrollRect.normalizedPosition;
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
                size = _data[index].PrefabHeight;
            }
            else if(UsesHorizontalLayout)
            {
                size = _data[index].PrefabWidth;
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
            float relativeScroll = UsesVerticalLayout ? 1 - newScrollValue.y : newScrollValue.x;
            Debug.Log("anchored: " + _scrollContentRectTransform.anchoredPosition + " -- calc: " + (relativeScroll * ScrollableSize));
            _scrollPosition = relativeScroll * ScrollableSize;
            _requiresRefresh = true;
        }      
            
        Range CalculateCurrentVisibleRange()
        {
            Profiler.BeginSample("UIScrollRectExtension.CalculateCurrentVisibleRange", this);

            float startPosition = _scrollPosition - _boundsDelta; // TODO CHECK
            float endPosition = startPosition + ScrollViewSize + _boundsDelta;

            int startIndex = FindIndexOfElementAtPosition(startPosition);
            int endIndex = FindIndexOfElementAtPosition(endPosition);

            Profiler.EndSample();
            return new Range(startIndex, endIndex - startIndex + 1);
        }

        int FindIndexOfElementAtPosition(float position)
        {
            if(position > _scrollContentRectTransform.rect.width)
            {
                return _data.Count - 1;
            }

            for(int i = 0; i < _data.Count; ++i)
            {
                if(_data[i].PrefabTotalAcumulatedWidth >= position)
                {
                    return i;
                }
            }

            return 0;
        }
            
        void SetInitialVisibleElements()
        {
            Range visibleElements = CalculateCurrentVisibleRange();
            for(int i = 0; i < visibleElements.count; i++)
            {
                AddCell(visibleElements.from + i, true);
            }
            _visibleElementRange = visibleElements;
        }

        float GetContentPanelSize()
        {
            float size = _defaultStartPadding;
            for(int i = 0; i < _data.Count; ++i)
            {
                size += GetCellSize(i);
                size += Spacing; // TODO check that last item has no space
            }

            size += EndPadding;

            return size;
        }
            
        void SetupCellSizes()
        {
            Profiler.BeginSample("UIScrollRectExtension.SetupCellSizes", this);

            float acumulatedWidth = 0f;
            for(int i = 0; i < _data.Count; ++i)
            {
                string prefabName = _data[i].PrefabName;

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
                    _data[i].SetupPrefabSizes(trans.rect.width, trans.rect.height);
                    _data[i].SetupAcumulatedPrefabSizes(acumulatedWidth, acumulatedWidth);

                    acumulatedWidth += trans.rect.width;

                    if(i < _data.Count)
                    {
                        acumulatedWidth += Spacing;
                    }
                }
            }

            // TODO Clear after creating????
//            _prefabs.Clear();

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
//            // TODO check fake elements without layout group
            int padding = _defaultStartPadding;
            if(_visibleElementRange.from > 0)
            {
                padding += (int)_data[_visibleElementRange.from - 1].PrefabTotalAcumulatedWidth;
            }

            StartPadding = padding;
        }
            
        public void ScrollToTop()
        {
//            _scrollRect.normalizedPosition = new Vector2(0, 1);
         //   Scroll(new Vector2(-_data[0].PrefabAcumulatedWidth, 0f)); // TODO
//            var finalPos = new Vector2(_defaultStartPadding, );
//            Scroll(new 
//            ScrollToPosition(_data[0].PrefabAcumulatedWidth);
        }

        public void ScrollToBottom()
        {
            Scroll(new Vector2(-_data[_data.Count - 1].PrefabAcumulatedWidth, 0f)); // TODO
//            _scrollRect.horizontalNormalizedPosition = 1;
//            ScrollToPosition(_data[_data.Count - 1].PrefabAcumulatedWidth);
        }

        public void ScrollToElement(int index)
        {
            Scroll(new Vector2(-_data[index].PrefabAcumulatedWidth, 0f)); // TODO=
        }

        public void ScrollToPosition(Vector2 finalPos)
        {
            Scroll(finalPos); // TODO=
        }
            
        void Scroll(Vector2 finalPos)
        {
            StopScrolling();

            _smoothScrollCoroutine = ScrollAnimation(finalPos, _scrollAnimationDuration); // TODO
            StartCoroutine(_smoothScrollCoroutine);
        }

        IEnumerator ScrollAnimation(Vector2 finalPos, float time)
        {
            Go.killAllTweensWithTarget(_scrollContentRectTransform);

            if(time < 0.05f)
            {
                yield break;
            }
                
//            float currentTime += Time.deltaTime;

            // TODO if is last indexes, we need to move until it fills the screen
            var tween = Go.to(_scrollContentRectTransform, 0.3f, new GoTweenConfig().anchoredPosition(finalPos));
            yield return tween.waitForCompletion();

//            var posOffset = normalizedFinalPos - startPos;
//            var elapsedTime = 0.0f;
//            while(elapsedTime <= time)
//            {
//                elapsedTime += Time.deltaTime;
//                _scrollRect.normalizedPosition = EaseVector(elapsedTime, startPos, posOffset, time);
//                yield return null;
//            }
//
//            _scrollRect.normalizedPosition = normalizedFinalPos;

            if(_disableDragWhileScrollAnimation)
            {
                EnableScroll();
            }
        }

//        protected virtual Vector2 EaseVector(float currentTime, Vector2 startValue, Vector2 displacement, float time)
//        {
//            return new Vector2( displacement.x * Mathf.Sin(currentTime / time * (Mathf.PI * 0.5f)) + startValue.x, displacement.y * Mathf.Sin(currentTime / time * (Mathf.PI * 0.5f)) + startValue.y);
//        }
            
//            float currentTime += Time.deltaTime;
//            _scrollRect
//
//            // TODO if is last indexes, we need to move until it fills the screen
//            var tween = Go.to(_scrollContentRectTransform, 0.3f, new GoTweenConfig().anchoredPosition(new Vector2(-position, 0f)));
//            yield return tween.waitForCompletion();
//        }

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

            if(_disableDragWhileScrollAnimation)
            {
                DisableScroll();
            }
        }

        void MyOnDrag(PointerEventData eventData)
        {
            if(!_disableDragWhileScrollAnimation)
            {
                StopScrolling();
            }
        }

        void MyLateUpdate()
        {
            if(_requiresRefresh)
            {
                RefreshVisibleElements();
            }
        }
    }
}