using SocialPoint.CrossPromotion;
using UnityEngine;
using UnityEngine.UI;

public class CrossPromoCellController : BaseCrossPromoCellController
{
    [SerializeField]
    protected ScrollRect _scrollContainer;
    [SerializeField]
    protected Image _bannerImage;
    [SerializeField]
    protected Image _iconImage;
    [SerializeField]
    protected Image _buttonImage;
    [SerializeField]
    protected RectTransform _buttonContainer;
    [SerializeField]
    protected Button[] _buttons;

    //Guideline measures
    protected static float _defaultBannerWidth = 800f;
    protected static float _smallBannerHeight = 283f;
    protected static float _bigBannerHeight = 425f;
    protected static float _iconSize = 128f;
    protected static float _iconMargin = 10f;
    protected static float _buttonHeight = 64f;
    protected static float _buttonLeftMarginToCenterPercent = 0.73f;
    protected static float _buttonBottomMarginToCenterPercent = 0.2f;

    float _visibilityPointInScroll;
    float _separatorRatio;

    public override void InitCell(SocialPoint.CrossPromotion.CrossPromotionManager crossPromoManager, BaseCrossPromoPopupController popupController, int bannerId, int position)
    {
        base.InitCell(crossPromoManager, popupController, bannerId, position);

        int totalCells = crossPromoManager.Data.BannerInfo.Count;
        float cellHeightPercent = 1.0f / (totalCells + (totalCells - 1) * _separatorRatio);
        float separatorHeightPercent = cellHeightPercent * _separatorRatio;
        float scrollMaxPos = 1.0f - (float)position * (cellHeightPercent + separatorHeightPercent);
        _visibilityPointInScroll = scrollMaxPos - (cellHeightPercent * 0.49f);//Use 49% as mark for visibility to avoid missing events due to floating point precision for mid point calculation

        CrossPromotionBannerData bannerData = _cpm.Data.BannerInfo[bannerId];
        CrossPromoUIUtils.SetImage(_bannerImage, _cpm.GetTexture2DForPopupImage(bannerData.BgImage));
        CrossPromoUIUtils.SetImage(_iconImage, _cpm.GetTexture2DForPopupImage(bannerData.IconImage));
        CrossPromoUIUtils.SetImage(_buttonImage, _cpm.GetTexture2DForPopupImage(bannerData.ButtonTextImage));

        for(int i = 0; i < _buttons.Length; i++)
        {
            Button button = _buttons[i];
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickBanner);
        }
    }

    public void SetElementsSize(float width, float height, RectTransform separator, float separatorRatio)
    {
        _separatorRatio = separatorRatio;

        LayoutElement cellLayout = GetComponent<LayoutElement>();
        cellLayout.preferredWidth = cellLayout.minWidth = width;
        cellLayout.preferredHeight = cellLayout.minHeight = height;

        LayoutElement separatorLayout = separator.GetComponent<LayoutElement>();
        separatorLayout.preferredWidth = separatorLayout.minWidth = width;
        separatorLayout.preferredHeight = separatorLayout.minHeight = height * separatorRatio;

        SetIconSizeAndPos(width);
        SetNewGameFlagSizeAndPos(width);
        SetButtonSizeAndPos(width, height);
    }

    protected void SetIconSizeAndPos(float width)
    {
        float scale = width / _defaultBannerWidth;
        float newMargin = _iconMargin * scale;
        float newSize = _iconSize * scale;
        RectTransform iconRectTransform = _iconImage.GetComponent<RectTransform>();
        iconRectTransform.anchorMin = new Vector2(0.0f, 1.0f);
        iconRectTransform.anchorMax = new Vector2(0.0f, 1.0f);
        iconRectTransform.offsetMin = new Vector2(newMargin, -(newMargin + newSize));
        iconRectTransform.offsetMax = new Vector2(newMargin + newSize, -newMargin);
    }

    protected void SetNewGameFlagSizeAndPos(float width)
    {
        //Use icon measures for bottom aligment
        float scale = width / _defaultBannerWidth;
        float newMargin = _iconMargin * scale;
        float newSize = _iconSize * scale;
        float bottomOffset = newMargin + newSize;
        //Use preset offsets in prefab to calculate new scaled offsets
        RectTransform newGameRectTransform = _newGameFlag.GetComponent<RectTransform>();
        float scalePercent = Mathf.Abs(bottomOffset / newGameRectTransform.offsetMin.y);
        float topOffset = Mathf.Abs(newGameRectTransform.offsetMax.y * scalePercent);
        float rightOffset = Mathf.Abs(newGameRectTransform.offsetMax.x * scalePercent);
        float leftOffset = Mathf.Abs(newGameRectTransform.offsetMin.x * scalePercent);
        newGameRectTransform.anchorMin = new Vector2(1.0f, 1.0f);
        newGameRectTransform.anchorMax = new Vector2(1.0f, 1.0f);
        newGameRectTransform.offsetMin = new Vector2(-leftOffset, -bottomOffset);
        newGameRectTransform.offsetMax = new Vector2(rightOffset, topOffset);
    }

    protected void SetButtonSizeAndPos(float width, float height)
    {
        float scale = width / _defaultBannerWidth;
        float newButtonHeight = Mathf.Min(_buttonHeight * scale, _buttonBottomMarginToCenterPercent * height);//Visual image must be this size
        float verticalOffset = newButtonHeight * 0.75f;
        float horizontalOffset = verticalOffset * 3.0f;
        _buttonContainer.anchorMin = new Vector2(_buttonLeftMarginToCenterPercent, _buttonBottomMarginToCenterPercent);
        _buttonContainer.anchorMax = new Vector2(_buttonLeftMarginToCenterPercent, _buttonBottomMarginToCenterPercent);
        _buttonContainer.offsetMin = new Vector2(-horizontalOffset, -verticalOffset);
        _buttonContainer.offsetMax = new Vector2(horizontalOffset, verticalOffset);
    }

    protected override void CheckVisibility()
    {
        float heightFactorPercent = _cpm.Data.PopupHeightFactor / _cpm.Data.BannerInfo.Count;
        float visibility = (1 - _scrollContainer.verticalNormalizedPosition) * (1 - heightFactorPercent) + heightFactorPercent;
        if(visibility >= 1 - _visibilityPointInScroll)
        {
            SendVisibilityEvent();
        }
    }

    public override void SetBannerGrey()
    {
        _bannerImage.color = Color.grey;
    }

    public override void SetBannerWhite()
    {
        _bannerImage.color = Color.white;
    }
}
