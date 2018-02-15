using System;
using Jitter.LinearMath;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public static class InterpolationSettings
    {
        public static bool Enable = true;
        public static double RotationLerpSpeed = 13.0;
    }

    public class ConstantSpeedNetworkInterpolate : INetworkBehaviour, INetworkInterpolate
    {
        public static double PositionLerpSpeed = 18.0;
        const double _epsilon = 1e-4f;
        const double _defMaxDistance = 4f;
        const double _minTimeToProcessData = 0.1;
        const int _minIterationCount = 1;
        const float _maxStartPositionErrorSQ = 4f;

        public bool Enable{ get; set; }

        NetworkGameObject _go;

        JQuaternion _serverRotation;

        IGameTime _gameTime;

        int _iterationCount;
        bool _canSmooth;
        JVector _dir;
        double _maxDistance;
        double _maxDistanceSQ;

        JVector _serverPos;

        double _rotationLerpSpeed;

        double _serverTimeStamp;
        float _serverSpeed;
        bool _hideIfNotInterpolation;

        UnityEngine.Transform _viewModel;

        public JVector ServerPosition { get { return _serverPos; } }

        public JQuaternion ServerRotation { get { return _serverRotation; } }

        public ConstantSpeedNetworkInterpolate Init(IGameTime gameTime, double maxDistance = _defMaxDistance, bool hideIfNotInterpolation = false, double rotationLerpSpeed = -1.0)
        {
            _gameTime = gameTime;
            _maxDistance = maxDistance;
            Enable = true;
            _iterationCount = 0;
            _dir = JVector.Zero;
            _rotationLerpSpeed = rotationLerpSpeed;
            _canSmooth = false;
            _hideIfNotInterpolation = hideIfNotInterpolation;
            return this;
        }

        public void OnNewObject(Transform t)
        {
            _serverPos = t.Position;
            _serverRotation = t.Rotation;
        }

        public void OnServerTransform(Transform t, float serverTimestampf)
        {
            if(_go == null)
            {
                return;
            }

            _serverTimeStamp = (double)serverTimestampf;
            _canSmooth = CanSmooth(t);

            if(_canSmooth)
            {
                var serverDeltaPos = t.Position - _serverPos;
                var serverDeltaMag = serverDeltaPos.Length();
                _serverSpeed = 0f;
                if(serverDeltaMag > _epsilon)
                {
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
                }
            }

            if(!_canSmooth)
            {
                ResetToCurrentTransform(t);
            }
            else
            {
                if((_go.Transform.Position - _serverPos).LengthSquared() > _maxStartPositionErrorSQ)
                {
                    _go.Transform.Position = _serverPos;
                }
            }

            _serverPos = t.Position;
            _serverRotation = t.Rotation;
            _go.Transform.Scale = t.Scale;
            _iterationCount++;

            RefreshVisibility();
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

            if(_serverTimeStamp <= 0f)
            {
                return false;
            }

            return true;
        }

        void ResetToCurrentTransform(Transform t)
        {
            _go.Transform.Position = _serverPos = t.Position;
            _serverSpeed = 0;
        }

        public void OnAwake()
        {
            _maxDistanceSQ = _maxDistance * _maxDistance;
            var view = _go.GetBehaviour<UnityViewBehaviour>();
            _viewModel = view.View.transform.childCount > 0 ? view.View.transform.GetChild(0) : view.View.transform;
            RefreshVisibility();
        }

        public void OnStart()
        {
            
        }

        void RefreshVisibility()
        {
            if(_hideIfNotInterpolation && _viewModel != null)
            {
                _viewModel.gameObject.SetActive(_iterationCount > 1);
            }
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
            var delta = _serverSpeed * dt;
            _go.Transform.Position = _go.Transform.Position + _dir * delta;
        }

        void InterpolateRotation(float dt)
        {
            var rotationLerpSpeed = _rotationLerpSpeed > 0.0 ? _rotationLerpSpeed : InterpolationSettings.RotationLerpSpeed;
            JQuaternion targetRotation = JQuaternion.Identity;
            JQuaternionUtils.Slerp(ref _go.Transform.Rotation, ref _serverRotation, (float)Math.Min(1f, (dt * rotationLerpSpeed)), out targetRotation);
            _go.Transform.Rotation = targetRotation;
        }

        public object Clone()
        {
            var other = _go != null ? _go.Context.Pool.Get<ConstantSpeedNetworkInterpolate>() : new ConstantSpeedNetworkInterpolate();
            other.Init(_gameTime, _maxDistance);
            other.Enable = Enable;
            other._hideIfNotInterpolation = _hideIfNotInterpolation;
            other._rotationLerpSpeed = _rotationLerpSpeed;
            return other;
        }

        public void Dispose()
        {
            _go.Context.Pool.Return(this);
        }

        public NetworkGameObject GameObject
        {
            set
            {
                _go = value;
                _iterationCount = 0;
            }
        }
    }
}
