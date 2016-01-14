using System;
using System.Collections;
using Zenject;
using SocialPoint.Network;
using UnityEngine;
using SocialPoint.IO;
using SocialPoint.Base;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(HttpClientInstaller))]
public class HttpClientInstallerEditor: Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.HelpBox("To setup a editor http proxy, create a \"Project/.proxy\" file containing server and port. This file is ignored so it won't affect other developers.", MessageType.Info, true);
    }
}
#endif

public class HttpClientInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public bool PinSSLCertificate = true;
    };

    //TODO: add url to confluence with info on how to obtain it
    byte[] _SSLCertificate = {0x44, 0x63, 0x4D, 0x75, 0x77, 0x87, 0xD2, 0x55, 0x65, 0x49, 0x5E, 0x04, 0x2B, 0xF3,
        0xBB, 0x1B, 0x75, 0x5E, 0x07, 0x73, 0x70, 0xFD, 0x99, 0x23, 0x47, 0x6E, 0x4F, 0x33,
        0x25, 0xFB, 0x99, 0x23, 0x55, 0x53, 0x62, 0x71, 0x35, 0xF0, 0xD6, 0x0D, 0x66, 0x20,
        0x45, 0x71, 0x07, 0xF3, 0xBF, 0x51, 0x46, 0x7C, 0x6D, 0x7A};
    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
#if UNITY_EDITOR
        var proxyPath = FileUtils.Combine(Application.dataPath, "../.proxy");
        if(FileUtils.Exists(proxyPath))
        {
            var proxy = FileUtils.ReadAllText(proxyPath);
            DebugUtils.Log(string.Format("Using editor proxy '{0}'", proxy));
            Container.BindInstance("http_client_proxy", proxy);
        }
#endif
        if(Settings.PinSSLCertificate)
        {
            Container.BindInstance("pin_ssl_certificate", _SSLCertificate);
        }
        Container.Rebind<IHttpClient>().ToSingle<HttpClient>();
        Container.Bind<IDisposable>().ToLookup<IHttpClient>();
    }
}
