using Examples.Lockstep;
using FixMath.NET;
using Photon.Stardust.S2S.Server;
using SocialPoint.Lockstep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace LockStepClientBot
{
    public class ClientBot : IGameClient
    {
        LockstepClient _lockClient;
        LockstepNetworkClient _lockNetClient;
        Model _model;
        IUpdateScheduler _scheduler;

        bool _clicked = false;

        public ClientBot()
        {
            var config = new Config();
            _model = new Model(config);
            _lockClient = new LockstepClient();

            _lockClient.Simulate += OnSimulate;
            _lockClient.RegisterCommandLogic<ClickCommand>(new ClickCommandLogic(_model));
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
                    _lockClient.AddPendingCommand<ClickCommand>(cmd, onClickCommand);
                }
            }
        }

        void onClickCommand()
        {
            _clicked = false;
        }

        public void SetUp(INetworkClient client, IUpdateScheduler scheduler)
        {
            var factory = new LockstepCommandFactory();
            factory.Register<ClickCommand>(1);
            _lockNetClient = new LockstepNetworkClient(client , _lockClient, factory);
            scheduler.Add(_lockClient);
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
