using UnityEngine;
using UnityEngine.EventSystems;
using SocialPoint.AdminPanel;
using SocialPoint.GUI;
using System.Collections;
using Zenject;

public class AdminPanelButton : MonoBehaviour
{
    [Inject]
    AdminPanel AdminPanel;

    public float WaitTime = 1.0f;
    private bool _down = false;
    private float _timeSinceDown = 0.0f;
    private AdminPanelController _adminPanelController;

    public void OnPointerUp(BaseEventData data)
    {
        _down = false;
    }

    public void OnPointerDown(BaseEventData data)
    {
        _down = true;
        _timeSinceDown = 0.0f;
    }

    public void Update()
    {
        if(_down)
        {
            _timeSinceDown += Time.deltaTime;
            if(_timeSinceDown >= WaitTime)
            {
                _down = false;
                OnActivation();
            }
        }
    }

    private void OnActivation()
    {
        if(_adminPanelController == null)
        {
            _adminPanelController = UIViewController.Factory.Create<AdminPanelController>();
            _adminPanelController.AdminPanel = AdminPanel;
            _adminPanelController.transform.SetParent(transform.parent, false);
        }
        _adminPanelController.Show();
    }
}
