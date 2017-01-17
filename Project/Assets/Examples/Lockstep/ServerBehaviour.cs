using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Attributes;
using SocialPoint.Lockstep;

namespace Examples.Lockstep
{
    public class ServerBehaviour : IDisposable
    {
        Model _model;
        LockstepNetworkServer _server;
        LockstepClient _client;

        public ServerBehaviour(LockstepNetworkServer server, Config config)
        {
            _server = server;
            _model = new Model(config);
            _client = new LockstepClient();

            var cmdFactory = new LockstepCommandFactory();
            CommandType.Setup(_model, cmdFactory, _client);
            server.RegisterLocalClient(_client, cmdFactory);

            _server.CommandFailed += OnCommandFailed;
            _server.ErrorProduced += OnError;
            _server.MatchStarted += OnMatchStarted;
            _server.MatchFinished += OnMatchFinished;
            _client.Simulate += Simulate;
        }

        public void Dispose()
        {
            _server.CommandFailed -= OnCommandFailed;
            _server.ErrorProduced -= OnError;
            _server.MatchStarted -= OnMatchStarted;
            _server.MatchFinished -= OnMatchFinished;
            _client.Simulate -= Simulate;
        }

        void Simulate(int dt)
        {
            _model.Simulate(dt);
        }

        public void OnCommandFailed(Error err, byte playerNum)
        {
            // TODO: mark player as a cheater
            _server.PlayerResults[playerNum] = new AttrInt(-1);
            _server.Fail(err);
        }

        public void OnError(Error err)
        {
            _server.Fail(err);
        }

        public void OnMatchFinished(Dictionary<byte, Attr> playerResults)
        {
            // TODO: only replay if playerResults do not match up
            // for example: if all players say they won

            // replays the whole game to see who really won
            _model.Reset();
            _server.ReplayLocalClient();
            _client.Start();
            _client.Update(_server.UpdateTime);
            _client.Stop();

            // overwrite results
            playerResults.Clear();
            var itr = _model.Results.GetEnumerator();
            while(itr.MoveNext())
            {
                playerResults.Add(itr.Current.Key, itr.Current.Value);
            }
            itr.Dispose();
        }

        public void OnMatchStarted(byte[] data)
        {
            // TODO: game specific model setup
        }
    }
}