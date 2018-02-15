using UnityEngine;
using System;

namespace SocialPoint.TestingBot
{
    public class MouseAction : IMouseAction
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

        public event Action<IMouseAction> Started;

        public event Action<IMouseAction> Finished;

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

    public static class MouseActionExtensions
    {
        public static IMouseAction SimulateClick(this TestableActionStandaloneInputModule inputModule,
                                                 Vector2 position, 
                                                 float duration = 0.1f)
        {
            return inputModule.SimulateDrag(position, position, duration);
        }

        public static IMouseAction SimulateDrag(this TestableActionStandaloneInputModule inputModule,
                                                Vector2 startPosition, 
                                                Vector2 endPosition, 
                                                float pressDuration)
        {
            var mouseAction = new MouseAction(startPosition, endPosition, pressDuration);
            inputModule.SimulateMouseAction(mouseAction);
            return mouseAction;
        }
    }
}