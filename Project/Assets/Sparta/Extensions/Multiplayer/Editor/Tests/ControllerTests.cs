using NUnit.Framework;
using System.IO;
using SocialPoint.IO;
using SocialPoint.Network;

namespace SocialPoint.Multiplayer
{
    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    class ControllerTests
    {
        LocalNetworkServer _server;
        LocalNetworkClient _client;
        LocalNetworkClient _client2;

        NetworkServerSceneController _serverCtrl;
        NetworkClientSceneController _clientCtrl;
        NetworkClientSceneController _client2Ctrl;

        [SetUp]
        public void SetUp()
        {
            var localServer = new LocalNetworkServer();
            _server = localServer;  
            _client = new LocalNetworkClient(localServer);
            _client2 = new LocalNetworkClient(localServer);
            _serverCtrl = new NetworkServerSceneController(_server);
            _clientCtrl = new NetworkClientSceneController(_client);
            _client2Ctrl = new NetworkClientSceneController(_client2);
            _serverCtrl.Restart(_server);
            _serverCtrl.SyncController.BroadcastSyncMessageManually = true;
            _clientCtrl.Restart(_client);
            _client2Ctrl.Restart(_client2);
            _server.Start();
            _client.Connect();
            _client2.Connect();
        }

        void UpdateServerInterval()
        {
            _serverCtrl.Update(_serverCtrl.SyncController.SyncInterval);
        }

        [Test]
        public void SceneSync()
        {
            var go = _serverCtrl.Instantiate(1);
            UpdateServerInterval();
            Assert.That(_clientCtrl.Equals(_serverCtrl.Scene));
            Assert.That(_client2Ctrl.Equals(_serverCtrl.Scene));
            go.Transform.Position.X = 2.0f;
            Assert.That(!_clientCtrl.Equals(_serverCtrl.Scene));
            Assert.That(!_client2Ctrl.Equals(_serverCtrl.Scene));
            UpdateServerInterval();
            Assert.That(_clientCtrl.Equals(_serverCtrl.Scene));
            Assert.That(_client2Ctrl.Equals(_serverCtrl.Scene));
            _serverCtrl.Destroy(go.Id);
            UpdateServerInterval();
            Assert.That(_clientCtrl.Equals(_serverCtrl.Scene));
            Assert.That(_client2Ctrl.Equals(_serverCtrl.Scene));
        }
    }
}
