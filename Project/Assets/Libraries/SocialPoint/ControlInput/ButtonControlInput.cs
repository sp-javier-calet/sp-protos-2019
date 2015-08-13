using System;
using System.Collections.Generic;
using UnityEngine;

public class ButtonControlInput
{
    //name of the virtual axe defined on Input Manager at the moment has no use
    private String _axeName;
    private List<IControlInput> _aditionalInputChanels;

    //events will be called on the ControlInputManager
    public event Action EventJustPressed = delegate {
		};
        
    public event Action EventJustReleased = delegate {
		};

    /// <summary>
    /// Do not create them directly, use ControlInputManager.createButtonControlInput it ensures the input has ben defined on unity
    /// Input Editor and there is only one <see cref="ButtonControlInput"/> with this axeName created.
    /// </summary>
    /// <param name="axeName">Axe name.</param>
    public ButtonControlInput(String axeName)
    {
        _axeName = axeName;
        _aditionalInputChanels = new List<IControlInput>();
    }

    public bool Pressed
    {
        get { return Input.GetButton(_axeName) || CFInput.GetButton(_axeName) || _aditionalInputChanels.Exists(c => c.Pressed); }
    }

    public bool JustPressed
    {
        get { return CFInput.GetButtonDown(_axeName) || Input.GetButtonDown(_axeName) || _aditionalInputChanels.Exists(c => c.JustPressed); }
    }

    public bool JustReleased
    {
        get { return CFInput.GetButtonUp(_axeName) || Input.GetButtonUp(_axeName) || _aditionalInputChanels.Exists(c => c.JustReleased); }
    }

    /// <summary>
    /// This Update function should be called only from ControlInputManager
    /// </summary>
    public void Update()
    {
        if(JustPressed)
            EventJustPressed();
        if(JustReleased)
            EventJustReleased();
    }

    public void AddAditionalInputChannel(IControlInput inputChannel)
    {
        _aditionalInputChanels.Add(inputChannel);
    }

    public void RemoveAditionalInputChannel(IControlInput inputChannel)
    {
        if(_aditionalInputChanels.Contains(inputChannel))
            _aditionalInputChanels.Remove(inputChannel);
    }
}