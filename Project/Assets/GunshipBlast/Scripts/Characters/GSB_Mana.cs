
using DG.Tweening;

public class GSB_Mana : Translation
{
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
