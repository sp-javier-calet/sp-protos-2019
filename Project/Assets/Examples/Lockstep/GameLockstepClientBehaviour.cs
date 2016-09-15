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
    Replay
}

public class GameLockstepClientBehaviour : MonoBehaviour, IPointerClickHandler
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
        _model.Reset();
        SetupGameScreen();
        _mode = GameLockstepMode.Local;
        _replay.Record();
        _lockstep.Start(TimeUtils.TimestampMilliseconds);
    }

    public void OnReplayClicked()
    {
        _model.Reset();
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
            Log.e("Could not load replay file: "+e);
            return;
        }

        SetupGameScreen();
        _mode = GameLockstepMode.Replay;
        _replay.Replay();
        _lockstep.Start(TimeUtils.TimestampMilliseconds);
    }

    public void OnClientClicked()
    {
        _netClient = ServiceLocator.Instance.Resolve<INetworkClient>();
        _netLockstepClient = ServiceLocator.Instance.Resolve<ClientLockstepNetworkController>();
        _netClient.Connect();
        _netLockstepClient.SendPlayerReady();
    }

    public void OnServerClicked()
    {
        _netServer = ServiceLocator.Instance.Resolve<INetworkServer>();
        ServiceLocator.Instance.Resolve<ServerLockstepNetworkController>();
        _netServer.Start();
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

    void Simulate(long tsmillis)
    {
        _model.Simulate(tsmillis);
    }

    void OnInstantiate(Fix64 x, Fix64 y, Fix64 z)
    {
        ObjectPool.Spawn(_unitPrefab, transform,
            new Vector3((float)x, (float)y, (float)z), Quaternion.identity);
    }

    void Update()
    {
        _manaSlider.value = _model.ManaView;
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