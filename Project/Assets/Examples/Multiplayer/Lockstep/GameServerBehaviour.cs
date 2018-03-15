using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Lockstep;

namespace Examples.Multiplayer.Lockstep
{
    public class GameServerBehaviour : IDisposable
    {
        readonly LockstepNetworkServer _server;
        LockstepClient _client;

        public GameServerBehaviour(LockstepNetworkServer server, Config config, GameNetworkSceneController localSceneController = null)
        {
            _server = server;
            SceneController = localSceneController;
            if(SceneController == null)
            {
                SceneController = new GameNetworkSceneController(config, new LockstepClient());
            }

            if(SceneController != null)
            {
                _server.RegisterLocalClient(SceneController.Client, SceneController.CommandFactory);
            }

            _server.CommandFailed += OnCommandFailed;
            _server.ErrorProduced += OnError;
            _server.MatchStarted += OnMatchStarted;
            _server.MatchFinished += OnMatchFinished;
        }

        public GameNetworkSceneController SceneController { get; private set; }

        public void Dispose()
        {
            _server.CommandFailed -= OnCommandFailed;
            _server.ErrorProduced -= OnError;
            _server.MatchStarted -= OnMatchStarted;
            _server.MatchFinished -= OnMatchFinished;
        }

        void OnCommandFailed(Error err, byte playerNum)
        {
            // TODO: mark player as a cheater
            _server.PlayerResults[playerNum] = new AttrInt(-1);
            _server.Fail(err);
        }

        void OnError(Error err)
        {
            _server.Fail(err);
        }

        void OnMatchFinished(Dictionary<byte, Attr> playerResults, AttrDic customData)
        {
            // TODO: only replay if playerResults do not match up
            // for example: if all players say they won

            // replays the whole game to see who really won
            //_model.Reset();
            //_server.ReplayLocalClient();
            //_client.Start();
            //_client.Update(_server.UpdateTime);
            //_client.Stop();

            // overwrite results
            //playerResults.Clear();
            /*var itr = _model.Results.GetEnumerator();
            while(itr.MoveNext())
            {
                playerResults.Add(itr.Current.Key, itr.Current.Value);
            }
            itr.Dispose();*/

            //add any aditional data the game would like to send to the backend through the matchmaking
            //customData.SetValue("battle_duration",_client.UpdateTime);
        }

        void OnMatchStarted(byte[] data)
        {
            // TODO: game specific model setup
        }
    }
}