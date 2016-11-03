using UnityEngine;
using System.Collections;
using SocialPoint.TestingBot;
using SocialPoint.EventSystems;
using SocialPoint.Base;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using SocialPoint.Utils;

public class ExampleUITestingBot : BaseTestingBot
{
    const float _waitTime = 1.5f;

    float _remainingWaitTime = 0f;

    const string _testingBotView = "TestingBotView";

    protected override void OnAwake()
    {
        InputModuleChanged += OnImputModuleChanged;
        OnImputModuleChanged(_inputModule);
    }

    TestingBotView _view;

    void OnImputModuleChanged(TestableActionStandaloneInputModule inputModule)
    {
        if(_view != null)
        {
            GameObject.Destroy(_view);
        }
        var testingBotViewPrefab = Resources.Load<GameObject>(_testingBotView);
        _view = GameObject.Instantiate<GameObject>(testingBotViewPrefab).GetComponent<TestingBotView>();
        _view.InputModule = inputModule;
    }

    protected override void OnUpdate()
    {
        _remainingWaitTime -= Time.unscaledDeltaTime;
        if(_remainingWaitTime <= 0f)
        {
            ApplyNextAction();
        }
    }

    void ApplyNextAction()
    {
        var clickableElements = GetUIClickableElements();
        if(clickableElements.Count > 0)
        {
            var randomGO = clickableElements[Random.Range(0, clickableElements.Count)];
            ClickOnUIGameObject(randomGO);
        }
        _remainingWaitTime = _waitTime;
    }
}
