using SocialPoint.Base;
using System.Collections.Generic;
using System;
using SocialPoint.Dependency;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    public sealed class SimulateNetworkServer : SimulateNetworkBase, INetworkServer, INetworkServerDelegate
    {
        INetworkServer _server;
        List<INetworkServerDelegate> _delegates;

        public SimulateNetworkServer(INetworkServer server) :
            base(server)
        {
            _delegates = new List<INetworkServerDelegate>();
            _server = server;
            _server.RegisterReceiver(this);
            _server.AddDelegate(this);
        }

        public bool Running
        {
            get
            {
                return _server.Running;
            }
        }

        public string Id
        {
            get
            {
                return _server.Id;
            }
        }

        public void Start()
        {
            ClearSimulationData();
            _server.Start();
        }

        public void Stop()
        {
            ClearSimulationData();
            _server.Stop();
        }

        public void Fail(Error err)
        {
            _server.Fail(err);
        }

        public void AddDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public int GetTimestamp()
        {
            return _server.GetTimestamp();
        }

        public bool LatencySupported
        {
            get
            {
                return _server.LatencySupported;
            }
        }

        #region INetworkServerDelegate implementation

        void INetworkServerDelegate.OnServerStarted()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }
        }

        void INetworkServerDelegate.OnServerStopped()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }
        }

        void INetworkServerDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkServerDelegate.OnNetworkError(Error err)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnNetworkError(err);
            }
        }

        void INetworkServerDelegate.OnClientConnected(byte clientId)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected(clientId);
            }
        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected(clientId);
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
    }

    public class SimulateNetworkServerFactory : INetworkServerFactory
    {
        readonly INetworkServerFactory _serverFactory;
        readonly LocalNetworkInstaller.ServerSettingsData _settings;

        public SimulateNetworkServerFactory(INetworkServerFactory serverFactory, LocalNetworkInstaller.ServerSettingsData settings)
        {
            _serverFactory = serverFactory;
            _settings = settings;
        }

        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            var server = new SimulateNetworkServer(_serverFactory.Create());
            SetupServer(server);

            return server;
        }

        #endregion
        
        void SetupServer(SimulateNetworkServer server)
        {
            server.ReceptionDelay = _settings.ReceptionDelay.Average;
            server.ReceptionDelayVariance = _settings.ReceptionDelay.Variance;
            server.EmissionDelay = _settings.EmissionDelay.Average;
            server.EmissionDelayVariance = _settings.EmissionDelay.Variance;

            var dlgs = Services.Instance.ResolveList<INetworkServerDelegate>();
            for(var i = 0; i < dlgs.Count; i++)
            {
                server.AddDelegate(dlgs[i]);
            }
        }
    }
}
