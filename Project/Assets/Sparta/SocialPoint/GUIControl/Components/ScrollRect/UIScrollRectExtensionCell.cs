using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Profiling;
using SocialPoint.Pooling;
using SocialPoint.Base;
using System;
using System.Collections;

namespace SocialPoint.GUIControl
{
    public partial class UIScrollRectExtension<TCellData, TCell> where TCellData : UIScrollRectCellData where TCell : UIScrollRectCellItem<TCellData>
    {
        /// <summary>
        /// This event will be called when a cell's visibility changes
        /// First param (int) is the cell index, second param (bool) is whether or not it is visible
        /// </summary>
        public event Action<int, bool> CellVisibilityChange;

        Range _visibleElementRange;
        Dictionary<int, TCell> _visibleCells;
        IEnumerator<KeyValuePair<int, TCell>> _visibleCellsEnumerator
        {
            get
            {
                return _visibleCells.GetEnumerator();    
            }
        }

        GameObject InstantiateCellPrefabIfNeeded(GameObject prefab)
        {
            return _usePooling ? UnityObjectPool.Spawn(prefab) : UnityEngine.Object.Instantiate(prefab);
        }

        void DestroyCellPrefabIfNeeded(GameObject prefab, bool animate, Action callback)
        {
            if(animate)
            {
                var trans = prefab.transform as RectTransform;
                var originalPivot = trans.pivot;
                var originalScale = trans.localScale;

                StartCoroutine(DestroyAnimated(prefab, originalPivot, originalScale, callback));
            }
            else
            {
                DoDestroyCellPrefabIfNeeded(prefab);
            }
        }

        IEnumerator DestroyAnimated(GameObject prefab, Vector2 originalPivot, Vector3 originalScale, Action callback)
        {
            var trans = prefab.transform as RectTransform;

            Go.killAllTweensWithTarget(trans);

            //            if(time < 0.05f)
            //            {
            //                yield break;
            //            } 

            trans.pivot = new Vector2(0f, 0.5f);
            GoTween tween = Go.to(trans, 0.2f, new GoTweenConfig().scale(new Vector3(0f, 1f, 1f)));
            yield return tween.waitForCompletion();

            DoDestroyCellPrefabIfNeeded(prefab, originalPivot, originalScale, callback);
        }

        void DoDestroyCellPrefabIfNeeded(GameObject prefab, Vector2 originalPivot = default(Vector2), Vector3 originalScale = default(Vector3), Action callback = null)
        {
            if(_usePooling)
            {
                UnityObjectPool.Recycle(prefab);

                var trans = prefab.transform as RectTransform;
                trans.pivot = originalPivot;
                trans.localScale = originalScale;
            }
            else
            {
                prefab.DestroyAnyway();
            }

            if(callback != null)
            {
                callback();
            }
        }

        void ShowCell(int index, bool insertAtEnd)
        {
            Profiler.BeginSample("UIScrollRectExtension.ShowCell", this);

            var newCell = GetCellGameObject(index);
            if(newCell != null)
            {
                var cell = newCell.GetComponent<TCell>();
                if(cell != null)
                {
                    _visibleCells[index] = cell;
                    cell.UpdateData(_data[index]); 

                    var trans = newCell.transform;
                    trans.SetParent(_scrollContentRectTransform, false);
                    trans.transform.ResetLocalTransform();

                    if(insertAtEnd)
                    {
                        trans.SetAsLastSibling();
                    }
                    else
                    {
                        trans.SetAsFirstSibling(); 
                    }

                    if(CellVisibilityChange != null)
                    {
                        CellVisibilityChange(index, true);
                    }
                }
            }

            Profiler.EndSample();
        }

        TCell GetVisibleCellByUID(string uid)
        {
            while(_visibleCellsEnumerator.MoveNext())
            {
                var current = _visibleCellsEnumerator.Current as TCell;
                if(current != null && current.UID.Equals(uid))
                {
                    _visibleCellsEnumerator.Dispose();
                    return current;
                }
            }

            _visibleCellsEnumerator.Dispose();
            return null;
        }

        int GetDataByUID(string uid)
        {
            return _data.FindIndex(x => x.UID.Equals(uid));
        }
            
