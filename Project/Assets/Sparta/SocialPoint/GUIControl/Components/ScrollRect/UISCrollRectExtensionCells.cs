using UnityEngine;
using System;
using SocialPoint.Pooling;
using SocialPoint.Base;
using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.GUIControl
{
    public partial class UIScrollRectExtension<TCellData, TCell> where TCellData : UIScrollRectCellData where TCell : UIScrollRectCellItem<TCellData>
    {
        public bool _usePooling;
        public bool _useAnimationForRemoveCells = true;

        public enum CellPositionShowOrHide
        {
            AtBegin,
            AtMiddle,
            AtEnd
        }

        public class Cells
        {
            public int Id;
            public int Index;
            public TCell Cell;

            public Cells(int id, int index, TCell cell)
            {
                Id = id;
                Index = index;
                Cell = cell;
            }

            public void UpdateData(TCellData data)
            {
                if(Cell != null)
                {
                    Cell.UpdateData(data);
                }
            }
        }
            
        List<int> _tempVisibleCells = new List<int>();
        List<Cells> _visibleCells = new List<Cells>();

        public int FirstVisibleCellIndex { get { return _visibleCells.Count > 0 ? _visibleCells[0].Index : 0; }}
        public int LastVisibleCellIndex { get { return _visibleCells.Count > 0 ? _visibleCells[_visibleCells.Count - 1].Index : 0; }}

        public int FirstTempVisibleCellIndex { get { return _tempVisibleCells.Count > 0 ? _tempVisibleCells[0] : 0; }}
        public int LastTempVisibleCellIndex { get { return _tempVisibleCells.Count > 0 ? _tempVisibleCells[_tempVisibleCells.Count - 1] : 0; }}

        public bool VisibleElementsHaveNotChanged()
        {
            if(_tempVisibleCells.Count == _visibleCells.Count)
            {
                for(int i = 0; i < _visibleCells.Count; ++i)
                {
                    if(_visibleCells[i].Index != _tempVisibleCells[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        List<int> CalculateCurrentVisibleIndexs()
        {
            Profiler.BeginSample("UIScrollRectExtension.CalculateCurrentVisibleRange", this);

            var newVisibleRange = new List<int>();

            float startPosition = ScrollPosition - _boundsDelta;
            float endPosition = ScrollPosition + ScrollViewSize + _boundsDelta;

            int startIndex = FindIndexOfElementAtPosition(startPosition, 0, _data.Count - 1);
            int endIndex = FindIndexOfElementAtPosition(endPosition, 0, _data.Count - 1);

            for(int i = startIndex; i <= endIndex; ++i)
            {
                newVisibleRange.Add(i);
            }

            Profiler.EndSample();


            return newVisibleRange;
        }
            
        int FindIndexOfElementAtPosition(float position)
        {
            return FindIndexOfElementAtPosition(position, FirstVisibleCellIndex, LastVisibleCellIndex);
        }

        int FindIndexOfElementAtPosition(float position, int startIndex, int endIndex)
        {
            if(startIndex >= endIndex)
            {
                return startIndex;
            }

            int midId = (startIndex + endIndex) / 2;
            if(GetCellAccumulatedSize(midId) > position)
            {
                return FindIndexOfElementAtPosition(position, startIndex, midId);
            }
            else
            {
                return FindIndexOfElementAtPosition(position, midId + 1, endIndex);
            }
        }

        public TCell CreateCell(TCellData data, int index)
        { 
            GameObject prefab;
            if(_prefabs.TryGetValue(data.Prefab, out prefab))
            {
                var go = InstantiateCellPrefabIfNeeded(prefab);
                if(go != null)
                {
                    var goCell = go.GetComponent<TCell>();
                    if(goCell != null)
                    {
                        var newCell = new Cells(data.Id, index, goCell);
                        newCell.UpdateData(data);

                        _visibleCells.Add(newCell);

                        UpdateCellsIndexs();

                        return goCell;
                    }
                }
            }

            return null;
        }

        void UpdateCellsData()
        {
            for(int i = 0; i < _visibleCells.Count; ++i)
            {
                var cell = _visibleCells[i];
                if(cell != null)
                {
                    var data = GetDataById(cell.Id);
                    if(data != null)
                    {
                        cell.UpdateData(data);
                    }
                }
            }
        }

        void UpdateCellsIndexs()
        {
            for(int i = 0; i < _visibleCells.Count; ++i)
            {
                var cell = _visibleCells[i];
                if(cell != null)
                {
                    var index = GetDataIndexById(cell.Id);
                    if(IndexIsValid(index))
                    {
                        cell.Index = index;
                    }
                }
            }
        }

        public void RemoveVisibleCell(int id, bool animate)
        {
            var cell = _visibleCells.Find(x => x.Id == id);
            if(cell != null)
            {
                DestroyCellPrefabIfNeeded(cell.Cell.gameObject, animate);
                _visibleCells.Remove(cell);

                UpdateCellsIndexs();
            }
        }

        GameObject InstantiateCellPrefabIfNeeded(GameObject prefab)
        {
            return _usePooling ? UnityObjectPool.Spawn(prefab) : UnityEngine.Object.Instantiate(prefab);
        }

        void DestroyCellPrefabIfNeeded(GameObject prefab, bool animate)
        {
            var trans = prefab.transform as RectTransform;
            var originalPivot = trans.pivot;
            var originalScale = trans.localScale;

            if(animate)
            {
                StartCoroutine(DestroyAnimated(prefab, originalPivot, originalScale));
            }
            else
            {
                DoDestroyCellPrefabIfNeeded(prefab, originalPivot, originalScale);
            }
        }

        void DoDestroyCellPrefabIfNeeded(GameObject prefab, Vector2 originalPivot, Vector3 originalScale)
        {
            if(_usePooling)
            {
                UnityObjectPool.Recycle(prefab);

                var trans = prefab.transform as RectTransform;
                trans.pivot = originalPivot;
                trans.localScale = originalScale;

                Debug.Log("finished removing animated");
            }
            else
            {
                prefab.DestroyAnyway();

                Debug.Log("finished removing");
            }
        }
            
        IEnumerator DestroyAnimated(GameObject prefab, Vector2 originalPivot, Vector3 originalScale)
        {
            var trans = prefab.transform as RectTransform;

            Go.killAllTweensWithTarget(trans);

//            if(time < 0.05f)
//            {
//                yield break;
//            }

            trans.pivot = new Vector2(0f, 0.5f);
            GoTween tween = Go.to(trans, 0.3f, new GoTweenConfig().scale(new Vector3(0f, 1f, 1f)));

//            if(_scrollAnimationEaseType == GoEaseType.AnimationCurve && _scrollAnimationCurve != null)
//            {
//                tween = Go.to(_scrollContentRectTransform, 0.3f, new GoTweenConfig().anchoredPosition(GetFinalScrollPosition(finalPosition)).setEaseType(_scrollAnimationEaseType).setEaseCurve(_scrollAnimationCurve));
//            }
//            else
//            {
//                tween = Go.to(_scrollContentRectTransform, 0.3f, new GoTweenConfig().anchoredPosition(GetFinalScrollPosition(finalPosition)).setEaseType(_scrollAnimationEaseType));
//            }

            yield return tween.waitForCompletion();

            DoDestroyCellPrefabIfNeeded(prefab, originalPivot, originalScale);
        }

        void ShowCell(int index, CellPositionShowOrHide showAtPosition, bool showAnimated = true)
        {
            var data = GetDataByIndex(index);
            if(data != null)
            {
                var newCell = CreateCell(data, index);

                var trans = newCell.transform;
                trans.SetParent(_scrollContentRectTransform, false);
                trans.localScale = Vector3.one;
                trans.localPosition = Vector3.zero;

                #if UNITY_EDITOR
                if(_renameCells)
                {
                    newCell.gameObject.name = "cell with index: " + index + " and id: " + data.Id;
                }
                #endif

                switch(showAtPosition)
                {
                case CellPositionShowOrHide.AtBegin:
                    trans.SetAsFirstSibling(); 
                    break;

                case CellPositionShowOrHide.AtMiddle:
                    trans.SetAsFirstSibling();  // TODO
                    break;

                case CellPositionShowOrHide.AtEnd:
                    trans.SetAsLastSibling();
                    break;
                }
                //            if(showAtEnd)
                //            {
                //                trans.SetAsLastSibling();
                //            }
                //            else
                //            {
                //                trans.SetAsFirstSibling(); 
                //            }

                //            if(CellVisibilityChange != null)
                //            {
                //                CellVisibilityChange(Id, true);
                //            }
            }
        }
            
        void HideCell(int id, bool animate = true)
        {
            RemoveVisibleCell(id, animate);
//            var go = removedCell.gameObject;
            //            int Id = 0;
            //            switch(cellPosition)
            //            {
            //            case CellPositionShowOrHide.AtBegin:
            //                Id = _visibleCells.FirstCurrentCellId;
            //                break;
            //                
            //            case CellPositionShowOrHide.AtMiddle:
            //                Id = _visibleCells.LastCurrentCellId;
            //                break;
            //                
            //            case CellPositionShowOrHide.AtEnd:
            //                Id = _visibleCells.LastCurrentCellId;
            //                break;
            //            }
            //
            //
            //            int Id = hideAtEnd ? _visibleCells.LastCurrentCellId : _visibleCells.FirstCurrentCellId;
//            TCell removedCell = _visibleCells.GetVisibleCell(Id);

//            DestroyCellPrefabIfNeeded(go, _useAnimationForRemoveCells); //TODO
//            _visibleCells.RemoveVisibleCell(Id);

            //            if(!hideAtEnd)
            //            {
            //                _visibleCellsIds.from += 1;
            //            }

            //            if(CellVisibilityChange != null)
            //            {
            //                CellVisibilityChange(element, false);
            //            }
        }

//        public void AddVisibleCell(int Id, TCell cell)
//        {
//            _visibleCells[Id] = cell;
//        }
//
//        public void RemoveVisibleCell(int Id)
//        {
////            _currentIds.Remove(Id);
//            _visibleCells.Remove(Id);
//        }

        public void ClearAllVisibleCells()
        {
            for(int i = 0; i < _visibleCells.Count; ++i)
            {
                HideCell(i, false);
            }

//            _currentIds.Clear();
            _visibleCells.Clear();
            _tempVisibleCells.Clear();
        }

//        public TCell GetCellForIdInTableView(int Id)
//        {
//            GameObject prefab = null;
//            if(_prefabs.TryGetValue(_data[Id].Prefab, out prefab))
//            {
//                var go = InstantiateCellPrefabIfNeeded(prefab);
//                TCell cell = go.GetComponent<TCell>();
//                cell.UpdateData(_data[Id]);
//                return cell;
//            }
//
//            return null;
//        }
    }
}
