
using System;
using System.Collections.Generic;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.UI;
using SocialPoint.Rendering.Components;

public class CP_SceneManager : MonoBehaviour
{
    public enum BattleState
    {
        E_NONE,
        E_SEMAPHORE,
        E_PLAYING,
        E_WIN,
        E_WIN_AFTER,
        E_GAMEOVER,
        E_GAMEOVER_AFTER
    }

    public enum SceneObjectTypes
    {
        E_NONE = 0,

        E_WALL_LEFT = (1 << 0),
        E_WALL_RIGHT = (1 << 1),

        E_CHECKPOINT = (1 << 2),

        E_BOXES_01 = (1 << 3),
        E_BOXES_02 = (1 << 4),

        E_WATER_01 = (1 << 7),

        E_FISHES = (1 << 10),

        E_BALL_LOW = (1 << 13),
        E_BALL_MEDIUM = (1 << 14),
        E_BALL_HIGH = (1 << 15),

        E_CHEST_NUTS = (1 << 16)
    }

    int[] Stage01 =
    {
        (int)SceneObjectTypes.E_NONE, (int)SceneObjectTypes.E_NONE, (int)SceneObjectTypes.E_NONE, (int)SceneObjectTypes.E_NONE,
        (int)SceneObjectTypes.E_NONE
    };
    //int[] Stage01 = {(int)SceneObjectTypes.E_NONE, (int)SceneObjectTypes.E_BOXES_01 + (int)SceneObjectTypes.E_WATER_01 + (int)SceneObjectTypes.E_FISHES, (int)SceneObjectTypes.E_NONE, (int)SceneObjectTypes.E_NONE};
    /*
    int[] Stage01 =
    {
        (int)SceneObjectTypes.E_NONE,                 (int)SceneObjectTypes.E_BALL_LOW,                                                                     (int)SceneObjectTypes.E_CHEST_NUTS,                                                                          (int)SceneObjectTypes.E_BOXES_01,
        (int)SceneObjectTypes.E_NONE,                 (int)SceneObjectTypes.E_WATER_01,                                                                     (int)SceneObjectTypes.E_BALL_MEDIUM,                                                                         (int)SceneObjectTypes.E_BOXES_02,
        (int)SceneObjectTypes.E_NONE,                 (int)SceneObjectTypes.E_BALL_HIGH,                                                                    (int)SceneObjectTypes.E_WATER_01 + (int)SceneObjectTypes.E_FISHES,                                           (int)SceneObjectTypes.E_BOXES_01 + (int)SceneObjectTypes.E_CHEST_NUTS,
        (int)SceneObjectTypes.E_NONE,                 (int)SceneObjectTypes.E_BOXES_02 + (int)SceneObjectTypes.E_WATER_01,                                  (int)SceneObjectTypes.E_CHEST_NUTS + (int)SceneObjectTypes.E_BALL_LOW,                                       (int)SceneObjectTypes.E_CHEST_NUTS + (int)SceneObjectTypes.E_WATER_01,
        (int)SceneObjectTypes.E_BALL_HIGH,            (int)SceneObjectTypes.E_WATER_01 + (int)SceneObjectTypes.E_FISHES + (int)SceneObjectTypes.E_BOXES_01, (int)SceneObjectTypes.E_BOXES_02 + (int)SceneObjectTypes.E_CHEST_NUTS,                                       (int)SceneObjectTypes.E_WATER_01 + (int)SceneObjectTypes.E_BALL_LOW,
        (int)SceneObjectTypes.E_CHEST_NUTS,           (int)SceneObjectTypes.E_WATER_01 + (int)SceneObjectTypes.E_FISHES + (int)SceneObjectTypes.E_BOXES_02, (int)SceneObjectTypes.E_WATER_01 + (int)SceneObjectTypes.E_FISHES + (int)SceneObjectTypes.E_BALL_HIGH,       (int)SceneObjectTypes.E_NONE,
        (int)SceneObjectTypes.E_NONE
    };
    */

    const int kMaxSceneSize = 256;
    public const int kScenePieceSize = 16;

