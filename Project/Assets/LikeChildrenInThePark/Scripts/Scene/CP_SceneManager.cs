
using System.Collections.Generic;
using SocialPoint.Utils;
using UnityEngine;

public class CP_SceneManager : MonoBehaviour
{
    public GameObject SceneMainPiece = null;
    public List<GameObject> SceneBackgrounds = null;
    public int ScenePiecesLength = 0;

    GameObject _sceneObjectBase = null;

    void Awake()
    {
        _sceneObjectBase = new GameObject("SceneBase");
        _sceneObjectBase.transform.SetParent(transform, true);

        GenerateMap();
    }

    void GenerateMap()
    {
        for(var i = 0; i < ScenePiecesLength; ++i)
        {
            if(SceneMainPiece != null)
            {
                GameObject newSceneMainPiece = Instantiate(SceneMainPiece);
                newSceneMainPiece.transform.SetParent(_sceneObjectBase.transform, true);

                newSceneMainPiece.transform.position = new Vector3(i * 16.0f, 0.0f, 0.0f);
            }

            var randomBackground = RandomUtils.Range(0, SceneBackgrounds.Count);
            if(SceneBackgrounds[randomBackground] != null)
            {
                GameObject newSceneBackground = Instantiate(SceneBackgrounds[randomBackground]);
                newSceneBackground.transform.SetParent(_sceneObjectBase.transform, true);

                newSceneBackground.transform.position = new Vector3(i * 16.0f, 0.0f, 0.0f);
            }
        }
    }
}
