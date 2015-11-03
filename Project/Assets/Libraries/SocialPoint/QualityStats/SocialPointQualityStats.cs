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
    public class SocialPointQualityStats : IDisposable
    {
        IDeviceInfo _deviceInfo;
        List<QualityStatsHttpClient> _qualityStatsHttpClients;

        DateTime _loadingStarted;
        DateTime _loadingFinished;
        bool _timeToMapSent;

        static readonly float kByteConverter = 1.0f / 1024.0f;
        static readonly DateTime kNewDateTime = new DateTime();

        const string kClientPerformanceStats = "client_performance.stats";
        const string kClientPerformanceHttpRequest = "client_performance.http_request";

        public delegate void TrackEventDelegate(string eventName,AttrDic data = null,ErrorDelegate del = null);

        public TrackEventDelegate TrackEvent{ private get; set; }

        public SocialPointQualityStats(IDeviceInfo deviceInfo, IAppEvents appEvents)
        {
            if(deviceInfo == null)
            {
                throw new ArgumentNullException("deviceInfo", "deviceInfo cannot be null or empty!");
            }
            _deviceInfo = deviceInfo;

            AppEvents = appEvents;

            _loadingStarted = new DateTime();
            _loadingFinished = new DateTime();
            _timeToMapSent = false;

            _qualityStatsHttpClients = new List<QualityStatsHttpClient>();

            OnApplicationDidFinishLaunching();
        }

        public void Dispose()
        {
            if(_appEvents != null)
            {
                DisconnectAppEvents(_appEvents);
            }
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

        #region App Events

        void ConnectAppEvents(IAppEvents appEvents)
        {
            appEvents.RegisterWillGoBackground(100, OnAppWillGoBackground);
            appEvents.RegisterGameWasLoaded(0, OnGameLoaded);
        }

        void DisconnectAppEvents(IAppEvents appEvents)
        {
            appEvents.UnregisterWillGoBackground(OnAppWillGoBackground);
            appEvents.UnregisterGameWasLoaded(OnGameLoaded);
        }

        void OnApplicationDidFinishLaunching()
        {
            _loadingStarted = TimeUtils.Now.ToLocalTime();
        }

        void OnGameLoaded()
        {
            _loadingFinished = TimeUtils.Now.ToLocalTime();
        }

        void OnAppWillGoBackground()
        {
            var stats = GetStats();
            SendClientPerformance(stats, kClientPerformanceStats);

            var httpRequests = GetHttpRequests();
            foreach(var request in httpRequests)
            {
                var requestDic = request.AsDic;
                SendClientPerformance(requestDic, kClientPerformanceHttpRequest);
            }
            ResetQualityStatsHttpClients();
        }

        #endregion

        void ResetQualityStatsHttpClients()
        {
            foreach(var client in _qualityStatsHttpClients)
            {
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
            client.Set("storage_stats", getStorageData());
            client.Set("app_info", GetAppData());
            client.Set("network_stats", GetNetworkData());
            client.Set("performance", GetPerformanceData());

            return data;
        }

        AttrList GetHttpRequests()
        {
            var requestList = new AttrList();

            var data = GetClientStats();

            foreach(var statsIt in data)
            {
                QualityStatsHttpClient.Stats stats = statsIt.Value;
                foreach(var dataIt in stats.Requests)
                {
                    var request = GetPerformanceData(statsIt, dataIt);
                    requestList.Add(request);
                }
            }

            return requestList;
        }

        QualityStatsHttpClient.MStats GetClientStats()
        {
            var data = new QualityStatsHttpClient.MStats();
            foreach(var client in _qualityStatsHttpClients)
            {
                QualityStatsHttpClient.MStats clientData = client.getStats();
                foreach(var statsIt in clientData)
                {
                    QualityStatsHttpClient.Stats mergedStats;
                    if(!data.TryGetValue(statsIt.Key, out mergedStats))
                    {
                        mergedStats = new QualityStatsHttpClient.Stats();
                        data[statsIt.Key] = mergedStats;
                    }
                    QualityStatsHttpClient.Stats stats = statsIt.Value;
                    mergedStats.DataDownloaded += stats.DataDownloaded;
                    mergedStats.SumDownloadSpeed += stats.SumDownloadSpeed;
                    foreach(var dataIt in stats.Requests)
                    {
                        QualityStatsHttpClient.Data requestMergedData;
                        if(!mergedStats.Requests.TryGetValue(dataIt.Key, out requestMergedData))
                        {
                            requestMergedData = new QualityStatsHttpClient.Data();
                            mergedStats.Requests[dataIt.Key] = requestMergedData;
                        }
                        QualityStatsHttpClient.Data requestData = dataIt.Value;
                        requestMergedData.Amount += requestData.Amount;
                        requestMergedData.SumTimes += requestData.SumTimes;
                        requestMergedData.SumWaitTimes += requestData.SumWaitTimes;
                        requestMergedData.SumConnectionTimes += requestData.SumConnectionTimes;
                        requestMergedData.SumTransferTimes += requestData.SumTransferTimes;
                    }
                }
            }
            return data;
        }

        #region AttrDic Data

        static AttrDic GetPerformanceData(KeyValuePair<string,QualityStatsHttpClient.Stats> statsIt, KeyValuePair<int,QualityStatsHttpClient.Data> dataIt)
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

            QualityStatsHttpClient.Data requestData = dataIt.Value;
            var dAmount = (double)requestData.Amount;

            performance.SetValue("number_of_calls", requestData.Amount);
            performance.SetValue("avg_time", requestData.SumTimes / dAmount);
            performance.SetValue("avg_wait_time", requestData.SumWaitTimes / dAmount);
            performance.SetValue("avg_conn_time", requestData.SumConnectionTimes / dAmount);
            performance.SetValue("avg_trans_time", requestData.SumTransferTimes / dAmount);

            return data;
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

        AttrDic getStorageData()
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

            dict.SetValue("size_cache_dir", Caching.spaceOccupied);

            if(_loadingFinished.CompareTo(kNewDateTime) != 0)
            {
                var timeToMap = (_loadingFinished - _loadingStarted).TotalSeconds;

                if(!_timeToMapSent && timeToMap > 0.0f)
                {
                    dict.SetValue("time_to_map", timeToMap);
                    _timeToMapSent = true;
                }
            }

            return dict;
        }

        #endregion

    }
}
