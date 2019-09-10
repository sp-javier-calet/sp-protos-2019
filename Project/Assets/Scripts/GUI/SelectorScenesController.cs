//-----------------------------------------------------------------------
// SelectorScenesController.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using SocialPoint.Base;
using System;

public class SelectorScenesController : MonoBehaviour
{
    [SerializeField] Button _prefabButton;

    [SerializeField] ScrollRect _scrollRect;

    public Action<string> OnGoToScene { get; set; }

    string[] _scenes;

    void Start()
    {
        _scenes = ScenesData.Instance.ScenesNames;

        ShowScenesUI();
    }

    void ShowScenesUI()
    {
        foreach(string sceneName in _scenes)
        {
            InstantiateScenesButton(sceneName);
        }
    }

    void InstantiateScenesButton(string nameScene)
    {
        Button button = Instantiate(_prefabButton, _scrollRect.content, true);
        var transformAccess = button.transform;
        transformAccess.localPosition = Vector3.zero;
        transformAccess.localScale = Vector3.one;
        button.GetComponentInChildren<Text>().text = nameScene.ToUpper();
        button.onClick.AddListener(() => GoToScene(nameScene));
    }

    public void GoToScene(string nameScene)
    {
        OnGoToScene?.Invoke(nameScene);

        gameObject.SetActive(false);
    }
}
