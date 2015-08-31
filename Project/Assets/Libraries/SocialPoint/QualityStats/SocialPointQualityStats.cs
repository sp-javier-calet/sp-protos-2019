using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Hardware;
using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Base;

namespace SocialPoint.QualityStats
{
    public class SocialPointQualityStats
    {
        private IDeviceInfo _deviceInfo;
        private List<QualityStatsHttpClient> _qualityStatsHttpClients;

        private DateTime _loadingStarted;
        private DateTime _loadingFinished;
        private bool _timeToMapSent;

        private static readonly float kByteConverter = 1.0f / 1024.0f;
        private static readonly DateTime kNewDateTime = new DateTime();

        private const string kClientPerformanceStats = "client_performance.stats";
        private const string kClientPerformanceHttpRequest = "client_performance.http_request";

        public delegate void TrackEventDelegate(string eventName,AttrDic data = null, ErrorDelegate del = null);

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

        public void AddQualityStatsHttpClient(QualityStatsHttpClient client)
        {
            _qualityStatsHttpClients.Add(client);
        }

        private IAppEvents _appEvents;

        private IAppEvents AppEvents
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
                ConnectAppEvents(_appEvents);
            }
        }

        #region App Events

        private void ConnectAppEvents(IAppEvents appEvents)
        {
            appEvents.WillGoBackground += OnAppWillGoBackground;
        }

        private void DisconnectAppEvents(IAppEvents appEvents)
        {
            appEvents.WillGoBackground -= OnAppWillGoBackground;
        }

        private void OnApplicationDidFinishLaunching()
        {
            _loadingStarted = TimeUtils.Now.ToLocalTime();
        }

        public void OnGameLoaded()
        {
            _loadingFinished = TimeUtils.Now.ToLocalTime();
        }

        private void OnAppWillGoBackground()
        {
            AttrDic stats = GetStats();
            SendClientPerformance(stats, kClientPerformanceStats);

            AttrList httpRequests = GetHttpRequests();
            foreach(var request in httpRequests)
            {
                AttrDic requestDic = request.AsDic;
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

        private void SendClientPerformance(AttrDic data, string eventName)
        {
            if(TrackEvent != null)
            {
                TrackEvent(eventName, data);
            }
        }

        private AttrDic GetStats()
        {
            AttrDic data = new AttrDic();
            AttrDic client = new AttrDic();

            data.Set("client", client);

            client.Set("memory_stats", GetMemoryData());
            client.Set("storage_stats", getStorageData());
            client.Set("app_info", GetAppData());
            client.Set("network_stats", GetNetworkData());
            client.Set("performance", GetPerformanceData());

            return data;
        }

        private AttrList GetHttpRequests()
        {
            AttrList requestList = new AttrList();

            var data = GetClientStats();

            foreach(var statsIt in data)
            {
                QualityStatsHttpClient.Stats stats = statsIt.Value;
                foreach(var dataIt in stats.Requests)
                {
                    AttrDic request = GetPerformanceData(statsIt, dataIt);
                    requestList.Add(request);
                }
            }

            return requestList;
        }

        private QualityStatsHttpClient.MStats GetClientStats()
        {
            QualityStatsHttpClient.MStats data = new QualityStatsHttpClient.MStats();
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

        private AttrDic GetPerformanceData(KeyValuePair<string,QualityStatsHttpClient.Stats> statsIt, KeyValuePair<int,QualityStatsHttpClient.Data> dataIt)
        {
            AttrDic data = new AttrDic();
            AttrDic client = new AttrDic();
            AttrDic performance = new AttrDic();

            data.Set("client", client);
            client.Set("performance", performance);

            string url = statsIt.Key;
            performance.SetValue("url", url);

            string code = dataIt.Key.ToString();
            performance.SetValue("code", code);

            QualityStatsHttpClient.Data requestData = dataIt.Value;
            double dAmount = (double)requestData.Amount;

            performance.SetValue("number_of_calls", requestData.Amount);
            performance.SetValue("avg_time", requestData.SumTimes / dAmount);
            performance.SetValue("avg_wait_time", requestData.SumWaitTimes / dAmount);
            performance.SetValue("avg_conn_time", requestData.SumConnectionTimes / dAmount);
            performance.SetValue("avg_trans_time", requestData.SumTransferTimes / dAmount);

            return data;
        }

        private AttrDic GetMemoryData()
        {
            IMemoryInfo memory = _deviceInfo.MemoryInfo;
            AttrDic dict = new AttrDic();

            double activeMemory = (double)memory.ActiveMemory * kByteConverter;
            double freeMemory = (double)memory.FreeMemory * kByteConverter;
            double totalMemory = (double)memory.TotalMemory * kByteConverter;
            double usedMemory = (double)memory.UsedMemory * kByteConverter;

            // max. two decimal places
            dict.SetValue("active_memory", activeMemory);
            dict.SetValue("free_memory", freeMemory);
            dict.SetValue("total_memory", totalMemory);
            dict.SetValue("user_memory", usedMemory);

            return dict;
        }

        private AttrDic getStorageData()
        {
            IStorageInfo storage = _deviceInfo.StorageInfo;
            AttrDic dict = new AttrDic();

            double freeStorage = (double)storage.FreeStorage * kByteConverter;
            double totalStorage = (double)storage.TotalStorage * kByteConverter;
            double usedStorage = (double)storage.UsedStorage * kByteConverter;

            // max. two decimal places
            dict.SetValue("free_storage", freeStorage);
            dict.SetValue("total_storage", totalStorage);
            dict.SetValue("used_storage", usedStorage);

            return dict;
        }

        private AttrDic GetAppData()
        {
            IAppInfo appInfo = _deviceInfo.AppInfo;
            AttrDic dict = new AttrDic();

            dict.SetValue("seed_id", appInfo.SeedId);
            dict.SetValue("id", appInfo.Id);
            dict.SetValue("version", appInfo.Version);
            dict.SetValue("short_version", appInfo.ShortVersion);
            dict.SetValue("language", appInfo.Language);
            dict.SetValue("country", appInfo.Country);
            dict.SetValue("stored_id", appInfo.Country);

            return dict;
        }

        private AttrDic GetNetworkData()
        {
            INetworkInfo network = _deviceInfo.NetworkInfo;
            AttrDic dict = new AttrDic();

            dict.SetValue("connectivity", network.Connectivity.ToString());
            dict.SetValue("proxy_host", network.Proxy != null ? network.Proxy.Host : "");
            dict.SetValue("port_proxy", network.Proxy != null ? network.Proxy.Port : -1);

            return dict;
        }

        private AttrDic GetPerformanceData()
        {
            AttrDic dict = new AttrDic();

            dict.SetValue("size_cache_dir", Caching.spaceOccupied);

            if(_loadingFinished.CompareTo(kNewDateTime) == 0)
            {
                throw new Exception("SocialPointQualityStats.OnGameLoaded is not being called.");
            }
            else
            {
                double timeToMap = (_loadingFinished - _loadingStarted).TotalSeconds;

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
