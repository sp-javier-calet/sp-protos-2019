
using System.Collections.Generic;
using DG.Tweening;
using SocialPoint.Rendering.Components;
using UnityEngine;
using Timer = SocialPoint.Utils.Timer;

public class GSB_EnemyController : MonoBehaviour
{
    public enum EShipType
    {
        E_SHIP_BLUE,
        E_SHIP_GREEN,
        E_SHIP_ORANGE,
        E_SHIP_RED
    }

    public List<GameObject> ShipTypes = new List<GameObject>();
    public GameObject ShipTargeted = null;

    GSB_ShipData _shipData = null;
    BCSHModifier _shipTargetedBCSH;
    Timer _bcshTimer = new Timer();
    bool _firstSelectedShip = false;

    void Awake()
    {
        if(ShipTargeted != null)
        {
            _shipTargetedBCSH = ShipTargeted.GetComponent<BCSHModifier>();
            _bcshTimer.Wait(0f);

            ShipTargeted.transform.DOLocalRotate(new Vector3(0f, 0f, -360f), 1f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
            ShipTargeted.SetActive(false);
        }
    }

    public void SetShipType(EShipType shipType)
    {
        var shipTypeInt = (int) shipType;

        for(var i = 0; i < 4; ++i)
        {
            var shipTypeActive = (i == shipTypeInt) ? true : false;

            ShipTypes[i].SetActive(shipTypeActive);
            if(shipTypeActive)
            {
                _shipData = ShipTypes[i].GetComponent<GSB_ShipData>();
            }
        }
    }

    public void SetTargetEnabled(bool enabled, bool firstSelectedShip)
    {
        if(ShipTargeted != null)
        {
            ShipTargeted.SetActive(enabled);
        }

        _firstSelectedShip = firstSelectedShip;

        if(_shipTargetedBCSH != null)
        {
            var nextState = "dark";
            if(_firstSelectedShip)
            {
                nextState = "dark_first";
            }

            _shipTargetedBCSH.ApplyBCSHStateProgressive(nextState, 0, 0f);
            _bcshTimer.Wait(0f);
        }
    }

    void Update()
    {
        if(_shipTargetedBCSH != null && _bcshTimer.IsFinished)
        {
            var nextState = "bright";
            if(_shipTargetedBCSH.CurrentAppliedBCSHState == 1)
            {
                nextState = "dark";
            }

            if(_firstSelectedShip)
            {
                nextState = "bright_first";
                if(_shipTargetedBCSH.CurrentAppliedBCSHState == 3)
                {
                    nextState = "dark_first";
                }
            }

            _shipTargetedBCSH.ApplyBCSHStateProgressive(nextState, 0, 0.25f * Time.timeScale);
            _bcshTimer.Wait(0.25f * Time.timeScale);
        }

        if(_shipData != null)
        {
            transform.position += (Vector3.down * _shipData.MovementSpeed * Time.timeScale);
        }
    }
}
