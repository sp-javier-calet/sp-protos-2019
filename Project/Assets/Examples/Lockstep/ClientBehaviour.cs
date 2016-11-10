using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SocialPoint.Base;
using SocialPoint.Lockstep;
using SocialPoint.Lockstep.Network;
using SocialPoint.Dependency;
using SocialPoint.IO;
using SocialPoint.Pooling;
using SocialPoint.Network;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Matchmaking;
using SocialPoint.Attributes;
using FixMath.NET;
using System;
using System.IO;

namespace Examples.Lockstep
{

    public enum GameLockstepMode
    {
        None,
        Local,
        Replay,
        Client,
        Server,
        Host
    }

    public class ClientBehaviour : MonoBehaviour, IPointerClickHandler, IMatchmakingClientDelegate, INetworkClientDelegate, INetworkServerDelegate
    {
        [SerializeField]
        Slider _manaSlider;

        [SerializeField]
        Text _fullscreenText;

        [SerializeField]
        GameObject _unitPrefab;

        [SerializeField]
        GameObject _loadingPrefab;

        [SerializeField]
        GameObject _gameContainer;

        [SerializeField]
        GameObject _setupContainer;

        ClientLockstepController _lockstep;
        Model _model;
        ClientLockstepController _lockstepServer;
        LockstepReplay _replay;
        INetworkClient _netClient;
        ClientLockstepNetworkController _netLockstepClient;
        INetworkServer _netServer;
        ServerLockstepNetworkController _netLockstepServer;
        IMatchmakingClientController _matchClient;
        GameLockstepMode _mode;
        XRandom _random;

        FloatingPanelController _clientFloating;
        FloatingPanelController _serverFloating;

        static readonly Vector2 ClientFloatingPanelPosition = new Vector2(600, 200);
        static readonly Vector2 ServerFloatingPanelPosition = new Vector2(600, 90);

        string ReplayPath
        {
            get
            {
                return Path.Combine(Application.persistentDataPath, "last_replay.rpl");
            }
        }

        void Start()
        {
            _lockstep = ServiceLocator.Instance.Resolve<ClientLockstepController>();
            _lockstep.ClientConfig.LocalSimulationDelay = 500;
            _replay = ServiceLocator.Instance.Resolve<LockstepReplay>();
            _lockstep.Simulate += SimulateClient;

            _model = new Model();
            _model.OnInstantiate += OnInstantiate;

            _lockstep.RegisterCommandLogic<ClickCommand>(new ClickCommandLogic(_model));
            _lockstep.SimulationStarted += OnGameStarted;
            var factory = ServiceLocator.Instance.Resolve<LockstepCommandFactory>();
            CommandType.Setup(_model, factory, _lockstep);


            _mode = GameLockstepMode.None;

            if(_gameContainer != null)
            {
                _gameContainer.SetActive(false);
            }
        }

        void OnDestroy()
        {
            _lockstep.Simulate -= SimulateClient;
            _model.OnInstantiate -= OnInstantiate;
        }

        void OnGameStarted()
        {
            if(_mode == GameLockstepMode.Replay)
            {
                _replay.Replay();
            }
            else
            {
                _replay.Record();
            }

            _random = _lockstep.CreateRandomGenerator();
        }

        void StartLocalGame(GameLockstepMode mode)
        {
            _mode = mode;
            _lockstep.Start();
        }

        public void OnLocalClicked()
        {
            SetupGameScreen();
            StartLocalGame(GameLockstepMode.Local);
        }

        public void OnReplayClicked()
        {
            try
            {
                var stream = new FileStream(ReplayPath, FileMode.Open);
                var reader = new SystemBinaryReader(stream);
                _replay.Deserialize(reader);
                stream.Close();
                stream.Dispose();
            }
            catch(IOException e)
            {
                Log.e("Could not load replay file: " + e);
                return;
            }

            SetupGameScreen();
            StartLocalGame(GameLockstepMode.Replay);
        }

        public void OnClientClicked()
        {
            SetupGameScreen();
            StartClient(GameLockstepMode.Client);
        }

        void StartClient(GameLockstepMode mode)
        {
            _mode = mode;
            _netClient = ServiceLocator.Instance.Resolve<INetworkClient>();
            _netClient.RemoveDelegate(this);
            _netClient.AddDelegate(this);
            _netLockstepClient = ServiceLocator.Instance.Resolve<ClientLockstepNetworkController>();
            _netClient.Connect();
            _netLockstepClient.SendPlayerReady();
            _fullscreenText.text = string.Empty;
        }

        public void OnServerClicked()
        {
            SetupGameScreen();
            _mode = GameLockstepMode.Server;
            StartServer();
            _netLockstepServer.RegisterLocalClient(
                ServiceLocator.Instance.Resolve<ClientLockstepController>(),
                ServiceLocator.Instance.Resolve<LockstepCommandFactory>()
            );
            _netLockstepServer.LocalPlayerReady();
        }

        void StartServer()
        {
            _netServer = ServiceLocator.Instance.Resolve<INetworkServer>();
            _netLockstepServer = ServiceLocator.Instance.Resolve<ServerLockstepNetworkController>();
            _netLockstepServer.RegisterDelegate(new ServerBehaviour(_netLockstepServer));
            _netServer.RemoveDelegate(this);
            _netServer.AddDelegate(this);
            _netServer.Start();
            if(_serverFloating == null)
            {
                _serverFloating = FloatingPanelController.Create(new AdminPanelLockstepServerGUI(_netLockstepServer));
                _serverFloating.Border = false;
                _serverFloating.ScreenPosition = ServerFloatingPanelPosition;
                _serverFloating.Show();
            }
        }

