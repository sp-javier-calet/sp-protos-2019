//-----------------------------------------------------------------------
// FullUITestingController.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

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

    UIStackController _stackController;
    string _latestCheckPoint;

    void Start()
    {
        _stackController = Services.Instance.Resolve<UIStackController>();
        if(_stackController == null)
        {
            throw new InvalidOperationException("Could not find UI Stack Controller");
        }
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Alpha1))
        {
            _stackController.PushImmediate(instantiatePrefab(_popup), false);
        }
        else if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Alpha1))
        {
            _stackController.Push(instantiatePrefab(_popup), false);
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha1))
        {
            _stackController.PushImmediate(instantiatePrefab(_popup));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha1))
        {
            _stackController.Push(instantiatePrefab(_popup));
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Alpha2))
        {
            _stackController.PushImmediate(instantiatePrefab(_screen), false);
        }
        else if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Alpha2))
        {
            _stackController.Push(instantiatePrefab(_screen), false);
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha2))
        {
            _stackController.PushImmediate(instantiatePrefab(_screen));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha2))
        {
            _stackController.Push(instantiatePrefab(_screen));
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha3))
        {
            _stackController.PopImmediate();
        }
        else if(Input.GetKeyUp(KeyCode.Alpha3))
        {
            _stackController.Pop();
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Alpha4))
        {
            _stackController.ReplaceImmediate(instantiatePrefab(_popupReplace), false);
        }
        else if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Alpha4))
        {
            _stackController.Replace(instantiatePrefab(_popupReplace), false);
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha4))
        {
            _stackController.ReplaceImmediate(instantiatePrefab(_popupReplace));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha4))
        {
            _stackController.Replace(instantiatePrefab(_popupReplace));
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha5))
        {
            _stackController.ReplaceImmediate(instantiatePrefab(_screenReplace));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha5))
        {
            _stackController.Replace(instantiatePrefab(_screenReplace));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha6))
        {
            var top = _stackController.Top;
            if(StackNode.IsValid(top))
            {
                _latestCheckPoint = top.GameObject.name;
                _stackController.SetCheckPoint(_latestCheckPoint);
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha7))
        {
            if(!string.IsNullOrEmpty(_latestCheckPoint))
            {
                _stackController.PopUntilCheckPoint(_latestCheckPoint);
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha8))
        {
            _stackController.PopUntil(0);
        }
        else if(Input.GetKeyUp(KeyCode.Alpha9))
        {
            _stackController.PopUntil(-1);
        }
        else if(Input.GetKeyUp(KeyCode.Space))
        {
            Services.Instance.Resolve<HUDNotificationsController>().ShowNotification("Hello!!! This is a very long text, to test a HUDNotification...");
        }
    }

    GameObject instantiatePrefab(GameObject basePrefab)
    {
        return GameObject.Instantiate(basePrefab);
    }
}
