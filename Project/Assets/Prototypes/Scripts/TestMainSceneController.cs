using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Dependency;

public class TestMainSceneController : MonoBehaviour
{
    const string kCubesize = "cube_size";

    [SerializeField]
    private Transform _cubeTransform;

    ConfigModel _config;

    void Start()
    {
        _config = Services.Instance.Resolve<ConfigModel>();

        SetCubeSize();
    }

    void SetCubeSize()
    {
        Vector3 cubeSize = _config.GetGlobal(kCubesize) == null ? Vector3.one : Vector3.one * _config.GetGlobal(kCubesize).AsValue.ToFloat();
        _cubeTransform.localScale = cubeSize;
    }
}
