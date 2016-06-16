using System;
using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using UnityEngine.UI;

namespace SocialPoint.Crash
{
    public class AdminPanelCrashReporterException : Exception
    {
        public AdminPanelCrashReporterException() :
            base("This is a forced exception.")
        {
        }
    }

    public class AdminPanelCrashReporter : IAdminPanelGUI, IAdminPanelConfigurer
    {
        ICrashReporter _reporter;
        readonly IBreadcrumbManager _breadcrumbs;
        Text _textAreaComponent;
        bool _showOldBreadcrumbs;

        public AdminPanelCrashReporter(ICrashReporter reporter, IBreadcrumbManager breadcrumbs)
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
                layout.CreateButton("Refresh", UpdateBreadcrumbContent);
                layout.CreateToggleButton("Last session breadcrumbs", _showOldBreadcrumbs, value => {
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

                layout.CreateToggleButton("Enabled", _reporter.IsEnabled, value => {
                    if(value)
                    {
                        _reporter.Enable();
                    }
                    else
                    {
                        _reporter.Disable();
                    }
                });

                layout.CreateToggleButton("Error logs", _reporter.ErrorLogActive, value => {
                    _reporter.ErrorLogActive = value;
                });
                layout.CreateToggleButton("Exceptions logs", _reporter.ExceptionLogActive, value => {
                    _reporter.ExceptionLogActive = value;
                });
                layout.CreateButton("Clear unique exceptions", () => {
                    layout.AdminPanel.Console.Print("Removed pending unique exceptions");
                    _reporter.ClearUniqueExceptions();
                });
                layout.CreateMargin(2);

                layout.CreateConfirmButton("Force Exception", ButtonColor.Red, () => {
                    throw new AdminPanelCrashReporterException();
                });
                layout.CreateConfirmButton("Force NullReferenceException", ButtonColor.Red, () => {
                    #pragma warning disable 1720
                    ((string)null).Trim();
                    #pragma warning restore 1720
                });
                layout.CreateConfirmButton("Force Handled Exception", ButtonColor.Red, () => {
                    try
                    {
                        throw new AdminPanelCrashReporterException();
                    }
                    catch(Exception e)
                    {
                        _reporter.ReportHandledException(e);
                    }
                });
                layout.CreateMargin(2);

                layout.CreateConfirmButton("Force crash", ButtonColor.Red, _reporter.ForceCrash);
            }
        }

        void UpdateBreadcrumbContent()
        {
            if(_textAreaComponent != null && _breadcrumbs != null)
            {
                _textAreaComponent.text = (_showOldBreadcrumbs) ?
                    _breadcrumbs.OldBreadcrumb :
                        _breadcrumbs.CurrentBreadcrumb;
            }
        }


        public class AdminPanelCrashReporterBaseGUI : IAdminPanelGUI
        {
            BaseCrashReporter _crashReporter;

            public AdminPanelCrashReporterBaseGUI(BaseCrashReporter crashReporter)
            {
                _crashReporter = crashReporter;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("CrashReporterBase");

                var crashReporterInfo = StringUtils.StartBuilder();
                crashReporterInfo.Append("Send Interval: ").Append(_crashReporter.SendInterval.ToString()).AppendLine("s")
                                 .Append("Pending Crashes: ").AppendLine(_crashReporter.HasCrashLogs ? "Yes" : "No")
                                 .Append("Pending Exceptions: ").AppendLine(_crashReporter.HasExceptionLogs ? "Yes" : "No");
                layout.CreateTextArea(StringUtils.FinishBuilder(crashReporterInfo));
            }
        }
    }
}
