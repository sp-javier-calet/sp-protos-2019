using UnityEngine;
using System;
using UnityEngine.SocialPlatforms;
using SocialPoint.Pooling;
using SocialPoint.Base;

namespace SocialPoint.GUIControl
{
    public partial class UIScrollRectExtension<TCellData, TCell> where TCellData : UIScrollRectCellData where TCell : UIScrollRectCellItem<TCellData>
    {
        int _cleanCumulativeIndex;
        float _cumulativeSize;
        int _defaultStartPadding;

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

        /// <summary>
        /// Get the position that the table would need to scroll to to have a certain element at the start
        /// </summary>
        /// <param name="index">The desired element index</param>
        /// <param name="above">Should the start of the table be above the element or below the element?</param>
        /// <returns>The position to scroll to, can be used with scrollPosition property</returns>
        public float GetScrollPositionForIndex(int index, bool above)
        {
            float cumulativeSize = _defaultStartPadding;
            for(int i = 0; i < _data.Count; ++i)
            {
                cumulativeSize += _data[i].PrefabWidth;
                cumulativeSize += Spacing;

                if(i > index)
                {
                    break;
                }
            }

            if(above)
            {
                cumulativeSize -= _data[index].PrefabWidth;
            }
                
            return cumulativeSize;
        }

        void OnScrollViewValueChanged(Vector2 newScrollValue)
        {
            RefreshVisibleElements();
        }      
            
        Range CalculateCurrentVisibleRange()
        {
            float startPosition = Math.Abs(_scrollContentRectTransform.anchoredPosition.x);
            float endPosition = startPosition + ScrollViewSize;

            int startIndex = FindIndexOfElementAtPosition(startPosition);
            int endIndex = FindIndexOfElementAtPosition(endPosition);

            return new Range(startIndex, endIndex - startIndex + 1);
        }

        int FindIndexOfElementAtPosition(float position)
        {
            float cumulativeSize = _defaultStartPadding;
            for(int i = 0; i < _data.Count; ++i)
            {
                cumulativeSize += _data[i].PrefabWidth;
                if(cumulativeSize >= position)
                {
                    return i;
                }

                cumulativeSize += Spacing;
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
                        _prefabs.Add(prefabName, CreateCellPrefab(prefab));
                    }
                } 

                if(prefab != null)
                {
                    trans = prefab.transform as RectTransform;
                    _data[i].SetupPrefabSizes(trans.rect.width, trans.rect.height);
                }
            }

            // TODO Clear after creating????
//            _prefabs.Clear();
        }

        GameObject CreateCellPrefab(GameObject prefab)
        {
            if(_usePooling)
            {
                UnityObjectPool.CreatePool(prefab, 1);
            }

            return prefab;
        }

        GameObject GetCellPrefab(GameObject prefab)
        {
            return _usePooling ? UnityObjectPool.Spawn(prefab) : UnityEngine.Object.Instantiate(prefab);
        }

        void HideCellPrefab(GameObject prefab)
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

        void AddCell(int index, bool insertAtEnd = true)
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

            HideCellPrefab(removedCell.gameObject);
//            StoreCellForReuse(removedCell);
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
                var go = GetCellPrefab(prefab);
                TCell cell = go.GetComponent<TCell>();
                cell.UpdateData(_data[index]);
                return cell;
            }

            return null;
        }
           
        void RefreshVisibleElements()
        {
//            _requiresRefresh = false;
//
//            if(this.IsEmpty)
//            {
//                return;
//            }

            Range newVisibleElements = CalculateCurrentVisibleRange();
            int oldTo = _visibleElementRange.Last();
            int newTo = newVisibleElements.Last();

            if(newVisibleElements.from > oldTo || newTo < _visibleElementRange.from)
            {
                //We jumped to a completely different segment this frame, destroy all and recreate
//                RecalculateVisibleElementsFromScratch();
                return;
            }

            //Remove elements that disappeared to the start
            for(int i = _visibleElementRange.from; i < newVisibleElements.from; ++i)
            {
                Debug.Log("hide element at top");
                RemoceCell(false);
            }
            //Remove elements that disappeared to the end
            for(int i = newTo; i < oldTo; ++i)
            {
                Debug.Log("hide element at bottom");
                RemoceCell(true);
            }
            //Add elements that appeared on start
            for(int i = _visibleElementRange.from - 1; i >= newVisibleElements.from; --i)
            {
                Debug.Log("add element to top");
                AddCell(i, false);
            }
            //Add elements that appeared on end
            for(int i = oldTo + 1; i <= newTo; ++i)
            {
                Debug.Log("add element to bottom");
                AddCell(i, true);
            }

            _visibleElementRange = newVisibleElements;
            UpdatePaddingElements();
        }

        void UpdatePaddingElements()
        {
            // TODO check fake elements without layout group
            float hiddenElementsSizeSum = 0;
            for(int i = 0; i < _visibleElementRange.from; ++i)
            {
                hiddenElementsSizeSum += _data[i].PrefabWidth;
                if(i < _visibleElementRange.from - 1)
                {
                    hiddenElementsSizeSum += Spacing;
                }
            }

            StartPadding = (int)hiddenElementsSizeSum;

            // TODO RIGHT TO LEFT
//            hiddenElementsSizeSum = 0;
//            for(int i = _visibleElementRange.from; i <= _visibleElementRange.Last(); i++)
//            {
//                // TODO check spacing
//                hiddenElementsSizeSum += _data[i].PrefabWidth;
//
//            }
//
//            SetupRectTransformSize(_fakeEndContentPlaceholder, hiddenElementsSizeSum, true);
        }
    }
}