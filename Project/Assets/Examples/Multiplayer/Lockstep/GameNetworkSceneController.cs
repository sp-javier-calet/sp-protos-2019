using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Lockstep;
using SocialPoint.NetworkModel;

namespace Examples.Multiplayer.Lockstep
{
    public class GameNetworkSceneController : IDisposable
    {
        readonly LockstepClient _client;
        readonly Config _config;
        readonly DurationNetworkSceneBehaviour _durationBehaviour;
        readonly List<PlayerNetworkSceneBehaviour> _playerBehaviours;
        readonly ResultsNetworkSceneBehaviour _resultsBehaviour;
        public readonly LockstepCommandFactory CommandFactory;
        public readonly NetworkScene Scene;

        public event Action<Attr> ClientFinished;

        PlayerNetworkSceneBehaviour _playerBehaviour;

        public GameNetworkSceneController(Config config, LockstepClient client)
        {
            _client = client;
            _config = config;
            _playerBehaviours = new List<PlayerNetworkSceneBehaviour>();
            _client.Simulate += Simulate;
            _client.SimulationStarted += OnSimulationStarted;

            Scene = new NetworkScene();
            CommandFactory = new LockstepCommandFactory();
            GameCommandType.Setup(Scene, CommandFactory, _client);

            _durationBehaviour = new DurationNetworkSceneBehaviour(config);
            _durationBehaviour.Finished += OnDurationFinished;
            Scene.RegisterDelegate(_durationBehaviour);

            _resultsBehaviour = new ResultsNetworkSceneBehaviour();
            Scene.RegisterDelegate(_resultsBehaviour);

            for(var i = 0; i < config.NumPlayers; i++)
            {
                var playerBehaviour = new PlayerNetworkSceneBehaviour((byte) i, config);
                _playerBehaviours.Add(playerBehaviour);
                Scene.RegisterDelegate(playerBehaviour);
            }
        }

        public float LocalPlayerManaPercent { get { return _playerBehaviour == null ? 0.0f : (float) _playerBehaviour.Mana / _config.MaxMana; } }
        public long LocalPlayerMana { get { return _playerBehaviour == null ? 0 : _playerBehaviour.Mana; } }

        public long TimeLeft { get { return _durationBehaviour.TimeLeft; } }

        public LockstepClient Client { get { return _client; } }

        public void Dispose()
        {
            _client.Simulate -= Simulate;
            _client.SimulationStarted -= OnSimulationStarted;
            Scene.Dispose();
        }

        public event Action Finished;

        public Attr GetResultsAttr()
        {
            return _resultsBehaviour.ToAttr();
        }

        public void SendResult(LockstepNetworkClient client)
        {
            _resultsBehaviour.Send(client);
        }

        public void SendLocalPlayerResult()
        {
            var result = _resultsBehaviour.GetPlayerResult(_playerBehaviour.PlayerNumber); 
            if(ClientFinished != null)
            {
                ClientFinished(result);
            }
        }

        void OnDurationFinished()
        {
            if(Finished != null)
            {
                Finished();
            }
        }

        void OnSimulationStarted()
        {
            for(var i = 0; i < _playerBehaviours.Count; i++)
            {
                var player = _playerBehaviours[i];
                if(player.PlayerNumber == _client.PlayerNumber)
                {
                    _playerBehaviour = player;
                    break;
                }
            }
        }

        void Simulate(int dt)
        {
            Scene.Update(dt);
        }
    }
}