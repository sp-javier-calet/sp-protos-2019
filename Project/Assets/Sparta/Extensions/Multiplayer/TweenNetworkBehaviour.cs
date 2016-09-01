using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public class TweenNetworkBehaviour : INetworkBehaviour
    {
        Vector3 _origin;
        Vector3 _destination;
        Vector3 _delta;
        float _duration;
        Easing.Function _easing;
        float _time;
        NetworkGameObject _go;

        public TweenNetworkBehaviour(Vector3 dest, float duration, Easing.Function easing)
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
                _go.Transform.Position = new Vector3(
                    _easing(_time, _origin.x, _delta.x, _duration),
                    _easing(_time, _origin.y, _delta.y, _duration),
                    _easing(_time, _origin.z, _delta.z, _duration));
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
        public static void Tween(this NetworkServerSceneController ctrl, int id, Vector3 dest, float duration, Easing.Function easing)
        {
            ctrl.AddBehaviour(id, new TweenNetworkBehaviour(dest, duration, easing));
        }

        public static void Tween(this NetworkServerSceneController ctrl, int id, Vector3 dest, float duration)
        {
            ctrl.Tween(id, dest, duration, Easing.Linear);
        }
    }
}
