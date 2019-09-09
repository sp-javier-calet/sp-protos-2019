
using System;
using System.Collections.Generic;
using SocialPoint.Utils;
using TMPro;
using UnityEngine;

public class GSB_SceneManager : MonoBehaviour
{
    [Serializable]
    public class WaveData
    {
        public int NumEnemies;
        public float Rhythm;
        public float RhythmInterWave;
        public float EnemiesSpeedMultiplier = 1f;
    }

    public List<WaveData> WaveDatas = new List<WaveData>();

    [Serializable]
    public class CombinationUniqueData
    {
        public int ShipColorUniqueAmmoReward;
    }
    [Serializable]
    public class CombinationRepeatData
    {
        public int ShipColorRepeatAmount;
        public int ShipColorRepeatAmmoReward;
    }

    public List<CombinationUniqueData> CombinationDatas = new List<CombinationUniqueData>();
    public List<CombinationRepeatData> CombinationRepeatDatas = new List<CombinationRepeatData>();

    public enum EBattleState
    {
        E_NONE,
        E_WAVE_START,
        E_PLAYING,
        E_WIN,
        E_LOSE,
        E_GAMEOVER,
        E_WAVE_END
    }

    public static GSB_SceneManager Instance = null;

    public GameObject WorldUIParent = null;
    public GameObject WorldUICombo = null;
    public GameObject HealthBox = null;
    public GameObject AmmoBox = null;
    public GameObject TimeBar = null;
    public RectTransform TimeBarFiller = null;
    public MeshFilter SelectionMesh;
    public List<LineRenderer> SelectionLine = new List<LineRenderer>();
    public TextMeshProUGUI WaveLabel = null;
    public TextMeshProUGUI GameOverLabel = null;
    public TextMeshProUGUI WinLabel = null;
    public TextMeshProUGUI LoseLabel = null;

    public float SlowDown = 0.1f;
    public int HealthMax = 18;
    public int AmmoMax = 4;
    public float AmmoRegenerationTime = 0.75f;
    public int TargetTimeMS = 2000;
    public float StartingInterWaves = 5;
    public int MaxDifficultyWave = 5;
    public int HealthRecoveryAfterWave = 3;
    public int ShootStopTime = 1000;

    GSB_PlayerController _player;
    public GSB_PlayerController Player { get { return _player; } }

    List<GSB_EnemyController> _enemies = new List<GSB_EnemyController>();
    public List<GSB_EnemyController> Enemies { get { return _enemies; } }

    EBattleState _battleState = EBattleState.E_NONE;
    public EBattleState BattleState { get { return _battleState; } }

    EBattleState _battleSubState = EBattleState.E_NONE;
    public EBattleState BattleSubState { get { return _battleSubState; } }

    int _currentWave = 0;
    public int CurrentWave { get { return _currentWave; } }

    long _stateStartTime = 0;
    long _stateTime = 0;

    List<WaveData> _currentWaveDatasInStage = new List<WaveData>();
    int _currentWaveDataIdx = 0;

    WaveData _currentWaveData = null;
    public WaveData CurrentWaveData { get { return _currentWaveData; } }

    Timer _waveTimer = new Timer();
    int _currentWaveEnemy = 0;
    List<int> _lastEnemyPositions = new List<int>();
    List<int> _lastEnemyTypes = new List<int>();

    void Awake()
    {
        Instance = this;

        GeneratePlayer();

        _currentWave = 0;

        ChangeState(EBattleState.E_WAVE_START);
    }

    void GeneratePlayer()
    {
        if(GSB_GameManager.Instance.PlayerOfflineGO != null)
        {
            GameObject player = Instantiate(GSB_GameManager.Instance.PlayerOfflineGO);
            if(player != null)
            {
                _player = player.GetComponent<GSB_PlayerController>();
            }
        }
    }

