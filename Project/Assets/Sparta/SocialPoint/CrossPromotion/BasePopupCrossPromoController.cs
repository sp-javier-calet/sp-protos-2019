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

        protected static float _originalPopupWidth = 866f;
        protected static float _originalPopupHeight = 584;

        protected static float _originalCellWidth = 804f;
        protected static float _originalCellHeight = 296f;

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
            float ratioIphone = 960f / 640f;
            float currentRatio = screenSize.x / screenSize.y;
            Margin = (Mathf.Approximately(ratioIphone, currentRatio)) ? _iphone4Margin : _defaultMargin;
            CellWidth = _originalCellWidth - Margin;
            CellHeight = CellWidth / _cpm.Data.AspectRatio;

            //Set popup size
            Vector2 popupSize;
            do
            {
                SetPopupSize();
                popupSize = GetPopupSize();
                Debug.Log("Screen Size: " + screenSize);
                Debug.Log("Popup Size: " + popupSize);
            }
            while(false/*!CheckIfFits(screenSize.x, screenSize.y, popupSize.x, popupSize.y)*/);

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
            return  Vector2.zero;
        }

        protected virtual void SetPopupSize()
        {
        }

        protected bool CheckIfFits(float screenWidth, float screenHeight, float finalWidth, float finalHeight)
        {
            // Calculate if it fits
            float widthAdjust = screenWidth - finalWidth;
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
                    CellHeight += heightAdjust;
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
