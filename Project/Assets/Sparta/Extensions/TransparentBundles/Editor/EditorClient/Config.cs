using UnityEngine;
using System;

namespace SocialPoint.TransparentBundles
{
    public static class Config
    {
        public static string MacVolumePath = "/Users/" + Environment.UserName + "/mount/3dshare/";

        public static string WinVolumePath = "//spserver.spoint.es/3dshare/";

        #if UNITY_EDITOR_OSX
        public static string VolumePath = MacVolumePath;
        #else
        public static string VolumePath = WinVolumePath;
        #endif
        public static string IconsPath = VolumePath + "TA/TransparentBundles/tool_icons/";

        public static string SmbConnectionUrl = "//guest@" + WinVolumePath.Substring(2);

        public static string ContactMail = "transparent.bundles@socialpoint.es";

        public static string ContactUrl = "https://mail.google.com/mail/?view=cm&fs=1&to=" + ContactMail + "&su=Transparent Bundles Contact";

        public static string HelpUrl = "https://sites.google.com/a/socialpoint.es/technical-art/07---tools-documentation/transparent-bundles";

        public const string UpdateImageName = "update.png";
        public const string RemoveImageName = "remove.png";
        public const string InBuildImageName = "in_build.png";
        public const string OutBuildImageName = "out_build.png";
        public const string UpdateQueuedImageName = "update_queued.png";
        public const string RemoveQueuedImageName = "remove_queued.png";
        public const string InBuildQueuedImageName = "in_build_queued.png";
        public const string OutBuildQueuedImageName = "out_build_queued.png";
        public const string ServerDbImageName = "server_db.png";
        public const string WarningImageName = "warning.png";
        public const string ErrorImageName = "error.png";
        public const string InServerImageName = "in_server.png";
        public const string HelpImageName = "help.png";
        public const string SleepImageName = "sleep.png";
    }
}
