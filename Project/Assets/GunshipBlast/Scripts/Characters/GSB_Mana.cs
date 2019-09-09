
using DG.Tweening;
using UnityEngine;

public class GSB_Mana : Translation
{
    bool _destroyed = false;
    bool _particlesPlayed = false;

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

    public override void Update()
    {
        base.Update();
        
        if (!_particlesPlayed)
        {
            ParticleSystem[] particles = transform.GetComponentsInChildren<ParticleSystem>();
            if (particles != null && particles.Length > 0)
            {
                for (var i = 0 ; i < particles.Length; ++i)
                {
                    if (!particles[i].isPlaying)
                    {
                        particles[i].Play();
                    }
                }
            }

            _particlesPlayed = true;
        }
    }
}
