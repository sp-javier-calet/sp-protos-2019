using Zenject;
using System;

public class PurchaseInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
    };
    
    public SettingsData Settings;

	public override void InstallBindings()
	{
        if(Container.HasBinding<PurchaseStore>())
        {
            return;
        }
        Container.BindAllInterfacesToSingle<PurchaseStore>();
	}

}
