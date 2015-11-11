using System;
using Zenject;
using UnityEngine;
using SocialPoint.GUIControl;
using SocialPoint.Base;

public class GUIInstaller : MonoInstaller
{
	const string UIViewControllerSuffix = "Controller";

	[Serializable]
	public class SettingsData
	{
        public float PopupFadeSpeed = PopupsController.DefaultFadeSpeed;
	};

	public SettingsData Settings;
    
    public override void InstallBindings()
    {
		UIViewController.Factory.Define((type) => {
            var name = type.Name;
			if(name.EndsWith(UIViewControllerSuffix))
			{
				name = name.Substring(0, name.Length-UIViewControllerSuffix.Length);
			}
			return string.Format("{0}", name);
		});

        UIViewController.AwakeFilter += (ctrl) => {
            Container.Inject(ctrl);
        };

        Container.BindInstance("popup_fade_speed", Settings.PopupFadeSpeed);

        var popups = GameObject.FindObjectOfType<PopupsController>();
        if(popups != null)
        {
            Container.Rebind<PopupsController>().ToSingleInstance(popups);
        }
		var screens = GameObject.FindObjectOfType<ScreensController>();
		if(screens != null)
        {
            Container.Rebind<ScreensController>().ToSingleInstance(screens);
        }

    }


}