using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.Networking;

public class GSB_NetworkController : NetworkManager
{
    class NetworkMessage : MessageBase
    {
        public int AssignedBCSH;
    }

    short _playerControllerId = -1;
    public short PlayerControllerId { get { return _playerControllerId; } }

    GSB_PlayerOnlineController _playerOnlineController = null;
    public GSB_PlayerOnlineController PlayerOnlineController { get { return _playerOnlineController; } set { _playerOnlineController = value; } }

    public bool IsServer = false;
    bool _localClientRemoved = false;

    public List<GameObject> PlayerOnlineControllers = new List<GameObject>();

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnServerDisconnect: " + conn.connectionId);

        GSB_GameManager.Instance.NetworkGameState.NumPlayers--;

        base.OnServerDisconnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader messageReader)
    {
        //        var numOfAllowedPlayers = PlayerPrefs.GetInt(MainMenuController.kNumberOfPlayersKey) + 1;
        //        _numPlayers++;
        //        if (_numPlayers > numOfAllowedPlayers)
        //        {
        //            StopClient();
        //            return;
        //        }

        Debug.Log("OnServerAddPlayer");

        // TODO check to avoid joining more clients
        StartCoroutine(AddPlayer(conn, playerControllerId, messageReader));
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        Debug.Log("OnServerRemovePlayer");

        base.OnServerRemovePlayer(conn, player);
    }

    IEnumerator AddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader messageReader)
    {
        Debug.Log("AddPlayer");

        // we add this return just to avoid problems when trying to access to singletons that are not loaded in Awake
        yield return null;

        var message = messageReader.ReadMessage<NetworkMessage>();

        var player = GeneratePlayer(playerControllerId);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

        GSB_GameManager.Instance.NetworkGameState.NumPlayers++;
        GSB_GameManager.Instance.NetworkGameState.VersusPlayers.Add(player);
    }

    GameObject GeneratePlayer(short playerControllerId)
    {
        Debug.Log("NETWORK GeneratePlayer: " + playerControllerId + "    I AM " + _playerControllerId);

        if(GSB_GameManager.Instance.PlayerOnlineGO != null)
        {
            GameObject playerGO = Instantiate(GSB_GameManager.Instance.PlayerOnlineGO);
            if(playerGO != null)
            {
                if(playerControllerId == _playerControllerId)
                {
                    _playerOnlineController = playerGO.GetComponent<GSB_PlayerOnlineController>();
                }

                PlayerOnlineControllers.Add(playerGO);

                DontDestroyOnLoad(playerGO);
            }

            return playerGO;
        }

        return null;
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        _playerControllerId = (short) conn.connectionId;

        var clientConnectMsg = new NetworkMessage();

        Debug.Log("OnClientConnect: " + _playerControllerId);

        ClientScene.AddPlayer(conn, _playerControllerId, clientConnectMsg);

        _localClientRemoved = false;
    }



    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnClientDisconnect: " + _playerControllerId);

        ClientScene.RemovePlayer((short)conn.connectionId);

        if(!_localClientRemoved && conn.connectionId == _playerControllerId)
        {
            _localClientRemoved = true;
        }

        base.OnClientDisconnect(conn);
    }

    public override void OnStopClient()
    {
        Debug.Log("OnStopClient: " + _playerControllerId);

        if(!_localClientRemoved)
        {
            ClientScene.RemovePlayer(_playerControllerId);
        }

        _localClientRemoved = true;

        GSB_GameManager.Instance.NetworkGameState.NumPlayers--;

        base.OnStopClient();
    }
}
