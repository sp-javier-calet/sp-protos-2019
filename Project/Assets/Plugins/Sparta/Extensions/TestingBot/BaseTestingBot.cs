using UnityEngine;
using System.Collections;
using SocialPoint.EventSystems;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace SocialPoint.TestingBot
{
    public class BaseTestingBot :  MonoBehaviour
    {
        protected TestableActionStandaloneInputModule _inputModule;

        protected LayerMask _guiMask = new LayerMask();

        protected virtual void OnAwake()
        {
        }

        void Awake()
        {
            RefreshInputModule();
            InitGuiMask();
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

        void InitGuiMask()
        {
            var layerNames = SocialPoint.GUIControl.UILayersController.LayerNames;
            for(int i = 0; i < layerNames.Length; ++i)
            {
                _guiMask.value |= 1 << LayerMask.NameToLayer(layerNames[i]);
            }
        }

        protected List<GameObject> GetUIClickableElements()
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

        protected Vector3 GetUIGameObjectPosition(GameObject go)
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

        protected void ClickOnUIGameObject(GameObject go)
        {
            var canvas = go.GetComponentInParent<Canvas>();
            if(canvas != null)
            {
                var position = GetUIGameObjectPosition(go);
                var screenPosition = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, position);
                _inputModule.SimulateClick(screenPosition);
            }
        }

        protected TestableActionStandaloneInputModule FindInputModule()
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