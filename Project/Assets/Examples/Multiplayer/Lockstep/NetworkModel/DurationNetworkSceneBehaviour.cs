using System;
using SocialPoint.NetworkModel;
using SocialPoint.Utils;

namespace Examples.Multiplayer.Lockstep
{
    public class DurationNetworkSceneBehaviour : INetworkSceneDelegate
    {
        readonly Config _config;
        bool _finishedTriggered;
        long _time;

        public DurationNetworkSceneBehaviour(Config config)
        {
            _time = 0;
            _config = config;
        }

        public long TimeLeft { get { return _config.Duration - _time; } }

        void INetworkSceneDelegate.OnObjectInstantiated(NetworkGameObject ngo)
        {
        }

        void INetworkSceneDelegate.OnObjectAdded(NetworkGameObject ngo)
        {
        }

        void INetworkSceneDelegate.OnObjectRemoved(NetworkGameObject ngo)
        {
        }

        void INetworkSceneDelegate.OnObjectDestroyed(NetworkGameObject ngo)
        {
        }

        void IDeltaUpdateable<int>.Update(int elapsed)
        {
            if(TimeLeft <= 0)
            {
                if(!_finishedTriggered)
                {
                    _finishedTriggered = true;
                    if(Finished != null)
                    {
                        Finished();
                    }
                    else
                    {
                        throw new Exception("Game duration finished!");
                    }
                }
            }
            else
            {
                _time += elapsed;
            }
        }

        public event Action Finished;
    }
}