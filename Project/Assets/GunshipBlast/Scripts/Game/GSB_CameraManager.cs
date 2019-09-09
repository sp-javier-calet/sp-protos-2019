
using UnityEngine;

public class GSB_CameraManager : CameraManager
{

#region SINGLETON

    public static GSB_CameraManager SharedInstance
    {
        get
        {
            if(_instance == null)
            {
                _instance = GameContext.AddMainComponent<GSB_CameraManager>();
            }

            return (GSB_CameraManager) _instance;
        }
    }

#endregion SINGLETON

#region PUBLIC METHODS

    public override void ChangedResolution(Resolution adaptedResolution)
    {
        float newOrthographicSize = 5.0f;

        float aspectRatio = adaptedResolution.width / (float)adaptedResolution.height;

        Debug.Log("ChangedResolution: " + adaptedResolution.width + " " + adaptedResolution.height + " " + aspectRatio);

        // 3:4 iPads
        if (aspectRatio >= 0.7f && aspectRatio < 0.85f)
        {
            newOrthographicSize = 4.6f;
        }
        // 2:1 IphoneX
        else if (aspectRatio >= 0.4f && aspectRatio < 0.54f)
        {
            newOrthographicSize = 5.75f;
        }

        if (_gameCamera != null)
        {
            _gameCamera.orthographicSize = newOrthographicSize;
        }
        if (_uiCamera != null)
        {
            _uiCamera.orthographicSize = newOrthographicSize;
        }
    }

#endregion PUBLIC METHODS

}
