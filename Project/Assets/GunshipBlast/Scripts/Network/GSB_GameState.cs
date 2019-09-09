
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GSB_GameState : NetworkBehaviour
{
    // SERVER ONLY SET DATA ////////////////////////////////////////////////////////////////////////////////

    [System.Serializable]
    public struct NetPlayerData
    {
        public int Id;
    };

    public class NetPlayerDatas : SyncListStruct<NetPlayerData> {}

    [SyncVar]
    NetPlayerDatas _netPlayerDatas = new NetPlayerDatas();

    ////////////////////////////////////////////////////////////////////////////////////////////////////////



    [SyncVar]
    int _numPlayers = 0;
    public int NumPlayers { set { _numPlayers = value; } get { return _numPlayers; } }

    public List<GameObject> VersusPlayers = new List<GameObject>();

    void Awake()
    {
        _netPlayerDatas.Callback = NetPlayerDatasChanged;
    }

    void NetPlayerDatasChanged(SyncList<NetPlayerData>.Operation op, int itemIndex)
    {
        Debug.Log("NetPlayerDatas changed:" + op);
    }

    public void ClearPlayerDatas()
    {
        _netPlayerDatas.Clear();
    }

    int prevNumPlayers = -1;
    int prevCount = -1;
    void Update()
    {
        if(prevNumPlayers != _numPlayers)
        {
            Debug.Log("NumPlayers: " + _numPlayers);
            prevNumPlayers = _numPlayers;
        }

        if(prevCount != _netPlayerDatas.Count)
        {
            Debug.Log("NetPlayerDatas count: " + _netPlayerDatas.Count);
            prevCount = _netPlayerDatas.Count;
        }
    }

    [ClientRpc]
    public void RpcStartVersusPlay()
    {
        GSB_GameManager.Instance.SetGameState(GSB_GameManager.GameState.E_PLAYING_2_VERSUS);
    }

    [ClientRpc]
    public void RpcPlayerHasDiedServer(int playerId)
    {
        if(playerId != GSB_GameManager.Instance.NetworkController.PlayerControllerId)
        {
            GSB_SceneManager.Instance.ChangeSubState(GSB_SceneManager.EBattleState.E_WIN);
        }
        else
        {
            GSB_SceneManager.Instance.ChangeSubState(GSB_SceneManager.EBattleState.E_LOSE);
        }
    }

    [ClientRpc]
    public void RpcPlayerHasSentShips(int playerId, int numShips)
    {
        if(playerId != GSB_GameManager.Instance.NetworkController.PlayerControllerId)
        {
            if(GSB_SceneManager.Instance.Player != null)
            {
                GSB_SceneManager.Instance.GenerateExtraInterWave(numShips);
            }
        }
    }

    public override int GetNetworkChannel()
    {
        return Channels.DefaultUnreliable;
    }

    public override float GetNetworkSendInterval()
    {
        return 0.01f;
    }
}
