
using DG.Tweening;
using UnityEngine;

public class CP_Checkpoint : MonoBehaviour
{
    public GameObject Bar = null;

    CP_SceneManager _sceneManager = null;
    bool _isLastCheckpoint = false;

    public void SetVictoryCheckpoint(CP_SceneManager sceneManager)
    {
        _sceneManager = sceneManager;
        _isLastCheckpoint = true;
    }

    public void PlayAnimation()
    {
        if(Bar != null)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(Bar.transform.DOLocalRotate(new Vector3(0f, 200f, 0f), 0.15f, RotateMode.Fast).SetLoops(10));
            seq.Append(Bar.transform.DOLocalRotate(new Vector3(0f, 20f, 0f), 0.15f, RotateMode.Fast));
            seq.Play();
        }

        if(_isLastCheckpoint && _sceneManager != null)
        {
            _sceneManager.SetCurrentGameState(CP_SceneManager.BattleState.E_WIN);

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(3.0f);
            seq.onComplete += WinFinish;
            seq.Play();
        }
    }

    void WinFinish()
    {
        _sceneManager.SetCurrentGameState(CP_SceneManager.BattleState.E_WIN_AFTER);
    }
}
