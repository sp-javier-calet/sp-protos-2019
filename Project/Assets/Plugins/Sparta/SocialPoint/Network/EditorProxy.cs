#if UNITY_EDITOR
using UnityEngine;
#endif
using SocialPoint.IO;
using SocialPoint.Base;

namespace SocialPoint.Network
{
    public static class EditorProxy
    {
        const string ProxyFilePath = "../.proxy";

        /// <summary>
        /// Gets the proxy configured by the Sparta tools
        /// Editor only.
        /// </summary>
        /// <returns>proxy string. Null if proxy is not defined.</returns>
        public static string GetProxy()
        {
            string proxy = null;

#if UNITY_EDITOR
            var proxyPath = FileUtils.Combine(Application.dataPath, ProxyFilePath);
            if(FileUtils.ExistsFile(proxyPath))
            {
                proxy = FileUtils.ReadAllText(proxyPath).Trim();
                Log.i(string.Format("Using editor proxy '{0}'", proxy));
            }
#endif

            return proxy;
        }
    }
}
