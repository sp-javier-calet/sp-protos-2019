using UnityEngine;
using Zenject;
using SocialPoint.ServerSync;

public class SyncIndicator : MonoBehaviour
{
    [Inject]
    ICommandQueue _commandQueue;

    [PostInject]
    void PostInject()
    {
        _commandQueue.SyncChange += OnCommandQueueSyncChange;
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        _commandQueue.SyncChange -= OnCommandQueueSyncChange;
    }

    void OnCommandQueueSyncChange()
    {
        gameObject.SetActive(!_commandQueue.Synced);
    }
		
}
