using Zenject;
using System;
using SocialPoint.Social;
using SocialPoint.AdminPanel;

public class GoogleInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public bool UseEmpty = false;
    };

    public SettingsData Settings = new SettingsData();


    public override void InstallBindings()
    {
        if(Settings.UseEmpty)
        {
            Container.Rebind<IGoogle>().ToSingle<EmptyGoogle>();
        }
        else
        {
            Container.Rebind<IGoogle>().ToSingleGameObject<UnityGoogle>();
        }

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelGoogle>();
    }
}
