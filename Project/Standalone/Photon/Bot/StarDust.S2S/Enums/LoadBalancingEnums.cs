// -----------------------------------------------------------------------
// <copyright file="LoadbalancingEnums.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Photon.Stardust.S2S.Server.Enums
{
    public enum LoadBalancingOperationCode : byte
        {
            Authenticate = 230,
            JoinLobby = 229,
            LeaveLobby = 228,
            CreateGame = 227,
            JoinGame = 226,
            JoinRandomGame = 225,
            CancelJoinRandomGame = 224,
            DebugGame = 223,
            FiendFriends = 222,
        }


       public enum LoadBalancingEventCode : byte
       {
           GameList = 230,
           GameListUpdate = 229,
           QueueState = 228,
           AppStats = 226,
           GameServerOffline = 225
       }

       public enum LoadBalancingParameterCode : byte
       {
           Address = 230,
           PeerCount = 229,
           GameCount = 228,
           MasterPeerCount = 227,
           GameId = LiteOpKey.GameId, // (226)
           UserId = 225,
           ApplicationId = 224,
           Position = 223,
           GameList = 222,
           Secret = 221,
           AppVersion = 220,
           NodeId = 219,
           Info = 218,
           ClientAuthenticationType = 217,
           ClientAuthenticationParams = 216,

           LobbyName=213,
           LobbyType = 212
       }

    public enum LoadBalancingGameCode
    {
        MaxPlayer = 255,
        IsVisible = 254,
        IsOpen = 253,
        PlayerCount = 252,
        Removed = 251,
        LobbyProperties = 250,
    }
}
