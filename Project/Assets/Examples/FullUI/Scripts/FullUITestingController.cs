using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Dependency;
using System;
using SocialPoint.GUIControl;

public class FullUITestingController : MonoBehaviour
{
    [SerializeField]
    GameObject _popup;

    [SerializeField]
    GameObject _popupReplace;

    [SerializeField]
    GameObject _screen;

    [SerializeField]
    GameObject _screenReplace;

    PopupsController _popupsController;
    ScreensController _screensController;
    string _latestCheckPoint;

    void Start()
    {
        _popupsController = Services.Instance.Resolve<PopupsController>();
        if(_popupsController == null)
        {
            throw new InvalidOperationException("Could not find Popups UI Controller");
        }

        _screensController = Services.Instance.Resolve<ScreensController>();
        if(_screensController == null)
        {
            throw new InvalidOperationException("Could not find Screens UI Controller");
        }
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Alpha1))
        {
            _popupsController.Push(instantiatePrefab(_popup));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha2))
        {
            _screensController.Push(instantiatePrefab(_screen));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha3))
        {
            if(_screensController.Count > 1)
            {
                _screensController.Pop();
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha4))
        {
            if(_popupsController.Count > 1)
            {
                _popupsController.Replace(instantiatePrefab(_popupReplace));
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha5))
        {
            if(_screensController.Count > 1)
            {
                _screensController.Replace(instantiatePrefab(_screenReplace));
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha6))
        {
            var top = _popupsController.Top;
            if(top != null)
            {
                _latestCheckPoint = top.name;
                _popupsController.SetCheckPoint(_latestCheckPoint);
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha7))
        {
            if(!string.IsNullOrEmpty(_latestCheckPoint))
            {
                _popupsController.PopUntilCheckPoint(_latestCheckPoint);
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha8))
        {
            _popupsController.PopUntil(1);
        }
    }

    GameObject instantiatePrefab(GameObject basePrefab)
    {
        return GameObject.Instantiate(basePrefab);
    }
}
