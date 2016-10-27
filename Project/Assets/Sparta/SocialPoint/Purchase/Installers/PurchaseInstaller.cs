using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.Purchase;
using SocialPoint.ServerEvents;
using SocialPoint.ServerSync;
using SocialPoint.Utils;

public class PurchaseInstaller : Installer
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
            Container.Resolve<IHttpClient>(),
            Container.Resolve<ICommandQueue>(),
            Container.Resolve<NativeCallsHandler>());
    }

    void SetupPurchaseStore(SocialPointPurchaseStore store)
    {
        store.TrackEvent = Container.Resolve<IEventTracker>().TrackSystemEvent;
        store.LoginData = Container.Resolve<ILoginData>();
    }
}
