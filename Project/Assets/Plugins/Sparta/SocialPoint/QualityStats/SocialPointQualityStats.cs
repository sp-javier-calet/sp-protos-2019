using System;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Hardware;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.QualityStats
{
    public sealed class SocialPointQualityStats : IUpdateable, IDisposable
    {
        IDeviceInfo _deviceInfo;
        List<QualityStatsHttpClient> _qualityStatsHttpClients;

        DateTime _loadingStarted;
        double _timeToMap;
        bool _timeToMapSent;
        float _totalDataDownloaded;
        float _frameTimeSum;
        float _frameTimeVarianceSum;
        int _numSessionSamples;

        static readonly float kByteConverter = 1.0f / 1024.0f;

        const string kClientPerformanceStats = "client_performance.stats";
        const string kClientPerformanceHttpRequest = "client_performance.http_request";

        public delegate void TrackEventDelegate(string eventName, AttrDic data = null, ErrorDelegate del = null);

        public TrackEventDelegate TrackEvent{ private get; set; }

        public SocialPointQualityStats(IDeviceInfo deviceInfo, IAppEvents appEvents, IUpdateScheduler scheduler)
        {
            if(deviceInfo == null)
            {
                throw new ArgumentNullException("deviceInfo", "deviceInfo cannot be null or empty!");
            }
            _deviceInfo = deviceInfo;

            AppEvents = appEvents;
            Scheduler = scheduler;

            _loadingStarted = new DateTime();
            _timeToMapSent = false;
            _timeToMap = 0.0;
            _frameTimeSum = 0.0f;
            _frameTimeVarianceSum = 0.0f;
            _numSessionSamples = 1;

            _qualityStatsHttpClients = new List<QualityStatsHttpClient>();

            OnApplicationDidFinishLaunching();
        }

        public void Dispose()
        {
            if(_appEvents != null)
            {
                DisconnectAppEvents(_appEvents);
            }
            if(_scheduler != null)
            {
                _scheduler.Remove(this);
            }
        }

        public void Update()
        {
            float dt = Time.deltaTime;
            _frameTimeSum += dt;

            float average = GetAverageFrameTime();
            _frameTimeVarianceSum += ((dt - average) * (dt - average));

            ++_numSessionSamples;
        }

        public void AddQualityStatsHttpClient(QualityStatsHttpClient client)
        {
            _qualityStatsHttpClients.Add(client);
        }

        IAppEvents _appEvents;

        IAppEvents AppEvents
        {
            get
            {
                return _appEvents;
            }
            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException("_appEvents", "_appEvents cannot be null or empty!");
                }
                if(_appEvents != null)
                {
                    DisconnectAppEvents(_appEvents);
                }
                _appEvents = value;
                if(_appEvents != null)
                {
                    ConnectAppEvents(_appEvents);
                }
            }
        }

        IUpdateScheduler _scheduler;

        IUpdateScheduler Scheduler
        {
            get
            {
                return _scheduler;
            }
            set
            {
                if(_scheduler != null)
                {
                    _scheduler.Remove(this);
                }
                _scheduler = value;
                _scheduler.Add(this);
            }
        }

        #region App Events

        void ConnectAppEvents(IAppEvents appEvents)
        {
            appEvents.WillGoBackground.Add(100, OnAppWillGoBackground);
            appEvents.GameWasLoaded.Add(0, OnGameLoaded);
        }

        void DisconnectAppEvents(IAppEvents appEvents)
        {
            appEvents.WillGoBackground.Remove(OnAppWillGoBackground);
            appEvents.GameWasLoaded.Remove(OnGameLoaded);
        }

        void OnApplicationDidFinishLaunching()
        {
            _loadingStarted = TimeUtils.Now.ToLocalTime();
        }

        void OnGameLoaded()
        {
            var loadingFinished = TimeUtils.Now.ToLocalTime();
            _timeToMap = (loadingFinished - _loadingStarted).TotalSeconds;
            if(_appEvents != null)
            {
                _appEvents.GameWasLoaded.Remove(OnGameLoaded);
            }
        }

        public void OnAppWillGoBackground()
        {
            var httpRequests = GetHttpRequests();
            var itr = httpRequests.GetEnumerator();
            while(itr.MoveNext())
            {
                var request = itr.Current;
                var requestDic = request.AsDic;
                SendClientPerformance(requestDic, kClientPerformanceHttpRequest);
            }
            itr.Dispose();

            var stats = GetStats();
            SendClientPerformance(stats, kClientPerformanceStats);
        }

        #endregion

        void ResetQualityStatsHttpClients()
        {
            for(int i = 0, _qualityStatsHttpClientsCount = _qualityStatsHttpClients.Count; i < _qualityStatsHttpClientsCount; i++)
            {
                var client = _qualityStatsHttpClients[i];
                client.Reset();
            }
        }

        void SendClientPerformance(AttrDic data, string eventName)
        {
            if(TrackEvent != null)
            {
                TrackEvent(eventName, data);
            }
        }

        AttrDic GetStats()
        {
            var data = new AttrDic();
            var client = new AttrDic();

            data.Set("client", client);

            client.Set("memory_stats", GetMemoryData());
            client.Set("storage_stats", GetStorageData());
            client.Set("app_info", GetAppData());
            client.Set("mobile", GetMobileData());
            client.Set("network_stats", GetNetworkData());
            client.Set("performance", GetPerformanceData());
            client.Set("device_stats", GetDeviceData());

            return data;
        }

        AttrList GetHttpRequests()
        {
            var requestList = new AttrList();

            var data = GetClientStats();

            _totalDataDownloaded = 0.0f;

            var itr = data.GetEnumerator();
            while(itr.MoveNext())
            {
                var statsItr = itr.Current;
                var statsValue = statsItr.Value;
                _totalDataDownloaded += (float)statsValue.DataDownloaded;
                var itr2 = statsValue.Requests.GetEnumerator();
                while(itr2.MoveNext())
                {
                    var dataIt = itr2.Current;
                    var request = GetPerformanceRequest(statsItr, dataIt);
                    requestList.Add(request);
                }
                itr2.Dispose();
            }
            itr.Dispose();

            return requestList;
        }

        QualityStatsHttpClient.MStats GetClientStats()
        {
            var data = new QualityStatsHttpClient.MStats();
            for(int i = 0, _qualityStatsHttpClientsCount = _qualityStatsHttpClients.Count; i < _qualityStatsHttpClientsCount; i++)
            {
                var client = _qualityStatsHttpClients[i];
                var clientData = client.getStats();
                var itr = clientData.GetEnumerator();
                while(itr.MoveNext())
                {
                    var statsIt = itr.Current;
                    QualityStatsHttpClient.Stats mergedStats;
                    if(!data.TryGetValue(statsIt.Key, out mergedStats))
                    {
                        mergedStats = new QualityStatsHttpClient.Stats();
                        mergedStats.Requests = new QualityStatsHttpClient.MRequests();
                        data[statsIt.Key] = mergedStats;
                    }
                    var stats = statsIt.Value;
                    mergedStats.DataDownloaded += stats.DataDownloaded;
                    mergedStats.SumDownloadSpeed += stats.SumDownloadSpeed;
                    var itr2 = stats.Requests.GetEnumerator();
                    while(itr2.MoveNext())
                    {
                        var dataIt = itr2.Current;
                        QualityStatsHttpClient.Data requestMergedData;
                        if(!mergedStats.Requests.TryGetValue(dataIt.Key, out requestMergedData))
                        {
                            requestMergedData = new QualityStatsHttpClient.Data();
                            mergedStats.Requests[dataIt.Key] = requestMergedData;
                        }
                        requestMergedData.Add(dataIt.Value);
                    }
                    itr2.Dispose();
                }
                itr.Dispose();
            }
            ResetQualityStatsHttpClients();
            return data;
        }

        #region AttrDic Data

        static AttrDic GetPerformanceRequest(KeyValuePair<string,QualityStatsHttpClient.Stats> statsIt, KeyValuePair<int,QualityStatsHttpClient.Data> dataIt)
        {
            var data = new AttrDic();
            var client = new AttrDic();
            var performance = new AttrDic();

            data.Set("client", client);
            client.Set("performance", performance);

            var url = statsIt.Key;
            performance.SetValue("url", url);

            var code = dataIt.Key.ToString();
            performance.SetValue("code", code);

            GetPerformanceData(performance, dataIt.Value);

            return data;
        }

        static void GetPerformanceData(AttrDic performance, QualityStatsHttpClient.Data requestData)
        {
            var dAmount = (double)requestData.Amount;

            performance.SetValue("number_calls", requestData.Amount);
            performance.SetValue("avg_size", requestData.SumSize / dAmount);
            performance.SetValue("avg_download_speed", requestData.SumSpeed / dAmount);
            performance.SetValue("avg_time", requestData.SumTimes / dAmount);
            performance.SetValue("avg_wait_time", requestData.SumWaitTimes / dAmount);
            performance.SetValue("avg_conn_time", requestData.SumConnectionTimes / dAmount);
            performance.SetValue("avg_trans_time", requestData.SumTransferTimes / dAmount);
        }

        AttrDic GetMemoryData()
        {
            var memory = _deviceInfo.MemoryInfo;
            var dict = new AttrDic();

            var activeMemory = (double)memory.ActiveMemory * kByteConverter;
            var freeMemory = (double)memory.FreeMemory * kByteConverter;
            var totalMemory = (double)memory.TotalMemory * kByteConverter;
            var usedMemory = (double)memory.UsedMemory * kByteConverter;

            // max. two decimal places
            dict.SetValue("active_memory", activeMemory);
            dict.SetValue("free_memory", freeMemory);
            dict.SetValue("total_memory", totalMemory);
            dict.SetValue("user_memory", usedMemory);

            return dict;
        }

        AttrDic GetStorageData()
        {
            var storage = _deviceInfo.StorageInfo;
            var dict = new AttrDic();

            var freeStorage = (double)storage.FreeStorage * kByteConverter;
            var totalStorage = (double)storage.TotalStorage * kByteConverter;
            var usedStorage = (double)storage.UsedStorage * kByteConverter;

            // max. two decimal places
            dict.SetValue("free_storage", freeStorage);
            dict.SetValue("total_storage", totalStorage);
            dict.SetValue("used_storage", usedStorage);

            return dict;
        }

        AttrDic GetAppData()
        {
            var appInfo = _deviceInfo.AppInfo;
            var dict = new AttrDic();

            dict.SetValue("seed_id", appInfo.SeedId);
            dict.SetValue("id", appInfo.Id);
            dict.SetValue("version", appInfo.Version);
            dict.SetValue("short_version", appInfo.ShortVersion);
            dict.SetValue("language", appInfo.Language);
            dict.SetValue("country", appInfo.Country);
            dict.SetValue("stored_id", appInfo.Country);

            return dict;
        }

        AttrDic GetMobileData()
        {
            var architecture = _deviceInfo.Architecture;
            var dict = new AttrDic();

            dict.SetValue("arch_info", architecture);

            return dict;
        }

        AttrDic GetNetworkData()
        {
            var network = _deviceInfo.NetworkInfo;
            var dict = new AttrDic();

            dict.SetValue("connectivity", network.Connectivity.ToString());
            dict.SetValue("proxy_host", network.Proxy != null ? network.Proxy.Host : "");
            dict.SetValue("port_proxy", network.Proxy != null ? network.Proxy.Port : -1);

            return dict;
        }

        AttrDic GetPerformanceData()
        {
            var dict = new AttrDic();

            AddSizeCacheDir(dict);
            AddTimeToMap(dict);
            AddTotalDataDownloaded(dict);
            AddFPSInfo(dict);

            return dict;
        }

        AttrDic GetDeviceData()
        {
            var deviceInfo = _deviceInfo;
            var dict = new AttrDic();

            dict.SetValue("max_texture_size", deviceInfo.MaxTextureSize);
            dict.SetValue("screen_width", (int)deviceInfo.ScreenSize.x);
            dict.SetValue("screen_height", (int)deviceInfo.ScreenSize.y);
            dict.SetValue("screen_dpi", (int)deviceInfo.ScreenDpi);
            dict.SetValue("cpu_cores", deviceInfo.CpuCores);
            dict.SetValue("cpu_freq", deviceInfo.CpuFreq);
            dict.SetValue("cpu_model", deviceInfo.CpuModel);
            dict.SetValue("cpu_arch", deviceInfo.CpuArchitecture);
            dict.SetValue("opengl_vendor", deviceInfo.OpenglVendor);
            dict.SetValue("opengl_renderer", deviceInfo.OpenglRenderer);
            //dict.SetValue("opengl_extensions", deviceInfo.OpenglExtensions);
            dict.SetValue("opengl_shading", deviceInfo.OpenglShadingVersion);
            dict.SetValue("opengl_version", deviceInfo.OpenglVersion);
            dict.SetValue("opengl_memory", deviceInfo.OpenglMemorySize);

            return dict;
        }


        static void AddSizeCacheDir(AttrDic dict)
        {
            dict.SetValue("size_cache_dir", Caching.currentCacheForWriting.spaceOccupied);
        }

        void AddTimeToMap(AttrDic dict)
        {
            if(!_timeToMapSent && _timeToMap > 0.0f)
            {
                dict.SetValue("time_to_map", _timeToMap);
                _timeToMapSent = true;
            }
        }

        void AddTotalDataDownloaded(AttrDic dict)
        {
            dict.SetValue("total_data_downloaded", _totalDataDownloaded);
        }

        void AddFPSInfo(AttrDic dict)
        {
            dict.SetValue("avg_frame", GetAverageFrameTime());
            dict.SetValue("variance_frame", GetVarianceFrameTime());
        }

        #endregion

        float GetAverageFrameTime()
        {
            return (_frameTimeSum / _numSessionSamples);
        }

        float GetVarianceFrameTime()
        {
            return (_frameTimeVarianceSum / _numSessionSamples);
        }
    }
}
