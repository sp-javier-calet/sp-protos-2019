using UnityEngine;
using UnityEngine.EventSystems;
using SocialPoint.EventSystems;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SocialPoint.TestingBot
{
    public class MouseAction
    {
        public Vector2 StartPosition { get; private set; }

        public Vector2 EndPosition { get; private set; }

        public float ClickDuration { get; private set; }

        public bool Clicked { get; private set; }

        public Vector2 Position { get; private set; }

        public float ElapsedTime { get; private set; }

        public bool IsFinished { get; private set; }

        public bool HasStarted { get; private set; }

        public MouseAction(Vector2 startPosition, Vector2 endPosition, float clickDuration)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            ClickDuration = clickDuration;

            Clicked = false;
            Position = startPosition;
        }

        public event Action<MouseAction> Started;

        public event Action<MouseAction> Finished;

        public void Start()
        {
            Clicked = false;
            Position = StartPosition;
            HasStarted = true;
            if(Started != null)
            {
                Started(this);
            }
        }

        public void Finish()
        {
            if(IsFinished)
            {
                return;
            }
            IsFinished = true;
            Clicked = false;
            if(Finished != null)
            {
                Finished(this);
            }
        }

        public void Update(float dt)
        {
            if(!HasStarted)
            {
                Start();
                return;
            }

            ElapsedTime += dt;
            if(ElapsedTime >= ClickDuration && Clicked)
            {
                Clicked = false;
                Position = EndPosition;
                Finish();
            }
            else
            {
                Clicked = true;
                Position = Vector2.Lerp(StartPosition, EndPosition, ElapsedTime / ClickDuration);
            }
        }
    }

    public class TestableActionStandaloneInputModule : ActionStandaloneInputModule
    {
        private readonly MouseState _mouseState = new MouseState();

        Queue<MouseAction> _pendingMouseActions = new Queue<MouseAction>();

        MouseAction _currentMouseAction;

        public event Action<MouseAction> MouseActionStarted;

        public event Action<MouseAction> MouseActionFinished;

        public event Action<MouseAction> MouseActionEnqueued;

        void OnMouseActionStarted(MouseAction mouseAction)
        {
            if(MouseActionStarted != null)
            {
                MouseActionStarted(mouseAction);
            }
        }

        void OnMouseActionFinished(MouseAction mouseAction)
        {
            mouseAction.Started -= OnMouseActionStarted;
            mouseAction.Finished -= OnMouseActionFinished;
            if(MouseActionFinished != null)
            {
                MouseActionFinished(mouseAction);
            }
        }

        bool _lastMouseActionClicked = false;

        PointerEventData.FramePressState GetFramePressStateByMouseAction(MouseAction mouseAction, bool lastMouseActionClicked)
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

        MouseState GetMousePointerEventDataByMouseAction(MouseAction mouseAction)
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

        public MouseAction SimulateClick(Vector2 position, float duration = 0.1f)
        {
            return SimulateDrag(position, position, duration);
        }

        public MouseAction SimulateDrag(Vector2 startPosition, Vector2 endPosition, float pressDuration)
        {
            var mouseAction = new MouseAction(startPosition, endPosition, pressDuration);
            _pendingMouseActions.Enqueue(mouseAction);

            mouseAction.Started += OnMouseActionStarted;
            mouseAction.Finished += OnMouseActionFinished;

            if(MouseActionEnqueued != null)
            {
                MouseActionEnqueued(mouseAction);
            }

            return mouseAction;
        }

        public override void Process()
        {
            if(_currentMouseAction == null && _pendingMouseActions.Count > 0)
            {
                _currentMouseAction = _pendingMouseActions.Dequeue();
            }
            if(_currentMouseAction != null)
            {
                _currentMouseAction.Update(Time.unscaledDeltaTime);
            }
            base.Process();
            if(_currentMouseAction != null && _currentMouseAction.IsFinished)
            {
                _currentMouseAction = null;
            }
        }
    }
}