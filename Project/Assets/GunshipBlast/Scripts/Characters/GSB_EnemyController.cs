
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
    public GameObject VSIcon = null;
    public GameObject ShipTargeted = null;
    public GameObject ExplosionInPlayerShip = null;
    public GameObject ExplosionDestroyed = null;

    EShipType _shipType = EShipType.E_SHIP_RED;
    public EShipType ShipType { get { return _shipType; } }

    GSB_ShipData _shipData = null;
    BCSHModifier _shipTargetedBCSH;
    Timer _bcshTimer = new Timer();
    RaycastHit _hitDown;
    bool _firstSelectedShip = false;
    float _waveSpeedMultiplier = 1;

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

    public void SetShipType(EShipType shipType, bool versusShip = false)
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

        if(versusShip && VSIcon != null)
        {
            VSIcon.SetActive(true);
        }

        _shipType = shipType;
    }

    public void SetWaveSpeedMultiplier(float waveSpeedMultiplier)
    {
        _waveSpeedMultiplier = waveSpeedMultiplier;
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

        if (enabled)
        {
            GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_selectship");
        }
    }

    bool GetHitDistance(out float distance, out RaycastHit hit, Vector3 initPosition, Vector3 direction, float maxDistance = 0.0001f, int layerMask = 1 << 9)
    {
        distance = 0f;

        Ray downRay = new Ray(initPosition, direction);
        if (Physics.Raycast(downRay, out hit, maxDistance, layerMask))
        {
            distance = hit.distance;
            return true;
        }

        return false;
    }

    public void DestroyShip(bool inPlayerShip = false)
    {
        GSB_SceneManager.Instance.Player.ShipHasBeenDestroyed(this);

        if(GSB_SceneManager.Instance.Enemies.Contains(this))
        {
            GSB_SceneManager.Instance.Enemies.Remove(this);
        }

        if(inPlayerShip && ExplosionInPlayerShip != null)
        {
            GameObject explosion = Instantiate(ExplosionInPlayerShip);
            if(explosion != null)
            {
                explosion.transform.position = transform.position;
            }

            GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_damage");
        }

        if(ExplosionDestroyed != null)
        {
            GameObject explosion = Instantiate(ExplosionDestroyed);
            if(explosion != null)
            {
                explosion.transform.position = transform.position;
            }

            if(!inPlayerShip && GSB_SceneManager.Instance.Player != null)
            {
                GSB_SceneManager.Instance.Player.GenerateManaParticles(transform.position);
            }

            GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_explosion");
        }

        transform.DOScale(Vector3.zero, 0.4f).onComplete += DestroyShipInternal;
    }

    void DestroyShipInternal()
    {
        Destroy(gameObject);
    }

    void FixedUpdate()
    {
        if(_shipData != null && !GSB_SceneManager.Instance.Player.Shooting)
        {
            var speed = _shipData.MovementSpeed;
            speed *= _waveSpeedMultiplier;
            speed *= (Time.timeScale * (Time.fixedDeltaTime / Time.deltaTime));

            transform.position += (Vector3.down * speed);

            var dist = 0f;
            if (GetHitDistance(out dist, out _hitDown, transform.position, -Vector3.up, speed))
            {
                GSB_SceneManager.Instance.Player.MakeDamage(_shipData.DamagePoints);

                DestroyShip(true);
            }
        }
    }

    void LateUpdate()
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
    }
}
