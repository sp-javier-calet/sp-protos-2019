using System;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.ServerEvents;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.CrossPromotion
{
    public class CrossPromotionInstaller : SubInstaller
    {
        [Serializable]
        public class SettingsData
        {
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Listen<CrossPromotionManager>().Then(SetupManager);

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelCrossPromotion>(CreateAdminPanel);
            #endif
        }

        void SetupManager(CrossPromotionManager mng)
        {
            var eventTracker = Container.Resolve<IEventTracker>();
            mng.TrackSystemEvent = eventTracker.TrackSystemEvent;
            mng.TrackUrgentSystemEvent = eventTracker.TrackUrgentSystemEvent;
            mng.AppEvents = Container.Resolve<IAppEvents>();
        }

        #if ADMIN_PANEL
        AdminPanelCrossPromotion CreateAdminPanel()
        {
            return new AdminPanelCrossPromotion(Container.Resolve<CrossPromotionManager>());
        }
        #endif
    }
}
