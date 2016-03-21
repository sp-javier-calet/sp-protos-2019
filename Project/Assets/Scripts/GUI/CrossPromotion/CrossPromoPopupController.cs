using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using SocialPoint.CrossPromotion;
using SocialPoint.Utils;

public class CrossPromoPopupController : BaseCrossPromoPopupController
{
    //Popup parent/head container
    [SerializeField]
    protected RectTransform _mainContainer;

    //Scroll area for the banners
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

    protected override Vector2 GetPopupSize()
    {
        return GetSize(_mainContainer);
    }

    protected Vector2 GetCellAreaSize()
    {
        return GetSize(_cellContainer);
    }

    /**
     * Helper function to get current size of some UI elements in the popup.
     * If current size is 0 in both axis its probably because its not yet 
     * initialized in screen by Unity nor set through script,
     * in this case an original estimated size is returned.
     * */
    private static Vector2 GetSize(RectTransform rTransform)
    {
        Vector2 currentSize = new Vector2(rTransform.rect.width, rTransform.rect.height);
        if(currentSize.x == 0 && currentSize.y == 0)
        {
            currentSize = GetOriginalSize(rTransform);
        }
        return currentSize;
    }

    /**
     * Helper function to get original size of some UI elements in the popup.
     * */
    private static Vector2 GetOriginalSize(RectTransform rTransform)
    {
        float widthPercent = rTransform.anchorMax.x - rTransform.anchorMin.x;
        float heightPercent = rTransform.anchorMax.y - rTransform.anchorMin.y;
        return new Vector2(Screen.width * widthPercent, Screen.height * heightPercent);
    }

    protected override void SetPopupSize()
    {
        UpdateCellMeasures();

        float desiredAspectRatio = CalculateDesiredAspecRatio();
        Vector2 offsets = GetSizeToFit(desiredAspectRatio) * 0.5f;

        //Set final popup size
        _mainContainer.anchorMin = new Vector2(0.5f, 0.5f);
        _mainContainer.anchorMax = new Vector2(0.5f, 0.5f);
        _mainContainer.offsetMin = new Vector2(-offsets.x, -offsets.y);
        _mainContainer.offsetMax = new Vector2(offsets.x, offsets.y);

        UpdateCellMeasures();
    }

    protected void UpdateCellMeasures()
    {
        Vector2 cellAreaSize = GetCellAreaSize();
        _cellWidth = cellAreaSize.x;
        _cellHeight = _cellWidth / _cpm.Data.AspectRatio;
    }

    protected float CalculateDesiredAspecRatio()
    {
        Vector2 currentCellAreaSize = GetCellAreaSize();
        float desiredCellAreaHeight = _cellHeight * _cpm.Data.PopupHeightFactor;
        float growFactor = desiredCellAreaHeight / currentCellAreaSize.y;
        Vector2 currentPopupSize = GetPopupSize();
        Vector2 desiredPopupSize = new Vector2(currentPopupSize.x, currentPopupSize.y * growFactor);
        float finalPopupAspectRatio = desiredPopupSize.x / desiredPopupSize.y;
        return finalPopupAspectRatio;
    }

    protected Vector2 GetSizeToFit(float aspectRatio)
    {
        float maxHorizontalSize = Screen.width - (2 * _minHorizontalMargin);
        float maxVerticalSize = Screen.height - (2 * _minVerticalMargin);

        float horizontalSize = maxHorizontalSize;
        float verticalSize = horizontalSize / aspectRatio;
        if(verticalSize > maxVerticalSize)
        {
            verticalSize = maxVerticalSize;
            horizontalSize = verticalSize * aspectRatio;
        }

        return new Vector2(horizontalSize, verticalSize);
    }

    protected override void CreateCells()
    {
        //Prepare prototype for cloning
        _cellPrototype.gameObject.SetActive(false);
        _cellPrototype.SetElementsSize(_cellWidth, _cellHeight);

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

    void OnEnable()
    {
        //Set scroll position to the top
        StartCoroutine(SetInitialPosition());
    }

    private IEnumerator SetInitialPosition()
    {
        yield return null;
        ScrollRect scrollRect = _cellContainer.GetComponent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 1.0f;
        scrollRect.velocity = Vector2.zero;
    }
}
