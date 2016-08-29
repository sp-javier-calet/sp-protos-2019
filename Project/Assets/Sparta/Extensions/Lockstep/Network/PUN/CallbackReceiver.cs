using System;
using System.Collections.Generic;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.Lockstep.Network.PUN
{
    public sealed class CallbackReceiver : MonoBehaviourSingleton<CallbackReceiver>, IPunCallbacks
    {
        public event Action ConnectedToPhoton;
        public event Action LeftRoom;
        public event Action<PhotonPlayer> MasterClientSwitched;
        public event Action<object[]> PhotonCreateRoomFailed;
        public event Action<object[]> PhotonJoinRoomFailed;
        public event Action CreatedRoom;
        public event Action JoinedLobby;
        public event Action LeftLobby;
        public event Action<DisconnectCause> FailedToConnectToPhoton;
        public event Action ConnectionFail;
        public event Action DisconnectedFromPhoton;
        public event Action PhotonInstantiate;
        public event Action ReceivedRoomListUpdate;
        public event Action JoinedRoom;
        public event Action<PhotonPlayer> PhotonPlayerConnected;
        public event Action<PhotonPlayer> PhotonPlayerDisconnected;
        public event Action<object[]> PhotonRandomJoinFailed;
        public event Action ConnectedToMaster;
        public event Action PhotonMaxCccuReached;
        public event Action<ExitGames.Client.Photon.Hashtable> PhotonCustomRoomPropertiesChanged;
        public event Action<object[]> PhotonPlayerPropertiesChanged;
        public event Action UpdatedFriendList;
        public event Action<string> CustomAuthenticationFailed;
        public event Action<Dictionary<string, object>> CustomAuthenticationResponse;
        public event Action<ExitGames.Client.Photon.OperationResponse> WebRpcResponse;
        public event Action<object[]> OwnershipRequest;
        public event Action LobbyStatisticsUpdate;

        protected override void SingletonAwakened()
        {
            if(PhotonNetwork.SendMonoMessageTargets == null)
            {
                PhotonNetwork.SendMonoMessageTargets = new HashSet<GameObject>();
            }
            PhotonNetwork.SendMonoMessageTargets.Add(gameObject);
        }

        protected override void SingletonDestroyed()
        {
            if(PhotonNetwork.SendMonoMessageTargets != null)
            {
                PhotonNetwork.SendMonoMessageTargets.Remove(gameObject);
            }
        }

        #region IPunCallbacks implementation

        public void OnConnectedToPhoton()
        {
            if(ConnectedToPhoton != null)
            {
                ConnectedToPhoton();
            }
        }

        public void OnLeftRoom()
        {
            if(LeftRoom != null)
            {
                LeftRoom();
            }
        }

        public void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            if(MasterClientSwitched != null)
            {
                MasterClientSwitched(newMasterClient);
            }
        }

        public void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
            if(PhotonCreateRoomFailed != null)
            {
                PhotonCreateRoomFailed(codeAndMsg);
            }
        }

        public void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            if(PhotonJoinRoomFailed != null)
            {
                PhotonJoinRoomFailed(codeAndMsg);
            }
        }

        public void OnCreatedRoom()
        {
            if(CreatedRoom != null)
            {
                CreatedRoom();
            }
        }

        public void OnJoinedLobby()
        {
            if(JoinedLobby != null)
            {
                JoinedLobby();
            }
        }

        public void OnLeftLobby()
        {
            if(LeftLobby != null)
            {
                LeftLobby();
            }
        }

        public void OnFailedToConnectToPhoton(DisconnectCause cause)
        {
            if(FailedToConnectToPhoton != null)
            {
                FailedToConnectToPhoton(cause);
            }
        }

        public void OnConnectionFail(DisconnectCause cause)
        {
            if(ConnectionFail != null)
            {
                ConnectionFail();
            }
        }

        public void OnDisconnectedFromPhoton()
        {
            if(DisconnectedFromPhoton != null)
            {
                DisconnectedFromPhoton();
            }
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            if(PhotonInstantiate != null)
            {
                PhotonInstantiate();
            }
        }

        public void OnReceivedRoomListUpdate()
        {
            if(ReceivedRoomListUpdate != null)
            {
                ReceivedRoomListUpdate();
            }
        }

        public void OnJoinedRoom()
        {
            if(JoinedRoom != null)
            {
                JoinedRoom();
            }
        }

        public void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            if(PhotonPlayerConnected != null)
            {
                PhotonPlayerConnected(newPlayer);
            }
        }

        public void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            if(PhotonPlayerDisconnected != null)
            {
                PhotonPlayerDisconnected(otherPlayer);
            }
        }

        public void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            if(PhotonRandomJoinFailed != null)
            {
                PhotonRandomJoinFailed(codeAndMsg);
            }
        }

        public void OnConnectedToMaster()
        {
            if(ConnectedToMaster != null)
            {
                ConnectedToMaster();
            }
        }

        public void OnPhotonMaxCccuReached()
        {
            if(PhotonMaxCccuReached != null)
            {
                PhotonMaxCccuReached();
            }
        }

        public void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            if(PhotonCustomRoomPropertiesChanged != null)
            {
                PhotonCustomRoomPropertiesChanged(propertiesThatChanged);
            }
        }

        public void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
        {
            if(PhotonPlayerPropertiesChanged != null)
            {
                PhotonPlayerPropertiesChanged(playerAndUpdatedProps);
            }
        }

        public void OnUpdatedFriendList()
        {
            if(UpdatedFriendList != null)
            {
                UpdatedFriendList();
            }
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            if(CustomAuthenticationFailed != null)
            {
                CustomAuthenticationFailed(debugMessage);
            }
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            if(CustomAuthenticationResponse != null)
            {
                CustomAuthenticationResponse(data);
            }
        }

        public void OnWebRpcResponse(ExitGames.Client.Photon.OperationResponse response)
        {
            if(WebRpcResponse != null)
            {
                WebRpcResponse(response);
            }
        }

        public void OnOwnershipRequest(object[] viewAndPlayer)
        {
            if(OwnershipRequest != null)
            {
                OwnershipRequest(viewAndPlayer);
            }
        }

        public void OnLobbyStatisticsUpdate()
        {
            if(LobbyStatisticsUpdate != null)
            {
                LobbyStatisticsUpdate();
            }
        }

        #endregion
    }
}