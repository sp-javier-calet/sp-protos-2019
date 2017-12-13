using System.Collections;

namespace SpartaTools.Editor.Build.XcodeEditor
{
    /// <summary>
    /// Public interface for the Editor class
    /// There is only a implentation, but we need to hide some details 
    /// and public accessors to other classes.
    /// 
    /// All provided paths must be absolute or relative to one of the supported variables:
    ///   {XCODE_PROJECT_PATH}
    ///   {XCODE_ROOT_PATH}
    ///   {ROOT_PATH}
    /// 
    /// </summary>
    public interface XCodeProjectEditor
    {
        void AddHeaderSearchPath(string path);

        void AddLibrarySearchPath(string path);

        void CopyFile(string basePath, string src, string dst);

        void AddFile(string path);

        void AddFile(string path, string[] flags);

        void AddFolder(string path);

        void AddLibrary(string path, bool weak);

        void AddFramework(string framework, bool weak);

        void SetBuildSetting(string name, string value);
    
        void AddLocalization(string lang);

        void AddLocalization(string name, string path);

        void AddLocalization(string name, string path, string variantGroup);

        void AddPlistFields(IDictionary data);

        void AddShellScript(string script, string shell);

        void AddShellScript(string script, string shell, int order);

        void AddShellScript(string script, string shell, string target, int order);

        void SetSystemCapability(string name, bool enabled);

        void SetProvisioningProfile(string path);

        void AddKeychainAccessGroup(string entitlementsFile, string accessGroup);

        void AddKeychainAccessGroup(string accessGroup);

        void AddPushNotificationsEntitlement(bool isProduction); 

        void AddPushNotificationsEntitlement(string entitlementsFile, bool isProduction); 

        void Commit();
    }
}