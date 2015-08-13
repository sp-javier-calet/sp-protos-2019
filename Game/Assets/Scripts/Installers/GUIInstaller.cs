using System;
using Zenject;
using UnityEngine;
using SocialPoint.GUI;
using SocialPoint.Base;

public class GUIInstaller : MonoInstaller
{
	const string UIViewControllerSuffix = "Controller";

	[Serializable]
	public class SettingsData
	{
        public GameObject FirstScreen;
        public float PopupFadeSpeed = PopupsController.DefaultFadeSpeed;
	};

	public SettingsData Settings;
    
    public override void InstallBindings()
    {
		UIViewController.Factory.Define((Type type) => {
			var name = type.ToString();
			if(name.EndsWith(UIViewControllerSuffix))
			{
				name = name.Substring(0, name.Length-UIViewControllerSuffix.Length);
			}
			return string.Format("GUI/{0}", name);
		});

        UIViewController.Factory.Filter += (ctrl, t) => {
            if(ctrl != null)
            {
                Container.Inject(ctrl);
            }
        };

        Container.BindInstance("popup_fade_speed", Settings.PopupFadeSpeed);

        var popups = GameObject.FindObjectOfType<PopupsController>();
        if(popups != null)
        {
            Container.Bind<PopupsController>().ToInstance(popups);
        }
        var firstScreen = Settings.FirstScreen;
        if(firstScreen != null)
        {
            if(firstScreen.transform.IsPrefab())
            {
                firstScreen = Instantiate(firstScreen);
            }
            Container.BindInstance("first_screen", firstScreen);
        }
		var screens = GameObject.FindObjectOfType<ScreensController>();
		if(screens != null)
        {
            Container.Bind<ScreensController>().ToInstance(screens);
        }

        if(firstScreen != null)
        {
            Container.InjectGameObject(firstScreen);
        }
        if(popups != null)
        {
            Container.Inject(popups);
        }
        if(screens != null)
        {
            Container.Inject(screens);
        }

		
    }
    
}