
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
        E_TUTORIAL,
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
    public GameObject EnemyGO = null;

    void Awake()
    {
        Application.targetFrameRate = 60;
        Time.fixedDeltaTime =  Time.timeScale * 0.02f;

        Instance = this;
        DontDestroyOnLoad(this);

        GameAudioManager.SharedInstance.Initialise();
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Music/01_GSB_Title");
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Music/02_GSB_Battle");
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Sounds/GSB_ammoreward");
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Sounds/GSB_canon");
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Sounds/GSB_damage");
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Sounds/GSB_explosion");
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Sounds/GSB_selectship");
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Sounds/GSB_wavestart");
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Sounds/GSB_closeshape");
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Sounds/GSB_cancel");
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Sounds/GSB_ammorecover");
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Sounds/GSB_ammofull");
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Sounds/GSB_youwin");
        GameAudioManager.SharedInstance.AddAudioClip("Audio/Sounds/GSB_gameover");

        GSB_CameraManager.SharedInstance.Initialise();

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

            case GameState.E_TUTORIAL:
            {
                StartCoroutine(LoadAsyncScene("GSB_TutorialScene"));

                break;
            }

            case GameState.E_PLAYING_1_PLAYER:
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

                StartCoroutine(LoadAsyncScene("GSB_BattleScene"));

                break;
            }

            case GameState.E_PLAYING_2_VERSUS:
            {
                GameAudioManager.SharedInstance.StopSound(0);
                GameAudioManager.SharedInstance.PlaySound("Audio/Music/02_GSB_Battle", true);

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
