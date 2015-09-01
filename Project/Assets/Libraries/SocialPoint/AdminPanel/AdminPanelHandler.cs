using System;
using System.Collections.Generic;
using SocialPoint.Console;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelHandler
    {
        // TODO React to connection if already opened
        public static event Action <AdminPanelHandler> OnAdminPanelInit;

        internal static void InitializeHandler(AdminPanelHandler handler)
        {
            if(OnAdminPanelInit != null)
            {
                OnAdminPanelInit(handler);
            }
        }

        private Dictionary<string, AdminPanelGUI> _categories;
        private ConsoleApplication _consoleApplication;
        internal AdminPanelHandler(Dictionary<string, AdminPanelGUI> categories, ConsoleApplication consoleApplication)
        {
            _categories = categories;
            _consoleApplication = consoleApplication;
        }
         
        public void AddPanelGUI(string category, AdminPanelGUI panel)
        {
            AdminPanelGUIGroup group = GetCategoryLayout(category);
            group.Add(panel);
        }

        private AdminPanelGUIGroup GetCategoryLayout(string category)
        {
            AdminPanelGUI group;
            if(!_categories.TryGetValue(category, out group))
            {
                group = new AdminPanelGUIGroup();
                _categories.Add(category, group);
            }
            return (AdminPanelGUIGroup)group;
        }

        public void RegisterCommand(string commandName, string description, ConsoleCommandDelegate commandDelegate)
        {
            _consoleApplication
                .AddCommand(commandName)
                .WithDescription(description)
                .WithDelegate(commandDelegate);
        }

        public void RegisterCommand(string commandName, ConsoleCommand command)
        {
            _consoleApplication.AddCommand(commandName, command);
        }
    }
}
