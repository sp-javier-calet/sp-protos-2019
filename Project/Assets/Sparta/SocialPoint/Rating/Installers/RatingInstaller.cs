using System;
using SocialPoint.Dependency;
using SocialPoint.Alert;
using SocialPoint.Hardware;
using SocialPoint.Attributes;
using SocialPoint.AppEvents;
using SocialPoint.Utils;
using SocialPoint.Login;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Rating
{
    public class RatingInstaller : ServiceInstaller, IInitializable
    {
        [Serializable]
        public class SettingsData
        {
            public int UsesUntilPrompt = AppRater.DefaultUsesUntilPrompt;
            public int EventsUntilPrompt = AppRater.DefaultEventsUntilPrompt;
            public long DaysUntilPrompt = AppRater.DefaultDaysUntilPrompt;
            public long DaysBeforeReminding = AppRater.DefaultDaysBeforeReminding;
            public int UserLevelUntilPrompt = AppRater.DefaultUserLevelUntilPrompt;
            public int MaxPromptsPerDay = AppRater.DefaultMaxPromptsPerDay;
            public bool PromptAfterBackground = true;
            public bool NativeRateDialog = false;
        }

        public SettingsData Settings = new SettingsData();


        public override void InstallBindings()
        {
            Container.Bind<IInitializable>().ToInstance(this);
            Container.Rebind<AppRater>().ToMethod<AppRater>(CreateAppRater, SetupAppRater);
            Container.Rebind<IAppRater>().ToLookup<AppRater>();
            Container.Bind<IDisposable>().ToLookup<AppRater>();

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelAppRater>(CreateAdminPanel);
            #endif
        }

        #if ADMIN_PANEL
        AdminPanelAppRater CreateAdminPanel()
        {
            return new AdminPanelAppRater(
                Container.Resolve<IAppRater>());
        }
        #endif

        AppRater CreateAppRater()
        {
            IAppEvents appEvents = null;
            if(Settings.PromptAfterBackground)
            {
                appEvents = Container.Resolve<IAppEvents>();
            }

            return new AppRater(
                Container.Resolve<IDeviceInfo>(),
                Container.Resolve<IAttrStorage>("volatile"),
                appEvents);
        }

        void SetupAppRater(AppRater rater)
        {
            rater.GUI = new DefaultAppRaterGUI(
                Container.Resolve<IAlertView>(),
                Container.Resolve<INativeUtils>(),
                Settings.NativeRateDialog);
            rater.UsesUntilPrompt = Settings.UsesUntilPrompt;
            rater.EventsUntilPrompt = Settings.EventsUntilPrompt;
            rater.DaysUntilPrompt = Settings.DaysUntilPrompt;
            rater.DaysBeforeReminding = Settings.DaysBeforeReminding;
            rater.UserLevelUntilPrompt = Settings.UserLevelUntilPrompt;
            rater.MaxPromptsPerDay = Settings.MaxPromptsPerDay;
            rater.Init();
        }

        public void Initialize()
        {
            Container.Resolve<IAppRater>();
        }
    }
}