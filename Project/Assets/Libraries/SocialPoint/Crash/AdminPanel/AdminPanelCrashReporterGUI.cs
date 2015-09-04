using UnityEngine.UI;
using SocialPoint.AdminPanel;

namespace SocialPoint.Crash
{
    public class AdminPanelCrashReporterGUI : AdminPanelGUI, AdminPanelConfigurer
    {
        public ICrashReporter CrashReporter;

        public BreadcrumbManager BreadcrumbManager;

        private Text _textAreaComponent;
        private bool _showOldBreadcrumbs;
        
        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Crash Reporter", this));
        }
        
        public void OnCreateGUI(AdminPanelLayout layout)
        {
            if(BreadcrumbManager != null)
            {
                layout.CreateLabel("Breadcrumbs");
                layout.CreateVerticalScrollLayout()
                    .CreateTextArea(BreadcrumbManager.CurrentBreadCrumb, out _textAreaComponent);
                layout.CreateButton("Refresh", () => { UpdateBreadcrumbContent(); });
                layout.CreateToggleButton("Last session breadcrumbs", _showOldBreadcrumbs, (value) => { 
                    _showOldBreadcrumbs = value; 
                    UpdateBreadcrumbContent();
                });
                layout.CreateMargin();
            }

            if(CrashReporter != null)
            {
                layout.CreateLabel("CrashReporter");

                CrashReporterBase crBase = CrashReporter as CrashReporterBase;
                if(crBase != null)
                {
                    layout.CreateOpenPanelButton("CrashReporterBase Options", new AdminPanelCrashReporterBaseGUI(crBase));
                }
                
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

                layout.CreateToggleButton("Error logs", CrashReporter.ErrorLogActive, (value) => { CrashReporter.ErrorLogActive = value; });
                layout.CreateToggleButton("Exceptions logs", CrashReporter.ExceptionLogActive, (value) => { CrashReporter.ExceptionLogActive = value; });
                layout.CreateButton("Clear unique exceptions", () => { 
                    layout.AdminPanel.Console.Print("Removed pending unique exceptions");
                    CrashReporter.ClearUniqueExceptions();
                });
                layout.CreateMargin(2);

                layout.CreateConfirmButton("Force crash", () => {
                    CrashReporter.ForceCrash();
                }, ButtonColor.Red);
            }
        }

        private void UpdateBreadcrumbContent()
        {
            if(_textAreaComponent != null && BreadcrumbManager != null)
            {
                _textAreaComponent.text = (_showOldBreadcrumbs)? 
                                            BreadcrumbManager.OldBreadCrumb:
                                            BreadcrumbManager.CurrentBreadCrumb;
            }
        }


        public class AdminPanelCrashReporterBaseGUI : AdminPanelGUI
        {
            private CrashReporterBase _crashReporter;

            public AdminPanelCrashReporterBaseGUI(CrashReporterBase crashReporter)
            {
                _crashReporter = crashReporter;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("CrashReporterBase");
                string crashReporterInfo = "Send Interval: " + _crashReporter.SendInterval + "s\n"
                    + "Pending Crashes: " + (_crashReporter.HasCrashLogs ? "Yes" : "No") + "\n"
                    + "Pending Exceptions: " + (_crashReporter.HasExceptionLogs ? "Yes" : "No") + "\n";
                layout.CreateTextArea(crashReporterInfo);
            }
        }
    }
}