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
    private GameObject _prefabButton = null;

    [SerializeField]
    private ScrollRect _scrollRect = null;

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
        Debug.Log("SCENES: ");
        for(int i = 0; i < _scenes.Length; i++)
        {
            var name = _scenes[i];
            Debug.Log(name);
            InstantiateScenesButton(name);
        }
    }

    void InstantiateScenesButton(string nameScene)
    {
        GameObject button = Instantiate<GameObject>(_prefabButton);
        button.transform.SetParent(_scrollRect.content);
        button.transform.localPosition = Vector3.zero;
        button.transform.localScale = Vector3.one;
        button.GetComponentInChildren<Text>().text = nameScene.ToUpper();
        Button buttonComponent = button.GetComponent<Button>();
        buttonComponent.onClick.AddListener(() => GoToScene(nameScene));
    }

    public void GoToScene(string nameScene)
    {
        Debug.Log("CLICKED BUTTON : "+nameScene);

        if(OnGoToScene != null)
        {
            OnGoToScene(nameScene);
        }

        Hide();
    }
//
//    protected override void OnDisappeared()
//    {
//        base.OnDisappeared();
//        //Destroy Canvas of all Scenes UI buttons
//        Destroy(_scrollRect.transform.parent.gameObject);
//
//        Destroy(gameObject);
//    }

}
