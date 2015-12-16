using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneAdditiveLoader : MonoBehaviour
{
    [SerializeField]
    List<string> _scenes = new List<string>();

    void Start()
    {
        _scenes.ForEach(scene => Application.LoadLevelAdditive(scene));
    }
}
