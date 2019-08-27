using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CP_NetworkController : NetworkManager
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

        Debug.Log("OnServerAddPlayer");

        // TODO check to avoid joining more clients
        StartCoroutine(AddPlayer(conn, playerControllerId, messageReader));
    }

    IEnumerator AddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader messageReader)
    {
        Debug.Log("AddPlayer");

        // we add this return just to avoid problems when trying to access to singletons that are not loaded in Awake
        yield return null;

        /*
        var message = messageReader.ReadMessage<NetworkMessage>();
        var selectedClass = message.ChosenCharacter;

        var startPos = GetStartPosition();
        var player = Instantiate(Players[selectedClass], startPos.position, startPos.rotation);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        */

        var player = GeneratePlayer();
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }

    GameObject GeneratePlayer()
    {
        if(CP_GameManager.Instance.PlayerGO != null)
        {
            GameObject playerGO = Instantiate(CP_GameManager.Instance.PlayerGO);
            return playerGO;
        }

        return null;
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("OnClientConnect");

        var clientConnectMsg = new NetworkMessage();
        //clientConnectMsg.ChosenCharacter = ChosenCharacter;

        ClientScene.AddPlayer(conn, 0, clientConnectMsg);
    }
}
