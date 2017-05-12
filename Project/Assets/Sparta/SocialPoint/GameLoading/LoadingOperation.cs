using System;

namespace SocialPoint.GameLoading
{
    public interface ILoadingOperation
    {
        float Progress{ get; }

        string Message{ get; }

        bool HasExpectedDuration{ get; }

        float ExpectedDuration{ get; }

        void Start();
    }

    public sealed class LoadingOperation : ILoadingOperation
    {
        public float Progress { set; get; }

        public string Message { set; get; }

        public float ExpectedDuration { private set; get; }

        public bool HasExpectedDuration
        {
            get
            {
                return ExpectedDuration >= 0.0f;
            }
        
        }

        Action _start;

        public LoadingOperation(float duration, Action start = null)
        {
            Progress = 0.0f;
            ExpectedDuration = duration;
            _start = start;
        }

        public LoadingOperation(Action start = null) : this(-1.0f, start)
        {
        }

        public void Start()
        {
            if(_start != null)
            {
                _start();
            }
        }

        public void Update(float progress, string message = null)
        {
            Progress = progress;
            if(!string.IsNullOrEmpty(message))
            {
                Message = message;
            }
        }

        public void Finish(string message = null)
        {
            Update(1.0f, message);
        }
    }
}
