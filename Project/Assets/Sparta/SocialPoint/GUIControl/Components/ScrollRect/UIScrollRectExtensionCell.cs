using UnityEngine.SocialPlatforms;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using SocialPoint.Pooling;
using SocialPoint.Base;
using UnityEngine.Profiling;
using System;

namespace SocialPoint.GUIControl
{
    public partial class UIScrollRectExtension<TCellData, TCell> where TCellData : UIScrollRectCellData where TCell : UIScrollRectCellItem<TCellData>
    {
        /// <summary>
        /// This event will be called when a cell's visibility changes
        /// First param (int) is the element index, second param (bool) is whether or not it is visible
        /// </summary>
        public event Action<int, bool> CellVisibilityChange;

        Range _visibleElementRange;
        List<TCell> _visibleCells;

        void ShowCell(int index, bool showAtEnd)
        {
            TCell newCell = GetCellAndPrefabByIndex(index);

            var trans = newCell.transform;
            trans.SetParent(_scrollContentRectTransform, false);
            trans.localScale = Vector3.one;
            trans.localPosition = Vector3.zero;

            #if UNITY_EDITOR
            newCell.gameObject.name = "cell " + index;
            #endif

            _visibleCells.Insert(index, newCell);

            if(showAtEnd)
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

        void HideCell(int index, bool animate, Action callback)
        {
            var removedCell = GetVisibleCellByIndex(index);
            if(removedCell != null)
            {
                DestroyCellPrefabIfNeeded(removedCell.gameObject, animate, callback);

                _visibleCells.Remove(removedCell);
                _visibleElementRange.count -= 1;

                if(index == _visibleElementRange.from)
                {
                    _visibleElementRange.from += 1;
                }
            }

            if(CellVisibilityChange != null)
            {
                CellVisibilityChange(index, false);
            }
        }

        TCell GetVisibleCellByIndex(int index)
        {
            return _visibleCells.Find(x => x.Index == index);
        }

        public TCell GetCellAndPrefabByIndex(int index)
        {
            GameObject prefab = null;
            if(_prefabs.TryGetValue(_data[index].Prefab, out prefab))
            {
                var go = InstantiateCellPrefabIfNeeded(prefab);
                if(go != null)
                {
                    TCell cell = go.GetComponent<TCell>();
                    if(cell != null)
                    {
                        cell.UpdateData(_data[index]);
                        return cell;
                    }
                }
            }

            return null;
        }

        void ClearAllVisibleCells()
        {
            for(int i = 0; i < _visibleCells.Count; ++i)
            {
                HideCell(i, false, null);
            }

            _visibleElementRange = new Range(0, 0);
        }

        void RecalculateVisibleCells()
        {
            ClearAllVisibleCells();
            SetInitialVisibleElements();
        }

        void ReloadVisibleCells()
        {
            for(int i = _visibleElementRange.from; i < _visibleElementRange.RelativeCount(); ++i)
            {
                var cell = GetVisibleCellByIndex(i);
                cell.UpdateData(_data[i]);
            }
        }

        void RefreshVisibleCells(bool reload)
        {
            _requiresRefresh = false;

            if(_data.Count == 0)
            {
                return;
            }

            Profiler.BeginSample("UIScrollRectExtension.RefreshVisibleElements", this);

            Range newVisibleElements = CalculateCurrentVisibleRange();
            if(_visibleElementRange.Equals(newVisibleElements))
            {
                return;
            }

            int oldFrom = _visibleElementRange.from;
            int newFrom = newVisibleElements.from;

            int oldTo = _visibleElementRange.Last();
            int newTo = newVisibleElements.Last();

            bool _somethingHasChanged = false;

            if(newFrom > oldTo || newTo < oldFrom)
            {
                _somethingHasChanged = true;

                //We jumped to a completely different segment this frame, destroy all and recreate
                RecalculateVisibleCells();
            }
            else
            {
                //Remove elements that disappeared to the start
                for (int i = oldFrom; i < newFrom; ++i)
                {
                    HideCell(_visibleElementRange.from, false, null);
                    _somethingHasChanged = true;
                }
                    
                //Remove elements that disappeared to the end
                for (int i = newTo; i < oldTo; ++i)
                {
                    HideCell(_visibleElementRange.Last(), false, null);
                    _somethingHasChanged = true;
                }

                //Add elements that appeared on start
                for (int i = oldFrom - 1; i >= newFrom; --i)
                {
                    ShowCell(i, false);
                    _somethingHasChanged = true;
                }

                //Add elements that appeared on end
                for (int i = oldTo + 1; i <= newTo; ++i)
                {
                    ShowCell(i, true);
                    _somethingHasChanged = true;
                }

                _visibleElementRange = newVisibleElements;
            }

            if(_somethingHasChanged)
            {
                UpdatePaddingElements();
                UpdateScroll();
            }

            if(reload)
            {
                ReloadVisibleCells();
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

        void DestroyCellPrefabIfNeeded(GameObject prefab, bool animate, Action callback)
        {
            var trans = prefab.transform as RectTransform;
            var originalPivot = trans.pivot;
            var originalScale = trans.localScale;

            if(animate)
            {
                StartCoroutine(DestroyAnimated(prefab, originalPivot, originalScale, callback));
            }
            else
            {
                DoDestroyCellPrefabIfNeeded(prefab, originalPivot, originalScale, callback);
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

        void DoDestroyCellPrefabIfNeeded(GameObject prefab, Vector2 originalPivot, Vector3 originalScale, Action callback)
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
    }
}
