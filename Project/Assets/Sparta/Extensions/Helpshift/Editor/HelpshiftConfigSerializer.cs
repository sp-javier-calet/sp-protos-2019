using HSMiniJSON;
using System.IO;
using System.Collections.Generic;
using SocialPoint.Dependency;
using UnityEngine;
using UnityEditor;

namespace SocialPoint.Extension.Helpshift
{
    /// <summary>
    /// Serialize Helpshift Installer data to json files.
    /// This is required by the Helpshift SDK to initialize their code from native code,
    /// even before Unity Engine is running.
    /// </summary>
    public static class HelpshiftConfigSerializer
    {
        const string HelpshiftAndroidConfigPath = "Plugins/Android/res/raw/";
        const string HelpshiftAndroidConfigFile = "helpshiftinstallconfig.json";

        const string HelpshiftIosConfigPath = "Plugins/iOS/";
        const string HelpshiftIosConfigFile = "HelpshiftInstallConfig.json";

        const string ApiKeyJsonKey = "__hs__apiKey";
        const string DomainNameJsonKey = "__hs__domainName";
        const string AppIdJsonKey = "__hs__appId";

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