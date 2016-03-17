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
    protected RectTransform _cellContainer;

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

    protected override Vector2 GetOriginalPopupSize()
    {
        float widthPercent = _mainContainer.anchorMax.x - _mainContainer.anchorMin.x;
        float heightPercent = _mainContainer.anchorMax.y - _mainContainer.anchorMin.y;
        return new Vector2(Screen.width * widthPercent, Screen.height * heightPercent);
    }

    protected override Vector2 GetPopupSize()
    {
        return new Vector2(_mainContainer.rect.width, _mainContainer.rect.height);
    }

    protected override Vector2 GetOriginalCellAreaSize()
    {
        float widthPercent = _cellContainer.anchorMax.x - _cellContainer.anchorMin.x;
        float heightPercent = _cellContainer.anchorMax.y - _cellContainer.anchorMin.y;
        return new Vector2(Screen.width * widthPercent, Screen.height * heightPercent);
    }

    protected override Vector2 GetCellAreaSize()
    {
        return new Vector2(_cellContainer.rect.width, _cellContainer.rect.height);
    }

    protected override void SetPopupSize()
    {
        Vector2 currentCellAreaSize = GetOriginalCellAreaSize();
        float desiredCellAreaHeight = CellHeight * _cpm.Data.PopupHeightFactor;
        float growFactor = desiredCellAreaHeight / currentCellAreaSize.y;
        Vector2 currentPopupSize = GetOriginalPopupSize();//GetPopupSize();
        Vector2 desiredPopupSize = new Vector2(currentPopupSize.x, currentPopupSize.y * growFactor);
        float finalPopupAspectRatio = desiredPopupSize.x / desiredPopupSize.y;

        //Rect newPopupRect = new Rect(0, 0, Mathf.CeilToInt(CellWidth), Mathf.CeilToInt(CellHeight * _cpm.Data.PopupHeightFactor));
        //_mainContainer.rect = newPopupRect;
        float horizontalOffset = (Screen.width * 0.5f) - Margin;
        float verticalOffset = horizontalOffset / finalPopupAspectRatio;

        _mainContainer.anchorMin = new Vector2(0.5f, 0.5f);
        _mainContainer.anchorMax = new Vector2(0.5f, 0.5f);
        _mainContainer.offsetMin = new Vector2(-horizontalOffset, -verticalOffset);
        _mainContainer.offsetMax = new Vector2(horizontalOffset, verticalOffset);

        Vector2 cellAreaSize = GetCellAreaSize();
        CellWidth = cellAreaSize.x;
        CellHeight = CellWidth / _cpm.Data.AspectRatio;

        /*UIWidget containerScroll = GridObj.transform.parent.parent.GetComponent<UIWidget>();
        containerScroll.width = Mathf.CeilToInt(CellWidth);
        containerScroll.height = Mathf.CeilToInt(CellHeight * _cpm.Data.PopupHeightFactor);

        PopupSprite.ResetAndUpdateAnchors();*/
    }

    protected override void CreateCells()
    {
        _cellPrototype.gameObject.SetActive(false);
        LayoutElement cellLayout = _cellPrototype.GetComponent<LayoutElement>();
        cellLayout.preferredWidth = cellLayout.minWidth = CellWidth;
        cellLayout.preferredHeight = cellLayout.minHeight = CellHeight;

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
