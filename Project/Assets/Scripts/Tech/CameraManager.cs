
using UnityEngine;

public class CameraManager : MonoBehaviour
{

#region SINGLETON

    static protected CameraManager _instance = null;
    
    public static CameraManager Instance
    {
        get
        {
            return _instance;
        }
    }

#endregion SINGLETON
    
#region PRIVATE ATTRIBUTES

    protected Camera _gameCamera;
    protected Camera _uiCamera;
    protected Resolution _lastScreenResolution;
    
#endregion PRIVATE ATTRIBUTES
    
#region PUBLIC PROPERTIES

    public Camera GameCamera
    {
        get { return _gameCamera; }
    }
    
    public Camera UICamera
    {
        get { return _uiCamera; }
    }

#endregion PUBLIC PROPERTIES
    
#region PUBLIC METHODS

    public void Initialise()
    {
        Transform cameraMainTR = GameObject.Find("GameLoop/Cameras/Main").transform;
        if(cameraMainTR != null)
        {
            _gameCamera = cameraMainTR.GetComponent<Camera>();
        }
        
        Transform cameraUITR = GameObject.Find("GameLoop/Cameras/UI").transform;
        if(cameraUITR != null)
        {
            _uiCamera = cameraUITR.GetComponent<Camera>();
        }
    }

    public virtual void ChangedResolution(Resolution adaptedResolution) {}

    void Update()
    {
        if (_lastScreenResolution.width != Screen.currentResolution.width || 
            _lastScreenResolution.height != Screen.currentResolution.height)
        {
            ChangedResolution(Screen.currentResolution);

            _lastScreenResolution = Screen.currentResolution;
        }
    }

#endregion PUBLIC METHODS
    
}
