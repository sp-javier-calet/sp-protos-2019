using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SocialPoint.Attributes;
using SocialPoint.ServerSync;
using SocialPoint.Utils;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject]
    ICommandQueue _commandQueue;
    
    [Inject]
    PlayerModel _playerModel;
    
    [Inject]
    ConfigModel _configModel;
    
    [Inject]
    ISerializer<PlayerModel> _playerSerializer;
    
    void Start()
    {
        _commandQueue.AutoSync = SyncDelegate;

    }
    
    public Attr SyncDelegate()
    {
        return _playerSerializer.Serialize(_playerModel);
    }
    
}