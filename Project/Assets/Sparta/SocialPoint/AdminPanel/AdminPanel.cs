using System;
using System.Collections.Generic;
using SocialPoint.Console;

namespace SocialPoint.AdminPanel
{
    public sealed class AdminPanel
    {
        public Dictionary<string, IAdminPanelGUI> Categories { get; private set; }

        public AdminPanelConsole Console { get; private set; }

        public event Action ChangedVisibility;

        public bool Visible { get; private set; }

        List<IAdminPanelConfigurer> _configurers = new List<IAdminPanelConfigurer>();

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
