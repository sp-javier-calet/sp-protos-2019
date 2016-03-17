using UnityEngine;
using System.Collections;
using System;
using SocialPoint.Utils;

namespace SocialPoint.CrossPromotion
{
    public class BasePopupCrossPromoController : MonoBehaviour
    {
        [SerializeField]
        protected GameObject _activityView;

        protected CrossPromotionManager _cpm;

        protected static int _iphone4Margin = 47;
        protected static int _defaultMargin = 135;

        public float CellWidth { get; protected set; }

        public float CellHeight { get; protected set; }

        public float Margin { get; protected set; }

        Action _closeCallback = null;
        long _timeOpened;

        public virtual void Init(CrossPromotionManager crossPromoManager, Action closeCallback)
        {
            _cpm = crossPromoManager;
            _closeCallback = closeCallback;
            _timeOpened = TimeUtils.Timestamp;

            _activityView.SetActive(false);

            //Check screen settings and desired size
            Vector2 screenSize = GetScreenSize();
            Vector2 popupSize = GetPopupSize();
            Vector2 cellAreaSize = GetCellAreaSize();
            float ratioIphone = 960f / 640f;
            float currentRatio = screenSize.x / screenSize.y;
            Margin = (Mathf.Approximately(ratioIphone, currentRatio)) ? _iphone4Margin : _defaultMargin;
            CellWidth = cellAreaSize.x;
            CellHeight = CellWidth / _cpm.Data.AspectRatio;

            //Set popup size
            do
            {
                SetPopupSize();
                popupSize = GetPopupSize();
            }
            while(popupSize.x < 0/*!CheckIfFits(screenSize.x, screenSize.y, popupSize.x, popupSize.y)*/);

            //Initialize cells
            CreateCells();

            _cpm.SendPopupImpressedEvent();
        }

        protected virtual Vector2 GetScreenSize()
        {
            return Vector2.zero;
        }

        protected virtual Vector2 GetPopupSize()
        {
            return Vector2.zero;
        }

        protected virtual Vector2 GetCellAreaSize()
        {
            return Vector2.zero;
        }

        protected virtual void SetPopupSize()
        {
        }

        protected bool CheckIfFits(float screenWidth, float screenHeight, float finalWidth, float finalHeight)
        {
            // Calculate if it fits
            float widthAdjust = screenWidth - finalWidth - Margin;
            float heightAdjust = screenHeight - finalHeight;

            // Check if size fits
            if(widthAdjust < 0 || heightAdjust < 0)
            {
                // Recalculate sizes
                if(widthAdjust < heightAdjust)
                {
                    // Recalculate adjusting width
                    CellWidth += widthAdjust;
                    CellHeight = CellWidth / _cpm.Data.AspectRatio;
                }
                else
                {
                    // Recalculate adjusting height
                    CellHeight += (heightAdjust / _cpm.Data.PopupHeightFactor);
                    CellWidth = CellHeight * _cpm.Data.AspectRatio;
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        protected virtual void CreateCells()
        {
        }

        public void OnClose()
        {
            _cpm.SendPopupClosedEvent(TimeUtils.Timestamp - _timeOpened);
            if(_closeCallback != null)
                _closeCallback();
            _cpm.DisposePopupTextures();
        }

        public void SetActivityView(bool active)
        {
            _activityView.SetActive(active);
        }
    }
}
