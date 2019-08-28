
using UnityEngine;
using UnityEngine.Networking;

public class CP_PlayerOnlineController : NetworkBehaviour
{
    [SyncVar (hook = "SetCurrentBCSHApplied")]
    int _currentBCSHApplied = 0;

    CP_PlayerController _playerController = null;

    void Awake()
    {
        _playerController = transform.GetComponent<CP_PlayerController>();
    }

    public int CurrentBCSHApplied
    {
        set { SetCurrentBCSHApplied (value); }
        get { return _currentBCSHApplied; }
    }

    public void SetCurrentBCSHApplied(int bcshIndex)
    {
        _currentBCSHApplied = bcshIndex;

        if(_playerController != null && _playerController.BodyBCSH != null)
        {
            _playerController.BodyBCSH.ApplyBCSHState(_currentBCSHApplied);
        }
    }

    public override void OnStartLocalPlayer()
    {
        Debug.Log("OnStartLocalPlayer");

        base.OnStartLocalPlayer();
    }

    public override void OnStartServer()
    {
        Debug.Log("OnStartServer");

        base.OnStartServer();
    }
}
