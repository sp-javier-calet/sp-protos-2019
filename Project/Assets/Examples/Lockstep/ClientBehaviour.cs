using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SocialPoint.Base;
using SocialPoint.Lockstep;
using SocialPoint.Dependency;
using SocialPoint.IO;
using SocialPoint.Pooling;
using SocialPoint.Network;
using SocialPoint.Utils;
using SocialPoint.Matchmaking;
using SocialPoint.Attributes;
using FixMath.NET;
using System;
using System.IO;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

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

        [SerializeField]
        Text _timeText;

        [SerializeField]
        Config _gameConfig;

        LockstepClient _lockstep;
        Model _model;
        LockstepClient _lockstepServer;
        LockstepReplay _replay;
        INetworkClient _netClient;
        LockstepNetworkClient _netLockstepClient;
        INetworkServer _netServer;
        LockstepNetworkServer _netLockstepServer;
        IMatchmakingClient _matchClient;
        GameLockstepMode _mode;
        XRandom _random;
        ServerBehaviour _serverBehaviour;
        CloudRegionCode _photonRegion = CloudRegionCode.none;

        #if ADMIN_PANEL
        FloatingPanelController _clientFloating;
        FloatingPanelController _serverFloating;

        static readonly Vector2 ClientFloatingPanelPosition = new Vector2(600, 200);
        static readonly Vector2 ServerFloatingPanelPosition = new Vector2(600, 90);
        #endif

        string ReplayPath
        {
            get
            {
                return Path.Combine(Application.persistentDataPath, "last_replay.rpl");
            }
        }

        void Start()
        {
            _lockstep = Services.Instance.Resolve<LockstepClient>();
            _lockstep.ClientConfig.LocalSimulationDelay = 500;
            _replay = Services.Instance.Resolve<LockstepReplay>();
            _lockstep.Simulate += SimulateClient;

            _model = new Model(_gameConfig);
            _model.OnInstantiate += OnInstantiate;
            _model.OnDurationEnd += OnDurationEnd;

            _lockstep.RegisterCommandLogic<ClickCommand>(new ClickCommandLogic(_model));
            _lockstep.SimulationStarted += OnGameStarted;
            var factory = Services.Instance.Resolve<LockstepCommandFactory>();
            CommandType.Setup(_model, factory, _lockstep);


            _mode = GameLockstepMode.None;

            if(_gameContainer != null)
            {
                _gameContainer.SetActive(false);
            }
        }

        void OnDestroy()
        {
            if(_lockstep != null)
            {
                _lockstep.Simulate -= SimulateClient;
            }
            if(_model != null)
            {
                _model.OnInstantiate -= OnInstantiate;
                _model.OnDurationEnd -= OnDurationEnd;
            }
            if(_netLockstepClient != null)
            {
                _netLockstepClient.EndReceived -= OnClientEndReceived;
            }
        }

        void OnGameStarted()
        {
            _fullscreenText.text = string.Empty;
            if(_mode == GameLockstepMode.Replay)
            {
                _replay.Replay();
            }
            else
            {
                _replay.Record();
            }

            if(_timeText != null)
            {
                _timeText.text = _model.TimeString;
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

        void SetupPhoton(PhotonNetworkBase photon)
        {
            if(photon != null)
            {
                photon.Config.ForceRegion = _photonRegion;
            }
        }

        void StartClient(GameLockstepMode mode)
        {
            _mode = mode;

            var clientFactory = Services.Instance.Resolve<INetworkClientFactory>();
            _netClient = clientFactory.Create();
            _netClient.RemoveDelegate(this);
            _netClient.AddDelegate(this);
            SetupPhoton(_netClient as PhotonNetworkBase);
            _netLockstepClient = Services.Instance.Resolve<LockstepNetworkClient>();
            _netLockstepClient.EndReceived -= OnClientEndReceived;
            _netLockstepClient.EndReceived += OnClientEndReceived;
            _netClient.Connect();
            _netLockstepClient.SendPlayerReady();
        }

        void OnClientEndReceived(Attr result)
        {
            _fullscreenText.text = string.Format("match result {0}", result);
            if(_matchClient != null)
            {
                _matchClient.Clear();
            }
        }

        public void OnServerClicked()
        {
            SetupGameScreen();
            _mode = GameLockstepMode.Server;
            StartServer();
            _netLockstepServer.RegisterLocalClient(
                Services.Instance.Resolve<LockstepClient>(),
                Services.Instance.Resolve<LockstepCommandFactory>()
            );
            _netLockstepServer.LocalPlayerReady();
        }

        void StartServer()
        {
            var factory = Services.Instance.Resolve<INetworkServerFactory>();
            _netServer = factory.Create();
            SetupPhoton(_netServer as PhotonNetworkBase);

            _netLockstepServer = new LockstepNetworkServer(_netServer,
                Services.Instance.Resolve<IMatchmakingServer>(),
                Services.Instance.Resolve<IUpdateScheduler>());
            _netLockstepServer.Config = Services.Instance.Resolve<LockstepConfig>();
            _netLockstepServer.ServerConfig = Services.Instance.Resolve<LockstepServerConfig>();
            _netLockstepServer.ServerConfig.MatchmakingEnabled = false;
            _serverBehaviour = new ServerBehaviour(_netLockstepServer, _gameConfig);
            _netServer.RemoveDelegate(this);
            _netServer.AddDelegate(this);
            _netServer.Start();

            #if ADMIN_PANEL
            if(_serverFloating == null)
            {
                _serverFloating = FloatingPanelController.Create(new AdminPanelLockstepServerGUI(_netLockstepServer));
                _serverFloating.Border = false;
                _serverFloating.ScreenPosition = ServerFloatingPanelPosition;
                _serverFloating.Show();
            }
            #endif
        }

        public void OnHostClicked()
        {
            SetupGameScreen();
            StartServer();
            _netLockstepServer.UnregisterLocalClient();
            _netLockstepServer.ServerConfig.MatchmakingEnabled = false;
            StartClient(GameLockstepMode.Host);
        }

        public void OnMatchClicked()
        {
            SetupGameScreen();
            _matchClient = Services.Instance.Resolve<IMatchmakingClient>();
            _matchClient.RemoveDelegate(this);
            _matchClient.AddDelegate(this);
            _fullscreenText.text = "connecting to matchmaker...";
            _matchClient.Start(null, false, string.Empty);
        }

        #region IMatchmakingClientDelegate implementation

        void IMatchmakingClientDelegate.OnStart()
        {
            _fullscreenText.text = string.Format("matchmaking start");
        }

        void IMatchmakingClientDelegate.OnSearchOpponent()
        {
            _fullscreenText.text = string.Format("searching for opponent");
        }

        void IMatchmakingClientDelegate.OnWaiting(int waitTime)
        {
            _fullscreenText.text = string.Format("estimated waiting time {0}", waitTime);
        }

        void IMatchmakingClientDelegate.OnMatched(Match match)
        {
            _fullscreenText.text = string.Format("match {0} player {1}", match.Id, match.PlayerId);
            StartClient(GameLockstepMode.Client);
        }

        void IMatchmakingClientDelegate.OnStopped(bool successful)
        {
            _fullscreenText.text = string.Format("match stopped - successful: {0}", successful);
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

            if(_netClient != null)
            {
                _netClient.Disconnect();
                _netClient.Dispose();
            }
            if(_netLockstepServer != null)
            {
                _netLockstepServer.Stop();
            }
            if(_netServer != null)
            {
                _netServer.Stop();
                _netServer.Dispose();
            }
            if(_matchClient != null)
            {
                _matchClient.Stop();
            }
            if(_serverBehaviour != null)
            {
                _serverBehaviour.Dispose();
                _serverBehaviour = null;
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
            #if ADMIN_PANEL 
            if(_clientFloating == null)
            {
                _clientFloating = FloatingPanelController.Create(new AdminPanelLockstepClientGUI(_lockstep));
                _clientFloating.Border = false;
                _clientFloating.ScreenPosition = ClientFloatingPanelPosition;
                _clientFloating.Show();
            }
            #endif
        }

        void SimulateClient(int dt)
        {
            _model.Simulate(dt);
            if(_timeText != null)
            {
                _timeText.text = _model.TimeString;
            }
        }

        static readonly Fix64 InstanceMinScale = (Fix64)0.2f;
        static readonly Fix64 InstanceMaxScale = (Fix64)2.0f;

        void OnInstantiate(Fix64 x, Fix64 y, Fix64 z)
        {
            var scale = new Vector3(
                            (float)_random.Range(InstanceMinScale, InstanceMaxScale),
                            (float)_random.Range(InstanceMinScale, InstanceMaxScale),
                            (float)_random.Range(InstanceMinScale, InstanceMaxScale));
            
            var unit = UnityObjectPool.Spawn(_unitPrefab, transform,
                           new Vector3((float)x, (float)y * scale.y, (float)z), Quaternion.identity);
            
            unit.transform.localScale = scale;
        }

        void OnDurationEnd()
        {
            Attr result;
            byte playerNum = 0;
            if(_netLockstepClient != null)
            {
                playerNum = _netLockstepClient.PlayerNumber;
            }
            _model.Results.TryGetValue(playerNum, out result);
            if(_netLockstepClient != null)
            {
                _netLockstepClient.SendPlayerFinish(result);
            }
            else
            {
                _lockstep.Stop();
                OnClientEndReceived(result);
            }
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

            var loading = UnityObjectPool.Spawn(
                              _loadingPrefab, transform, p, Quaternion.identity);
            _lockstep.AddPendingCommand(cmd, (c) => FinishLoading(loading));
        }

        public void FinishLoading(GameObject loading)
        {
            UnityObjectPool.Recycle(loading);
        }

        public void OnRegionValueChanged(int pos)
        {
            switch(pos)
            {
            case 0:
                _photonRegion = CloudRegionCode.none;
                break;
            case 1:
                _photonRegion = CloudRegionCode.eu;
                break;
            case 2:
                _photonRegion = CloudRegionCode.us;
                break;
            case 3:
                _photonRegion = CloudRegionCode.asia;
                break;
            case 4:
                _photonRegion = CloudRegionCode.jp;
                break;
            case 5:
                _photonRegion = CloudRegionCode.au;
                break;
            case 6:
                _photonRegion = CloudRegionCode.usw;
                break;
            case 7:
                _photonRegion = CloudRegionCode.sa;
                break;
            case 8:
                _photonRegion = CloudRegionCode.cae;
                break;
            }
        }
    }
}
