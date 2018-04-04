using System;
using SocialPoint.NetworkModel;
using SocialPoint.Utils;

namespace Examples.Multiplayer.Lockstep
{
    public class PlayerNetworkSceneBehaviour : INetworkSceneDelegate
    {
        readonly Config _config;

        public readonly byte PlayerNumber;

        public PlayerNetworkSceneBehaviour(byte playerNumber, Config config)
        {
            PlayerNumber = playerNumber;
            _config = config;
            Mana = 0;
        }

        public long Mana { get; private set; }

        void INetworkSceneDelegate.OnObjectInstantiated(NetworkGameObject ngo)
        {
        }

        void INetworkSceneDelegate.OnObjectAdded(NetworkGameObject ngo)
        {
            var owner = ngo.Behaviours.Get<OwnerNetworkBehavior>();
            if(owner == null || owner.PlayerNumber != PlayerNumber)
            {
                return;
            }

            Mana -= _config.UnitCost;
        }

        void INetworkSceneDelegate.OnObjectRemoved(NetworkGameObject ngo)
        {
        }

        void INetworkSceneDelegate.OnObjectDestroyed(NetworkGameObject ngo)
        {
        }

        void IDeltaUpdateable<int>.Update(int elapsed)
        {
            if(Mana > _config.MaxMana)
            {
                Mana = _config.MaxMana;
            }
            else
            {
                Mana += elapsed * _config.ManaSpeed;
            }
        }
    }
}