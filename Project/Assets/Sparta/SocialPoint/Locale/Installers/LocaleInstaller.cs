using System;
using SocialPoint.Locale;
using SocialPoint.AdminPanel;
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
}
