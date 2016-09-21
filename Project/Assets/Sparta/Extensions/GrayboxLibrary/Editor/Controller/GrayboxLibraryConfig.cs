namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryConfig
    {
        public static string DB_CONFIG = "Server=GrayboxTool.c4rdfbnb9wen.eu-west-1.rds.amazonaws.com;Database=GrayboxTool;User ID=GrayboxTool;Password=SM9tyR8h21PRoVz;Pooling=true";

        public static string MAC_VOLUME_PATH = "/Volumes/3dshare/";

        public static string WIN_VOLUME_PATH = "//spserver/3dshare/";

#if UNITY_EDITOR_OSX
        public static string VOLUME_PATH = MAC_VOLUME_PATH;
#else
        public static string VOLUME_PATH = WIN_VOLUME_PATH;
#endif
        public static string ICONS_PATH = VOLUME_PATH + "TA/UnityGrayboxLibrary/tool_icons/";

#if UNITY_EDITOR_OSX
        public static string PKG_DEFFAULT_FOLDER = VOLUME_PATH + "TA/UnityGrayboxLibrary/Packages";
#else
        public static string PKG_DEFFAULT_FOLDER = (VOLUME_PATH + "TA/UnityGrayboxLibrary/Packages").Replace("/", "\\");
#endif
        public static string SMB_CONNECTION_URL = "//guest@"+ WIN_VOLUME_PATH.Substring(2);

        public static string CONTACT_URL = "https://mail.google.com/mail/?view=cm&fs=1&to=techart@socialpoint.es&su=Graybox Tool Contact";

    }
}