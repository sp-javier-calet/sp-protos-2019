using UnityEngine;
using System.Collections;
using SocialPoint.EventSystems;

public class TestingBotView : MonoBehaviour
{
    [SerializeField]
    GameObject _testingBotClickViewPrefab;

    [SerializeField]
    Canvas _canvas;

    TestableActionStandaloneInputModule _inputModule;

    public TestableActionStandaloneInputModule InputModule
    {
        get
        {
            return _inputModule;
        }
        set
        {
            if(_inputModule != value)
            {
                if(_inputModule != null)
                {
                    _inputModule.MouseActionEnqueued -= OnMouseActionEnqueued;
                }
                _inputModule = value;
                _inputModule.MouseActionEnqueued += OnMouseActionEnqueued;
            }
        }
    }

    void OnMouseActionEnqueued(MouseAction m)
    {
        GameObject clickView = GameObject.Instantiate(_testingBotClickViewPrefab);
        clickView.transform.SetParent(transform);
        clickView.transform.localScale = Vector3.one;
        clickView.GetComponent<TestingBotClickView>().Init(m, _canvas);
    }
}
