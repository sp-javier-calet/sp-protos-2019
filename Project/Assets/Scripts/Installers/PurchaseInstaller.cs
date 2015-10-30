using Zenject;
using System;

public class PurchaseInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
        Container.Rebind<PurchaseStore>().ToSingle<PurchaseStore>();
	}

}
