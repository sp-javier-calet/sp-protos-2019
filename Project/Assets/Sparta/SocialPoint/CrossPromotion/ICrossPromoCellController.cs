﻿using UnityEngine;
using System.Collections;

namespace SocialPoint.CrossPromotion
{
    public class ICrossPromoCellController : MonoBehaviour
    {
        public GameObject NewGame;

        protected int _bannerId;
        protected int _position;
        bool _selfGame;
        bool _isNew;


        protected CrossPromotionManager _cpm;
        protected IPopupCrossPromoController _pc;
        protected bool _sentTrack = false;

        public virtual void InitCell(CrossPromotionManager crossPromoManager, IPopupCrossPromoController popupController, int bannerId, int position)
        {
            _cpm = crossPromoManager;
            _pc = popupController;
            _bannerId = bannerId;
            _position = position;

            CrossPromotionBannerData bannerData = _cpm.Data.bannerInfo[bannerId];
            _selfGame = bannerData.currentGame;
            _isNew = bannerData.showRibbon;

            NewGame.SetActive(_isNew);
        }

        public void OnClickBanner()
        {
            OnButtonPressed();
        }

        public void OnButtonPressed()
        {
            bool isUrgent = !_selfGame;
            _pc.SetActivityView(isUrgent);
            _cpm.SendBannerClickedEvent(_bannerId, _position, isUrgent, _selfGame, () => {
                _pc.OnClose();
            });
        }

        private void Update()
        {
            CheckVisibilty();
        }

        protected virtual void CheckVisibilty()
        {
        }

        protected void SendVisibilityEvent()
        {
            if(!_sentTrack)
            {
                _cpm.SendBannerImpressedEvent(_bannerId, _position);
                _sentTrack = true;
            }
        }

        public virtual void SetBannerGrey()
        {
        }

        public virtual void SetBannerWhite()
        {
        }
    }
}
