
using SocialPoint.GUI;
using Zenject;
using UnityEngine;
using System;
using System.Collections.Generic;

public class AdminPanelAction
{
    string _name;
    Action _callback;

    public string Name
    {
        get
        {
            return _name;
        }
    }

    public AdminPanelAction(string name, Action cbk)
    {
        _name = name;
        _callback = cbk;
    }

    public void Activate()
    {
        if(_callback != null)
        {
            _callback();
        }
    }
}

public class AdminPanelSection
{
    string _name;
    List<AdminPanelAction> _actions = new List<AdminPanelAction>();

    public string Name
    {
        get
        {
            return _name;
        }
    }

    public AdminPanelSection(string name)
    {
        _name = name;
    }

    public void AddAction(AdminPanelAction action)
    {
        _actions.Add(action);
    }
}

public class AdminPanelController : UIViewController
{
    List<AdminPanelSection> _sections = new List<AdminPanelSection>();

    public void OnCloseButtonClicked()
    {
        Hide();
    }

    public void AddSection(AdminPanelSection section)
    {
        _sections.Add(section);
    }

    override protected void OnLoad()
    {
        base.OnLoad();
    }
}