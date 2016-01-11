using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneAdditiveLoader : MonoBehaviour
{
    [SerializeField]
    List<string> _scenes = new List<string>();

    void Start()
    {
        _scenes.ForEach(scene => SceneManager.LoadScene(scene, LoadSceneMode.Additive));
    }
}
