using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.ServerEvents;
using SocialPoint.ServerSync;
using SocialPoint.Utils;

namespace SocialPoint.Purchase
{
    public class PurchaseInstaller : ServiceInstaller
    {
        public override void InstallBindings()
        {
            Container.Rebind<IGamePurchaseStore>().ToMethod<SocialPointPurchaseStore>(CreatePurchaseStore, SetupPurchaseStore);
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelPurchase>(CreateAdminPanel);
        }

        AdminPanelPurchase CreateAdminPanel()
        {
            return new AdminPanelPurchase(
                Container.Resolve<IStoreProductSource>(),
                Container.Resolve<IGamePurchaseStore>(),
                Container.Resolve<ICommandQueue>());
        }

        SocialPointPurchaseStore CreatePurchaseStore()
        {
            return new SocialPointPurchaseStore(
                Container.Resolve<NativeCallsHandler>());
        }

        void SetupPurchaseStore(SocialPointPurchaseStore store)
        {
            store.HttpClient = Container.Resolve<IHttpClient>();
            store.CommandQueue = Container.Resolve<ICommandQueue>();
            store.TrackEvent = Container.Resolve<IEventTracker>().TrackSystemEvent;
            store.LoginData = Container.Resolve<ILoginData>();
        }
    }
}
