using System;
using System.Collections.Generic;
using SocialPoint.Console;

namespace SocialPoint.AdminPanel
{
    public class AdminPanel
    {
        public Dictionary<string, IAdminPanelGUI> Categories { get; private set; }
        public AdminPanelConsole Console { get; private set; }

        public AdminPanel(List<IAdminPanelConfigurer> configurers)
        {
            Categories = new Dictionary<string, IAdminPanelGUI>();
            Console = new AdminPanelConsole();

            foreach(var config in configurers)
            {
                config.OnConfigure(this);
            }

            Console.OnConfigure(this);
        }
         
        public AdminPanelGUIGroup RegisterGUI(string category, IAdminPanelGUI gui)
        {
            var group = GetCategoryLayout(category);
            group.Add(gui);
            return group;
        }

        private AdminPanelGUIGroup GetCategoryLayout(string category)
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
