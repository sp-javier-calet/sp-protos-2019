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
        if(_commandQueue != null)
        {
            _commandQueue.SyncChange += OnCommandQueueSyncChange;
        }
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if(_commandQueue != null)
        {
            _commandQueue.SyncChange -= OnCommandQueueSyncChange;
        }
    }

    void OnCommandQueueSyncChange()
    {
        if(_commandQueue != null)
        {
            gameObject.SetActive(!_commandQueue.Synced);
        }
    }

    #region IPointerDownHandler implementation

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    #endregion

    #region IPointerUpHandler implementation

    public void OnPointerUp(PointerEventData eventData)
    {
        if(_commandQueue != null)
        {
            _commandQueue.Send();
        }
    }

    #endregion
}
