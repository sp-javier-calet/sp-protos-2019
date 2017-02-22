﻿namespace SocialPoint.TransparentBundles
{
    public class Config
    {
        /*FOR TESTING ONLY*/
        //Should be an image in a web server
        public static string MacVolumePath = "/Volumes/3dshare/";

        public static string WinVolumePath = "//spserver.spoint.es/3dshare/";

        #if UNITY_EDITOR_OSX
        public static string VolumePath = MacVolumePath;
        #else
        public static string VolumePath = WinVolumePath;
#endif
        public static string IconsPath = VolumePath + "TA/TransparentBundles/tool_icons/";

        public static string SmbConnectionUrl = "//guest@" + WinVolumePath.Substring(2);

        public static string ContactMail = "transparent.bundles@socialpoint.es";

        public static string ContactUrl = "https://mail.google.com/mail/?view=cm&fs=1&to="+ ContactMail + "&su=Transparent Bundles Contact";

        public static string HelpUrl = "https://sites.google.com/a/socialpoint.es/technical-art/07---tools-documentation/transparent-bundles";
    }
}