using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.GUIControl
{
    //An example implementation of a class that communicates with a TableView
    public abstract class UISimpleTableController<T, R> : UITableBaseController<R> where T : UITableBaseCellController<R>
    {
        [SerializeField]
        T _cellPrefab;

        List<R> _cellsData;

        private int _numInstancesCreated = 0;

        public void UpdateTableData(List<R> cellsData)
        {
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

                return visibleTableCells.GetEnumerator();
            }
        }

        public override int GetNumberOfElementsForTableView(UITableBaseController<R> tableView)
        {
            return _cellsData.Count;
        }

        public override Vector2 GetSizeForElement(UITableBaseController<R> tableView, int index)
        {
            return new Vector2((_cellPrefab.transform as RectTransform).rect.width, (_cellPrefab.transform as RectTransform).rect.height);
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
