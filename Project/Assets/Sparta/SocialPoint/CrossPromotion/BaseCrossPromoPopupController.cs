using System;
using SocialPoint.GUIControl;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.CrossPromotion
{
    public class BaseCrossPromoPopupController : UIViewController
    {
        [SerializeField]
        protected GameObject _activityView;

        protected CrossPromotionManager _cpm;

        protected float _minHorizontalMargin;
        protected float _minVerticalMargin;

        protected static int _iphone4HorizontalMargin = 47;
        protected static int _defaultHorizontalMargin = 135;
        protected static int _iphone4VerticalMargin = 23;
        protected static int _defaultVerticalMargin = 60;

        protected float _cellWidth;
        protected float _cellHeight;

        Action _closeCallback;
        long _timeOpened;

        public virtual void Init(CrossPromotionManager crossPromoManager, Action closeCallback)
        {
            _cpm = crossPromoManager;
            _closeCallback = closeCallback;
            _timeOpened = TimeUtils.Timestamp;

            _activityView.SetActive(false);

            //Set desired margins for screen
            Vector2 screenSize = GetScreenSize();
            const float ratioIphone = 960f / 640f;
            float currentRatio = screenSize.x / screenSize.y;
            if(Mathf.Approximately(ratioIphone, currentRatio))
            {
                SetMargins(_iphone4HorizontalMargin, _iphone4VerticalMargin);
            }
            else
            {
                SetMargins(_defaultHorizontalMargin, _defaultVerticalMargin);
            }

            //Initialize popup UI to fit screen with current settings
            SetPopupSize();

            //Initialize cells
            CreateCells();

            _cpm.SendPopupImpressedEvent();
        }

        public void SetMargins(int horizontal, int vertical)
        {
            _minHorizontalMargin = _iphone4HorizontalMargin = _defaultHorizontalMargin = horizontal;
            _minVerticalMargin = _iphone4VerticalMargin = _defaultVerticalMargin = vertical;
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

        protected virtual Vector2 GetScreenSize()
        {
            return Vector2.zero;
        }

        protected virtual Vector2 GetPopupSize()
        {
            return Vector2.zero;
        }

        protected virtual void SetPopupSize()
        {
        }

        protected virtual void CreateCells()
        {
        }
    }
}
