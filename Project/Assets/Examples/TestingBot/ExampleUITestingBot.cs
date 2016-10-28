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

    LayerMask _guiMask = new LayerMask();

    const string _testingBotView = "TestingBotView";

    protected override void OnAwake()
    {
        string[] layerNames = {"UI",
            "BillboardsUI",
            "GUILevel1",
            "GUILevel2",
            "GUILevel3",
            "GUILevel4",
            "GUILevel5",
            "GUILevel6",
            "GUILevel7",
            "GUILevel8"
        };
        for(int i = 0; i < layerNames.Length; ++i)
        {
            _guiMask.value |= 1 << LayerMask.NameToLayer(layerNames[i]);
        }
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

    List<GameObject> GetClickableElements()
    {
        List<GameObject> clickableElements = new List<GameObject>();
        MonoBehaviour[] monoBehaviours = GameObject.FindObjectsOfType<MonoBehaviour>();
        for(int i = 0; i < monoBehaviours.Length; ++i)
        {
            var monoBehaviour = monoBehaviours[i];
            if(monoBehaviour is IPointerClickHandler)
            {
                if((_guiMask.value & 1 << monoBehaviour.gameObject.layer) != 0)
                {
                    clickableElements.Add(monoBehaviour.gameObject);
                }
            }
        }

        return clickableElements;
    }

    Vector3 GetUIGameObjectPosition(GameObject go)
    {
        var rectTransform = go.transform as RectTransform;
        if(rectTransform != null)
        {
            return go.transform.TransformPoint(rectTransform.rect.center);
        }
        else
        {
            return go.transform.position;
        }
    }

    void ClickOnUIGameObject(GameObject go)
    {
        Transform t = go.transform;
        while(t != null)
        {
            var canvas = t.gameObject.GetComponent<Canvas>();
            if(canvas != null)
            {
                var position = GetUIGameObjectPosition(go);
                var screenPosition = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, position);
                _inputModule.SimulateClick(screenPosition);
                break;
            }
            t = t.parent;
        }
    }

    void ApplyNextAction()
    {
        var clickableElements = GetClickableElements();
        if(clickableElements.Count > 0)
        {
            var randomGO = clickableElements[Random.Range(0, clickableElements.Count)];
            ClickOnUIGameObject(randomGO);
        }
        _remainingWaitTime = _waitTime;
    }
}
