using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocialPoint.Base;
using UnityEngine.SceneManagement;

public class SelectorScenesController : MonoBehaviour
{
    [SerializeField]
    private string _forcedSceneName = string.Empty;

    [SerializeField]
    private GameObject _prefabButton = null;

    [SerializeField]
    private ScrollRect _scrollRect = null;

    private string[] _scenes;

    void Start()
    {
        _scenes = ScenesData.Instance.ScenesNames;

        if(_forcedSceneName != string.Empty && _scenes.Contains<string>(_forcedSceneName))
        {
            GoToScene(_forcedSceneName);
            return;
        }

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
        button.GetComponentInChildren<Text>().text = nameScene.ToUpper();
        Button buttonComponent = button.GetComponent<Button>();
        buttonComponent.onClick.AddListener(() => GoToScene(nameScene));
    }

    public void GoToScene(string nameScene)
    {
        Debug.Log("CLICKED BUTTON : "+nameScene);

        SceneManager.LoadScene(nameScene, LoadSceneMode.Additive);

        Clean();

    }

    void Clean()
    {
        //Destroy Canvas of all Scenes UI buttons
        Destroy(_scrollRect.transform.parent.gameObject);

        Destroy(gameObject);
    }
}
