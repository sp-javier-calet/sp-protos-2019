using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Purchase;
using SocialPoint.Network;
using SocialPoint.ServerSync;
using SocialPoint.Login;
using SocialPoint.ServerEvents;

public class PurchaseInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Rebind<IGamePurchaseStore>().ToSingleMethod<SocialPointPurchaseStore>(CreatePurchaseStore);
        Container.Bind<IStoreProductSource>().ToGetter<ConfigModel>((Config) => Config.Store);
        Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelPurchase>(CreateAdminPanel);
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
        var store = new SocialPointPurchaseStore(
            Container.Resolve<IHttpClient>(),
            Container.Resolve<ICommandQueue>());

        store.TrackEvent = Container.Resolve<IEventTracker>().TrackSystemEvent;
        var login = Container.Resolve<ILogin>();
        store.RequestSetup = login.SetupHttpRequest;
        store.GetUserId = () => login.UserId;

        var model = Container.Resolve<StoreModel>();
        model.Init(store);

        return store;
    }
}
