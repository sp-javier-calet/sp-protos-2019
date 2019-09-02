
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CP_GameState : NetworkBehaviour
{
    // SERVER ONLY SET DATA ////////////////////////////////////////////////////////////////////////////////

    [System.Serializable]
    public struct NetPlayerData
    {
        public int Id;
        public int AssignedBCSH;
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

    public int GetPlayerBCSH(int playerId)
    {
        for(var i = 0; i < _netPlayerDatas.Count; ++i)
        {
            if(_netPlayerDatas[i].Id == playerId)
            {
                return _netPlayerDatas[i].AssignedBCSH;
            }
        }

        return -1;
    }

    public int GetFreeBCSH()
    {
        for(var j = 1; j < 5; ++j)
        {
            var found = false;
            for(var i = 0; i < _netPlayerDatas.Count; ++i)
            {
                if(_netPlayerDatas[i].AssignedBCSH == j)
                {
                    found = true;
                    break;
                }
            }

            if(!found)
            {
                return j;
            }
        }

        return -1;
    }

    [Command]
    public void CmdSetPlayerBCSH(int playerId, int bcshIndex)
    {
        for(var i = 0; i < _netPlayerDatas.Count; ++i)
        {
            if(_netPlayerDatas[i].Id == -1)
            {
                NetPlayerData data = _netPlayerDatas[i];
                data.AssignedBCSH = bcshIndex;

                _netPlayerDatas[i] = data;

                return;
            }
        }

        NetPlayerData newData = new NetPlayerData();
        newData.Id = playerId;
        newData.AssignedBCSH = bcshIndex;

        _netPlayerDatas.Add(newData);
    }

    public void RemovePlayerBCSH(int playerId)
    {
        for(var i = 0; i < _netPlayerDatas.Count; ++i)
        {
            if(_netPlayerDatas[i].Id == playerId)
            {
                _netPlayerDatas.RemoveAt(i);

                return;
            }
        }
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

    public override int GetNetworkChannel()
    {
        return Channels.DefaultUnreliable;
    }

    public override float GetNetworkSendInterval()
    {
        return 0.01f;
    }
}
