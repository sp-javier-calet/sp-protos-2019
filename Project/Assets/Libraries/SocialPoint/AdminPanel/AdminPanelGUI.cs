using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.AdminPanel
{
    public abstract class AdminPanelGUI
    {
        public static AdminPanelConsole AdminPanelConsole { set; private get; }
        public AdminPanelConsole Console { get { return AdminPanelGUI.AdminPanelConsole; }}

        public abstract void OnCreateGUI(AdminPanelLayout layout);
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
        
        public override void OnCreateGUI(AdminPanelLayout layout)
        {
            foreach(AdminPanelGUI gui in guiGroup)
            {
                gui.OnCreateGUI(layout);
            }
        }
    }
}