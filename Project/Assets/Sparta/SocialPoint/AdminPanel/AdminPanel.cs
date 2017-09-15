#if ADMIN_PANEL

using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Console;

namespace SocialPoint.AdminPanel
{
    public sealed class AdminPanel
    {
        /// <summary>
        /// Static method to ask if AdminPanel is available
        /// </summary>
        public const bool IsAvailable
        #if (ADMIN_PANEL && !NO_ADMIN_PANEL) || UNITY_EDITOR
        = true;
        #else
        = false;
        #endif        

        public Dictionary<string, IAdminPanelGUI> Categories { get; private set; }

        public AdminPanelConsole Console { get; private set; }

        public event Action ChangedVisibility;

        public bool Visible { get; private set; }

        List<IAdminPanelConfigurer> _configurers = new List<IAdminPanelConfigurer>();

        string _defaultCategory;

        public string DefaultCategory
        { 
            private get
            {
                return _defaultCategory;
            }
            set
            {
                if(!string.IsNullOrEmpty(_defaultCategory))
                {
                    Log.w("AdminPanel", string.Format("New default category '{0}' overrides the current '{1}'", value, _defaultCategory));
                }
                _defaultCategory = value;
            }
        }

        public IAdminPanelGUI DefaultPanel
        {
            get
            {
                if(string.IsNullOrEmpty(_defaultCategory))
                {
                    return null;
                }

                return GetCategoryLayout(_defaultCategory);
            }
        }

        public AdminPanel(List<IAdminPanelConfigurer> configurers)
        {
            Categories = new Dictionary<string, IAdminPanelGUI>();
            Console = new AdminPanelConsole();
            RegisterConfigurers(configurers);
            Console.OnConfigure(this);
        }

        public void OnAppearing()
        {
            Visible = true;
            if(ChangedVisibility != null)
            {
                ChangedVisibility();
            }
        }

        public void OnDisappeared()
        {
            Visible = false;
            if(ChangedVisibility != null)
            {
                ChangedVisibility();
            }
        }

        public void RegisterConfigurers(List<IAdminPanelConfigurer> configurers)
        {
            for(int i = 0, configurersCount = configurers.Count; i < configurersCount; i++)
            {
                var config = configurers[i];
                RegisterConfigurer(config);
            }
        }

        public void RegisterConfigurer(IAdminPanelConfigurer config)
        {
            if(!_configurers.Contains(config))
            {
                config.OnConfigure(this);
                _configurers.Add(config);
            }
        }

        public AdminPanelGUIGroup RegisterGUI(string category, IAdminPanelGUI gui)
        {
            var group = GetCategoryLayout(category);
            group.Add(gui);
            return group;
        }

        AdminPanelGUIGroup GetCategoryLayout(string category)
        {
            IAdminPanelGUI group;
            if(!Categories.TryGetValue(category, out group))
            {
                group = new AdminPanelGUIGroup();
                Categories.Add(category, group);
            }
            return (AdminPanelGUIGroup)group;
        }

        public void RegisterCommand(string commandName, string description, ConsoleCommandDelegate commandDelegate)
        {
            Console.Application.AddCommand(commandName)
                               .WithDescription(description)
                               .WithDelegate(commandDelegate);
        }

        public void RegisterCommand(string commandName, ConsoleCommand command)
        {
            Console.Application.AddCommand(commandName, command);
        }
    }
}

#endif
