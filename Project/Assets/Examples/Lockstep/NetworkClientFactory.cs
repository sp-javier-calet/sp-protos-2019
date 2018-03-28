using System.Collections.Generic;
using FixMath.NET;
using SocialPoint.Lockstep;
using SocialPoint.Attributes;

namespace Examples.Lockstep
{
    public class NetworkClientFactory : INetworkClientGameFactory
    {
        public object Create(LockstepNetworkClient client, Dictionary<string, string> config)
        {
            var bot = new ClientBot(client);
            bot.Start();
            return bot;
        }
    }

    public class ClientBot
    {
        Model _model;
        bool _clicked = false;
        LockstepNetworkClient _client;

        public ClientBot(LockstepNetworkClient client)
        {
            var config = new Config();
            _model = new Model(config);
            _model.OnDurationEnd += OnDurationEnd;
            client.EndReceived += OnClientEndReceived;
            client.Lockstep.Simulate += OnSimulate;
            client.Lockstep.RegisterCommandLogic<ClickCommand>(new ClickCommandLogic(_model));
            client.CommandFactory.Register<ClickCommand>(1);

            _client = client;
         }

        void OnDurationEnd()
        {
            Attr result;
            _model.Results.TryGetValue(_client.PlayerNumber, out result);
            _client.SendPlayerFinish(result);
        }

        void OnClientEndReceived(Attr result)
        {
            _client.Network.Disconnect();
        }

        void OnSimulate(int dt)
        {
            _model.Simulate(dt);
            if (!_clicked)
            {
                if (_model.ManaView > 0.7f)
                {
                    var cmd = new ClickCommand((Fix64)1, (Fix64)0, (Fix64)1);
                    _clicked = true;
                    _client.Lockstep.AddPendingCommand<ClickCommand>(cmd, OnClickCommand);
                }
            }
        }

        void OnClickCommand()
        {
            _clicked = false;
        }

        public void Start()
        {
            _client.SendPlayerReady();
        }
    }
}
