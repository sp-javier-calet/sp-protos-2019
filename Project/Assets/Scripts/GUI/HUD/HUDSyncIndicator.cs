using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using SocialPoint.ServerSync;

public class HUDSyncIndicator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
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

    #region IPointerDownHandler implementation
    
    public void OnPointerDown(PointerEventData eventData)
    {
    }
    
    #endregion
		
    #region IPointerUpHandler implementation

    public void OnPointerUp(PointerEventData eventData)
    {
        _commandQueue.Send();
    }

    #endregion
}
