//-----------------------------------------------------------------------
// HUDNotificationsController.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class HUDNotificationsController : MonoBehaviour
{
    [SerializeField]
    GameObject _notificationPrefab;

    [SerializeField]
    float _notificationDuration = 3f;

    [SerializeField]
    int _maxConcurrentNotifications = 4;

    [SerializeField]
    Transform _firstNotificationTransform;

    [SerializeField]
    Transform _lastNotificationTransform;

    Canvas _canvas;

    HUDNotification[] _notifications;
    int _notificationIndex = 0;
    int _activeNotifications = 0;

    void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _notifications = new HUDNotification[_maxConcurrentNotifications];
        for(int i = 0; i < _maxConcurrentNotifications; ++i)
        {
            var notification = Instantiate<GameObject>(_notificationPrefab);
            notification.transform.SetParent(transform);
            notification.transform.localScale = Vector3.one;
            var hudNotification = notification.GetComponent<HUDNotification>();
            notification.gameObject.SetActive(false);
            hudNotification.Finished += OnNotificationFinished;
            _notifications[i] = hudNotification;
        }
        _canvas.enabled = false;
    }

    void OnNotificationFinished(HUDNotification notification)
    {
        _activeNotifications = Mathf.Max(0, _activeNotifications - 1);
        if(_activeNotifications == 0)
        {
            _canvas.enabled = false;
        }
        notification.gameObject.SetActive(false);
    }

    void RepositionNotification(HUDNotification notification, int index)
    {
        float progress = _maxConcurrentNotifications > 1 ? ((float)index / (float)(_maxConcurrentNotifications - 1)) : 0.5f;
        notification.transform.position = Vector3.Lerp(_firstNotificationTransform.position, _lastNotificationTransform.position, progress);
    }

    void RepositionActiveNotifications()
    {
        int index = _notificationIndex + _maxConcurrentNotifications;
        for(int i = 0; i < _activeNotifications; ++i)
        {
            RepositionNotification(_notifications[(index - i - 1) % _maxConcurrentNotifications], i);
        }
    }

    public void ShowNotification(string text)
    {
        ShowNotification(text, Color.white);
    }

    public void ShowNotification(string text, Color color)
    {
        if(_activeNotifications == 0)
        {
            _canvas.enabled = true;
        }

        var notification = _notifications[_notificationIndex];
        notification.gameObject.SetActive(true);
        notification.Show(text, color, _notificationDuration);
        _notificationIndex = (_notificationIndex + 1) % _maxConcurrentNotifications;
        _activeNotifications = Mathf.Min(_maxConcurrentNotifications, _activeNotifications + 1);

        RepositionActiveNotifications();
    }

    void OnDestroy()
    {
        for(int i = 0; i < _notifications.Length; ++i)
        {
            _notifications[i].Finished -= OnNotificationFinished;
        }
    }
}