    void GenerateRandomCurrentWave()
    {
        _currentWaveDatasInStage.Clear();

        var difficultyWaveIntervals = WaveDatas.Count / MaxDifficultyWave;
        var startWaveDataIdx = (_currentWave - 1) * difficultyWaveIntervals;
        var amountInterWaves = StartingInterWaves + (_currentWave - 1);

        for(var i = 0; i < amountInterWaves; ++i)
        {
            var interWaveToAdd = (startWaveDataIdx + RandomUtils.Range(0, difficultyWaveIntervals+1));
            if(interWaveToAdd >= WaveDatas.Count)
            {
                interWaveToAdd = WaveDatas.Count - 1;
            }

            _currentWaveDatasInStage.Add(WaveDatas[interWaveToAdd]);
        }
    }

    public void ChangeSubState(EBattleState battleState)
    {
        _stateStartTime = TimeUtils.TimestampMilliseconds;

        switch(battleState)
        {
            case EBattleState.E_WIN:
            {
                GameAudioManager.SharedInstance.StopSound(0);
                GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_youwin");

                if(WinLabel != null)
                {
                    WinLabel.gameObject.SetActive(true);
                }

                _stateTime = 4000;

                break;
            }

            case EBattleState.E_LOSE:
            {
                GameAudioManager.SharedInstance.StopSound(0);
                GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_gameover");

                if(LoseLabel != null)
                {
                    LoseLabel.gameObject.SetActive(true);
                }

                _stateTime = 4000;

                break;
            }
        }

        _battleSubState = battleState;
    }

    public void ChangeState(EBattleState battleState)
    {
        _stateStartTime = TimeUtils.TimestampMilliseconds;

        switch(battleState)
        {
            case EBattleState.E_WAVE_START:
            {
                _currentWave++;
                _currentWaveData = null;
                _currentWaveDataIdx = 0;

                GenerateRandomCurrentWave();

                if(WaveLabel != null)
                {
                    WaveLabel.text = "WAVE ";
                    if(_currentWave < 10)
                    {
                        WaveLabel.text += "0";
                    }
                    WaveLabel.text += _currentWave;

                    WaveLabel.gameObject.SetActive(true);
                }

                if(Player != null)
                {
                    Player.MakeDamage(-HealthRecoveryAfterWave);
                }

                GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_wavestart");

                _stateTime = 3000;

                break;
            }

            case EBattleState.E_PLAYING:
            {
                if(WaveLabel != null)
                {
                    WaveLabel.gameObject.SetActive(false);
                }

                break;
            }

            case EBattleState.E_WAVE_END:
            {
                _stateTime = 2000;

                break;
            }

            case EBattleState.E_GAMEOVER:
            {
                if(GSB_GameManager.Instance.CurrentGameState == GSB_GameManager.GameState.E_PLAYING_1_PLAYER)
                {
                    GameAudioManager.SharedInstance.StopSound(0);
                    GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_gameover");

                    if(GameOverLabel != null)
                    {
                        GameOverLabel.gameObject.SetActive(true);
                    }

                    _stateTime = 4000;
                }
                else if(GSB_GameManager.Instance.CurrentGameState == GSB_GameManager.GameState.E_PLAYING_2_VERSUS)
                {
                    if(GSB_GameManager.Instance.NetworkController.PlayerOnlineController != null)
                    {
                        GSB_GameManager.Instance.NetworkController.PlayerOnlineController.CmdPlayerHasDiedClient(GSB_GameManager.Instance.NetworkController.PlayerControllerId);
                    }

                    _stateTime = 0;
                }

                break;
            }
        }

        _battleState = battleState;
    }

    public void OnPressedDown()
    {
        if (_player != null)
        {
            _player.OnPressedDown();
        }
    }
    public void OnPressedUp()
    {
        if (_player != null)
        {
            _player.OnPressedUp();
        }
    }

