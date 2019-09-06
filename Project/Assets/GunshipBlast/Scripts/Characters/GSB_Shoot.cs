
using DG.Tweening;
using SocialPoint.Utils;
using UnityEngine;

public class GSB_Shoot : MonoBehaviour
{
    public Vector3 OriginPosition;
    public Vector3 DestPosition;
    public int TimeTravel;
    public GSB_EnemyController TargetEnemy;

    long _startTime;
    Vector3 _diffSpace;
    bool _destroyed = false;

    void Start()
    {
        _startTime = TimeUtils.TimestampMilliseconds;
        _diffSpace = DestPosition - OriginPosition;
    }

    void Update()
    {
        float delta = (TimeUtils.TimestampMilliseconds - _startTime) / (float)TimeTravel;
        if(delta > 1f)
            delta = 1f;

        transform.position = OriginPosition + (_diffSpace * delta);

        if(!_destroyed && TimeUtils.TimestampMilliseconds > _startTime + TimeTravel)
        {
            _destroyed = true;

            if(TargetEnemy != null)
            {
                TargetEnemy.DestroyShip();
            }

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.1f);
            seq.onComplete += DestroyShoot;
        }
    }

    void DestroyShoot()
    {
        Destroy(gameObject);
    }
}
