using Zenject;
using System;
using UnityEngine;
using SocialPoint.AdminPanel;
using SocialPoint.Purchase;

public class PurchaseInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Rebind<IGamePurchaseStore>().ToSingle<PurchaseStore>();

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelPurchase>();
    }

}
