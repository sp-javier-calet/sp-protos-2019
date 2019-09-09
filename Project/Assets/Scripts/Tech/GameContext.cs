
using UnityEngine;

public class GameContext
{
	GameObject _gameContextGO;

	// Main singleton
	public static GameContext _instance = null;

	public static void CreateInstance ()
	{
		if (_instance == null)
		{
			_instance = new GameContext ();

			GameObject gameContext = GameObject.Find ("GameContext");
			if (gameContext == null)
			{
				gameContext = new GameObject ("GameContext");
				_instance._gameContextGO = gameContext;

#if UNITY_EDITOR
				if (Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad (gameContext);
				}
#else
				UnityEngine.Object.DontDestroyOnLoad (gameContext);
#endif
			}
		}
	}

	public static GameContext SharedInstance
	{
		get
		{
			if (_instance == null)
			{
				CreateInstance ();
			}

			return _instance;
		}
	}

	public static void DestroyInstance ()
	{
		if (_instance != null)
		{
			if (_instance._gameContextGO != null)
			{
				GameObject.DestroyImmediate (_instance._gameContextGO);
				_instance._gameContextGO = null;
			}

			_instance = null;
		}
	}
	
	public static T AddMainComponent <T> () where T : UnityEngine.Component
	{
		T t = SharedInstance._gameContextGO.GetComponent<T> ();

		if (t != null)
		{
			return t;
		}

		return SharedInstance._gameContextGO.AddComponent <T> ();
	}

	public static void Clean ()
	{
		if (_instance != null)
		{
			_instance._gameContextGO = null;
		
			_instance = null;
		}
	}
}
