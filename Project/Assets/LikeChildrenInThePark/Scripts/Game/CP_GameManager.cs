
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CP_GameManager : MonoBehaviour
{
    public static CP_GameManager Instance = null;

    public enum GameState
    {
        E_NONE,
        E_TITLE,
        E_PLAYING_1_PLAYER,
        E_PLAYING_4_VERSUS
    }

    public GameState CurrentGameState = GameState.E_NONE;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        SetGameState(GameState.E_TITLE);
    }

    public void SetGameState(GameState gameState)
    {
        switch(gameState)
        {
            case GameState.E_TITLE:
            {
                if(CurrentGameState == GameState.E_PLAYING_1_PLAYER || CurrentGameState == GameState.E_PLAYING_4_VERSUS)
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                }

                StartCoroutine(LoadAsyncScene("TitleScene"));

                break;
            }

            case GameState.E_PLAYING_1_PLAYER:
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

                StartCoroutine(LoadAsyncScene("GameScene"));

                break;
            }
        }

        CurrentGameState = gameState;
    }

    IEnumerator LoadAsyncScene(string name)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