        public void OnHostClicked()
        {
            SetupGameScreen();
            StartServer();
            _netLockstepServer.UnregisterLocalClient();
            StartClient(GameLockstepMode.Host);
        }

        public void OnMatchClicked()
        {
            SetupGameScreen();
            _matchClient = ServiceLocator.Instance.Resolve<IMatchmakingClientController>();
            _matchClient.RemoveDelegate(this);
            _matchClient.AddDelegate(this);
            _fullscreenText.text = "connecting to matchmaker...";
            _matchClient.Start();
        }

        #region IMatchmakingClientDelegate implementation

        void IMatchmakingClientDelegate.OnWaiting(int waitTime)
        {
            _fullscreenText.text = string.Format("estimated waiting time {0}", waitTime);
        }

        void IMatchmakingClientDelegate.OnMatched(Match match)
        {
            StartClient(GameLockstepMode.Client);
        }

        void IMatchmakingClientDelegate.OnError(Error err)
        {
            Log.e("IMatchmakingClientDelegate.OnError " + err);
            OnCloseClicked();
        }

        #endregion

        #region INetworkClientDelegate implementation

        void INetworkClientDelegate.OnClientConnected()
        {
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            OnCloseClicked();
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkClientDelegate.OnNetworkError(Error err)
        {
            Log.e("INetworkClientDelegate.OnNetworkError " + err);
        }

        #endregion

        #region INetworkServerDelegate implementation

        void INetworkServerDelegate.OnServerStarted()
        {
        }

        void INetworkServerDelegate.OnServerStopped()
        {
            OnCloseClicked();
        }

        void INetworkServerDelegate.OnClientConnected(byte clientId)
        {
        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
        }

        void INetworkServerDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkServerDelegate.OnNetworkError(Error err)
        {
            Log.e("INetworkServerDelegate.OnNetworkError " + err);
        }

        #endregion

        public void OnCloseClicked()
        {
            if(_gameContainer != null)
            {
                _gameContainer.SetActive(false);
            }
            if(_setupContainer != null)
            {
                _setupContainer.SetActive(true);
            }
            _fullscreenText.text = string.Empty;

            // remove created cubes
            foreach(Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            if(_mode == GameLockstepMode.Local)
            {
                var stream = new FileStream(ReplayPath, FileMode.OpenOrCreate);
                var writer = new SystemBinaryWriter(stream);
                _replay.Serialize(writer);
                stream.Close();
                stream.Dispose();
            }
            _lockstep.Stop();
            _model.Reset();

            if(_netLockstepClient != null)
            {
                Attr result;
                _model.Results.TryGetValue(_netLockstepClient.PlayerNumber, out result);
                _netLockstepClient.SendPlayerFinish(result);
            }

            if(_netClient != null)
            {
                _netClient.Disconnect();
            }
            if(_netLockstepServer != null)
            {
                _netLockstepServer.Stop();
            }
            if(_netServer != null)
            {
                _netServer.Stop();
            }
            if(_matchClient != null)
            {
                _matchClient.Stop();
                _matchClient.Clear();
            }
        }

        void SetupGameScreen()
        {
            if(_gameContainer != null)
            {
                _gameContainer.SetActive(true);
            }
            if(_setupContainer != null)
            {
                _setupContainer.SetActive(false);
            }
            if(_clientFloating == null)
            {
                _clientFloating = FloatingPanelController.Create(new AdminPanelLockstepClientGUI(_lockstep));
                _clientFloating.Border = false;
                _clientFloating.ScreenPosition = ClientFloatingPanelPosition;
                _clientFloating.Show();
            }
        }

        void SimulateClient(int dt)
        {
            _model.Simulate(dt);
        }

        static readonly Fix64 InstanceMinScale = (Fix64)0.2f;
        static readonly Fix64 InstanceMaxScale = (Fix64)2.0f;

        void OnInstantiate(Fix64 x, Fix64 y, Fix64 z)
        {
            var scale = new Vector3(
                            (float)_random.Range(InstanceMinScale, InstanceMaxScale),
                            (float)_random.Range(InstanceMinScale, InstanceMaxScale),
                            (float)_random.Range(InstanceMinScale, InstanceMaxScale));
            
            var unit = ObjectPool.Spawn(_unitPrefab, transform,
                           new Vector3((float)x, (float)y * scale.y, (float)z), Quaternion.identity);
            
            unit.transform.localScale = scale;
        }

        void Update()
        {
            if(_manaSlider != null && _model != null)
            {
                _manaSlider.value = _model.ManaView;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(_mode == GameLockstepMode.Replay)
            {
                return;
            }
            var p = eventData.pointerPressRaycast.worldPosition;
            var cmd = new ClickCommand(
                          (Fix64)p.x, (Fix64)p.y, (Fix64)p.z);

            var loading = ObjectPool.Spawn(
                              _loadingPrefab, transform, p, Quaternion.identity);
            _lockstep.AddPendingCommand(cmd, (c) => FinishLoading(loading));
        }

        public void FinishLoading(GameObject loading)
        {
            ObjectPool.Recycle(loading);
        }
    }
}