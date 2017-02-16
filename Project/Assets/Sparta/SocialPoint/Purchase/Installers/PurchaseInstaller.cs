using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.ServerEvents;
using SocialPoint.ServerSync;
using SocialPoint.Utils;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Purchase
{
    public class PurchaseInstaller : ServiceInstaller
    {
        public override void InstallBindings()
        {
            Container.Rebind<IGamePurchaseStore>().ToMethod<SocialPointPurchaseStore>(CreatePurchaseStore, SetupPurchaseStore);

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelPurchase>(CreateAdminPanel);
            #endif
        }

        #if ADMIN_PANEL
        AdminPanelPurchase CreateAdminPanel()
        {
            return new AdminPanelPurchase(
                Container.Resolve<IStoreProductSource>(),
                Container.Resolve<IGamePurchaseStore>(),
                Container.Resolve<ICommandQueue>());
        }
        #endif

        SocialPointPurchaseStore CreatePurchaseStore()
        {
            return new SocialPointPurchaseStore(
                Container.Resolve<NativeCallsHandler>());
        }

        void SetupPurchaseStore(SocialPointPurchaseStore store)
        {
            store.HttpClient = Container.Resolve<IHttpClient>();
            store.CommandQueue = Container.Resolve<ICommandQueue>();
            store.LoginData = Container.Resolve<ILoginData>();
            var tracker = Container.Resolve<IEventTracker>();
            if(tracker != null)
            {
                store.TrackEvent = tracker.TrackSystemEvent;
            }
        }
    }
}
