using UnityEngine;
using System.Collections;
using Zenject;

public class LoadingSceneLoader : MonoBehaviour
{
    [SerializeField]
    string _onLoginSuccessScene;

    [Inject]
    ScreensController _screens;

    void Start()
    {
        CreateLoadingScreen();
    }

    void CreateLoadingScreen()
    {
        GameLoadingController gameLoading = _screens.CreateChild<GameLoadingController>();
        gameLoading.SceneToLoad = _onLoginSuccessScene;
        gameLoading.SetParent(_screens.transform);
        gameLoading.Show();
    }
}
