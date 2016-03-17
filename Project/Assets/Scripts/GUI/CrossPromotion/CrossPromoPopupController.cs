using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using SocialPoint.CrossPromotion;
using SocialPoint.Utils;

public class CrossPromoPopupController : BasePopupCrossPromoController
{
    [SerializeField]
    protected RectTransform _mainContainer;

    [SerializeField]
    protected CrossPromoCellController _cellPrototype;

    [SerializeField]
    protected Image _titleImage;

    [SerializeField]
    protected Button _closeButton;

    public override void Init(SocialPoint.CrossPromotion.CrossPromotionManager crossPromoManager, Action closeCallback)
    {
        base.Init(crossPromoManager, closeCallback);

        UIUtils.SetImage(_titleImage, _cpm.GetTexture2DForPopupImage(_cpm.Data.PopupTitleImage));

        _closeButton.onClick.RemoveAllListeners();
        _closeButton.onClick.AddListener(OnClose);
    }

    protected override Vector2 GetScreenSize()
    {
        return new Vector2(Screen.width, Screen.height);
    }

    protected override Vector2 GetPopupSize()
    {
        return new Vector2(_mainContainer.rect.width, _mainContainer.rect.height);
    }

    protected override void SetPopupSize()
    {
        //Rect newPopupRect = new Rect(0, 0, Mathf.CeilToInt(CellWidth), Mathf.CeilToInt(CellHeight * _cpm.Data.PopupHeightFactor));
        //_mainContainer.rect = newPopupRect;
        float halfWidth = Screen.width / 2;
        float halfHeight = Screen.height / 2;
        _mainContainer.anchorMin = new Vector2(0.5f, 0.5f);
        _mainContainer.anchorMax = new Vector2(0.5f, 0.5f);
        _mainContainer.offsetMin = new Vector2(-halfWidth * 0.8f, -halfHeight * 0.8f);
        _mainContainer.offsetMax = new Vector2(halfWidth * 0.8f, halfHeight * 0.8f);
    }

    protected override void CreateCells()
    {
        _cellPrototype.gameObject.SetActive(false);
        int position = 0;
        foreach(var keyValue in _cpm.Data.BannerInfo)
        {
            CrossPromoCellController newCell = GameObject.Instantiate(_cellPrototype) as CrossPromoCellController;
            newCell.transform.SetParent(_cellPrototype.transform.parent);
            newCell.transform.localScale = _cellPrototype.transform.localScale;
            newCell.InitCell(_cpm, this, keyValue.Value.Uid, position);
            newCell.gameObject.SetActive(true);

            ++position;
        }
    }

}
