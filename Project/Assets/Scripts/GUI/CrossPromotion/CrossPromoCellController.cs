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

    public override void InitCell(SocialPoint.CrossPromotion.CrossPromotionManager crossPromoManager, BasePopupCrossPromoController popupController, int bannerId, int position)
    {
        base.InitCell(crossPromoManager, popupController, bannerId, position);

        CrossPromotionBannerData bannerData = _cpm.Data.BannerInfo[bannerId];
        UIUtils.SetImage(_bannerImage, _cpm.GetTexture2DForPopupImage(bannerData.BgImage));
        UIUtils.SetImage(_iconImage, _cpm.GetTexture2DForPopupImage(bannerData.IconImage));
        UIUtils.SetImage(_buttonImage, _cpm.GetTexture2DForPopupImage(bannerData.ButtonTextImage));
    }

    protected override void CheckVisibilty()
    {
    }

    public override void SetBannerGrey()
    {
    }

    public override void SetBannerWhite()
    {
    }
}
