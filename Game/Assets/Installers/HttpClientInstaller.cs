using UnityEngine;
using System.Collections;
using Zenject;
using SocialPoint.Network;

public class HttpClientInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
		Container.Bind<IHttpClient>().ToSingle<CurlHttpClient>();
	}

}
