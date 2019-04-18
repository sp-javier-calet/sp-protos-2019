//-----------------------------------------------------------------------
// AdminPanelButton.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using UnityEngine;
#if ADMIN_PANEL
using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using UnityEngine.EventSystems;

public class AdminPanelButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    AdminPanel _adminPanel;

    [SerializeField] float WaitTime = 1.0f;

    bool _down;
    float _timeSinceDown;
    AdminPanelController _adminPanelController;

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
        if(_adminPanel != null)
        {
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
                _adminPanelController.Hide();
            }
        }
        //

        if(!_down)
        {
            return;
        }

        _timeSinceDown += Time.deltaTime;
        if(!(_timeSinceDown >= WaitTime))
        {
            return;
        }

        _down = false;
        OnActivation();
    }

    void OnActivation()
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

#else
public class AdminPanelButton : MonoBehaviour
{
#pragma warning disable 0414
    [SerializeField] float WaitTime = 1.0f;
#pragma warning restore 0414

    void Start()
    {
        gameObject.SetActive(false);
    }
}

#endif