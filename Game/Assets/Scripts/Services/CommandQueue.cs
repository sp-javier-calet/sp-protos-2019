using Zenject;
using SocialPoint.Network;
using UnityEngine;

class CommandQueue : SocialPoint.ServerSync.CommandQueue
{
    public CommandQueue(MonoBehaviour behaviour, IHttpClient client):base(behaviour, client)
    {
    }
}