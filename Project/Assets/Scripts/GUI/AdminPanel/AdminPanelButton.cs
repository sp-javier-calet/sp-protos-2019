using UnityEngine;
using UnityEngine.EventSystems;
using SocialPoint.AdminPanel;
using SocialPoint.GUIControl;
using System.Collections;
using System.Collections.Generic;
using System;
using Zenject;

public class AdminPanelButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [InjectOptional]
    AdminPanel AdminPanel;

    [Inject]
    List<IAdminPanelConfigurer> _configurers;

    public float WaitTime = 1.0f;
    private bool _down = false;
    private float _timeSinceDown = 0.0f;
    private AdminPanelController _adminPanelController;

    public void OnPointerUp(PointerEventData data)
    {
        _down = false;
    }

    public void OnPointerDown(PointerEventData data)
    {
        _down = true;
        _timeSinceDown = 0.0f;
    }

    [PostInject]
    void PostInject()
    {
        if(AdminPanel != null)
        {
            AdminPanel.RegisterConfigurers(_configurers);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
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
