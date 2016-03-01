using System.Collections.Generic;

namespace SpartaTools.Editor.Sync
{
    public class ModuleSync
    {
        public enum SyncStatus
        {
            // New module in target project
            New,
            // Target project module is up to date with library
            UpToDate,
            // Project modules has changes
            HasChanges,
            // Module is not installed in the target project
            NotInstalled
        }

        public enum SyncAction
        {
            // No action
            None,
            // Update target project with library code
            Override,
            // Remove module code from target project
            Uninstall
        }

        public enum FileStatus
        {
            Equals,
            Modified,
            MissingFileInSource,
            MissingFileInTarget
        }

        public class FileSync
        {
            public string File;

            public FileStatus FileStatus { get; private set; }

            long _sizeInSource;

            public long SizeInSource
            {
                get
                {
                    return _sizeInSource;
                }
                set
                {
                    _sizeInSource = value;
                    UpdateStatus();
                }
            }

            long _sizeInTarget;

            public long SizeInTarget
            {
                get
                {
                    return _sizeInTarget;
                }
                set
                {
                    _sizeInTarget = value;
                    UpdateStatus();
                }
            }

            void UpdateStatus()
            {
                if(_sizeInSource == 0)
                {
                    FileStatus = FileStatus.MissingFileInSource;
                }
                else if(_sizeInTarget == 0)
                {
                    FileStatus = FileStatus.MissingFileInTarget;
                }
                else if(_sizeInSource != _sizeInTarget)
                {
                    FileStatus = FileStatus.Modified;
                }
                else
                {
                    FileStatus = FileStatus.Equals;
                }
            }
        }

        public string Name { get; private set; }

        public string Path { get; private set; }

        public Module.ModuleType Type { get; private set; }

        public Module ReferenceModule { get; private set; }

        public IList<FileSync> Files  { get; set; }

        public SyncStatus Status { get; private set; }

        public SyncAction Action;

        public ModuleSync(Module libraryModule, Module targetModule, IList<FileSync> diffFiles, SyncStatus status)
        {
            // Libraries modules have always preference as reference modules than target ones
            ReferenceModule = libraryModule ?? targetModule;
            Name = ReferenceModule.Name;
            Path = ReferenceModule.RelativePath;
            Type = ReferenceModule.Type;
            Files = diffFiles;
            Status = status;
            Action = GetDefaultAction(Status);
        }

        public SyncAction GetDefaultAction(SyncStatus status)
        {
            if(ReferenceModule.IsMandatory)
            {
                return SyncAction.Override;
            }

            switch(status)
            {
            case SyncStatus.NotInstalled: 
                return SyncAction.None;
            case SyncStatus.HasChanges: 
                return SyncAction.Override;
            case SyncStatus.UpToDate: 
                return SyncAction.None;
            }

            return SyncAction.None;
        }
    }
}