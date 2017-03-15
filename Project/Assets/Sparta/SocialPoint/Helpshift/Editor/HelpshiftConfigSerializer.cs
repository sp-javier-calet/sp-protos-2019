﻿using HSMiniJSON;
using System.IO;
using System.Collections.Generic;
using SocialPoint.Dependency;
using UnityEngine;
using UnityEditor;

namespace SocialPoint.Helpshift
{
    /// <summary>
    /// Serialize Helpshift Installer data to json files.
    /// This is required by the Helpshift SDK to initialize their code from native code,
    /// even before Unity Engine is running.
    /// </summary>
    public static class HelpshiftConfigSerializer
    {
        const string HSPluginVersion = "2.6.1";

        const string HelpshiftAndroidConfigPath = "Plugins/Android/res/raw/";
        const string HelpshiftAndroidConfigFile = "helpshiftinstallconfig.json";

        const string HelpshiftIosConfigPath = "Plugins/iOS/";
        const string HelpshiftIosConfigFile = "HelpshiftInstallConfig.json";

        // Install configuration keys
        const string SdkTypeKey = "sdkType";
        const string PluginVersionKey = "pluginVersion";
        const string RuntimeVersionKey = "runtimeVersion";
        const string AndroidNotificationIconNameKey = "notificationIcon";
        const string AndroidNotificationLargeIconNameKey = "largeNotificationIcon";
        const string DisableErrorLoggingKey = "disableErrorLogging";

        // Install Id keys
        const string ApiKeyJsonKey = "__hs__apiKey";
        const string DomainNameJsonKey = "__hs__domainName";
        const string AppIdJsonKey = "__hs__appId";

        const string YesKey = "yes";
        const string NoKey = "no";
        const string UnitySdkTypeKey = "unity";

        // Use game-defined icon for push notification, as in sp_unity_notifications plugin.
        // Notice that helpshift pushes does not use default notifications icons. If game does not define a custom icon, a white quad will be shown.
        const string NotificationIconName = "notify_icon_small.png";
        const string LargeNotificationIconName = "notify_icon_large.png";

        [InitializeOnLoadMethod]
        public static void Serialize()
        {
            var hs = InstallerAssetsManager.Open<HelpshiftInstaller>();
            if(hs == null)
            {
                return;
            }

            // Common config
            var installDic = new Dictionary<string, object>();
            installDic.Add(ApiKeyJsonKey, hs.Settings.ApiKey);
            installDic.Add(DomainNameJsonKey, hs.Settings.DomainName);

            installDic.Add(SdkTypeKey, UnitySdkTypeKey);
            installDic.Add(PluginVersionKey, HSPluginVersion);
            installDic.Add(RuntimeVersionKey, Application.unityVersion);

            installDic.Add(AndroidNotificationIconNameKey, NotificationIconName);
            installDic.Add(AndroidNotificationLargeIconNameKey, LargeNotificationIconName);
            installDic.Add(DisableErrorLoggingKey, YesKey);

            WriteInstallConfigWithId(installDic, hs.Settings.AndroidAppId, HelpshiftAndroidConfigPath, HelpshiftAndroidConfigFile);
            WriteInstallConfigWithId(installDic, hs.Settings.IosAppId, HelpshiftIosConfigPath, HelpshiftIosConfigFile);
        }

        static void WriteInstallConfigWithId(Dictionary<string, object> installDic, string appId, string filePath, string fileName)
        {
            installDic[AppIdJsonKey] = appId;
            var installJson = Json.Serialize(installDic);
            string iosPath = Path.Combine(Application.dataPath, filePath);
            Directory.CreateDirectory(iosPath);
            File.WriteAllText(iosPath + fileName, installJson);
        }
    }
}