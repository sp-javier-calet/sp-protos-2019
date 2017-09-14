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

    UIViewsStackController _uiViewsStackController;
    string _latestCheckPoint;

    void Start()
    {
        _uiViewsStackController = Services.Instance.Resolve<UIViewsStackController>();
        if(_uiViewsStackController == null)
        {
            throw new InvalidOperationException("Could not find UI Controller");
        }
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Alpha1))
        {
            _uiViewsStackController.Push(instantiatePrefab(_popup));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha2))
        {
            _uiViewsStackController.Push(instantiatePrefab(_screen));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha3))
        {
            if(_uiViewsStackController.Count > 1)
            {
                _uiViewsStackController.Pop();
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha4))
        {
            if(_uiViewsStackController.Count > 1)
            {
                _uiViewsStackController.Replace(instantiatePrefab(_popupReplace));
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha5))
        {
            if(_uiViewsStackController.Count > 1)
            {
                _uiViewsStackController.Replace(instantiatePrefab(_screenReplace));
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha6))
        {
            var top = _uiViewsStackController.Top;
            if(top != null)
            {
                _latestCheckPoint = top.name;
                _uiViewsStackController.SetCheckPoint(_latestCheckPoint);
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha7))
        {
            if(!string.IsNullOrEmpty(_latestCheckPoint))
            {
                _uiViewsStackController.PopUntilCheckPoint(_latestCheckPoint);
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha8))
        {
            _uiViewsStackController.PopUntil(1);
        }
    }

    GameObject instantiatePrefab(GameObject basePrefab)
    {
        return GameObject.Instantiate(basePrefab);
    }
}
