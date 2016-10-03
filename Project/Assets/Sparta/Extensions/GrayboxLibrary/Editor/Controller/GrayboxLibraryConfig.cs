namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryConfig
    {
        public static string DbConfig = "Server=GrayboxTool.c4rdfbnb9wen.eu-west-1.rds.amazonaws.com;Database=GrayboxTool;User ID=GrayboxTool;Password=SM9tyR8h21PRoVz;Pooling=true";

        public static string MacVolumePath = "/Volumes/3dshare/";

        public static string WinVolumePath = "//spserver/3dshare/";

        #if UNITY_EDITOR_OSX
        public static string VolumePath = MacVolumePath;
        #else
        public static string VolumePath = WinVolumePath;
#endif
        public static string IconsPath = VolumePath + "TA/UnityAssetLibrary/tool_icons/";

#if UNITY_EDITOR_OSX
        public static string PkgDefaultFolder = VolumePath + "TA/UnityAssetLibrary/Packages";
#else
        public static string PkgDefaultFolder = (VolumePath + "TA/UnityAssetLibrary/Packages").Replace("/", "\\");
#endif
        public static string SmbConnectionUrl = "//guest@" + WinVolumePath.Substring(2);

        public static string ContactUrl = "https://mail.google.com/mail/?view=cm&fs=1&to=techart@socialpoint.es&su=Graybox Tool Contact";

        public static string HelpUrl = "https://sites.google.com/a/socialpoint.es/technical-art/07---tools-documentation/greybox-library";

    }
}