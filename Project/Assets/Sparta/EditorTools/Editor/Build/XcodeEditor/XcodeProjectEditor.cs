namespace SpartaTools.Editor.Build.XcodeEditor
{
    /// <summary>
    /// Public interface for the Editor class
    /// There is only a implentation, but we need to hide some details 
    /// and public accessors to other classes.
    /// </summary>
    public abstract class XCodeProjectEditor
    {
        public abstract void AddFile(string relativePath);

        public abstract void AddFramework(string path, bool weak = false);

        public abstract void Commit();
    }
}