using System;
using NUnit.Framework;
using SocialPoint.Utils;
using SocialPoint.Network;

namespace SocialPoint.Network
{
    [TestFixture]
    [Ignore("Wont' do")]
    [Category("SocialPoint.Network")]
    class TcpSocketNetworkTests : BaseNetworkTests, INetworkClientDelegate, INetworkServerDelegate
    {
        Random _random = new Random();
        UpdateScheduler _scheduler;
        bool _delegateCalled;

        [SetUp]
        public void SetUp()
        {
            var ip = "127.0.0.1";
            var port = _random.Next(3000, 5000);
            _scheduler = new UpdateScheduler();
            _server = new TcpSocketNetworkServer(_scheduler, ip, port);
            _client = new TcpSocketNetworkClient(_scheduler, ip, port);
            _client2 = new TcpSocketNetworkClient(_scheduler, ip, port);
            _server.AddDelegate(this);
            _client.AddDelegate(this);
            _client2.AddDelegate(this);
        }

        override protected void WaitForEvents()
        {
            var i = 0;
            _delegateCalled = false;
            while(!_delegateCalled && i++ < 100)
            {
                _scheduler.Update(1.0f, 1.0f);
            }
        }

        #region INetworkClientDelegate implementation

        void INetworkClientDelegate.OnClientConnected()
        {
            _delegateCalled = true;
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            _delegateCalled = true;
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
            _delegateCalled = true;
        }

        void INetworkClientDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
            _delegateCalled = true;
        }

        #endregion

        #region INetworkServerDelegate implementation

        void INetworkServerDelegate.OnServerStarted()
        {
            _delegateCalled = true;
        }

        void INetworkServerDelegate.OnServerStopped()
        {
            _delegateCalled = true;
        }

        void INetworkServerDelegate.OnClientConnected(byte clientId)
        {
            _delegateCalled = true;
        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
            _delegateCalled = true;
        }

        void INetworkServerDelegate.OnMessageReceived(NetworkMessageData data)
        {
            _delegateCalled = true;
        }

        void INetworkServerDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
            _delegateCalled = true;
        }

        #endregion
    }
}
