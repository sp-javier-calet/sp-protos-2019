using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	void Awake ()
    {
        var mySequence = DOTween.Sequence();
        mySequence.AppendInterval(2.0f);
        mySequence.onComplete += OnExplosionFinished;
        mySequence.Play();
    }

    void OnExplosionFinished()
    {
        Destroy(this.gameObject);
    }
}
