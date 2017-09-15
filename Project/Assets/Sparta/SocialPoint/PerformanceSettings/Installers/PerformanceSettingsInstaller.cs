using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Attributes;

namespace SocialPoint.PerformanceSettings
{
    public class PerformanceSettingsInstaller : Installer, IInitializable
    {
        public override void InstallBindings()
        {
            Container.Bind<IInitializable>().ToInstance(this);
            Container.Rebind<PerformanceSettingsManager>().ToMethod<PerformanceSettingsManager>(CreatePerformanceSettingsManager);
        }

        PerformanceSettingsManager CreatePerformanceSettingsManager()
        {
            return new PerformanceSettingsManager(Container.Resolve<ILogin>(), Container.Resolve<IAttrStorage>("persistent"));
        }

        public void Initialize()
        {
            var performanceSettingsMgr = Container.Resolve<PerformanceSettingsManager>();
            performanceSettingsMgr.ExtraApplier = Container.Resolve<PerformanceSettingsManager.IExtraPerformanceSettingsApplier>();
        }
    }
}
