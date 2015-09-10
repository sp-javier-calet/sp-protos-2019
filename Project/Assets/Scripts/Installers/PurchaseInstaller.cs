using Zenject;
using System;

public class PurchaseInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
        if(Container.HasBinding<PurchaseStore>())
        {
            return;
        }
        Container.BindAllInterfacesToSingle<PurchaseStore>();
	}

}
