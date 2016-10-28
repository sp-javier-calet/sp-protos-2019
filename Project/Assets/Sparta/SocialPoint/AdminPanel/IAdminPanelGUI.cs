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

    /// <summary>
    /// Sorted GUIs appears in order when grouped
    /// </summary>
    public interface IAdminPanelSortedGUI : IAdminPanelGUI
    {
        string Label { get; }
    }

    public sealed class AdminPanelNestedGUI : IAdminPanelSortedGUI
    {
        readonly string _name;
        readonly IAdminPanelGUI _gui;

        public string Label
        {
            get
            {
                return _name;
            }
        }
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

    public sealed class AdminPanelGUIGroup : IAdminPanelGUI, IComparer<IAdminPanelGUI>
    {
        readonly List<IAdminPanelGUI> guiGroup;

        public AdminPanelGUIGroup()
        {
            guiGroup = new List<IAdminPanelGUI>();
        }

        public AdminPanelGUIGroup(IAdminPanelGUI gui) : this()
        {
            guiGroup.Add(gui);
            guiGroup.Sort(this);
        }

        public void Add(IAdminPanelGUI gui)
        {
            guiGroup.Add(gui);
            guiGroup.Sort(this);
        }

        #region IComparer implementation

        public int Compare(IAdminPanelGUI x, IAdminPanelGUI y)
        {
            var a = x as IAdminPanelSortedGUI;
            var b = y as IAdminPanelSortedGUI;

            if(a != null && b != null)
            {
                return string.Compare(a.Label, b.Label);
            }

            return 0;
        }

        #endregion

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            for(int i = 0, guiGroupCount = guiGroup.Count; i < guiGroupCount; i++)
            {
                var gui = guiGroup[i];
                gui.OnCreateGUI(layout);
            }
        }
    }
}