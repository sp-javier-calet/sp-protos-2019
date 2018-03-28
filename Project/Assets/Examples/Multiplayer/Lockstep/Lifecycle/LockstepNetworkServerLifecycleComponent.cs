using System.Collections;
using SocialPoint.Base;
using SocialPoint.Lifecycle;
using SocialPoint.Lockstep;
using SocialPoint.Matchmaking;
using SocialPoint.Network;
using SocialPoint.Utils;
using UnityEngine;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;

#endif

namespace Examples.Multiplayer.Lockstep
{
    public class LockstepNetworkServerLifecycleComponent : ISetupComponent, ICleanupComponent, ITeardownComponent, IErrorDispatcher
    {
        // To properly work the local player needs a token
        private const int LocalPlayerToken = int.MaxValue;
        
        readonly bool _localPlayer;
        readonly Config _config;
        LockstepNetworkServer _netLockstepServer;
        INetworkServer _server;
        GameServerBehaviour _serverBehaviour;

        public LockstepNetworkServerLifecycleComponent(IUpdateScheduler updateScheduler, INetworkServer netServer, Config config, GameNetworkSceneController sceneController = null, IMatchmakingServer matchmakingServer = null)
        {
            _server = netServer;
            _config = config;
            _netLockstepServer = new LockstepNetworkServer(_server, matchmakingServer, updateScheduler);
            _netLockstepServer.Config = config.Lockstep;
            _netLockstepServer.ServerConfig = config.LockstepServer;

            _localPlayer = sceneController != null;
            _serverBehaviour = new GameServerBehaviour(_netLockstepServer, config, sceneController);
        }

        public GameNetworkSceneController SceneController { get { return _serverBehaviour.SceneController; } }

        void ICleanupComponent.Cleanup()
        {
            SceneController.ClientFinished -= _netLockstepServer.LocalPlayerFinish;

            if(_serverBehaviour != null)
            {
                _serverBehaviour.Dispose();
            }

            _serverBehaviour = null;

            if(_netLockstepServer != null)
            {
                _netLockstepServer.Stop();
            }

            _netLockstepServer = null;

            _server.Stop();
            _server = null;

            _config.NumPlayers = Config.DefaultNumPlayers;

#if ADMIN_PANEL
            if(_floating != null)
            {
                _floating.Hide();
            }
#endif
        }

        IErrorHandler IErrorDispatcher.Handler { get; set; }

        IEnumerator ISetupComponent.Setup()
        {
#if ADMIN_PANEL
            if(_floating == null)
            {
                _floating = FloatingPanelController.Create(new AdminPanelLockstepServerGUI(_netLockstepServer));
                _floating.Border = false;
                _floating.ScreenPosition = FloatingPanelPosition;
                _floating.Show();
            }
#endif
            if(_localPlayer)
            {
                SceneController.ClientFinished += _netLockstepServer.LocalPlayerFinish;
                _netLockstepServer.LocalPlayerReady(LocalPlayerToken.ToString());
            }

            yield return null;
        }

        void TriggerError(Error err)
        {
            var handler = ((IErrorDispatcher)this).Handler;
            if(handler != null)
            {
                handler.OnError(err);
            }
        }
        
        IEnumerator ITeardownComponent.Teardown()
        {
            SceneController.SendLocalPlayerResult();
            while(_netLockstepServer.Running)
            {
                yield return null;
            }
            yield return null;
        }

        #if ADMIN_PANEL
        FloatingPanelController _floating;
        static readonly Vector2 FloatingPanelPosition = new Vector2(600, 90);
        #endif
    }
}