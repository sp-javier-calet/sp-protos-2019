using SocialPoint.AdminPanel;
using SocialPoint.EventSystems;
using SocialPoint.GUIControl;
using SocialPoint.Dependency;

using UnityEngine;
using UnityEngine.EventSystems;

using System.Collections;
using System.Collections.Generic;
using System;

public class AdminPanelButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    AdminPanel _adminPanel;
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

    void Start()
    {
        _adminPanel = Services.Instance.Resolve<AdminPanel>();
        _configurers = Services.Instance.ResolveList<IAdminPanelConfigurer>();
        if(_adminPanel != null)
        {
            _adminPanel.RegisterConfigurers(_configurers);
            _adminPanel.ChangedVisibility += OnAdminPanelChangedVisibility;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        if(_adminPanel != null)
        {
            _adminPanel.ChangedVisibility -= OnAdminPanelChangedVisibility;
        }
    }

    void OnAdminPanelChangedVisibility()
    {
        GetComponent<CanvasGroup>().alpha = _adminPanel.Visible ? 0.0f : 1.0f;
    }

    void Update()
    {
        // quick test to use admin panel with gamepad input
        if(Input.GetButtonDown("Submit"))
        {
            _down = true;
            _timeSinceDown = WaitTime;
        }
        else if(Input.GetButtonDown("Cancel"))
        {
            if(_adminPanelController && _adminPanelController.AdminPanel.Visible)
            {
                _adminPanelController.Hide(false);
            }
        }
        //

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
            _adminPanelController.AdminPanel = _adminPanel;
            _adminPanelController.transform.SetParent(transform.parent, false);
        }
        _adminPanelController.Show();
    }
}
