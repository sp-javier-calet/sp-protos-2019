using SocialPoint.Connectivity;
using UnityEngine;
using UnityEngine.EventSystems;
using SocialPoint.Dependency;
using SocialPoint.ServerSync;

public class HUDSyncIndicator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    ISessionConnectivity _connectivity;

    void Start()
    {
        _connectivity = Services.Instance.Resolve<ISessionConnectivity>();
        if(_connectivity != null)
        {
            _connectivity.ConnectivityChange += OnConnectivityChange;
        }
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if(_connectivity != null)
        {
            _connectivity.ConnectivityChange -= OnConnectivityChange;
        }
    }

    void OnConnectivityChange(ConnectivityStatus status)
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
        if(_connectivity != null)
        {
            _connectivity.Reconnect();
        }
    }

    #endregion
}
