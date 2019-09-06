
using UnityEngine.Networking;

public class GSB_PlayerOnlineController : NetworkBehaviour
{
    NetworkIdentity _networkIdentity;

    [Command]
    public void CmdPlayerHasDiedClient(int playerId)
    {
        GSB_GameManager.Instance.NetworkGameState.RpcPlayerHasDiedServer(playerId);
    }

    public override void OnStartLocalPlayer()
    {
        _networkIdentity = GetComponent<NetworkIdentity>();
        if(_networkIdentity != null)
        {
            if(_networkIdentity.isClient)
            {
                GSB_GameManager.Instance.NetworkController.PlayerOnlineController = this;
                GSB_GameManager.Instance.NetworkController.PlayerOnlineControllers.Add(gameObject);
            }
        }

        DontDestroyOnLoad(transform.gameObject);

        base.OnStartLocalPlayer();
    }
}
