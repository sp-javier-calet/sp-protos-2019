using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Attributes;
using SocialPoint.Lockstep;
using SocialPoint.Lockstep.Network;

namespace Examples.Lockstep
{
    public class ServerBehaviour : IServerLockstepNetworkDelegate
    {
        ServerLockstepNetworkController _server;
        Model _model;
        ClientLockstepController _client;

        public ServerBehaviour(ServerLockstepNetworkController server)
        {
            _server = server;
            _model = new Model();
            _client = new ClientLockstepController();

            _client.Simulate += Simulate;

            var cmdFactory = new LockstepCommandFactory();
            CommandType.Setup(_model, cmdFactory, _client);
            server.RegisterLocalClient(_client, cmdFactory);
        }

        void Simulate(int dt)
        {
            _model.Simulate(dt);
        }

        public void OnCommandFailed(Error err, byte playerNum)
        {
            // TODO: mark player as a cheater
            _server.PlayerResults[playerNum] = new AttrInt(-1);
            _server.Fail("Command failed: " + err);
        }

        public void OnError(Error err)
        {
            _server.Fail("Lockstep server failed: " + err);
        }

        public void OnFinish(Dictionary<byte, Attr> playerResults)
        {
            // TODO: only replay if playerResults do not match up
            // for example: if all players say they won

            // replays the whole game to see who really won
            _model.Reset();
            _server.ReplayLocalClient();
            _client.Start();
            _client.Update(_server.UpdateTime);
            _client.Stop();

            playerResults.Clear();
            var itr = _model.Results.GetEnumerator();
            while(itr.MoveNext())
            {
                playerResults.Add(itr.Current.Key, itr.Current.Value);
            }
            itr.Dispose();
        }

        public void OnStart(Attr data)
        {
            // TODO: game specific model setup
        }
    }
}