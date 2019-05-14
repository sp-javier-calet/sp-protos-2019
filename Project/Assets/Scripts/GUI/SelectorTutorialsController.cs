//-----------------------------------------------------------------------
// SelectorTutorialsController.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SocialPoint.Alert;
using UnityEngine;
using UnityEngine.UI;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.Tutorial;

public class SelectorTutorialsController : UIViewController
{
    [SerializeField] Button _prefabButton;

    [SerializeField] ScrollRect _scrollRect;

    public Action<string> OnGoToTutorial { get; set; }

    TutorialManager _tutorialManager;
    IAlertView _alertPrototype;
    List<string> _tutorials;

    public SelectorTutorialsController()
    {
        IsFullScreen = true;
    }

    protected override void OnStart()
    {
        base.OnStart();

        _alertPrototype = Services.Instance.Resolve<IAlertView>();
        if(_alertPrototype == null)
        {
            throw new InvalidOperationException("Could not find alert view");
        }

        _tutorialManager = Services.Instance.Resolve<TutorialManager>();
        if(_tutorialManager == null)
        {
            throw new InvalidOperationException("Could not find tutorial manager for tutorials selector");
        }

        _tutorials = _tutorialManager.Tutorials;

        ShowTutorialsUI();
    }

    void ShowTutorialsUI()
    {
        foreach(string t in _tutorials)
        {
            InstantiateScenesButton(t);
        }
    }

    void InstantiateScenesButton(string tutorialName)
    {
        Button button = Instantiate(_prefabButton, _scrollRect.content, true);
        var transformAccess = button.transform;
        transformAccess.localPosition = Vector3.zero;
        transformAccess.localScale = Vector3.one;
        button.GetComponentInChildren<Text>().text = tutorialName.ToUpper();
        button.onClick.AddListener(() => OnStartTutorial(tutorialName));
    }

    public void OnStartTutorial(string tutorialName)
    {
        if(_tutorialManager.HasActiveTutorial == false)
        {
            _tutorialManager.SetActiveTutorial(tutorialName);
        }
        else
        {
            var alertView = (IAlertView)_alertPrototype.Clone();
            alertView.Title = "Start Tutorial Error";
            alertView.Message =
                $"Could not start {tutorialName}. A tutorial is already running: {_tutorialManager.ActiveTutorialName}";

            alertView.Input = false;
            alertView.Buttons = new[] {"Ok"};
            alertView.Show(result => { });
        }
    }
}