    public GameObject SceneMainPiece = null;
    public List<GameObject> SceneBackgrounds = null;
    public int ScenePiecesLength = -1;
    public GameObject PlayerGO = null;
    public Button TurnBack = null;
    public Button TurnForward = null;
    public Button Suicide = null;
    public Animation GirlHeadUI = null;
    public PlayerStats PlayerStats = null;
    public bool SuicideEnabled = true;
    public BattleState CurrentBattleState = BattleState.E_NONE;
    public CP_Semaphore Semaphore = null;
    public CP_ProgressBar ProgressBar = null;
    public GameObject GameOverTextGO = null;
    public GameObject YouWinTextGO = null;
    public GameObject SecondTextGO = null;
    public GameObject ThirdTextGO = null;
    public GameObject FourthTextGO = null;

    GameObject _sceneObjectBase = null;
    List<int> _sceneBackgrounds = new List<int>();
    List<int> _sceneObjects = new List<int>();
    List<GameObject> _sceneBackgroundsGO = new List<GameObject>();
    CP_PlayerController _player = null;
    BCSHModifier SuicideBCSH = null;
    BCSHModifier TurnForwardBCSH = null;
    BCSHModifier TurnBackBCSH = null;

    int _sceneMapLastGeneratedIndex = -1;
    int _sceneMapLastChekpointIndex = -1;

    void Awake()
    {
        Application.targetFrameRate = 60;
        Time.fixedDeltaTime =  Time.timeScale * 0.02f;

        _sceneObjectBase = new GameObject("SceneBase");
        _sceneObjectBase.transform.SetParent(transform, true);

        if (ScenePiecesLength == -1 || ScenePiecesLength > kMaxSceneSize)
        {
            //ScenePiecesLength = kMaxSceneSize;
            ScenePiecesLength = Stage01.Length + 1;
        }

        GenerateMapData(ScenePiecesLength);
        SearchForLastChekpointIndex();

        for (var i = 0; i < 3; ++i)
        {
            GenerateMap(i, i, true);
        }
        _sceneMapLastGeneratedIndex = 1;

        GeneratePlayer();

        SetCurrentGameState(BattleState.E_SEMAPHORE);
    }

    void SearchForLastChekpointIndex()
    {
        for(var i = 0; i < _sceneObjects.Count; ++i)
        {
            if((_sceneObjects[i] & (int)SceneObjectTypes.E_CHECKPOINT) != 0)
            {
                _sceneMapLastChekpointIndex = i;
            }
        }
    }

    public void SetCurrentGameState(BattleState state)
    {
        switch(state)
        {
            case BattleState.E_SEMAPHORE:
            {
                SetTurnEnabled(false);
                SetSuicideEnabled(false);

                if(Semaphore != null)
                {
                    Semaphore.StartSemaphore(this);
                }

                break;
            }

            case BattleState.E_PLAYING:
            {
                SetTurnEnabled(true);

                if(_player != null)
                {
                    _player.StartRun();
                }

                break;
            }

            case BattleState.E_WIN:
            {
                SetTurnEnabled(false);
                SetSuicideEnabled(false);

                if(CP_GameManager.Instance.CurrentGameState == CP_GameManager.GameState.E_PLAYING_1_PLAYER)
                {
                    if(YouWinTextGO != null)
                    {
                        YouWinTextGO.SetActive(true);
                    }
                }

                break;
            }

            case BattleState.E_WIN_AFTER:
            {
                if(YouWinTextGO != null)
                {
                    YouWinTextGO.SetActive(false);
                }

                if(CP_GameManager.Instance.CurrentGameState == CP_GameManager.GameState.E_PLAYING_1_PLAYER)
                {
                    CP_GameManager.Instance.SetGameState(CP_GameManager.GameState.E_TITLE);
                }

                break;
            }

            case BattleState.E_GAMEOVER:
            {
                SetTurnEnabled(false);
                SetSuicideEnabled(false);

                if(GameOverTextGO != null)
                {
                    GameOverTextGO.SetActive(true);
                }

                break;
            }

            case BattleState.E_GAMEOVER_AFTER:
            {
                if(GameOverTextGO != null)
                {
                    GameOverTextGO.SetActive(false);
                }

                if(CP_GameManager.Instance.CurrentGameState == CP_GameManager.GameState.E_PLAYING_1_PLAYER)
                {
                    CP_GameManager.Instance.SetGameState(CP_GameManager.GameState.E_TITLE);
                }

                break;
            }
        }

        CurrentBattleState = state;
    }

