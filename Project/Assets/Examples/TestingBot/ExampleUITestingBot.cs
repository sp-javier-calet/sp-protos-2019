using System;
using UnityEngine;
using SocialPoint.TestingBot;
using Random = UnityEngine.Random;

public class ExampleUITestingBot : BaseTestingBot
{
    const float _waitTime = 1.5f;

    float _remainingWaitTime;

    [SerializeField] GameObject _testingBotView;

    protected override void OnAwake()
    {
        InputModuleChanged += OnInputModuleChanged;
        OnInputModuleChanged(_inputModule);
    }

    TestingBotView _view;

    void OnInputModuleChanged(TestableActionStandaloneInputModule inputModule)
    {
        if(_view != null)
        {
            Destroy(_view);
        }

        if(_testingBotView == null)
        {
            throw new ArgumentNullException("_testingBotView");
        }

        _view = Instantiate(_testingBotView).GetComponent<TestingBotView>();

        if(_view == null)
        {
            throw new MissingComponentException("TestingBotView");
        }

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
