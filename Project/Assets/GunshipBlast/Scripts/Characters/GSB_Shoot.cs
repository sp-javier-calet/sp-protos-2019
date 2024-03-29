﻿
using DG.Tweening;

public class GSB_Shoot : Translation
{
    public GSB_EnemyController TargetEnemy;
    bool _destroyed = false;

    void Awake()
    {
        TimeFinishedCallBack = TimeFinished;
    }

    void TimeFinished()
    {
        if(!_destroyed)
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
