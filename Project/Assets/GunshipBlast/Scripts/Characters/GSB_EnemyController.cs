
using System.Collections.Generic;
using DG.Tweening;
using SocialPoint.Rendering.Components;
using UnityEngine;
using Timer = SocialPoint.Utils.Timer;

public class GSB_EnemyController : MonoBehaviour
{
    public List<GameObject> ShipTypes = new List<GameObject>();
    public GameObject ShipTargeted = null;

    BCSHModifier _shipTargetedBCSH;
    Timer _bcshTimer = new Timer();

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

    void Update()
    {
        if(_shipTargetedBCSH != null && _bcshTimer.IsFinished)
        {
            var nextState = "bright";
            if(_shipTargetedBCSH.CurrentAppliedBCSHState == 1)
            {
                nextState = "dark";
            }

            _shipTargetedBCSH.ApplyBCSHStateProgressive(nextState, 0, 0.25f * Time.timeScale);
            _bcshTimer.Wait(0.25f * Time.timeScale);
        }
    }

    public void SetTargetEnabled(bool enabled)
    {
        if(ShipTargeted != null)
        {
            ShipTargeted.SetActive(enabled);
        }
    }
}