    void GenerateMapData(int length)
    {
        _sceneBackgrounds.Clear();
        _sceneObjects.Clear();

        List<int> previousRandoms = new List<int>();

        for(var i = 0; i < length; ++i)
        {
            var randomBackground = -1;
            while(randomBackground == -1 || previousRandoms.Contains(randomBackground))
            {
                randomBackground = RandomUtils.Range(0, SceneBackgrounds.Count);
            }

            if (previousRandoms.Count == 2)
            {
                previousRandoms.RemoveAt(0);
            }

            previousRandoms.Add(randomBackground);
            _sceneBackgrounds.Add(randomBackground);

            var sceneObjects = 0;
            if (i == 0)
            {
                sceneObjects += (int) SceneObjectTypes.E_WALL_LEFT;
            }
            else if (i == length-1)
            {
                sceneObjects += (int) SceneObjectTypes.E_WALL_RIGHT;
            }

            if(i == 1 || i % 4 == 1)
            {
                sceneObjects += (int) SceneObjectTypes.E_CHECKPOINT;
            }

            _sceneObjects.Add(sceneObjects);
        }

        var startingIndex = 1;
        for(var i = 0; i < Stage01.Length; ++i)
        {
            _sceneObjects[startingIndex+i] += Stage01[i];
        }
    }

    void GenerateMap(int mapPos, int mapIndex, bool goingRight)
    {
        if(SceneMainPiece != null)
        {
            GameObject newSceneMainPiece = Instantiate(SceneMainPiece);
            newSceneMainPiece.transform.SetParent(_sceneObjectBase.transform, true);

            newSceneMainPiece.transform.position = new Vector3((mapPos * kScenePieceSize) + (kScenePieceSize * 0.5f), 0.0f, 0.0f);

            if(SceneBackgrounds[_sceneBackgrounds[mapIndex]] != null)
            {
                GameObject newSceneBackground = Instantiate(SceneBackgrounds[_sceneBackgrounds[mapIndex]]);
                newSceneBackground.transform.SetParent(newSceneMainPiece.transform, false);
            }

            GenerateSceneObjects(mapPos, _sceneObjects[mapIndex], newSceneMainPiece);

            if (_sceneBackgroundsGO.Count == 3)
            {
                if (goingRight)
                {
                    Destroy(_sceneBackgroundsGO[0]);
                    _sceneBackgroundsGO.RemoveAt(0);

                    _sceneBackgroundsGO.Add(newSceneMainPiece);
                }
                else
                {
                    Destroy(_sceneBackgroundsGO[_sceneBackgroundsGO.Count-1]);
                    _sceneBackgroundsGO.RemoveAt(_sceneBackgroundsGO.Count-1);

                    _sceneBackgroundsGO.Insert(0, newSceneMainPiece);
                }
            }
            else
            {
                _sceneBackgroundsGO.Add(newSceneMainPiece);
            }
        }
    }

    void GenerateSceneObjects(int mapPos, int sceneObjects, GameObject parentGO)
    {
        foreach(var objectType in Enum.GetValues(typeof(SceneObjectTypes)))
        {
            if((SceneObjectTypes) objectType == SceneObjectTypes.E_NONE)
            {
                continue;
            }

            if ((sceneObjects & (int)objectType) != 0)
            {
                SceneObjectTypes objectToCreate = (SceneObjectTypes) objectType;
                GameObject baseGO = Resources.Load("Prefabs/SceneObjects/" + objectToCreate.ToString()) as GameObject;
                if (baseGO != null)
                {
                    GameObject newSceneBackground = Instantiate(baseGO);
                    newSceneBackground.transform.SetParent(parentGO.transform, false);

                    if((SceneObjectTypes)objectType == SceneObjectTypes.E_CHECKPOINT && mapPos == _sceneMapLastChekpointIndex)
                    {
                        CP_Checkpoint checkpoint = newSceneBackground.GetComponentInChildren<CP_Checkpoint>();
                        if(checkpoint != null)
                        {
                            checkpoint.SetVictoryCheckpoint(this);
                        }
                    }
                }
            }
        }
    }

