using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public List<GameObject> Cameras;
    public GameObject InitialCameraObject;

    GameObject _currentCameraObject;
    GameObject _nextCameraObject;

    GameObject _transitionCameraObject;
    IEnumerator _transitionCoroutine;
    Action _onTransitionEnded;

    void Awake()
    {
        Cameras = new List<GameObject>();
    }

    void OnEnable()
    {
        // get all first depth childs attached to the gameObject
        foreach(Transform trans in GetComponent<Transform>())
        {
            var go = trans.gameObject;
            var cam = go.GetComponentsInChildren<Camera>(true)[0];
            if(cam != null)
            {
                Cameras.Add(go);
            }
        }

        foreach(var camObject in Cameras)
        {
            if(InitialCameraObject != null && InitialCameraObject == camObject)
            {
                camObject.SetActive(true);
                _currentCameraObject = camObject;
            }
            else
            {
                camObject.SetActive(false);
            }
        }
    }

    public void SkipTransition()
    {
        OnEndTransition();
    }

    public void SwitchToCamera(string targetCameraName)
    {
        CreateTransitionToCamera(targetCameraName, 0.0f);
    }

    public void SwitchToCamera(string targetCameraName, Action callback)
    {
        CreateTransitionToCamera(targetCameraName, 0.0f, null, callback);
    }

    public void CreateTransitionToCamera(string targetCameraName, float time)
    {
        AnimationCurve curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        CreateTransitionToCamera(targetCameraName, time, curve);
    }

    public void CreateTransitionToCamera(string targetCameraName, float time, AnimationCurve curve)
    {
        Action callback = null;
        CreateTransitionToCamera(targetCameraName, time, curve, callback);
    }

    public void CreateTransitionToCamera(string targetCameraName, float time, AnimationCurve curve, Action callback)
    {
        var targetCamera = Cameras.Find(x => string.Equals(x.name, targetCameraName));
        CreateTransitionToCamera(targetCamera, time, curve, callback);
    }

    void CreateTransitionToCamera(GameObject targetCamera, float time, AnimationCurve curve, Action callback)
    {
        if(targetCamera != null)
        {
            if(targetCamera != _currentCameraObject)
            {
                CreateTransition(targetCamera, time, curve, callback);
            }
            else
            {
                Debug.LogWarning("CreateTransitionToCamera: Camera '" + targetCamera.name + "' already active");
            }
        }
        else
        {
            Debug.LogWarning("CreateTransitionToCamera: Camera '" + targetCamera.name + "' does not exists");
        }
    }

    void CreateTransition(GameObject targetCamera, float time, AnimationCurve curve, Action callback)
    {
        _currentCameraObject.SetActive(false);
        _nextCameraObject = targetCamera;
        _onTransitionEnded = callback;

        if(_transitionCoroutine == null && time > 0.0f)
        {
            var fromCamera = _currentCameraObject.GetComponentsInChildren<Camera>(true)[0];
            var toCamera = _nextCameraObject.GetComponentsInChildren<Camera>(true)[0];

            DestroyImmediate(_transitionCameraObject);
            _transitionCameraObject = new GameObject("TransitionCameraTemp");
            _transitionCameraObject.AddComponent<Camera>().CopyFrom(fromCamera);

            _transitionCoroutine = TransitionCoroutine(fromCamera, toCamera, time, curve);
            StartCoroutine(_transitionCoroutine);
        }
        else
        {
            OnEndTransition();
        }
    }

    IEnumerator TransitionCoroutine(Camera fromCamera, Camera toCamera, float transitionTime, AnimationCurve curve)
    {
        if(fromCamera == null)
        {
            throw new ArgumentNullException("fromCamera", "fromCamera cannot be null or empty!");
        }

        if(toCamera == null)
        {
            throw new ArgumentNullException("toCamera", "toCamera cannot be null or empty!");
        }

        if(curve == null)
        {
            throw new ArgumentNullException("curve", "curve cannot be null or empty!");
        }

        if(_transitionCameraObject == null)
        {
            throw new ArgumentNullException("_transitionCameraObject", "_transitionCameraObject cannot be null or empty!");
        }

        var cam = _transitionCameraObject.GetComponent<Camera>();
        if(cam == null)
        {
            throw new ArgumentNullException("cam", "cam cannot be null or empty!");
        }
            
        float elapsedTime = 0.0f;
        cam.cullingMask = toCamera.cullingMask;

        while(elapsedTime <= transitionTime)
        {
            elapsedTime += Time.deltaTime;
            float evalValue = elapsedTime / transitionTime;
            evalValue = Mathf.Min(evalValue, 1.0f);
            float val = curve.Evaluate(evalValue);

            cam.transform.position = Vector3.Lerp(fromCamera.transform.position, toCamera.transform.position, val);
            cam.transform.rotation = Quaternion.Lerp(fromCamera.transform.rotation, toCamera.transform.rotation, val);
            cam.transform.localScale = Vector3.Lerp(fromCamera.transform.localScale, toCamera.transform.localScale, val);

            cam.fieldOfView = Mathf.Lerp(fromCamera.fieldOfView, toCamera.fieldOfView, val);
            cam.orthographicSize = Mathf.Lerp(fromCamera.orthographicSize, toCamera.orthographicSize, val);
            cam.nearClipPlane = Mathf.Lerp(fromCamera.nearClipPlane, toCamera.nearClipPlane, val);
            cam.farClipPlane = Mathf.Lerp(fromCamera.farClipPlane, toCamera.farClipPlane, val);

            yield return null;
        }

        OnEndTransition();
    }

    void OnEndTransition()
    {
        if(_transitionCameraObject != null)
        {
            DestroyImmediate(_transitionCameraObject);
            _transitionCameraObject = null;
        }

        if(_nextCameraObject != null)
        {
            _currentCameraObject = _nextCameraObject;
            _nextCameraObject = null;
            _currentCameraObject.SetActive(true);
        }

        if(_onTransitionEnded != null)
        {
            _onTransitionEnded();
        }

        if(_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }
    }
}

