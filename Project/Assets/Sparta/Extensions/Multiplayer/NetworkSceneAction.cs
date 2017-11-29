﻿
using SocialPoint.Components;

namespace SocialPoint.Multiplayer
{
    public interface INetworkSceneActionHandler<T> : IStateActionHandler<NetworkScene, T>
    {
    }

    public interface INetworkSceneAction : IStateAppliable<NetworkScene>
    {
    }

    public class NetworkSceneMemento
    {
        public NetworkScene CurrentScene { get; private set; }

        public NetworkScene OldScene { get; private set; }

        //Difference in timestamp between memento scenes
        public float Delta { get; private set; }

        //Maximun acceptable delta timestamp (some actions should avoid perfoming changes if delta > threshold)
        public float Threshold { get; private set; }

        public byte ClientId { get; private set; }

        public NetworkSceneMemento(NetworkScene currentScene)
            : this(currentScene, currentScene, 0f, 1f, 0)
        {
        }

        public NetworkSceneMemento(NetworkScene currentScene, NetworkScene oldScene, float delta, float threshold, byte clientId)
        {
            CurrentScene = currentScene;
            OldScene = oldScene;
            Delta = delta;
            Threshold = threshold;
            ClientId = clientId;
        }
    }

    public class NetworkSceneActionProcessor : StateActionProcessor<NetworkSceneMemento>
    {
        public NetworkSceneActionProcessor()
        {
            Register(new AppliableStateActionHandler<NetworkSceneMemento>());
        }
    }
}
