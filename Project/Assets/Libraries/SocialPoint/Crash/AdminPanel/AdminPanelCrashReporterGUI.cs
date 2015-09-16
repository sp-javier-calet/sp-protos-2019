using UnityEngine.UI;
using System.Text;
using SocialPoint.AdminPanel;

namespace SocialPoint.Crash
{
    public class AdminPanelCrashReporterGUI : IAdminPanelGUI, IAdminPanelConfigurer
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
                    .CreateTextArea(BreadcrumbManager.CurrentBreadcrumb, out _textAreaComponent);
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
                                            BreadcrumbManager.OldBreadcrumb:
                                            BreadcrumbManager.CurrentBreadcrumb;
            }
        }


        public class AdminPanelCrashReporterBaseGUI : IAdminPanelGUI
        {
            private CrashReporterBase _crashReporter;

            public AdminPanelCrashReporterBaseGUI(CrashReporterBase crashReporter)
            {
                _crashReporter = crashReporter;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("CrashReporterBase");

                StringBuilder crashReporterInfo = new StringBuilder();
                crashReporterInfo.Append("Send Interval: ").Append(_crashReporter.SendInterval.ToString()).AppendLine("s")
                                 .Append("Pending Crashes: ").AppendLine(_crashReporter.HasCrashLogs ? "Yes" : "No")
                                 .Append("Pending Exceptions: ").AppendLine(_crashReporter.HasExceptionLogs ? "Yes" : "No");
                layout.CreateTextArea(crashReporterInfo.ToString());
            }
        }
    }
}