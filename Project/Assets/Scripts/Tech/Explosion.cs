using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float TimeToDisappear = 2.0f;

    bool _particlesPlayed = false;

	void Awake ()
    {
        var mySequence = DOTween.Sequence();
        mySequence.AppendInterval(TimeToDisappear);
        mySequence.onComplete += OnExplosionFinished;
        mySequence.Play();
    }

    void OnExplosionFinished()
    {
        Destroy(this.gameObject);
    }

    void Update()
    {
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
