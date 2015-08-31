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

        private Dictionary<string, AdminPanelGUILayout> _categories;
        private ConsoleApplication _consoleApplication;
        internal AdminPanelHandler(Dictionary<string, AdminPanelGUILayout> categories, ConsoleApplication consoleApplication)
        {
            _categories = categories;
            _consoleApplication = consoleApplication;
        }

        public void AddPanelGUI(string category, AdminPanelGUI panel)
        {
            AddPanelGUI(category, panel, AdminPanelGUIOptions.None);
        }
         
        public void AddPanelGUI(string category, AdminPanelGUI panel, AdminPanelGUIOptions options)
        {
            AdminPanelGUILayout layout = GetCategoryLayout(category);
            layout.Add(panel);
        }

        private AdminPanelGUILayout GetCategoryLayout(string category)
        {
            AdminPanelGUILayout layout;
            if(!_categories.TryGetValue(category, out layout))
            {
                layout = new AdminPanelGUILayout();
                _categories.Add(category, layout);
            }
            return layout;
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

    public class AdminPanelGUILayout
    {
        private List<AdminPanelGUI> panels;
        
        public AdminPanelGUILayout()
        {
            panels = new List<AdminPanelGUI>();
        }

        public AdminPanelGUILayout(AdminPanelGUI panel) : this()
        {
            panels.Add(panel);
        }
        
        public void Add(AdminPanelGUI panel)
        {
            panels.Add(panel);
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            foreach(AdminPanelGUI panel in panels)
            {
                panel.OnCreateGUI(layout);
            }
        }
    }
}
