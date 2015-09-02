using Zenject;
using SocialPoint.Crash;

public class AdminPanelCrashReporter : AdminPanelCrashReporterGUI 
{
    [Inject]
    public ICrashReporter InjectCrashReporter
    {
        set
        {
            CrashReporter = value;
        }
    }

    [Inject]
    public BreadcrumbManager InjectBreadcrumbManager
    {
        set
        {
            BreadcrumbManager = value;
        }
    }
}
