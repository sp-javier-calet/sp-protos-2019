using SocialPoint.Multiplayer;
using SocialPoint.Network;
using System;
using SocialPoint.Base;

public class GameMultiplayerClientBotLobbyBehaviour
{
    public Action<bool> PlayerJoinAnswerCallback;
    public Action<MultiplayerBattleSetupData> MatchmakingUpdateCallback;
    public Action LoadLevelCallback;

    INetworkClient _client;
    readonly NetworkClientSceneController _controller;

    public GameMultiplayerClientBotLobbyBehaviour(INetworkClient client, NetworkClientSceneController controller)
    {
        _client = client;
        _controller = controller;
    }

    public void Restart(INetworkClient client)
    {
        _client = client;

        _controller.RegisterAction<PlayerJoinRequestEvent>(GameMsgType.PlayerJoinRequestEvent, new EmptyPlayerJoinRequestEventHandler());
        _controller.RegisterAction<BattleMatchmakingUpdateEvent>(GameMsgType.BattleMatchmakingUpdateEvent, new BattleMatchmakingUpdateEventHandler(OnBattleMatchmakingUpdate));
        _controller.RegisterAction<LoadLevelEvent>(GameMsgType.LoadLevelEvent, new LoadLevelEventHandler(OnLoadLevel));
        _controller.RegisterAction<LevelLoadedEvent>(GameMsgType.LevelLoadedEvent, new EmptyLevelLoadedEventHandler());
    }

    public void OnDestroy()
    {
        if(_controller != null)
        {
            _controller.RegisterReceiver(null);
        }
    }

    public void SendJoinRequest(MultiplayerBattlePlayerData playerData)
    {
        if(_client.Connected)
        {
            var joinRequest = new PlayerJoinRequestEvent
            {
                Player = playerData
            };

            _controller.ApplyAction(joinRequest);
        }
    }

    public void OnPlayerJoinAnswer(PlayerJoinAnswerEvent eventData)
    {
        if(!eventData.CanJoin)
        {
            Log.e("PlayerJoinAnswer with canJoin = false!");
            _client.Disconnect();
        }

        if(PlayerJoinAnswerCallback != null)
        {
            PlayerJoinAnswerCallback(eventData.CanJoin);
        }
    }

    void OnBattleMatchmakingUpdate(MultiplayerBattleSetupData matchmakingInfo)
    {
        if(MatchmakingUpdateCallback != null)
        {
            MatchmakingUpdateCallback(matchmakingInfo);
        }
    }

    void OnLoadLevel(int arg)
    {
        if(LoadLevelCallback != null)
        {
            LoadLevelCallback();
        }
    }

    public void SendLevelLoadedEvent()
    {
        if(_client.Connected)
        {
            var levelLoadedEvent = new LevelLoadedEvent
            {
                PlayerId = _client.ClientId
            };
            _controller.ApplyAction(levelLoadedEvent);
        }
    }
}
