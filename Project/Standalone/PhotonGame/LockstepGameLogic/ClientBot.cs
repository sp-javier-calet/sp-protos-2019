
using System;
using System.Collections.Generic;
using FixMath.NET;
using SocialPoint.Lockstep;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace Examples.Lockstep
{
    public class ClientBotGameFactory : INetworkClientGameFactory
    {
        public object Create(INetworkClient client, IUpdateScheduler scheduler, Dictionary<string, string> config)
        {
            var bot = new ClientBot(client);
            bot.Start();
            return bot;
        }
    }

    public class ClientBot
    {
        LockstepClient _lockClient;
        LockstepNetworkClient _lockNetClient;
        Model _model;

        bool _clicked = false;

        public ClientBot(INetworkClient client)
        {
            var config = new Config();
            _model = new Model(config);
            _lockClient = new LockstepClient();

            _lockClient.Simulate += OnSimulate;
            _lockClient.RegisterCommandLogic<ClickCommand>(new ClickCommandLogic(_model));

            var factory = new LockstepCommandFactory();
            factory.Register<ClickCommand>(1);
            _lockNetClient = new LockstepNetworkClient(client, _lockClient, factory);
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
                    _lockClient.AddPendingCommand<ClickCommand>(cmd, OnClickCommand);
                }
            }
        }

        void OnClickCommand()
        {
            _clicked = false;
        }

        public void Start()
        {
            if (_lockNetClient == null)
            {
                _lockNetClient.SendPlayerReady();
            }
        }
    }
}
