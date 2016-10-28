using UnityEngine;
using System.Collections;
using SocialPoint.EventSystems;
using System;

namespace SocialPoint.TestingBot
{
    public class BaseTestingBot :  MonoBehaviour
    {
        protected TestableActionStandaloneInputModule _inputModule;

        protected virtual void OnAwake()
        {
        }

        void Awake()
        {
            RefreshInputModule();
            OnAwake();
        }

        public event Action<TestableActionStandaloneInputModule> InputModuleChanged;

        void RefreshInputModule()
        {
            if(_inputModule == null)
            {
                _inputModule = FindInputModule();
                if(_inputModule != null && InputModuleChanged != null)
                {
                    InputModuleChanged(_inputModule);
                }
            }
        }

        TestableActionStandaloneInputModule FindInputModule()
        {
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if(eventSystem != null)
            {
                return eventSystem.GetComponent<TestableActionStandaloneInputModule>();
            }
            return null;
        }

        protected virtual void OnUpdate()
        {
        }

        void Update()
        {
            RefreshInputModule();

            if(_inputModule != null)
            {
                OnUpdate();
            }
        }
    }
}