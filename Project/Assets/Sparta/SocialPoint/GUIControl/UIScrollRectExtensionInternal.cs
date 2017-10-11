using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public partial class UIScrollRectExtension<TData, TCell> where TCell : UIScrollRectCellExtension<TCell>
    {
        private void OnScrollViewValueChanged(Vector2 newScrollValue)
        {
//            float relativeScroll = IsVertical ? 1 - newScrollValue.y : newScrollValue.x;
//            _scrollPosition = relativeScroll * ScrollableSize;
//            _requiresRefresh = true;
        }

        private bool IsVertical
        {
            get
            {
                return _verticalLayoutGroup != null;
            }
        }

        private float ScrollViewSize
        {
            get
            {
                return IsVertical ? _scrollRectTransform.rect.height : _scrollRectTransform.rect.width; 
            }
        }

        void CreateCell()
        {
            
        }

        void HideCell()
        {
            
        }

        void GetNumberOfVisibleCells()
        {
            
        }

        float GetCumulativeCellsSize()
        {
            
        }

        int FindCellAtIndexPosition(int index)
        {
        }

        void ScrollToCell(int index)
        {
            
        }
    }
}