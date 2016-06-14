using UnityEngine;
using UnityEngine.EventSystems;
using SocialPoint.Dependency;
using SocialPoint.ServerSync;

public class HUDSyncIndicator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    ICommandQueue _commandQueue;

    void Start()
    {
        _commandQueue = ServiceLocator.Instance.Resolve<ICommandQueue>();
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
