
using System.Collections.Generic;
using SocialPoint.Utils;
using UnityEngine;

public class CP_SceneManager : MonoBehaviour
{
    const int kMaxSceneSize = 256;
    public const int kScenePieceSize = 16;

    public GameObject SceneMainPiece = null;
    public List<GameObject> SceneBackgrounds = null;
    public int ScenePiecesLength = -1;
    public GameObject PlayerGO = null;

    GameObject _sceneObjectBase = null;
    List<int> _sceneBackgrounds = new List<int>();
    List<GameObject> _sceneBackgroundsGO = new List<GameObject>();
    CP_PlayerController _player = null;

    int _sceneMapLastGeneratedIndex = -1;

    void Awake()
    {
        Application.targetFrameRate = 60;
        Time.fixedDeltaTime =  Time.timeScale * 0.02f;

        _sceneObjectBase = new GameObject("SceneBase");
        _sceneObjectBase.transform.SetParent(transform, true);

        if (ScenePiecesLength == -1 || ScenePiecesLength > kMaxSceneSize)
        {
            ScenePiecesLength = kMaxSceneSize;
        }

        GenerateMapData(ScenePiecesLength);

        for (var i = 0; i < 3; ++i)
        {
            GenerateMap(i, i, true);
        }
        _sceneMapLastGeneratedIndex = 1;

        GeneratePlayer();
    }

    void GenerateMapData(int length)
    {
        _sceneBackgrounds.Clear();

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
        }
    }

    void GenerateMap(int mapPos, int mapIndex, bool goingRight)
    {
        Debug.Log("GenerateMap " + mapPos + " " + mapIndex + " " + goingRight);

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

    void GeneratePlayer()
    {
        if(PlayerGO != null)
        {
            GameObject playerGO = Instantiate(PlayerGO);
            _player = playerGO.GetComponent<CP_PlayerController>();            

            if (_player != null)
            {
                _player.SetSceneManager(this);
            }
        }
    }

    public void CheckMapGeneration()
    {
        if (_player != null)
        {
            var playerXPosition = (int) _player.transform.position.x;
            var playerIndexPos = playerXPosition / kScenePieceSize;

            if (playerIndexPos != _sceneMapLastGeneratedIndex)
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
