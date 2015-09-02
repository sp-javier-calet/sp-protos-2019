using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.AdminPanel
{
    public interface AdminPanelConfigurer
    {
        void OnConfigure(AdminPanel adminPanel);
    }

    public interface AdminPanelGUI
    {
        void OnCreateGUI(AdminPanelLayout layout);
    }

    public class AdminPanelNestedGUI : AdminPanelGUI
    {
        private string _name;
        private AdminPanelGUI _gui;

        public AdminPanelNestedGUI(string name, AdminPanelGUI gui)
        {
            _name = name;
            _gui = gui;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateOpenPanelButton(_name, _gui);
        }
    }

    public class AdminPanelGUIGroup : AdminPanelGUI
    {
        private List<AdminPanelGUI> guiGroup;
        
        public AdminPanelGUIGroup()
        {
            guiGroup = new List<AdminPanelGUI>();
        }
        
        public AdminPanelGUIGroup(AdminPanelGUI gui) : this()
        {
            guiGroup.Add(gui);
        }
        
        public void Add(AdminPanelGUI gui)
        {
            guiGroup.Add(gui);
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            foreach(AdminPanelGUI gui in guiGroup)
            {
                gui.OnCreateGUI(layout);
            }
        }
    }
}