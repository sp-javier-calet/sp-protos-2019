using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SocialPoint.Network;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.QualityStats
{
    public class QualityStatsHttpClient : IHttpClient
    {
        #region IHttpClient implementation

        public event HttpRequestDelegate RequestSetup;

        public IHttpConnection Send(HttpRequest request, HttpResponseDelegate del = null)
        {
            var start = TimeUtils.Now;
            var url = request.Url;
            return _client.Send(request, (response) => {
                NewTrace(url, start, response);
                if(del != null)
                {
                    del(response);
                }
            });
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        [Obsolete]
        public string DefaultProxy
        {
            set
            {
                _client.DefaultProxy = value;
            }
        }

        #endregion

        public class Data
        {
            public int Amount;
            public double SumTimes;
            public double SumWaitTimes;
            public double SumConnectionTimes;
            public double SumTransferTimes;

            public Data()
            {
                Amount = 0;
                SumTimes = 0.0;
                SumWaitTimes = 0.0;
                SumConnectionTimes = 0.0;
                SumTransferTimes = 0.0;
            }

            public override string ToString()
            {
                return string.Format(
                    "[Data: Amount={0}, SumTimes={1}, SumWaitTimes={2}, SumConnectionTimes={3}, SumTransferTimes={4}]",
                    Amount, SumTimes, SumWaitTimes, SumConnectionTimes, SumTransferTimes);
            }
        }

        public class MRequests : Dictionary<int, Data>
        {
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach(var item in this)
                {
                    string s = string.Format(
                                   "[Request: key={0}, Value={1}]",
                                   item.Key.ToString(), item.Value.ToString());
                    sb.Append(s);
                }
                return sb.ToString();
            }
        }

        public class Stats
        {
            public double DataDownloaded;
            // in Kbytes
            public double SumDownloadSpeed;
            // in Kbytes
            public MRequests Requests;

            public Stats()
            {
                DataDownloaded = 0.0;
                SumDownloadSpeed = 0.0;
                Requests = new MRequests();
            }

            public override string ToString()
            {
                return string.Format(
                    "[Stats: DataDownloaded={0}, SumDownloadSpeed={1}, Requests={2}]",
                    DataDownloaded, SumDownloadSpeed, Requests);
            }
        }

        public class MStats : Dictionary<string, Stats>
        {
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach(var item in this)
                {
                    string s = string.Format(
                                   "[Request: key={0}, Value={1}]",
                                   item.Key, item.Value.ToString());
                    sb.Append(s);
                }
                return sb.ToString();
            }
        }

        private IHttpClient _client = null;
        private MStats _data = null;

        public QualityStatsHttpClient(IHttpClient client)
        {
            _client = client;
            if(_client == null)
            {
                throw new ArgumentNullException("_client", "_client cannot be null or empty!");
            }

            _data = new MStats();
        }

        public MStats getStats()
        {
            return _data;
        }

        public void Reset()
        {
            _data.Clear();
        }

        private void NewTrace(Uri url, DateTime start, HttpResponse response)
        {
            Stats stats;
            if(!_data.TryGetValue(url.ToString(), out stats))
            {
                stats = new Stats();
                _data[url.ToString()] = stats;
            }

            stats.DataDownloaded += (response.DownloadSize / 1024.0);
            stats.SumDownloadSpeed += (response.DownloadSpeed / 1024.0);

            Data data;
            if(!stats.Requests.TryGetValue(response.StatusCode, out data))
            {
                data = new Data();
                stats.Requests[response.StatusCode] = data;
            }

            data.Amount++;
            data.SumTimes += response.Duration;
            var end = TimeUtils.Now;
            data.SumWaitTimes += (end - start).TotalSeconds - response.Duration;
            data.SumConnectionTimes += response.ConnectionDuration;
            data.SumTransferTimes += response.TransferDuration;
        }
    }
}
