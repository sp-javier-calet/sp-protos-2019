
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GSB_GameManager : MonoBehaviour
{
    public static GSB_GameManager Instance = null;

    public enum GameState
    {
        E_NONE,
        E_TITLE,
        E_PLAYING_1_PLAYER,
        E_PLAYING_2_VERSUS
    }

    GSB_NetworkController _networkController;
    public GSB_NetworkController NetworkController
    {
        get
        {
            if(_networkController == null)
            {
                _networkController = FindObjectOfType<GSB_NetworkController>();
            }

            return _networkController;
        }
    }

    public GSB_GameState NetworkGameState;

    public GameState CurrentGameState = GameState.E_NONE;
    public GameObject PlayerOfflineGO = null;
    public GameObject PlayerOnlineGO = null;

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
                if(CurrentGameState == GameState.E_PLAYING_1_PLAYER || CurrentGameState == GameState.E_PLAYING_2_VERSUS)
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                }

                StartCoroutine(LoadAsyncScene("GSB_TitleScene"));

                break;
            }

            case GameState.E_PLAYING_1_PLAYER:
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

                StartCoroutine(LoadAsyncScene("GSB_BattleScene"));

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
