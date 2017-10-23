using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.GUIControl
{
    public abstract class UISimpleTableController<T, R> : UITableBaseController<R> where T : UITableBaseCellController<R>
    {
        [SerializeField]
        T _cellPrefab;

        Vector2 _elementSize;

        List<R> _cellsData;

        private int _numInstancesCreated = 0;

        public void UpdateTableData(List<R> cellsData)
        {
            if(_cellPrefab != null)
            {
                Rect rect = (_cellPrefab.transform as RectTransform).rect;
                _elementSize = new Vector2(rect.width, rect.height);
            }

            _cellsData = cellsData;
            ScrollPosition = 0.0f;
            ReloadData();
        }

        #region UITableBaseController

        public IEnumerator<T> VisibleCells
        {
            get
            {
                List<T> visibleTableCells = new List<T>();

                var visibleCells = VisibleCellsWithIndex;

                while(visibleCells.MoveNext())
                {
                    visibleTableCells.Add((T)visibleCells.Current.Value);
                }
                visibleCells.Dispose();

                return visibleTableCells.GetEnumerator();
            }
        }

        public override int GetNumberOfElementsForTableView(UITableBaseController<R> tableView)
        {
            return _cellsData.Count;
        }

        public override Vector2 GetSizeForElement(UITableBaseController<R> tableView, int index)
        {
            return _elementSize;
        }

        public override UITableBaseCellController<R> GetCellForIndexInTableView(UITableBaseController<R> tableView, int index)
        {
            T cell = tableView.GetReusableCell(_cellPrefab.ReuseIdentifier) as T;
            if(cell == null)
            {
                cell = (T)GameObject.Instantiate(_cellPrefab);
                cell.name = _cellPrefab.name + (++_numInstancesCreated).ToString();
            }
            cell.UpdateData(_cellsData[index]);
            return cell;
        }

        #endregion
    }
}
