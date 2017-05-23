using System;
using SocialPoint.Utils;

//Handlers receive amount of free bytes (1) and minimun expected (2)
using LowStorageHandler = System.Action<ulong, ulong>;

namespace SocialPoint.Hardware
{
    [Serializable]
    public struct StorageAnalyzerConfig
    {
        public float AnalysisInterval;
        public ulong FreeStorageWarning;
        public bool StopOnFirstWarning;
    }

    public interface IStorageAnalyzer : IDisposable
    {
        StorageAnalyzerConfig Config { get; set; }

        bool Running { get; }

        void Start();

        void Stop();

        void AnalyzeNow();

        void RegisterLowStorageWarningHandler(LowStorageHandler handler);

        void UnregisterLowStorageWarningHandler(LowStorageHandler handler);
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
        LowStorageHandler _onLowStorageWarning;

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

        public void AnalyzeNow()
        {
            CheckFreeStorageSpace();
        }

        public void RegisterLowStorageWarningHandler(LowStorageHandler handler)
        {
            _onLowStorageWarning += handler;
        }

        public void UnregisterLowStorageWarningHandler(LowStorageHandler handler)
        {
            _onLowStorageWarning -= handler;
        }

        public void Update()
        {
            AnalyzeNow();
        }

        void CheckFreeStorageSpace()
        {
            var freeBytesStorage = _storageInfo.FreeStorage;
            var requiredBytesStorage = _config.FreeStorageWarning;
            if(freeBytesStorage < requiredBytesStorage)
            {
                RaiseLowStorageWarning(freeBytesStorage, requiredBytesStorage);
            }
        }

        void RaiseLowStorageWarning(ulong freeBytesStorage, ulong requiredBytesStorage)
        {
            if(_onLowStorageWarning != null)
            {
                _onLowStorageWarning(freeBytesStorage, requiredBytesStorage);

                if(_config.StopOnFirstWarning)
                {
                    Stop();
                }
            }
        }
    }
}
