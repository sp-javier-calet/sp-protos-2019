using SocialPoint.AppEvents;
using SocialPoint.Base;
using SocialPoint.Dependency;
using UnityEditor;
using UnityEngine;

public class ImpersonateEditor : EditorWindow
{

    const string PlayerPrefSourceApplicationKey = "SourceApplicationKey";
    string _url = string.Empty;

    [MenuItem("Sparta/Impersonate", false, 1011)]
    public static void OpenImpersonateSettings()
    {
        EditorWindow.GetWindow(typeof(ImpersonateEditor), false, "Impersonate", true);
    }

    void OnEnable()
    {        
        _url = PlayerPrefs.GetString(PlayerPrefSourceApplicationKey);
        EditorPrefs.SetString(PlayerPrefSourceApplicationKey, _url);
    }

    void StorePlayerPrefs()
    {
        Log.e("STORE AT: " + PlayerPrefSourceApplicationKey + " / " + _url);
        PlayerPrefs.SetString(PlayerPrefSourceApplicationKey,_url);
    }

    public void OnGUI()
    {
        EditorGUILayout.LabelField("Impersonate settings", EditorStyles.boldLabel);

        string url = EditorGUILayout.TextField("Impersonate URL", _url);

        if (GUILayout.Button("Store URL", EditorStyles.miniButton))
        {
            StorePlayerPrefs();
        }

        if (GUILayout.Button("Store and reset game", EditorStyles.miniButton))
        {
            if(Application.isPlaying)
            {
                var appEvents = Services.Instance.Resolve<SocialPointAppEvents>().GetAppEvents() as UnityAppEvents;
                appEvents.LoadAppSource(_url);
            }
        }

        if(url != _url)
        {
            _url = url;
        }
    }
}
