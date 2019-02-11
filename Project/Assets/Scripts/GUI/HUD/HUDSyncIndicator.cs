using SocialPoint.Connectivity;
using UnityEngine;
using UnityEngine.EventSystems;
using SocialPoint.Dependency;
using SocialPoint.ServerSync;

public class HUDSyncIndicator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    IConnectivityWatcher _watcher;

    void Start()
    {
        _watcher = Services.Instance.Resolve<IConnectivityWatcher>();
        if(_watcher != null)
        {
            _watcher.ConnectivityChange += OnWatcherChange;
        }
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if(_watcher != null)
        {
            _watcher.ConnectivityChange -= OnWatcherChange;
        }
    }

    void OnWatcherChange(ConnectivityStatus status)
    {
        bool enabled = (status == ConnectivityStatus.Connected);
        gameObject.SetActive(enabled);

    }

    #region IPointerDownHandler implementation

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    #endregion

    #region IPointerUpHandler implementation

    public void OnPointerUp(PointerEventData eventData)
    {
        _watcher?.Reconnect();
    }

    #endregion
}
