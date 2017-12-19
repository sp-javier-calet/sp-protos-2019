using Jitter.LinearMath;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public class TweenNetworkBehaviour : NetworkBehaviour
    {
        JVector _origin;
        JVector _destination;
        JVector _delta;
        float _duration;
        Easing.Function _easing;
        float _time;

        public TweenNetworkBehaviour Init(JVector dest, float duration, Easing.Function easing)
        {
            _destination = dest;
            _duration = duration;
            _time = 0.0f;
            _easing = easing;
            return this;
        }

        public override object Clone()
        {
            var behaviour = GameObject.Context.Pool.Get<TweenNetworkBehaviour>();
            behaviour.Init(_destination, _duration, _easing);
            behaviour._delta = _delta;
            behaviour._time = _time;
            return behaviour;
        }

        protected override void OnStart()
        {
            _origin = GameObject.Transform.Position;
            _delta = _destination - _origin;
            _time = 0.0f;
        }

        protected override void Update(float dt)
        {
            _time += dt;

            if(GameObject != null)
            {
                GameObject.Transform.Position = new JVector(
                    _easing(_time, _origin.X, _delta.X, _duration),
                    _easing(_time, _origin.Y, _delta.Y, _duration),
                    _easing(_time, _origin.Z, _delta.Z, _duration));
            }
        }

        protected override void OnDestroy()
        {
            _time = 0.0f;
        }
    }

    public static class TweenNetworkGameObjectExtensions
    {
        private static readonly System.Type TweenNetworkBehaviourType = typeof(TweenNetworkBehaviour);

        public static void Tween(this NetworkServerSceneController ctrl, int id, JVector dest, float duration, Easing.Function easing)
        {
            var go = ctrl.FindObject(id);
            if(go != null)
            {
                go.AddBehaviour(new TweenNetworkBehaviour().Init(dest, duration, easing), TweenNetworkBehaviourType);
            }
        }

        public static void Tween(this NetworkServerSceneController ctrl, int id, JVector dest, float duration)
        {
            ctrl.Tween(id, dest, duration, Easing.Linear);
        }
    }
}
