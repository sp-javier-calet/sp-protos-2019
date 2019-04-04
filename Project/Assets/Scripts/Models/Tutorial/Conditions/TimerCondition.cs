using System;

namespace SocialPoint.Tutorial
{
    [Serializable]
    public class TimeCondition : ICondition
    {
        public float Time;

        [NonSerialized] bool _completed;
        [NonSerialized] float _counter;

        public void OnStartEvaluating()
        {
        }

        public bool Completed => _completed;

        public TimeCondition()
        {
            Time = 1.0f;
        }

        public void Update(float elapsed)
        {
            if(_completed)
            {
                return;
            }

            _counter += elapsed;
            _completed = (_counter >= Time);
        }
    }
}
