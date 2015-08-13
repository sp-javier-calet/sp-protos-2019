using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlInputManager :MonoBehaviour
{
    private Dictionary<string,ButtonControlInput> _buttonDictionary;
    private Dictionary<string,JoystickControlInput> _joystickDictionary;
    private bool _gamePadsConnected = false;

    void Awake()
    {
        StartCoroutine(CheckForControllers());
        _buttonDictionary = new Dictionary<string, ButtonControlInput>();
        _joystickDictionary = new Dictionary<string, JoystickControlInput>();
    }

    public void Start()
    {
    }

    void Update()
    {
        //checks all buttons so they can launch their events
        foreach(ButtonControlInput button in _buttonDictionary.Values)
        {
            button.Update();
        }
    }

    IEnumerator CheckForControllers()
    {
        while(true)
        {
            var controllers = Input.GetJoystickNames();
            if(!_gamePadsConnected && controllers.Length > 0)
            {
                _gamePadsConnected = true;
            }
            else
            if(_gamePadsConnected && controllers.Length == 0)
            {
                _gamePadsConnected = false;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public ButtonControlInput CreateButtonControlInput(String axeName)
    {
        if(_buttonDictionary.ContainsKey(axeName))
        {
            SocialPoint.Base.Debug.Log("button already created");
            return _buttonDictionary[axeName];
        }
			
        try
        {
            SocialPoint.Base.Debug.Log(Input.GetButton(axeName));
        }
        catch(Exception ex)
        {
            SocialPoint.Base.Debug.Log("axe is not defined: " + ex.Message);
            return null;
        }
			
        var button = new ButtonControlInput(axeName);
        _buttonDictionary.Add(axeName, button);
        return button;
    }

    public JoystickControlInput CreateJoysticControlInput(String joystickName, String axisXName, String axisYName)
    {
        if(_joystickDictionary.ContainsKey(joystickName))
        {
            SocialPoint.Base.Debug.Log("joystick already created");
            return _joystickDictionary[joystickName];
        }
        try
        {
            Input.GetAxis(axisXName);
            Input.GetAxis(axisYName);
        }
        catch(Exception ex)
        {
            SocialPoint.Base.Debug.Log("axe is not defined: " + ex.Message);
            return null;
        }
        var joystick = new JoystickControlInput(axisXName, axisYName);
        _joystickDictionary.Add(joystickName, joystick);
        return joystick;
    }

    #region getter/setters

    public JoystickControlInput GetJoystick(String name)
    {
        return _joystickDictionary[name];
    }

    public ButtonControlInput GetButton(String name)
    {
        return _buttonDictionary[name];
    }

    public  bool GamePadConnected
    {
        get{ return _gamePadsConnected; }
    }

    #endregion
}

