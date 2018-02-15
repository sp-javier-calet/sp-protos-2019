using UnityEngine;
using UnityEngine.EventSystems;
using SocialPoint.EventSystems;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SocialPoint.TestingBot
{
    public class TestableActionStandaloneInputModule : SPStandaloneInputModule
    {
        private readonly MouseState _mouseState = new MouseState();

        Queue<IMouseAction> _pendingMouseActions = new Queue<IMouseAction>();

        IMouseAction _currentMouseAction;
        bool _currentMouseActionFinished;

        public event Action<IMouseAction> MouseActionStarted;

        public event Action<IMouseAction> MouseActionFinished;

        public event Action<IMouseAction> MouseActionEnqueued;

        void OnMouseActionStarted(IMouseAction mouseAction)
        {
            _currentMouseActionFinished = false;
            if(MouseActionStarted != null)
            {
                MouseActionStarted(mouseAction);
            }
        }

        void OnMouseActionFinished(IMouseAction mouseAction)
        {
            _currentMouseActionFinished = true;
            mouseAction.Started -= OnMouseActionStarted;
            mouseAction.Finished -= OnMouseActionFinished;
            if(MouseActionFinished != null)
            {
                MouseActionFinished(mouseAction);
            }
        }

        bool _lastMouseActionClicked = false;

        PointerEventData.FramePressState GetFramePressStateByMouseAction(IMouseAction mouseAction, bool lastMouseActionClicked)
        {
            if(mouseAction.Clicked)
            {
                return lastMouseActionClicked ? PointerEventData.FramePressState.NotChanged : PointerEventData.FramePressState.Pressed;
            }
            else
            {
                return lastMouseActionClicked ? PointerEventData.FramePressState.Released : PointerEventData.FramePressState.NotChanged;
            }
        }

        MouseState GetMousePointerEventDataByMouseAction(IMouseAction mouseAction)
        {
            PointerEventData leftData;
            var created = GetPointerData(kMouseLeftId, out leftData, true);

            leftData.Reset();

            if(created)
                leftData.position = mouseAction.Position;
            Vector2 pos = mouseAction.Position;
            leftData.delta = pos - leftData.position;
            leftData.position = pos;
            leftData.scrollDelta = Vector2.zero;
            leftData.button = PointerEventData.InputButton.Left;
            eventSystem.RaycastAll(leftData, m_RaycastResultCache);
            var raycast = FindFirstRaycast(m_RaycastResultCache);
            leftData.pointerCurrentRaycast = raycast;
            m_RaycastResultCache.Clear();

            PointerEventData rightData;
            GetPointerData(kMouseRightId, out rightData, true);
            CopyFromTo(leftData, rightData);
            rightData.button = PointerEventData.InputButton.Right;

            PointerEventData middleData;
            GetPointerData(kMouseMiddleId, out middleData, true);
            CopyFromTo(leftData, middleData);
            middleData.button = PointerEventData.InputButton.Middle;

            var pressState = GetFramePressStateByMouseAction(mouseAction, _lastMouseActionClicked);
            _lastMouseActionClicked = mouseAction.Clicked;

            _mouseState.SetButtonState(PointerEventData.InputButton.Left, pressState, leftData);
            _mouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), rightData);
            _mouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), middleData);

            return _mouseState;
        }

        protected override MouseState GetMousePointerEventData(int id)
        {
            if(_currentMouseAction != null)
            {
                return GetMousePointerEventDataByMouseAction(_currentMouseAction);
            }
            else
            {
                return base.GetMousePointerEventData(id);
            }
        }

        public void SimulateMouseAction(IMouseAction mouseAction)
        {
            _pendingMouseActions.Enqueue(mouseAction);

            mouseAction.Started += OnMouseActionStarted;
            mouseAction.Finished += OnMouseActionFinished;

            if(MouseActionEnqueued != null)
            {
                MouseActionEnqueued(mouseAction);
            }
        }

        public override void Process()
        {
            PointerEventData leftData;
            GetPointerData(kMouseLeftId, out leftData, true);
            if(_currentMouseAction == null && _pendingMouseActions.Count > 0)
            {
                _currentMouseAction = _pendingMouseActions.Dequeue();
            }
            if(_currentMouseAction != null)
            {
                _currentMouseAction.Update(Time.unscaledDeltaTime);
            }
            base.Process();
            if(_currentMouseAction != null && _currentMouseActionFinished)
            {
                _currentMouseAction = null;
            }
        }
    }
}