        void HideFirstCell(bool animate = false, Action callback = null)
        {
            HideCell(_visibleElementRange.from, animate, callback);
        }

        void HideLastCell(bool animate = false, Action callback = null)
        {
            HideCell(_visibleElementRange.Last(), animate, callback);
        }

        void HideCell(int index, bool animate = false, Action callback = null)
        {
            Profiler.BeginSample("UIScrollRectExtension.HideCell", this);

            var last = (index == _visibleElementRange.Last());

            var cellToRemove = _visibleCells[index];
            if(cellToRemove != null)
            {
                var go = cellToRemove.gameObject;
                if(go != null)
                {
                    DestroyCellPrefabIfNeeded(go, animate, callback);
                    _visibleCells.Remove(index);

                    _visibleElementRange.count -= 1;
                    if(!last)
                    {
                        _visibleElementRange.from += 1;
                    }

                    if(CellVisibilityChange != null)
                    {
                        CellVisibilityChange(index, false);
                    }
                }
            }

            Profiler.EndSample();
        }
            
        GameObject GetCellGameObject(int index)
        {
            var prefab = _prefabs[_data[index].PrefabIndex];
            if(prefab != null)
            {
                var go = InstantiateCellPrefabIfNeeded(prefab);
                if(go != null)
                {
                    #if UNITY_EDITOR
                    go.name = "cell " + index;
                    #endif

                    return go;
                }
            }

            return null;
        }

        void ClearAllVisibleCells()
        {
            while(_visibleCells.Count > 0)
            {
                HideFirstCell();
            }

            _visibleElementRange = new Range(0, 0);
        }

        void RecalculateVisibleCells()
        {
            ClearAllVisibleCells();
            SetInitialVisibleElements();
        }

        void RefreshVisibleCells(bool reload)
        {
            _requiresRefresh = false;

            Profiler.BeginSample("UIScrollRectExtension.RefreshVisibleCells", this);

            var newVisibleElements = CalculateCurrentVisibleRange();
            if(_visibleElementRange.Equals(newVisibleElements))
            {
                if(reload)
                {
                    ReloadVisibleCells();
                }

                return;
            }

            var hasChanged = false;
            var oldFrom = _visibleElementRange.from;
            var newFrom = newVisibleElements.from;
            var oldTo = _visibleElementRange.Last();
            var newTo = newVisibleElements.Last();

            //We jumped to a completely different segment this frame, destroy all and recreate
            if(newFrom > oldTo || newTo < oldFrom)
            {
                RecalculateVisibleCells();
                UpdateScrollState();
               
                return;
            }
            else
            {
                //Remove elements that disappeared to the start
                for (int i = oldFrom; i < newFrom; ++i)
                {
                    hasChanged = true;
                    HideFirstCell();
                }

                //Remove elements that disappeared to the end
                for(int i = newTo; i < oldTo; ++i)
                {
                    hasChanged = true;
                    HideLastCell();
                }

                //Add elements that appeared on start
                for (int i = oldFrom - 1; i >= newFrom; --i)
                {
                    hasChanged = true;
                    ShowCell(i, false);
                }

                //Add elements that appeared on end
                for(int i = oldTo + 1; i <= newTo; ++i)
                {
                    hasChanged = true;
                    ShowCell(i, true);
                }

                _visibleElementRange = newVisibleElements;
            }

            if(hasChanged)
            {
                UpdatePaddingElements();
                UpdateScrollState();
            }

            if(reload)
            {
                ReloadVisibleCells();
            }

            Profiler.EndSample();
        }

        public void ReloadVisibleCells()
        {
            Profiler.BeginSample("UIScrollRectExtension.ReloadVisibleCells", this);
            for(int i = _visibleElementRange.from; i < _visibleElementRange.RelativeCount(); ++i)
            {
                var data = _data[i];
                if(data != null)
                {
                    var cell = GetVisibleCellByUID(data.UID);
                    if(cell != null)
                    {
                        cell.UpdateData(data);
                    }
                }
            }
            Profiler.EndSample();
        }

        void UpdateScrollState()
        {
            if(ScrollViewContentSize > ScrollViewSize)
            {
                EnableScroll();
            }
            else
            {
                DisableScroll();
            }
        }
    }
}
