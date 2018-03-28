using System.Collections;
using SocialPoint.Lifecycle;
using SocialPoint.Lockstep;
using SocialPoint.NetworkModel;
using SocialPoint.Utils;
#if ADMIN_PANEL
using UnityEngine;
using SocialPoint.AdminPanel;
#endif

namespace Examples.Multiplayer.Lockstep
{
    public class LockstepClientLifecycleComponent : ISetupComponent, ICleanupComponent, IStartComponent, ITeardownComponent, INetworkSceneDelegate
    {
        readonly ClientConfig _config;
        readonly LockstepClient _lockstep;
        readonly GameNetworkSceneController _sceneController;

        public LockstepClientLifecycleComponent(IUpdateScheduler updateScheduler, ClientConfig config)
        {
            _config = config;
            _lockstep = new LockstepClient(updateScheduler);
            _lockstep.Config = config.General.Lockstep;
            _lockstep.ClientConfig.RealTimeUpdate = config.General.RealtimeUpdate;
            _lockstep.ClientConfig.LocalSimulationDelay = config.LocalSimDelay;

            _sceneController = new GameNetworkSceneController(config.General, _lockstep);
            _sceneController.Scene.RegisterDelegate(this);
        }

        public LockstepClient Lockstep { get { return _lockstep; } }

        public GameNetworkSceneController SceneController { get { return _sceneController; } }

        void ICleanupComponent.Cleanup()
        {
            _lockstep.Dispose();
            _sceneController.Dispose();

#if ADMIN_PANEL
            if(_floating != null)
            {
                _floating.Hide();
                _floating = null;
            }
#endif
        }

        void INetworkSceneDelegate.OnObjectInstantiated(NetworkGameObject ngo)
        {
            if(ngo.Type == GameObjectType.Cube)
            {
                var cubeView = ngo.Behaviours.Add<CubeViewNetworkBehavior>();
                cubeView.Init(_lockstep.CreateRandomGenerator(), _config.Container, _config.CubePrefab);
            }
        }

        void INetworkSceneDelegate.OnObjectAdded(NetworkGameObject ngo)
        {
        }

        void INetworkSceneDelegate.OnObjectDestroyed(NetworkGameObject ngo)
        {
        }

        void INetworkSceneDelegate.OnObjectRemoved(NetworkGameObject ngo)
        {
        }

        void IDeltaUpdateable<int>.Update(int elapsed)
        {
        }

        IEnumerator ISetupComponent.Setup()
        {
#if ADMIN_PANEL
            if(Application.isPlaying && _floating == null)
            {
                _floating = FloatingPanelController.Create(new AdminPanelLockstepClientGUI(_lockstep));
                _floating.Border = false;
                _floating.ScreenPosition = FloatingPanelPosition;
                _floating.Show();
            }
#endif
            return null;
        }

        void IStartComponent.Start()
        {
            if(!_lockstep.Running)
            {
                _lockstep.Start();
            }
        }

#if ADMIN_PANEL
        FloatingPanelController _floating;
        static readonly Vector2 FloatingPanelPosition = new Vector2(600, 200);
#endif
        IEnumerator ITeardownComponent.Teardown()
        {
            if(_lockstep.Running)
            {
                _lockstep.Stop();
            }

            yield return null;
        }
    }
}