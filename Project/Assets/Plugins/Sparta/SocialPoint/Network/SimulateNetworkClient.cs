using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.IO;
using SocialPoint.Utils;
using System;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    public sealed class SimulateNetworkClient : SimulateNetworkBase, INetworkClient, INetworkClientDelegate, IDisposable
    {
        INetworkClient _client;
        List<INetworkClientDelegate> _delegates;
        IUpdateScheduler _scheduler;

        public SimulateNetworkClient(INetworkClient client, IUpdateScheduler scheduler = null) :
            base(client)
        {
            _delegates = new List<INetworkClientDelegate>();
            _client = client;
            _client.RegisterReceiver(this);
            _client.AddDelegate(this);
            _scheduler = scheduler;
            if(_scheduler != null)
            {
                _scheduler.Add(this);
            }
        }

        public SimulateNetworkClient(LocalNetworkServer server, IUpdateScheduler scheduler = null) :
            this(new LocalNetworkClient(server), scheduler)
        {
        }

        public bool Connected
        {
            get
            {
                return _client.Connected;
            }
        }

        public void Connect()
        {
            ClearSimulationData();
            _client.Connect();
        }

        public void Disconnect()
        {
            ClearSimulationData();
            _client.Disconnect();
        }

        public void AddDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public int GetDelay(int networkTimestamp)
        {
            return _client.GetDelay(networkTimestamp);
        }

        public bool LatencySupported
        {
            get
            {
                return _client.LatencySupported;
            }
        }

        public int Latency
        {
            get
            {
                return _client.Latency;
            }
        }

        #region INetworkClientDelegate implementation

        void INetworkClientDelegate.OnClientConnected()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected();
            }
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected();
            }
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkClientDelegate.OnNetworkError(Error err)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnNetworkError(err);
            }
        }

        #endregion

        #region INetworkMessageReceiver implementation

        protected override void ReceiveMessage(NetworkMessageData data, IReader reader)
        {
            base.ReceiveMessage(data, reader);
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(data);
            }
        }

        #endregion

        #region IDisposable implementation

        void IDisposable.Dispose()
        {
            if(_scheduler != null)
            {
                _scheduler.Remove(this);
            }
        }

        #endregion
    }

    public class SimulateNetworkClientFactory : INetworkClientFactory
    {
        readonly INetworkClientFactory _clientFactory;
        readonly LocalNetworkInstaller.ClientSettingsData _settings;

        public SimulateNetworkClientFactory(INetworkClientFactory clientFactory, LocalNetworkInstaller.ClientSettingsData settings)
        {
            _clientFactory = clientFactory;
            _settings = settings;
        }

        #region INetworkClientFactory implementation

        INetworkClient INetworkClientFactory.Create()
        {
            var client = new SimulateNetworkClient(_clientFactory.Create(), Services.Instance.Resolve<IUpdateScheduler>());
            SetupClient(client);

            return client;
        }

        #endregion

        void SetupClient(SimulateNetworkClient client)
        {
            client.ReceptionDelay = _settings.ReceptionDelay.Average;
            client.ReceptionDelayVariance = _settings.ReceptionDelay.Variance;
            client.EmissionDelay = _settings.EmissionDelay.Average;
            client.EmissionDelayVariance = _settings.EmissionDelay.Variance;

            var dlgs = Services.Instance.ResolveList<INetworkClientDelegate>();
            for(var i = 0; i < dlgs.Count; i++)
            {
                client.AddDelegate(dlgs[i]);
            }
        }
    }
}
