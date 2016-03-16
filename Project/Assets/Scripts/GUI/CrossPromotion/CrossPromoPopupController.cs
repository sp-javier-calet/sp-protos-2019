using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using SocialPoint.CrossPromotion;

public class CrossPromoPopupController : BasePopupCrossPromoController
{
    [SerializeField]
    protected CrossPromoCellController _cellPrefab;

    [SerializeField]
    protected Button _closeButton;

    public override void Init(SocialPoint.CrossPromotion.CrossPromotionManager crossPromoManager, Action closeCallback)
    {
        base.Init(crossPromoManager, closeCallback);

        _closeButton.onClick.RemoveAllListeners();
        _closeButton.onClick.AddListener(OnClose);
    }

    protected override void SetPopupSize()
    {
    }

    protected override void CreateCells()
    {
        _cellPrefab.gameObject.SetActive(false);
        //GridObj.GetComponent<UIGrid>().cellHeight = CellHeight;
        //GridObj.GetComponent<UIGrid>().cellWidth = CellWidth;
        int position = 0;
        foreach(var keyValue in _cpm.Data.BannerInfo)
        {
            CrossPromoCellController newCell = GameObject.Instantiate(_cellPrefab) as CrossPromoCellController;
            newCell.transform.SetParent(_cellPrefab.transform.parent);
            newCell.transform.localScale = _cellPrefab.transform.localScale;
            newCell.InitCell(_cpm, this, keyValue.Value.Uid, position);
            newCell.gameObject.SetActive(true);
            Debug.Log(keyValue.Value.BgImage);

            ++position;
        }
    }

}
