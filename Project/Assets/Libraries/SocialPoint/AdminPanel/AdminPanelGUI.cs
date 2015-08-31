using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public abstract class AdminPanelGUI
    {
        public static AdminPanelConsole AdminPanelConsole { set; private get; }
        public AdminPanelConsole Console { get { return AdminPanelGUI.AdminPanelConsole; }}

        public abstract void OnCreateGUI(AdminPanelLayout layout);
    }

    public sealed class AdminPanelGUIOptions
    {
        public static readonly AdminPanelGUIOptions None = new AdminPanelGUIOptions();
    }
}