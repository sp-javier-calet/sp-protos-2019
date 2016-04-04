using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Purchase;

public class PurchaseInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Rebind<IGamePurchaseStore>().ToSingle<PurchaseStore>();
        Container.Bind<IStoreProductSource>().ToGetter<ConfigModel>((Config) => Config.Store);
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelPurchase>();
    }

}
