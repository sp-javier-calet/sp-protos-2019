using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SocialPoint.Base;
using SocialPoint.Lockstep;
using SocialPoint.Lockstep.Network;
using SocialPoint.Dependency;
using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.Pooling;
using SocialPoint.Network;
using FixMath.NET;
using System;
using System.IO;

public enum GameLockstepMode
{
    None,
    Local,
    Replay,
    Client,
    Server,
    Host
}

public class GameLockstepBehaviour : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    Slider _manaSlider;

    [SerializeField]
    GameObject _unitPrefab;

    [SerializeField]
    GameObject _loadingPrefab;

    [SerializeField]
    GameObject _gameContainer;

    [SerializeField]
    GameObject _setupContainer;

    ClientLockstepController _lockstep;
    LockstepModel _model;
    LockstepReplay _replay;
    LockstepCommandFactory _factory;
    INetworkClient _netClient;
    ClientLockstepNetworkController _netLockstepClient;
    INetworkServer _netServer;
    ServerLockstepNetworkController _netLockstepServer;
    GameLockstepMode _mode;

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
        _lockstep.LocalSimulationDelay = 500;
        _replay = ServiceLocator.Instance.Resolve<LockstepReplay>();
        _factory = ServiceLocator.Instance.Resolve<LockstepCommandFactory>();
        _lockstep.Simulate += Simulate;

        _model = new LockstepModel();
        _model.OnInstantiate += OnInstantiate;

        _lockstep.RegisterCommandLogic<ClickCommand>(new ClickCommandLogic(_model));
        _factory.Register<ClickCommand>(1);

        _mode = GameLockstepMode.None;

        if(_gameContainer != null)
        {
            _gameContainer.SetActive(false);
        }
    }

    void OnDestroy()
    {
        _lockstep.Simulate -= Simulate;
        _model.OnInstantiate -= OnInstantiate;
    }

    public void OnLocalClicked()
    {
        SetupGameScreen();
        _mode = GameLockstepMode.Local;
        _replay.Record();
        _lockstep.Start();
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
        _mode = GameLockstepMode.Replay;
        _replay.Replay();
        _lockstep.Start();
    }

    public void OnClientClicked()
    {
        SetupGameScreen();
        _mode = GameLockstepMode.Client;
        StartClient();
    }

    void StartClient()
    {
        _netClient = ServiceLocator.Instance.Resolve<INetworkClient>();
        _netLockstepClient = ServiceLocator.Instance.Resolve<ClientLockstepNetworkController>();
        _netClient.Connect();
        _netLockstepClient.SendPlayerReady();
    }

    public void OnServerClicked()
    {
        SetupGameScreen();
        _mode = GameLockstepMode.Server;
        StartServer();
        _netLockstepServer.LocalPlayerReady();
    }

    void StartServer()
    {
        _netServer = ServiceLocator.Instance.Resolve<INetworkServer>();
        _netLockstepServer = ServiceLocator.Instance.Resolve<ServerLockstepNetworkController>();
        _netServer.Start();
    }

    public void OnHostClicked()
    {
        SetupGameScreen();
        _mode = GameLockstepMode.Host;
        StartServer();
        StartClient();
    }

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
        }
        if(_netLockstepServer != null)
        {
            _netLockstepServer.Stop();
        }
        if(_netServer != null)
        {
            _netServer.Stop();
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
    }

    void Simulate(int dt)
    {
        _model.Simulate(dt);
    }

    void OnInstantiate(Fix64 x, Fix64 y, Fix64 z)
    {
        ObjectPool.Spawn(_unitPrefab, transform,
            new Vector3((float)x, (float)y, (float)z), Quaternion.identity);
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