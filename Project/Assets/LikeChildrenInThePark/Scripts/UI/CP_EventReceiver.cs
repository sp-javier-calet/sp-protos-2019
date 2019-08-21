
using UnityEngine;
using UnityEngine.EventSystems;

public class CP_EventReceiver : MonoBehaviour
{
    public CP_SceneManager SceneManager;

    public class EventHandler : EventTrigger
    {
        CP_EventReceiver _parentViewController;
            
        public void SetParentViewController(CP_EventReceiver viewController)
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

