using System;
using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;

namespace SocialPoint.Tutorial
{
    [Serializable]
    public class TimeCondition : ICondition
    {
        public float Time;

        [NonSerialized] private bool _completed;
        [NonSerialized] private float _counter;
        
        public bool Completed { get { return _completed; } }

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

        public void Dispose()
        {
        }
    }
}
