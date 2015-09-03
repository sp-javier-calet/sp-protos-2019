using System;
using Zenject;
using SocialPoint.Alert;

public class AlertViewInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public bool UseNativeAlert = false;
        public string Prefab;
    }

    public SettingsData Settings;

    public override void InstallBindings()
    {
        if(Settings.UseNativeAlert)
        {
            Container.Bind<IAlertView>().ToSingle<AlertView>();
        }
        else
        {
            var unityAlertView = new UnityAlertView(Settings.Prefab != string.Empty ? Settings.Prefab : UnityAlertView.DefaultPrefab);
            Container.Bind<IAlertView>().ToInstance(unityAlertView);
        }
    }
}


