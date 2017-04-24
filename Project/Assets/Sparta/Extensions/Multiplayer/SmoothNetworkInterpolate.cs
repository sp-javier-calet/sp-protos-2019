using Jitter.LinearMath;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public static class InterpolationSettings
    {
        public static bool Enable = true;
        public static double RotationLerpSpeed = 13.0;
    }

    public class SmoothNetworkInterpolate : INetworkBehaviour, INetworkInterpolate
    {
        const double _epsilon = 1e-4f;
        const double _defMaxDistance = 4f;
        const int _minIterationCount = 1;

        public bool Enable{ get; set; }

        NetworkGameObject _go;

        JQuaternion _targetRotationPredicted;

        double _smoothingDuration;
        JVector _deltaPosition;

        IGameTime _gameTime;

        int _iterationCount;
        bool _canSmooth;
        double _speed;
        JVector _dir;
        double _smoothingTime;
        double _maxDistance;
        double _maxDistanceSQ;

        double _serverTimestamp;
        JVector _prevServerPos;
        double _rotationLerpSpeed;
        JVector _shadowPos;
        float _shadowLerpSpeed = 20f;

        public SmoothNetworkInterpolate Init(IGameTime gameTime, double maxDistance = _defMaxDistance, double rotationLerpSpeed = -1.0)
        {
            _gameTime = gameTime;
            _maxDistance = maxDistance;
            _maxDistanceSQ = maxDistance * maxDistance;
            Enable = true;
            _iterationCount = 0;
            _dir = JVector.Zero;
            _speed = 0f;
            _canSmooth = false;
            _rotationLerpSpeed = rotationLerpSpeed;
            return this;
        }

        public void OnServerTransform(Transform t, float serverTimestampf)
        {
            _serverTimestamp = (double)serverTimestampf;

            if(_go == null)
            {
                _iterationCount = 0;
                return;
            }

            _canSmooth = CanSmooth(t);
            if(_canSmooth)
            {   
                _smoothingTime += _serverTimestamp;

                if(_smoothingTime < 0f)
                {
                    var serverDeltaPos = t.Position - _prevServerPos;
                    var smoothingTimeAbs = Math.Abs(_smoothingTime);
                    if(smoothingTimeAbs > _epsilon && serverDeltaPos.LengthSquared() > _epsilon)
                    {
                        _speed = serverDeltaPos.Length() / _serverTimestamp;
                        _dir = serverDeltaPos.Normalized();

                        var currentRightPos = t.Position + _dir * ((float)(_speed * smoothingTimeAbs));
                        _shadowPos = currentRightPos;
                    }
                    else
                    {
                        _canSmooth = false;
                    }
                }
                else
                {
                    var deltaPos = t.Position - _shadowPos;
                    if(_smoothingTime < _epsilon || deltaPos.LengthSquared() < _epsilon)
                    {
                        _canSmooth = false;
                    }
                    else
                    {
                        _dir = deltaPos.Normalized();
                        _speed = deltaPos.Length() / _smoothingTime;
                        _targetRotationPredicted = t.Rotation;
                    }
                }
            }

            if(!_canSmooth)
            {
                ResetToCurrentTransform(t);
            }

            _prevServerPos = t.Position;
            _go.Transform.Scale = t.Scale;
            _iterationCount++;
        }

        bool CanSmooth(Transform t)
        {
            if(!Enable)
            {
                return false;
            }

            if(_iterationCount < _minIterationCount)
            {
                return false;
            }

            if((t.Position - _shadowPos).LengthSquared() > _maxDistanceSQ)
            {
                return false;
            }

            if(_serverTimestamp <= 0f)
            {
                return false;
            }

            return true;
        }

        void ResetToCurrentTransform(Transform t)
        {
            _go.Transform.Position = _shadowPos = t.Position;
            _smoothingDuration = 0f;
            _deltaPosition = JVector.Zero;
            _smoothingTime = 0f;
            _speed = 0f;
        }

        public void OnAwake()
        {
        }

        public void OnStart()
        {
            _deltaPosition = JVector.Zero;
            _iterationCount = 0;
        }

        public void OnDestroy()
        {
        }

        public void Update(float dt)
        {
            Enable &= InterpolationSettings.Enable;

            if(_iterationCount < _minIterationCount)
            {
                return;
            }

            if(Enable && _canSmooth)
            {
                InterpolatePosition(dt);
            }

            var finalPos = _go.Transform.Position;
            JVector.Lerp(ref _go.Transform.Position, ref _shadowPos, Math.Min(1f, dt * _shadowLerpSpeed), out finalPos);
            _go.Transform.Position = finalPos;

            InterpolateRotation(dt);
        }

        void InterpolatePosition(float dt)
        {
            _shadowPos = _shadowPos + _dir * ((float)(_speed * dt));
            _smoothingTime -= dt;
        }

        void InterpolateRotation(float dt)
        {
            var rotationLerpSpeed = _rotationLerpSpeed > 0.0 ? _rotationLerpSpeed : InterpolationSettings.RotationLerpSpeed;
            JQuaternion targetRotation = _go.Transform.Rotation;
            JQuaternionUtils.Slerp(ref _go.Transform.Rotation, ref _targetRotationPredicted, (float)Math.Min(1f, (dt * rotationLerpSpeed)), out targetRotation);
            _go.Transform.Rotation = targetRotation;
        }

        public object Clone()
        {
            var other = ObjectPool.Get<SmoothNetworkInterpolate>().Init(_gameTime, _maxDistance);
            other.Enable = Enable;
            return other;
        }

        public void Dispose()
        {
            ObjectPool.Return(this);
        }

        public NetworkGameObject GameObject
        {
            set
            {
                _go = value;
            }
        }
    }
}
