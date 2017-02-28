using System;
using System.Collections;
using SocialPoint.CrossPromotion;
using UnityEngine;
using UnityEngine.UI;

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
    protected RectTransform _separatorPrototype;

    [SerializeField]
    protected float _separatorCellRatio;

    [SerializeField]
    protected Image _titleImage;

    [SerializeField]
    protected Button _closeButton;

    public override void Init(SocialPoint.CrossPromotion.CrossPromotionManager crossPromoManager, Action closeCallback)
    {
        base.Init(crossPromoManager, closeCallback);

        CrossPromoUIUtils.SetImage(_titleImage, _cpm.GetTexture2DForPopupImage(_cpm.Data.PopupTitleImage));

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

    protected Vector2 GetVirtualScreenSize()
    {
        CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
        return canvasScaler.referenceResolution;
    }

    /**
     * Helper function to get current size of some UI elements in the popup.
     * If current size is 0 in both axis its probably because its not yet 
     * initialized in screen by Unity nor set through script,
     * in this case an original estimated size is returned.
     * */
    Vector2 GetSize(RectTransform rTransform)
    {
        var currentSize = new Vector2(rTransform.rect.width, rTransform.rect.height);
        if(Math.Abs(currentSize.x) < Single.Epsilon && Math.Abs(currentSize.y) < Single.Epsilon)
        {
            currentSize = GetOriginalSize(rTransform);
        }
        return currentSize;
    }

    /**
     * Helper function to get original size of some UI elements in the popup.
     * */
    Vector2 GetOriginalSize(RectTransform rTransform)
    {
        float widthPercent = rTransform.anchorMax.x - rTransform.anchorMin.x;
        float heightPercent = rTransform.anchorMax.y - rTransform.anchorMin.y;
        Vector2 screenSize = GetVirtualScreenSize();
        return new Vector2(screenSize.x * widthPercent, screenSize.y * heightPercent);
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
        float desiredCellAreaHeight = (_cellHeight * _cpm.Data.PopupHeightFactor)
                                      + (_cellHeight * _separatorCellRatio * (Mathf.CeilToInt(_cpm.Data.PopupHeightFactor) - 1));
        float growFactor = desiredCellAreaHeight / currentCellAreaSize.y;
        Vector2 currentPopupSize = GetPopupSize();
        var desiredPopupSize = new Vector2(currentPopupSize.x, currentPopupSize.y * growFactor);
        float finalPopupAspectRatio = desiredPopupSize.x / desiredPopupSize.y;
        return finalPopupAspectRatio;
    }

    protected Vector2 GetSizeToFit(float aspectRatio)
    {
        Vector2 screenSize = GetVirtualScreenSize();
        float maxHorizontalSize = screenSize.x - (2 * _minHorizontalMargin);
        float maxVerticalSize = screenSize.y - (2 * _minVerticalMargin);

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
        _separatorPrototype.gameObject.SetActive(false);
        _cellPrototype.SetElementsSize(_cellWidth, _cellHeight, _separatorPrototype, _separatorCellRatio);

        int position = 0;
        var iter = _cpm.Data.BannerInfo.GetEnumerator();
        while(iter.MoveNext())
        {
            var keyValue = iter.Current;
            if(position > 0)
            {
                Clone<RectTransform>(_separatorPrototype);
            }

            CrossPromoCellController newCell = Clone<CrossPromoCellController>(_cellPrototype);
            newCell.InitCell(_cpm, this, keyValue.Value.Uid, position);

            ++position;
        }
        iter.Dispose();
    }

    static T Clone<T>(T prototype) where T : Component
    {
        var newObject = UnityEngine.Object.Instantiate(prototype);
        newObject.transform.SetParent(prototype.transform.parent);
        newObject.transform.localScale = prototype.transform.localScale;
        newObject.gameObject.SetActive(true);
        return newObject;
    }

    void OnEnable()
    {
        //Set scroll position to the top
        StartCoroutine(SetInitialPosition());
    }

    IEnumerator SetInitialPosition()
    {
        yield return null;
        ScrollRect scrollRect = _cellContainer.GetComponent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 1.0f;
        scrollRect.velocity = Vector2.zero;
    }
}
