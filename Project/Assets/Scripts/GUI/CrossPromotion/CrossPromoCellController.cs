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

    //Guideline measures
    protected static float _defaultBannerWidth = 800f;
    protected static float _smallBannerHeight = 283f;
    protected static float _bigBannerHeight = 425f;
    protected static float _iconSize = 128f;
    protected static float _iconMargin = 10f;
    protected static float _buttonHeight = 64f;
    protected static float _buttonLeftMarginToCenter = 584f;
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
        //TODO: New Game Size, Button Size
    }

    protected void SetIconSizeAndPos(float width)
    {
        float newMargin = width * _iconMargin / _defaultBannerWidth;
        float newSize = width * _iconSize / _defaultBannerWidth;
        RectTransform iconRectTransform = _iconImage.GetComponent<RectTransform>();
        iconRectTransform.anchorMin = new Vector2(0.0f, 1.0f);
        iconRectTransform.anchorMax = new Vector2(0.0f, 1.0f);
        iconRectTransform.offsetMin = new Vector2(newMargin, -(newMargin + newSize));
        iconRectTransform.offsetMax = new Vector2(newMargin + newSize, -newMargin);
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
