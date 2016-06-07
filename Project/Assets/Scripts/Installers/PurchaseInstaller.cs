using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Purchase;
using SocialPoint.Network;
using SocialPoint.ServerSync;
using SocialPoint.Login;
using SocialPoint.ServerEvents;

public class PurchaseInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Rebind<IGamePurchaseStore>().ToMethod<SocialPointPurchaseStore>(CreatePurchaseStore, SetupPurchaseStore);
        Container.Bind<IStoreProductSource>().ToGetter<ConfigModel>((Config) => Config.Store);
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
            Container.Resolve<ICommandQueue>());
    }

    void SetupPurchaseStore(SocialPointPurchaseStore store)
    {
        store.TrackEvent = Container.Resolve<IEventTracker>().TrackSystemEvent;
        var login = Container.Resolve<ILogin>();
        store.RequestSetup = login.SetupHttpRequest;
        store.GetUserId = () => login.UserId;

        var model = Container.Resolve<StoreModel>();
        model.PurchaseStore = store;
    }
}
