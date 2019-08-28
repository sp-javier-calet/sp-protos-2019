
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CP_GameState : NetworkBehaviour
{
    public const int kMaxPlayers = 4;

    // SERVER ONLY SET DATA ////////////////////////////////////////////////////////////////////////////////

    [SyncVar]
    public int[] PlayerAssignedBCSHId = new int[kMaxPlayers];
    [SyncVar]
    public int[] PlayerAssignedBCSHIndex = new int[kMaxPlayers];

    ////////////////////////////////////////////////////////////////////////////////////////////////////////



    [SyncVar]
    int _numPlayers = 0;
    public int NumPlayers { set { _numPlayers = value; Debug.Log("NumPlayers: " + _numPlayers); } get { return _numPlayers; } }

    public List<GameObject> VersusPlayers = new List<GameObject>();

    void Awake()
    {
        for(var i = 0; i < kMaxPlayers; ++i)
        {
            PlayerAssignedBCSHId[i] = -1;
            PlayerAssignedBCSHIndex[i] = -1;
        }
    }

    public int GetPlayerBCSH(int playerId)
    {
        for(var i = 0; i < kMaxPlayers; ++i)
        {
            if(PlayerAssignedBCSHId[i] == playerId)
            {
                return PlayerAssignedBCSHIndex[i];
            }
        }

        return -1;
    }

    public int GetFreeBCSH()
    {
        for(var i = 0; i < kMaxPlayers; ++i)
        {
            if(PlayerAssignedBCSHIndex[i] == -1)
            {
                return (i+1);
            }
        }

        return -1;
    }

    public void SetPlayerBCSH(int playerId, int bcshIndex)
    {
        for(var i = 0; i < kMaxPlayers; ++i)
        {
            if(PlayerAssignedBCSHId[i] == -1)
            {
                PlayerAssignedBCSHId[i] = playerId;
                PlayerAssignedBCSHIndex[i] = bcshIndex;

                return;
            }
        }
    }

    public void RemovePlayerBCSH(int playerId)
    {
        for(var i = 0; i < kMaxPlayers; ++i)
        {
            if(PlayerAssignedBCSHId[i] == playerId)
            {
                PlayerAssignedBCSHId[i] = -1;
                PlayerAssignedBCSHIndex[i] = -1;

                return;
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
