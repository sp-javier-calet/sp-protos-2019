//-----------------------------------------------------------------------
// SelectorScenesController.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using SocialPoint.Base;
using SocialPoint.GUIControl;
using System;

public class SelectorScenesController : UIViewController
{
    [SerializeField] Button _prefabButton;

    [SerializeField] ScrollRect _scrollRect;

    public Action<string> OnGoToScene { get; set; }

    string[] _scenes;

    public SelectorScenesController()
    {
        IsFullScreen = true;
    }

    protected override void OnStart()
    {
        base.OnStart();
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

        Hide();
    }
}
