using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SocialPoint.CrossPromotion;

public class CrossPromoCellController : BaseCrossPromoCellController
{
    [SerializeField]
    protected Image _bannerImage;
    [SerializeField]
    protected Image _iconImage;
    [SerializeField]
    protected Image _buttonImage;

    private static Vector2 _defaultSpritePivot = new Vector2(0.5f, 0.5f);

    public override void InitCell(SocialPoint.CrossPromotion.CrossPromotionManager crossPromoManager, BasePopupCrossPromoController popupController, int bannerId, int position)
    {
        base.InitCell(crossPromoManager, popupController, bannerId, position);

        //gameObject.GetComponent<UIWidget>().width = Mathf.CeilToInt(popupController.CellWidth);
        //gameObject.GetComponent<UIWidget>().height = Mathf.CeilToInt(popupController.CellHeight);

        CrossPromotionBannerData bannerData = _cpm.Data.BannerInfo[bannerId];
        SetImage(_bannerImage, bannerData.BgImage);
        SetImage(_iconImage, bannerData.IconImage);
        SetImage(_buttonImage, bannerData.ButtonTextImage);

        //_panel = GetComponent<UIDragScrollView>().scrollView.panel;
    }

    private void SetImage(Image target, string url)
    {
        Texture2D imgTexture = _cpm.GetTexture2DForPopupImage(url);
        if(imgTexture != null)
        {
            Rect spriteRect = new Rect(0, 0, imgTexture.width, imgTexture.height);
            target.sprite = Sprite.Create(imgTexture, spriteRect, _defaultSpritePivot);
        }
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
