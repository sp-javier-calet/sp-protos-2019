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
                });
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
    }
}