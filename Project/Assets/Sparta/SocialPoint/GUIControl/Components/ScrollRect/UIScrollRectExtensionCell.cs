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
        List<TCell> _visibleCells;

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

        void ShowCell(int index, bool insertAtEnd)
        {
            var newCell = GetCellGameObject(index);
            var cell = newCell.GetComponent<TCell>();
            if(cell != null)
            {
                cell.UpdateData(_data[index]);
                _visibleCells.Add(cell);
            }

            var trans = newCell.transform;

            trans.SetParent(_scrollContentRectTransform, false);
            trans.localScale = Vector3.one;
            trans.localPosition = Vector3.zero;

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

        TCell GetVisibleCellByUID(string uid)
        {
            return _visibleCells.Find( x => x.UID.Equals(uid));
        }
            
        void CleanLastCell()
        {
            HideCell(_visibleElementRange.Last(), false, null);
        }

        void HideCell(int index, bool animate, Action callback)
        {
            bool removeAtStart = index == _visibleElementRange.from;

            var CellToRemove = GetVisibleCellByUID(_data[index].UID);
            if(CellToRemove != null)
            {
                var go = CellToRemove.gameObject;

                _visibleCells.Remove(CellToRemove);
                DestroyCellPrefabIfNeeded(go, animate, callback);
            }
           
            _visibleElementRange.count -= 1;
            if(removeAtStart)
            {
                _visibleElementRange.from += 1;
            }
                
            if(CellVisibilityChange != null)
            {
                CellVisibilityChange(index, false);
            }
        }
            
        GameObject GetCellGameObject(int index)
        {
            GameObject prefab;
            if(_prefabs.TryGetValue(_data[index].Prefab, out prefab))
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
                CleanLastCell();
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

            Profiler.BeginSample("UIScrollRectExtension.RefreshVisibleElements", this);

            Range newVisibleElements = CalculateCurrentVisibleRange();
            if(_visibleElementRange.Equals(newVisibleElements))
            {
                if(reload)
                {
                    ReloadVisibleCells();
                }

                return;
            }

            int oldFrom = _visibleElementRange.from;
            int newFrom = newVisibleElements.from;

            int oldTo = _visibleElementRange.Last();
            int newTo = newVisibleElements.Last();

            bool hasChanged = false;

            if(newFrom > oldTo || newTo < oldFrom)
            {
                //We jumped to a completely different segment this frame, destroy all and recreate
                RecalculateVisibleCells();
                return;
            }
            else
            {
                //Remove elements that disappeared to the start
                for (int i = oldFrom; i < newFrom; ++i)
                {
                    hasChanged = true;
                    HideCell(oldFrom, false, null);
                }

                //Remove elements that disappeared to the end
                for(int i = newTo; i < oldTo; ++i)
                {
                    hasChanged = true;
                    HideCell(oldTo, false, null);
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
            for(int i = _visibleElementRange.from; i < _visibleElementRange.RelativeCount(); ++i)
            {
                var data = _data[i];
                if(data != null)
                {
                    var cell = _visibleCells.Find( x => x.UID.Equals(data.UID));
                    if(cell != null)
                    {
                        cell.UpdateData(data);
                    }
                }
            }
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
