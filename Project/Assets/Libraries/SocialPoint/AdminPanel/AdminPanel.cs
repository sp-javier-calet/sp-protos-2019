using System;
using System.Collections.Generic;
using SocialPoint.Console;

namespace SocialPoint.AdminPanel
{
    public class AdminPanel
    {
        public Dictionary<string, AdminPanelGUI> Categories { get; private set; }
        public AdminPanelConsole Console { get; private set; }

        public AdminPanel()
        {
            Categories = new Dictionary<string, AdminPanelGUI>();
            Console = new AdminPanelConsole();
        }
         
        public void AddPanelGUI(string category, AdminPanelGUI panel)
        {
            AdminPanelGUIGroup group = GetCategoryLayout(category);
            group.Add(panel);
            panel.AdminPanel = this;
        }

        private AdminPanelGUIGroup GetCategoryLayout(string category)
        {
            AdminPanelGUI group;
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
