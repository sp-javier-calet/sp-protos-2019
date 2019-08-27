
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class CP_GameManager : NetworkBehaviour
{
    public static CP_GameManager Instance = null;

    public enum GameState
    {
        E_NONE,
        E_TITLE,
        E_PLAYING_1_PLAYER,
        E_PLAYING_4_VERSUS
    }

    CP_NetworkController _networkController;

    [SyncVar(hook = "NumPlayers")]
    int _numPlayers = 0;

    public int NumPlayers { get { return _numPlayers; } }

    public void SetNumPlayers(int numPlayers)
    {
        Debug.Log("SetNumPlayers" + numPlayers);

        _numPlayers = numPlayers;
    }

    public CP_NetworkController NetworkController
    {
        get
        {
            if(_networkController == null)
            {
                _networkController = FindObjectOfType<CP_NetworkController>();
            }

            return _networkController;
        }
    }

    public GameState CurrentGameState = GameState.E_NONE;
    public GameObject PlayerGO = null;

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

    void Update()
    {
        Debug.Log("NumPlayers: " + _numPlayers);
    }
}
