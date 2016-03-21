using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SocialPoint.CrossPromotion;
using SocialPoint.Utils;

public class CrossPromoCellController : BaseCrossPromoCellController
{
    [SerializeField]
    protected Image _bannerImage;
    [SerializeField]
    protected Image _iconImage;
    [SerializeField]
    protected Image _buttonImage;
    [SerializeField]
    protected RectTransform _buttonContainer;

    //Guideline measures
    protected static float _defaultBannerWidth = 800f;
    protected static float _smallBannerHeight = 283f;
    protected static float _bigBannerHeight = 425f;
    protected static float _iconSize = 128f;
    protected static float _iconMargin = 10f;
    protected static float _buttonHeight = 64f;
    protected static float _buttonLeftMarginToCenterPercent = 0.73f;
    protected static float _buttonBottomMarginToCenterPercent = 0.2f;

    public override void InitCell(SocialPoint.CrossPromotion.CrossPromotionManager crossPromoManager, BaseCrossPromoPopupController popupController, int bannerId, int position)
    {
        base.InitCell(crossPromoManager, popupController, bannerId, position);

        CrossPromotionBannerData bannerData = _cpm.Data.BannerInfo[bannerId];
        UIUtils.SetImage(_bannerImage, _cpm.GetTexture2DForPopupImage(bannerData.BgImage));
        UIUtils.SetImage(_iconImage, _cpm.GetTexture2DForPopupImage(bannerData.IconImage));
        UIUtils.SetImage(_buttonImage, _cpm.GetTexture2DForPopupImage(bannerData.ButtonTextImage));
    }

    public void SetElementsSize(float width, float height)
    {
        LayoutElement cellLayout = this.GetComponent<LayoutElement>();
        cellLayout.preferredWidth = cellLayout.minWidth = width;
        cellLayout.preferredHeight = cellLayout.minHeight = height;

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
        //Current button image is 128x128, but visual sprite is only 80x72
        float imageVerticalSize = 128;
        float imageVisualVerticalSize = 72;

        float scale = width / _defaultBannerWidth;
        float newButtonHeight = _buttonHeight * scale;//Visual image must be this size
        float requiredSize = imageVerticalSize * newButtonHeight / imageVisualVerticalSize;
        float verticalOffset = requiredSize * 0.5f;
        float horizontalOffset = verticalOffset * 2.5f;
        _buttonContainer.anchorMin = new Vector2(_buttonLeftMarginToCenterPercent, _buttonBottomMarginToCenterPercent);
        _buttonContainer.anchorMax = new Vector2(_buttonLeftMarginToCenterPercent, _buttonBottomMarginToCenterPercent);
        _buttonContainer.offsetMin = new Vector2(-horizontalOffset, -verticalOffset);
        _buttonContainer.offsetMax = new Vector2(horizontalOffset, verticalOffset);
    }

    protected override void CheckVisibility()
    {
    }

    public override void SetBannerGrey()
    {
    }

    public override void SetBannerWhite()
    {
    }
}
