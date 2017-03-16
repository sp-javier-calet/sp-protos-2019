#if ADMIN_PANEL

using System;
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

    public sealed class AdminPanelGUIGroup : IAdminPanelManagedGUI
    {
        readonly List<IAdminPanelGUI> guiGroup;

        public AdminPanelGUIGroup()
        {
            guiGroup = new List<IAdminPanelGUI>();
        }

        public AdminPanelGUIGroup(IAdminPanelGUI gui) : this()
        {
            Add(gui);
        }

        /// <summary>
        /// Add a new gui and Sort IAdminPanelSortedGUI which are together in the list.
        /// Since they are added sequentially in the group, it sorts only the last elegible elements,
        /// until a non-sorted gui element is found.
        /// </summary>
        public void Add(IAdminPanelGUI gui)
        {
            guiGroup.Add(gui);

            for(var i = guiGroup.Count - 1; i > 0; --i)
            {
                var prev = guiGroup[i - 1];
                var curr = guiGroup[i];
                try
                {
                    if(Compare(prev, curr) > 0)
                    {
                        // Switch elements
                        guiGroup[i - 1] = curr;
                        guiGroup[i] = prev;
                    }
                }
                catch(Exception)
                {
                    // Element is not comparable.
                    return;
                }
            }
        }

        int Compare(IAdminPanelGUI x, IAdminPanelGUI y)
        {
            var a = x as IAdminPanelSortedGUI;
            var b = y as IAdminPanelSortedGUI;

            if(a == null || b == null)
            {
                throw new Exception();
            }

            return string.Compare(a.Label, b.Label);
        }

        public void OnOpened()
        {
            for(int i = 0, guiGroupCount = guiGroup.Count; i < guiGroupCount; i++)
            {
                var managed = guiGroup[i] as IAdminPanelManagedGUI;
                if(managed != null)
                {
                    managed.OnOpened();
                }
            }
        }

        public void OnClosed()
        {
            for(int i = 0, guiGroupCount = guiGroup.Count; i < guiGroupCount; i++)
            {
                var managed = guiGroup[i] as IAdminPanelManagedGUI;
                if(managed != null)
                {
                    managed.OnClosed();
                }
            }
        }

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

#endif
