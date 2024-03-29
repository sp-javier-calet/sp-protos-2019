﻿//-----------------------------------------------------------------------
// SelectorTutorialsController.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using SocialPoint.Alert;
using UnityEngine;
using UnityEngine.UI;
using SocialPoint.Dependency;
using SocialPoint.Tutorial;

public class SelectorTutorialsController : MonoBehaviour
{
    [SerializeField] Button _prefabButton;

    [SerializeField] ScrollRect _scrollRect;

    public Action<string> OnGoToTutorial { get; set; }

    ITutorialManager _tutorialManager;
    IAlertView _alertPrototype;

    void Start()
    {
        _alertPrototype = Services.Instance.Resolve<IAlertView>();
        if(_alertPrototype == null)
        {
            throw new InvalidOperationException("Could not find alert view");
        }

        _tutorialManager = Services.Instance.Resolve<ITutorialManager>();
        if(_tutorialManager == null)
        {
            throw new InvalidOperationException("Could not find tutorial manager for tutorials selector");
        }

        ShowTutorialsUI();
    }

    void ShowTutorialsUI()
    {
        foreach(string t in _tutorialManager.Tutorials)
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

    void OnStartTutorial(string tutorialName)
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
            alertView.Buttons = new[] { "Ok" };
            alertView.Show(result => { });
        }
    }
}
