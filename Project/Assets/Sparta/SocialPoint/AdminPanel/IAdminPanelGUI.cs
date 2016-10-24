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

    public interface IAdminPanelManagedGUI : IAdminPanelGUI
    {
        void OnOpened();
        void OnClosed();
    }

    public sealed class AdminPanelNestedGUI : IAdminPanelGUI
    {
        readonly string _name;
        readonly IAdminPanelGUI _gui;

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

    public sealed class AdminPanelGUIGroup : IAdminPanelGUI
    {
        readonly List<IAdminPanelGUI> guiGroup;

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
            for(int i = 0, guiGroupCount = guiGroup.Count; i < guiGroupCount; i++)
            {
                IAdminPanelGUI gui = guiGroup[i];
                gui.OnCreateGUI(layout);
            }
        }
    }
}