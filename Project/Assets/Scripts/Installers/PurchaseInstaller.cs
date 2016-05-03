using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Purchase;
using SocialPoint.Network;
using SocialPoint.ServerSync;

public class PurchaseInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Rebind<IGamePurchaseStore>().ToSingleMethod<PurchaseStore>(CreatePurchaseStore);
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

    PurchaseStore CreatePurchaseStore()
    {
        return new PurchaseStore(
            Container.Resolve<IHttpClient>(),
            Container.Resolve<ICommandQueue>(),
            Container.Resolve<StoreModel>());
    }
}
