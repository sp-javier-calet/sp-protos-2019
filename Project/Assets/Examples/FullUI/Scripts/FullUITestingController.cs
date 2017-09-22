using UnityEngine;
using SocialPoint.Dependency;
using System;

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

    ScreensController _screens;
    string _latestCheckPoint;

    void Start()
    {
        _screens = Services.Instance.Resolve<ScreensController>();
        if(_screens == null)
        {
            throw new InvalidOperationException("Could not find UI Views Stack Controller");
        }
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Alpha1))
        {
            _screens.PushImmediate(instantiatePrefab(_popup));
        }
        else if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Alpha1))
        {
            _screens.Push(instantiatePrefab(_popup), true);
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha1))
        {
            _screens.PushImmediate(instantiatePrefab(_popup));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha1))
        {
            _screens.Push(instantiatePrefab(_popup), true);
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Alpha2))
        {
            _screens.PushImmediate(instantiatePrefab(_screen));
        }
        else if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Alpha2))
        {
            _screens.Push(instantiatePrefab(_screen));
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha2))
        {
            _screens.PushImmediate(instantiatePrefab(_screen));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha2))
        {
            _screens.Push(instantiatePrefab(_screen), true);
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha3))
        {
            _screens.PopImmediate();
        }
        else if(Input.GetKeyUp(KeyCode.Alpha3))
        {
            _screens.Pop();
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Alpha4))
        {
            _screens.ReplaceImmediate(instantiatePrefab(_popupReplace));
        }
        else if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Alpha4))
        {
            _screens.Replace(instantiatePrefab(_popupReplace));
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha4))
        {
            _screens.ReplaceImmediate(instantiatePrefab(_popupReplace));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha4))
        {
            _screens.Replace(instantiatePrefab(_popupReplace), true);
        }
        else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha5))
        {
            _screens.ReplaceImmediate(instantiatePrefab(_screenReplace));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha5))
        {
            _screens.Replace(instantiatePrefab(_screenReplace));
        }
        else if(Input.GetKeyUp(KeyCode.Alpha6))
        {
            var top = _screens.Top;
            if(_screens.IsValidStackNode(top))
            {
                _latestCheckPoint = top.GameObject.name;
                _screens.SetCheckPoint(_latestCheckPoint);
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha7))
        {
            if(!string.IsNullOrEmpty(_latestCheckPoint))
            {
                _screens.PopUntilCheckPoint(_latestCheckPoint);
            }
        }
        else if(Input.GetKeyUp(KeyCode.Alpha8))
        {
            _screens.PopUntil(0);
        }
        else if(Input.GetKeyUp(KeyCode.Alpha9))
        {
            _screens.PopUntil(-1);
        }
    }

    GameObject instantiatePrefab(GameObject basePrefab)
    {
        return GameObject.Instantiate(basePrefab);
    }
}
