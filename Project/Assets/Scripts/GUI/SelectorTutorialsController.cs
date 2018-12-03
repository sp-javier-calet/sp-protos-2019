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
    [SerializeField]
    private Button _prefabButton;

    [SerializeField]
    private ScrollRect _scrollRect;

    public Action<string> OnGoToTutorial { get; set; }

    private TutorialManager _tutorialManager;
    private IAlertView _alertPrototype;
    private List<string> _tutorials;

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
        for(int i = 0; i < _tutorials.Count; i++)
        {
            InstantiateScenesButton(_tutorials[i]);
        }
    }

    void InstantiateScenesButton(string tutorialName)
    {
        Button button = Instantiate<Button>(_prefabButton);
        button.transform.SetParent(_scrollRect.content);
        button.transform.localPosition = Vector3.zero;
        button.transform.localScale = Vector3.one;
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
            alertView.Message = string.Format("Could not start {0}. A tutorial is already running: {1}",
                tutorialName, _tutorialManager.ActiveTutorialTame);
            alertView.Input = false;
            alertView.Buttons = new []{ "Ok" };
            alertView.Show(result => { });
        }
    }
}
