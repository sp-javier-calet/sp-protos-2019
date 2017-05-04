using Jitter.LinearMath;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public class ConstantSpeedNetworkInterpolate : INetworkBehaviour, INetworkInterpolate
    {
        public static double PositionLerpSpeed = 18.0;
        const double _epsilon = 1e-4;
        const double _defMaxDistance = 4;
        const double _minTimeToProcessData = 0.1;
        const int _minIterationCount = 1;

        public bool Enable{ get; set; }

        NetworkGameObject _go;

        JQuaternion _targetRotationPredicted;

        IGameTime _gameTime;

        int _iterationCount;
        bool _canSmooth;
        JVector _dir;
        double _maxDistance;
        double _maxDistanceSQ;

        JVector _prevServerPos;

        double _rotationLerpSpeed;

        double _serverTimeStamp;
        float _serverSpeed;

        public ConstantSpeedNetworkInterpolate Init(IGameTime gameTime, double maxDistance = _defMaxDistance, double rotationLerpSpeed = -1.0)
        {
            _gameTime = gameTime;
            _maxDistance = maxDistance;
            _maxDistanceSQ = maxDistance * maxDistance;
            Enable = true;
            _iterationCount = 0;
            _dir = JVector.Zero;
            _rotationLerpSpeed = rotationLerpSpeed;
            _canSmooth = false;
            return this;
        }

        public void OnServerTransform(Transform t, float serverTimestampf)
        {
            var serverDeltaPos = t.Position - _prevServerPos;
            var serverDeltaMag = serverDeltaPos.Length();
            if(serverDeltaMag > _epsilon)
            {
                _serverTimeStamp = (double)serverTimestampf;

                if(_go == null)
                {
                    _iterationCount = 0;
                    return;
                }

                _canSmooth = CanSmooth(t);
                if(_canSmooth)
                {   
                    _serverSpeed = serverDeltaMag / ((float)_serverTimeStamp);
                    var positionDelta = t.Position - _go.Transform.Position;
                    if(positionDelta.LengthSquared() > _epsilon)
                    {
                        _dir = positionDelta.Normalized();
                    }
                    else
                    {
                        _dir = JVector.Zero;
                    }
                }

                if(!_canSmooth)
                {
                    ResetToCurrentTransform(t);
                }

                _prevServerPos = t.Position;
            }
            _targetRotationPredicted = t.Rotation;
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

            if((t.Position - _go.Transform.Position).ZeroYValue().LengthSquared() > _maxDistanceSQ)
            {
                return false;
            }

            if(_serverTimeStamp < _epsilon)
            {
                return false;
            }

            return true;
        }

        void ResetToCurrentTransform(Transform t)
        {
            _go.Transform.Position = _prevServerPos = t.Position;
            _serverSpeed = 0;
        }

        public void OnAwake()
        {
        }

        public void OnStart()
        {
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
            
            if(Enable)
            {
                InterpolatePosition(dt);
            }

            InterpolateRotation(dt);
        }

        void InterpolatePosition(float dt)
        {
            var currToServerLength = (_prevServerPos - _go.Transform.Position).Length();
            var delta = (float)_serverSpeed * dt;
            if(delta > currToServerLength)
            {
                _go.Transform.Position = _prevServerPos;
                _serverSpeed = 0f;
            }
            else
            {
                _go.Transform.Position = _go.Transform.Position + _dir * delta;
            }
        }

        void InterpolateRotation(float dt)
        {
            var rotationLerpSpeed = _rotationLerpSpeed > 0.0 ? _rotationLerpSpeed : InterpolationSettings.RotationLerpSpeed;
            JQuaternion targetRotation = JQuaternion.Identity;
            JQuaternionUtils.Slerp(ref _go.Transform.Rotation, ref _targetRotationPredicted, (float)Math.Min(1f, (dt * rotationLerpSpeed)), out targetRotation);
            _go.Transform.Rotation = targetRotation;
        }

        public object Clone()
        {
            var other = ObjectPool.Get<ConstantSpeedNetworkInterpolate>().Init(_gameTime, _maxDistance);
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
