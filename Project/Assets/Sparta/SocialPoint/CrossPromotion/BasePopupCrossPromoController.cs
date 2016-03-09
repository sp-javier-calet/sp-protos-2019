using UnityEngine;
using System.Collections;
using System;
using SocialPoint.Utils;

namespace SocialPoint.CrossPromotion
{
    public class BasePopupCrossPromoController : MonoBehaviour
    {
        protected CrossPromotionManager _cpm;
        public GameObject CellPrefab;
        public GameObject ActivityView;

        protected static int _iphone4Margin = 47;
        protected static int _defaultMargin = 135;

        protected static float _originalPopupWidth = 866f;
        protected static float _originalPopupHeight = 584;

        protected static float _originalCellWidth = 804f;
        protected static float _originalCellHeight = 296f;

        public float CellWidth { get; protected set; }

        public float CellHeight { get; protected set; }

        public float Margin { get; protected set; }

        public Vector2 ScreenSize { get; protected set; }

        public Vector2 PopupSize { get; protected set; }

        Action _closeCallback = null;
        long _timeOpened;

        public virtual void Init(CrossPromotionManager crossPromoManager, Action closeCallback)
        {
            _cpm = crossPromoManager;
            _closeCallback = closeCallback;
            _timeOpened = TimeUtils.Timestamp;

            ActivityView.SetActive(false);

            float ratioIphone = 960f / 640f;
            float currentRatio = (float)ScreenSize.x / (float)ScreenSize.y;

            Margin = ratioIphone == currentRatio ? _iphone4Margin : _defaultMargin;

            SetSize();
            SetPopupSize();

            if(!CheckIfFits(ScreenSize.x, ScreenSize.y, PopupSize.x, PopupSize.y))
            {
                SetPopupSize();
            }
            //Initialize cells
            CreateCells();


            _cpm.SendPopupImpressedEvent();
        }

        public virtual void SetSize()
        {
            CellWidth = _originalCellWidth - Margin;
            CellHeight = (_originalCellWidth / _cpm.Data.AspectRatio);
        }

        public virtual void SetPopupSize()
        {
            
        }

        public bool CheckIfFits(float screenWidth, float screenHeight, float finalWidth, float finalHeight)
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
                    CellWidth = (_originalCellWidth + widthAdjust) - Margin;
                    CellHeight = ((_originalCellWidth - Margin + widthAdjust) / _cpm.Data.AspectRatio);
                }
                else
                {
                    // Recalculate adjusting height
                    CellHeight = (((_originalCellWidth - Margin)/ _cpm.Data.AspectRatio) + heightAdjust);
                    CellWidth = (CellHeight * _cpm.Data.AspectRatio);
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
            ActivityView.SetActive(active);
        }
    }
}
