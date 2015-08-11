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
			var prefab = string.Format("GUI/{0}", name);
			var ctrl = UIViewControllerFactory.CreateFromResource(prefab);
			Container.Inject(ctrl);
			return ctrl;
		});

		var screens = GameObject.FindObjectOfType<ScreensController>();
		if(screens != null)
		{
			Container.Bind<ScreensController>().ToInstance(screens);
			if(Settings.FirstScreen != null)
			{
				var go = Settings.FirstScreen;
				if(Settings.FirstScreen.transform.IsPrefab())
				{
					go = Instantiate(go);
				}
				Container.InjectGameObject(go);
				screens.Push(go);
			}
		}
		
		var popups = GameObject.FindObjectOfType<PopupsController>();
		if(popups != null)
		{
			Container.Bind<PopupsController>().ToInstance(popups);
		}

    }
    
}