    void GeneratePlayer()
    {
        if(PlayerGO != null)
        {
            GameObject playerGO = Instantiate(PlayerGO);
            _player = playerGO.GetComponent<CP_PlayerController>();

            if (_player != null)
            {
                _player.SetSceneManager(this);

                if(ProgressBar != null)
                {
                    ProgressBar.AddPlayerToFollow(playerGO, _sceneMapLastChekpointIndex);
                }
            }
        }
    }

    public void CheckMapGeneration(bool forced = false)
    {
        if (_player != null)
        {
            var playerXPosition = (int) _player.transform.position.x;
            var playerIndexPos = playerXPosition / kScenePieceSize;

            if(forced)
            {
                _sceneMapLastGeneratedIndex = playerIndexPos;

                for(var i = 0; i < _sceneBackgroundsGO.Count; ++i)
                {
                    Destroy(_sceneBackgroundsGO[i]);
                }
                _sceneBackgroundsGO.Clear();

                for (var i = -1; i < 2; ++i)
                {
                    GenerateMap(_sceneMapLastGeneratedIndex+i, _sceneMapLastGeneratedIndex+i, true);
                }
            }
            else
            {
                if (playerIndexPos != _sceneMapLastGeneratedIndex || forced)
                {
                    var goingRight = (playerIndexPos > _sceneMapLastGeneratedIndex);
                    _sceneMapLastGeneratedIndex = playerIndexPos;

                    if (goingRight) playerIndexPos++;
                    else playerIndexPos--;

                    var playerIndexPosMap = playerIndexPos;

                    if (playerIndexPos < 0)
                    {
                        playerIndexPos += _sceneBackgrounds.Count;
                    }
                    else if (playerIndexPos >= _sceneBackgrounds.Count)
                    {
                        playerIndexPos -= _sceneBackgrounds.Count;
                    }

                    GenerateMap(playerIndexPosMap, playerIndexPos, goingRight);
                }
            }
        }
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

    public void OnPressedSuicide()
    {
        if (_player != null)
        {
            _player.OnPressedSuicide();
        }
    }

    public void SetTurnEnabled(bool enabled)
    {
        if (TurnBack != null)
        {
            TurnBack.interactable = enabled;

            if (TurnBackBCSH == null)
            {
                TurnBackBCSH = TurnBack.GetComponent<BCSHModifier>();
            }

            if (TurnBackBCSH != null)
            {
                if (enabled)
                {
                    TurnBackBCSH.ApplyBCSHState("default");
                }
                else
                {
                    TurnBackBCSH.ApplyBCSHState("disabled");
                }
            }
        }

        if (TurnForward != null)
        {
            TurnForward.interactable = enabled;

            if (TurnForwardBCSH == null)
            {
                TurnForwardBCSH = TurnForward.GetComponent<BCSHModifier>();
            }

            if (TurnForwardBCSH != null)
            {
                if (enabled)
                {
                    TurnForwardBCSH.ApplyBCSHState("default");
                }
                else
                {
                    TurnForwardBCSH.ApplyBCSHState("disabled");
                }
            }
        }
    }

    public void SetSuicideEnabled(bool enabled, bool gameplayReason = false)
    {
        if (Suicide != null)
        {
            if(gameplayReason)
            {
                SuicideEnabled = enabled;
            }

            Suicide.interactable = enabled;

            if (SuicideBCSH == null)
            {
                SuicideBCSH = Suicide.GetComponent<BCSHModifier>();
            }

            if (SuicideBCSH != null)
            {
                if (enabled)
                {
                    SuicideBCSH.ApplyBCSHState("default");
                }
                else
                {
                    SuicideBCSH.ApplyBCSHState("disabled");
                }
            }
        }
    }

    public void PressedTurnBack()
    {
        if (TurnBack != null)
        {
            TurnBack.gameObject.SetActive(false);
        }
        if (TurnForward != null)
        {
            TurnForward.gameObject.SetActive(true);
        }

        if (_player != null)
        {
            _player.Turn();
        }
    }
    public void PressedTurnForward()
    {
        if (TurnBack != null)
        {
            TurnBack.gameObject.SetActive(true);
        }
        if (TurnForward != null)
        {
            TurnForward.gameObject.SetActive(false);
        }

        if (_player != null)
        {
            _player.Turn();
        }
    }
}
