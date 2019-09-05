using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float TimeToDisappear = 2.0f;

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
}
