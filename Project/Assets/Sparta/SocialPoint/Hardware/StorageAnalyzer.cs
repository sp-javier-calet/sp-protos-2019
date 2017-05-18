using System;
using SocialPoint.Utils;

namespace SocialPoint.Hardware
{
    [Serializable]
    public struct StorageAnalyzerConfig
    {
        public float AnalysisInterval;
        public ulong FreeStorageWarning;
    }

    public interface IStorageAnalyzer : IDisposable
    {
        StorageAnalyzerConfig Config { get; set; }

        bool Running { get; }

        void Start();

        void Stop();

        void RegisterLowStorageWarningHandler(Action handler);

        void UnregisterLowStorageWarningHandler(Action handler);
    }

    public class StorageAnalyzer : IStorageAnalyzer, IUpdateable
    {
        public StorageAnalyzerConfig Config
        {
            get
            {
                return _config;
            }
            set
            {
                _config = value;
                if(Running)
                {
                    Start();
                }
            }
        }

        public bool Running
        {
            get;
            private set;
        }

        StorageAnalyzerConfig _config;
        IStorageInfo _storageInfo;
        IUpdateScheduler _scheduler;
        Action _onLowStorageWarning;

        public StorageAnalyzer(IStorageInfo storageInfo, IUpdateScheduler scheduler, StorageAnalyzerConfig config)
        {
            _storageInfo = storageInfo;
            _scheduler = scheduler;
            _config = config;
        }

        public void Dispose()
        {
            Stop();
            _storageInfo = null;
            _scheduler = null;
        }

        public void Start()
        {
            Stop();
            _scheduler.Add(this, _config.AnalysisInterval);
            Running = true;
        }

        public void Stop()
        {
            _scheduler.Remove(this);
            Running = false;
        }

        public void RegisterLowStorageWarningHandler(Action handler)
        {
            _onLowStorageWarning += handler;
        }

        public void UnregisterLowStorageWarningHandler(Action handler)
        {
            _onLowStorageWarning -= handler;
        }

        public void Update()
        {
            CheckFreeStorageSpace();
        }

        void CheckFreeStorageSpace()
        {
            var freeStorage = _storageInfo.FreeStorage;
            if(freeStorage < _config.FreeStorageWarning)
            {
                RaiseLowStorageWarning(freeStorage);
            }
        }

        void RaiseLowStorageWarning(ulong freeStorage)
        {
            if(_onLowStorageWarning != null)
            {
                _onLowStorageWarning();
            }
        }
    }
}
