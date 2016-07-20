using System.Collections.Generic;

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
    public abstract class XCodeProjectEditor
    {
        public abstract void AddHeaderSearchPath(string path);

        public abstract void AddLibrarySearchPath(string path);

        public abstract void CopyFile(string basePath, string src, string dst);

        public abstract void AddFile(string path);

        public abstract void AddFile(string path, string[] flags);

        public abstract void AddFolder(string path);

        public abstract void AddLibrary(string path);

        public abstract void AddFramework(string framework, bool weak);

        public abstract void SetBuildSetting(string name, string value);

        public abstract void AddVariantGroup(string variantGroup, string key, string value);

        public abstract void SetPlistField(string name, Dictionary<string, object>  value);

        public abstract void AddShellScript(string script);

        public abstract void SetSystemCapability(string name, bool enabled);

        public abstract void SetProvisioningProfile(string path);

        public abstract void AddKeychainAccessGroup(string accessGroup);

        public abstract void Commit();
    }
}