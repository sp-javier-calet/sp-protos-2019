using SocialPoint.Utils;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public class TweenNetworkBehaviour : INetworkBehaviour
    {
        JVector _origin;
        JVector _destination;
        JVector _delta;
        float _duration;
        Easing.Function _easing;
        float _time;
        NetworkGameObject _go;

        public TweenNetworkBehaviour(JVector dest, float duration, Easing.Function easing)
        {
            _destination = dest;
            _duration = duration;
            _time = 0.0f;
            _easing = easing;
        }

        public object Clone()
        {
            var behaviour = new TweenNetworkBehaviour(_destination, _duration, _easing);
            behaviour._delta = _delta;
            behaviour._time = _time;
            behaviour._go = _go;
            return behaviour;
        }

        public void OnStart(NetworkGameObject go)
        {
            _go = go;
            _origin = go.Transform.Position;
            _delta = _destination - _origin;
            _time = 0.0f;
        }

        public void Update(float dt)
        {
            _time += dt;

            if(_go != null)
            {
                _go.Transform.Position = new JVector(
                    _easing(_time, _origin.X, _delta.X, _duration),
                    _easing(_time, _origin.Y, _delta.Y, _duration),
                    _easing(_time, _origin.Z, _delta.Z, _duration));
            }
        }

        public void OnDestroy()
        {
            _time = 0.0f;
            _go = null;
        }
    }

    public static class TweenNetworkGameObjectExtensions
    {
        public static void Tween(this NetworkServerSceneController ctrl, int id, JVector dest, float duration, Easing.Function easing)
        {
            ctrl.AddBehaviour(id, new TweenNetworkBehaviour(dest, duration, easing));
        }

        public static void Tween(this NetworkServerSceneController ctrl, int id, JVector dest, float duration)
        {
            ctrl.Tween(id, dest, duration, Easing.Linear);
        }
    }
}
