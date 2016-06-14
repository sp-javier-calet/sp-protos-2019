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
        for(int i = 0; i < _scenes.Count; ++i)
        {
            SceneManager.LoadScene(_scenes[i], LoadSceneMode.Additive);
        }
    }
}
