using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.AdminPanel
{
    public interface IAdminPanelConfigurer
    {
        void OnConfigure(AdminPanel adminPanel);
    }

    public interface IAdminPanelGUI
    {
        void OnCreateGUI(AdminPanelLayout layout);
    }

    public class AdminPanelNestedGUI : IAdminPanelGUI
    {
        private string _name;
        private IAdminPanelGUI _gui;

        public AdminPanelNestedGUI(string name, IAdminPanelGUI gui)
        {
            _name = name;
            _gui = gui;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateOpenPanelButton(_name, _gui);
        }
    }

    public class AdminPanelGUIGroup : IAdminPanelGUI
    {
        private List<IAdminPanelGUI> guiGroup;
        
        public AdminPanelGUIGroup()
        {
            guiGroup = new List<IAdminPanelGUI>();
        }
        
        public AdminPanelGUIGroup(IAdminPanelGUI gui) : this()
        {
            guiGroup.Add(gui);
        }
        
        public void Add(IAdminPanelGUI gui)
        {
            guiGroup.Add(gui);
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            foreach(IAdminPanelGUI gui in guiGroup)
            {
                gui.OnCreateGUI(layout);
            }
        }
    }
}