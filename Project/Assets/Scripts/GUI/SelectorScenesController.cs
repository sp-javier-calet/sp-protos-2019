using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocialPoint.Base;
using UnityEngine.SceneManagement;
using SocialPoint.GUIControl;
using System;

public class SelectorScenesController : UIViewController
{
    [SerializeField]
    private Button _prefabButton;

    [SerializeField]
    private ScrollRect _scrollRect;

    public Action<string> OnGoToScene { get; set; }

    private string[] _scenes;

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
        for(int i = 0; i < _scenes.Length; i++)
        {
            var name = _scenes[i];
            InstantiateScenesButton(name);
        }
    }

    void InstantiateScenesButton(string nameScene)
    {
        Button button = Instantiate<Button>(_prefabButton);
        button.transform.SetParent(_scrollRect.content);
        button.transform.localPosition = Vector3.zero;
        button.transform.localScale = Vector3.one;
        button.GetComponentInChildren<Text>().text = nameScene.ToUpper();
        button.onClick.AddListener(() => GoToScene(nameScene));
    }

    public void GoToScene(string nameScene)
    {
        if(OnGoToScene != null)
        {
            OnGoToScene(nameScene);
        }

        Hide();
    }
}
