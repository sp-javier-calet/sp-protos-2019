using UnityEngine.UI;
using System.Text;
using SocialPoint.AdminPanel;

namespace SocialPoint.Crash
{
    public class AdminPanelCrashReporter : IAdminPanelGUI, IAdminPanelConfigurer
    {
        private ICrashReporter _reporter;
        private BreadcrumbManager _breadcrumbs;
        private Text _textAreaComponent;
        private bool _showOldBreadcrumbs;

        public AdminPanelCrashReporter(ICrashReporter reporter, BreadcrumbManager breadcrumbs)
        {
            _reporter = reporter;
            _breadcrumbs = breadcrumbs;
        }
        
        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Crash Reporter", this));
        }
        
        public void OnCreateGUI(AdminPanelLayout layout)
        {
            if(_breadcrumbs != null)
            {
                layout.CreateLabel("Breadcrumbs");
                _textAreaComponent = layout.CreateVerticalScrollLayout()
                    .CreateTextArea(_breadcrumbs.CurrentBreadcrumb);
                layout.CreateButton("Refresh", () => { UpdateBreadcrumbContent(); });
                layout.CreateToggleButton("Last session breadcrumbs", _showOldBreadcrumbs, (value) => { 
                    _showOldBreadcrumbs = value; 
                    UpdateBreadcrumbContent();
                });
                layout.CreateMargin();
            }

            if(_reporter != null)
            {
                layout.CreateLabel("CrashReporter");

                var crBase = _reporter as BaseCrashReporter;
                if(crBase != null)
                {
                    layout.CreateOpenPanelButton("CrashReporterBase Options", new AdminPanelCrashReporterBaseGUI(crBase));
                }
                
                layout.CreateToggleButton("Enabled", _reporter.WasEnabled, (value) => {
                    if(value)
                    {
                        _reporter.Enable();
                    }
                    else
                    {
                        _reporter.Disable();
                    }
                });

                layout.CreateToggleButton("Error logs", _reporter.ErrorLogActive, (value) => { _reporter.ErrorLogActive = value; });
                layout.CreateToggleButton("Exceptions logs", _reporter.ExceptionLogActive, (value) => { _reporter.ExceptionLogActive = value; });
                layout.CreateButton("Clear unique exceptions", () => { 
                    layout.AdminPanel.Console.Print("Removed pending unique exceptions");
                    _reporter.ClearUniqueExceptions();
                });
                layout.CreateMargin(2);

                layout.CreateConfirmButton("Force crash", ButtonColor.Red, () => {
                    _reporter.ForceCrash();
                });
            }
        }

        private void UpdateBreadcrumbContent()
        {
            if(_textAreaComponent != null && _breadcrumbs != null)
            {
                _textAreaComponent.text = (_showOldBreadcrumbs)? 
                    _breadcrumbs.OldBreadcrumb:
                        _breadcrumbs.CurrentBreadcrumb;
            }
        }


        public class AdminPanelCrashReporterBaseGUI : IAdminPanelGUI
        {
            private BaseCrashReporter _crashReporter;

            public AdminPanelCrashReporterBaseGUI(BaseCrashReporter crashReporter)
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