    void GenerateEnemy()
    {
        var randomPosition = -1;
        while(randomPosition == -1)
        {
            randomPosition = RandomUtils.Range(0, 9);

            if(_lastEnemyPositions.Contains(randomPosition) || _lastEnemyPositions.Contains(randomPosition-1) || _lastEnemyPositions.Contains(randomPosition+1))
            {
                randomPosition = -1;
            }
        }

        if (_lastEnemyPositions.Count == 2)
        {
            _lastEnemyPositions.RemoveAt(0);
        }

        _lastEnemyPositions.Add(randomPosition);

        if(GSB_GameManager.Instance.EnemyGO != null)
        {
            GameObject newEnemy = Instantiate(GSB_GameManager.Instance.EnemyGO);
            if(newEnemy != null)
            {
                newEnemy.transform.position = new Vector3(-2f + (randomPosition * 0.5f), 6f, -0.15f);

                GSB_EnemyController enemyCtrl = newEnemy.GetComponent<GSB_EnemyController>();
                if(enemyCtrl != null)
                {
                    var randomType = -1;
                    while(randomType == -1 || _lastEnemyTypes.Contains(randomType))
                    {
                        randomType = RandomUtils.Range(0, 4);
                    }

                    if (_lastEnemyTypes.Count == 2)
                    {
                        _lastEnemyTypes.RemoveAt(0);
                    }

                    _lastEnemyTypes.Add(randomType);

                    enemyCtrl.SetShipType((GSB_EnemyController.EShipType) randomType);
                    enemyCtrl.SetWaveSpeedMultiplier(_currentWaveData.EnemiesSpeedMultiplier);

                    _enemies.Add(enemyCtrl);
                }
            }
        }

    }

    void Update()
    {
        switch(_battleState)
        {
            case EBattleState.E_WAVE_START:
            {
                if(TimeUtils.TimestampMilliseconds > _stateStartTime + _stateTime)
                {
                    ChangeState(EBattleState.E_PLAYING);
                }

                break;
            }

            case EBattleState.E_PLAYING:
            {
                if(_currentWaveData == null)
                {
                    if(_currentWaveDataIdx < _currentWaveDatasInStage.Count)
                    {
                        _currentWaveEnemy = 0;
                        _currentWaveData = _currentWaveDatasInStage[_currentWaveDataIdx];
                        _currentWaveDataIdx++;

                        _waveTimer.Wait(_currentWaveData.Rhythm);
                    }
                    else
                    {
                        if(_enemies.Count == 0)
                        {
                            ChangeState(EBattleState.E_WAVE_END);
                        }
                    }
                }
                else
                {
                    if(_waveTimer.IsFinished)
                    {
                        if(_currentWaveEnemy == _currentWaveData.NumEnemies)
                        {
                            _currentWaveData = null;
                        }
                        else
                        {
                            GenerateEnemy();
                            _currentWaveEnemy++;

                            if(_currentWaveEnemy == _currentWaveData.NumEnemies)
                            {
                                _waveTimer.Wait(_currentWaveData.RhythmInterWave);
                            }
                            else
                            {
                                _waveTimer.Wait(_currentWaveData.Rhythm);
                            }
                        }
                    }
                }

                break;
            }

            case EBattleState.E_WAVE_END:
            {
                if(TimeUtils.TimestampMilliseconds > _stateStartTime + _stateTime)
                {
                    ChangeState(EBattleState.E_WAVE_START);
                }

                break;
            }

            case EBattleState.E_GAMEOVER:
            {
                if(TimeUtils.TimestampMilliseconds > _stateStartTime + _stateTime)
                {
                    if(GSB_GameManager.Instance.CurrentGameState == GSB_GameManager.GameState.E_PLAYING_1_PLAYER)
                    {
                        GSB_GameManager.Instance.SetGameState(GSB_GameManager.GameState.E_TITLE);
                    }
                }

                break;
            }
        }

        switch(_battleSubState)
        {
            case EBattleState.E_WIN:
            case EBattleState.E_LOSE:
            {
                if(TimeUtils.TimestampMilliseconds > _stateStartTime + _stateTime)
                {
                    GSB_GameManager.Instance.SetGameState(GSB_GameManager.GameState.E_TITLE);
                }

                break;
            }
        }
    }
}
