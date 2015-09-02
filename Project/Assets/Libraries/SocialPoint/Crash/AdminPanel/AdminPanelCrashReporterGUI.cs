using SocialPoint.AdminPanel;

namespace SocialPoint.Crash
{
    public class AdminPanelCrashReporterGUI : AdminPanelGUI, AdminPanelConfigurer
    {
        public ICrashReporter CrashReporter;

        public BreadcrumbManager BreadcrumbManager;
        
        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Crash Reporter", this));
        }
        
        public void OnCreateGUI(AdminPanelLayout layout)
        {
            if(CrashReporter != null)
            {
                layout.CreateLabel("CrashReporter");
            
                layout.CreateToggleButton("Enabled", CrashReporter.IsEnabled, (value) => {
                    if(value)
                    {
                        CrashReporter.Enable();
                    }
                    else
                    {
                        CrashReporter.Disable();
                    }
                });
            
                layout.CreateButton("Force crash", () => {
                    CrashReporter.ForceCrash();
                    layout.AdminPanel.Console.Print("hola");
                });

                layout.CreateMargin();
            }

            if(BreadcrumbManager != null)
            {
                layout.CreateLabel("Breadcrumbs");
            }
        }
    }
}