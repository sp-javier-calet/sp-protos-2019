using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.Networking;

public class GSB_NetworkController : NetworkManager
{



    /*
    [HideInInspector]
    public int ChosenCharacter = 0;
    public List<GameObject> Players = new List<GameObject>();

    private int _numPlayers;

    //subclass for sending network messages
    private class NetworkMessage : MessageBase
    {
        public int ChosenCharacter;
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

        // TODO check to avoid joining more clients
        StartCoroutine(AddPlayer(conn, playerControllerId, messageReader));
    }



    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        //base.OnClientSceneChanged(conn);
    }
    */

    class NetworkMessage : MessageBase
    {
        public int AssignedBCSH;
    }

    short _playerControllerId = -1;

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

        var player = GeneratePlayer();
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

        GSB_GameManager.Instance.NetworkGameState.NumPlayers++;
        GSB_GameManager.Instance.NetworkGameState.VersusPlayers.Add(player);
    }

    GameObject GeneratePlayer()
    {
        if(GSB_GameManager.Instance.PlayerOnlineGO != null)
        {
            GameObject playerGO = Instantiate(GSB_GameManager.Instance.PlayerOnlineGO);
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
    }



    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnClientDisconnect: " + _playerControllerId);

        //ClientScene.RemovePlayer(_playerControllerId);

        base.OnClientDisconnect(conn);
    }

    public override void OnStopClient()
    {
        Debug.Log("OnStopClient: " + _playerControllerId);

        ClientScene.RemovePlayer(_playerControllerId);

        GSB_GameManager.Instance.NetworkGameState.NumPlayers--;

        base.OnStopClient();
    }
}
