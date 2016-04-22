using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Zenject;

public class SceneAdditiveLoader : MonoBehaviour
{
    [SerializeField]
    List<string> _scenes = new List<string>();

    [SerializeField]
    MonoBehaviour[] _monoBehavioursToInject;

    void PostBindings(DiContainer container)
    {
        if(_monoBehavioursToInject != null)
        {
            for(int i = 0; i < _monoBehavioursToInject.Length; ++i)
            {
                container.Inject(_monoBehavioursToInject[i]);
            }
        }
    }

    void Start()
    {
        for(int i = 0; i < _scenes.Count - 1; ++i)
        {
            SceneManager.LoadScene(_scenes[i], LoadSceneMode.Additive);
        }
        if(_scenes.Count > 0)
        {
            ZenUtil.LoadSceneAdditive(_scenes[_scenes.Count - 1], null, PostBindings);
        }   
    }
}
