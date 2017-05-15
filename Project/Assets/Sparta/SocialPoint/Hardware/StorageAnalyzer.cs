using System;
using SocialPoint.Utils;

namespace SocialPoint.Hardware
{
    public class StorageAnalyzer : IUpdateable, IDisposable
    {
        [Serializable]
        public struct Config
        {
            public ulong FreeStorageWarning;
            public float UpdateInterval;
        }

        public Action<ulong> OnLowStorageWarning;

        Config _config;
        IStorageInfo _storageInfo;
        UpdateScheduler _scheduler;

        public StorageAnalyzer(IStorageInfo storageInfo, UpdateScheduler scheduler, Config config)
        {
            _storageInfo = storageInfo;
            _scheduler = scheduler;
            _config = config;
            Start();
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            Stop();
            _scheduler.Add(this, _config.UpdateInterval);
        }

        public void Stop()
        {
            _scheduler.Remove(this);
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
            if(OnLowStorageWarning != null)
            {
                OnLowStorageWarning(freeStorage);
            }
        }
    }
}
