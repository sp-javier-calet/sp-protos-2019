
using DG.Tweening;
using UnityEngine;

public class CP_Checkpoint : MonoBehaviour
{
    public GameObject Bar = null;

    public void PlayAnimation()
    {
        if(Bar != null)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(Bar.transform.DOLocalRotate(new Vector3(0f, 200f, 0f), 0.15f, RotateMode.Fast).SetLoops(10));
            seq.Append(Bar.transform.DOLocalRotate(new Vector3(0f, 20f, 0f), 0.15f, RotateMode.Fast));
            seq.Play();
        }
    }
}
