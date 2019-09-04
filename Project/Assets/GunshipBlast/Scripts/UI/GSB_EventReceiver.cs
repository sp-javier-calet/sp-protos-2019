
using UnityEngine;
using UnityEngine.EventSystems;

public class GSB_EventReceiver : MonoBehaviour
{
    public GSB_SceneManager SceneManager;

    public class EventHandler : EventTrigger
    {
        GSB_EventReceiver _parentViewController;

        public void SetParentViewController(GSB_EventReceiver viewController)
        {
            _parentViewController = viewController;
        }

        public override void OnPointerDown(PointerEventData data)
        {
            _parentViewController.SceneManager.OnPressedDown();
        }

        public override void OnPointerUp(PointerEventData data)
        {
            _parentViewController.SceneManager.OnPressedUp();
        }

        public override void OnPointerExit(PointerEventData data)
        {
            OnPointerUp(data);
        }

        public override void OnPointerClick(PointerEventData data)
        {

        }
    }

    EventHandler _eventHandler = null;

    void Start()
    {
        _eventHandler = this.gameObject.AddComponent<EventHandler>();
        _eventHandler.SetParentViewController(this);
    }
}

