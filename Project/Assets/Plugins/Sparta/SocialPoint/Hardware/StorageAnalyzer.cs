using System;
using SocialPoint.Utils;

namespace SocialPoint.Hardware
{
    [Serializable]
    public struct StorageAnalyzerConfig
    {
        public float AnalysisInterval;
        public StorageAmount FreeStorageWarning;
        public bool StopOnFirstWarning;
    }

    public interface IStorageAnalyzer : IDisposable
    {
        //Handlers receive amount of free bytes (1) and minimun expected (2)
        event Action<ulong, ulong> LowStorage;

        StorageAnalyzerConfig Config { get; set; }

        bool Running { get; }

        void Start();

        void Stop();

        void AnalyzeNow();
    }

    public class StorageAnalyzer : IStorageAnalyzer, IUpdateable
    {
        public event Action<ulong, ulong> LowStorage;

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

        public void Update()
        {
            AnalyzeNow();
        }

        void CheckFreeStorageSpace()
        {
            var freeBytesStorage = _storageInfo.FreeStorage;
            var requiredBytesStorage = _config.FreeStorageWarning.Bytes;
            if(freeBytesStorage < requiredBytesStorage)
            {
                RaiseLowStorageWarning(freeBytesStorage, requiredBytesStorage);
            }
        }

        void RaiseLowStorageWarning(ulong freeBytesStorage, ulong requiredBytesStorage)
        {
            if(LowStorage != null)
            {
                if(_config.StopOnFirstWarning)
                {
                    Stop();
                }

                LowStorage(freeBytesStorage, requiredBytesStorage);
            }
        }
    }
}
