using System;
using SocialPoint.Locale;
using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.Utils;

public class LocaleInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public bool EditorDebug = true;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.Rebind<Localization>().ToMethod<Localization>(CreateLocalization);
         
        Container.Rebind<LocalizeAttributeConfiguration>().ToMethod<LocalizeAttributeConfiguration>(CreateLocalizeAttributeConfiguration);

        Container.Listen<ILocalizationManager>().WhenResolved(SetupLocalizationManager);

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelLocale>(CreateAdminPanel);
    }

    LocalizeAttributeConfiguration CreateLocalizeAttributeConfiguration()
    {
        return new LocalizeAttributeConfiguration(
            Container.Resolve<Localization>(),
            Container.ResolveList<IMemberAttributeObserver<LocalizeAttribute>>());
    }

    AdminPanelLocale CreateAdminPanel()
    {
        return new AdminPanelLocale(
            Container.Resolve<ILocalizationManager>());
    }

    Localization CreateLocalization()
    {
        var locale = new Localization();
#if UNITY_EDITOR
        locale.Debug = Settings.EditorDebug;
#endif
        return locale;
    }

    void SetupLocalizationManager(ILocalizationManager mng)
    {
        var manager = mng as LocalizationManager;
        if(manager != null)
        {
            manager.AppEvents = Container.Resolve<IAppEvents>();
        }
    }
}
