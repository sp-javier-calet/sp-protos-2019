using System;
using System.Collections.Generic;
using System.Text;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.QualityStats
{
    public sealed class QualityStatsHttpClient : IHttpClient
    {
        #region IHttpClient implementation

        public event HttpRequestDelegate RequestSetup;

        public IHttpConnection Send(HttpRequest request, HttpResponseDelegate del = null)
        {
            if(RequestSetup != null)
            {
                RequestSetup(request);
            }
            var start = TimeUtils.Now;
            var url = request.Url;
            return _client.Send(request, response => {
                NewTrace(url, start, response);
                if(del != null)
                {
                    del(response);
                }
            });
        }

        public void Dispose()
        {
        }

        public string Config
        {
            set
            {
            }
        }

        #endregion

        public sealed class Data
        {
            public int Amount;
            public double SumSize;
            public double SumSpeed;
            public double SumTimes;
            public double SumWaitTimes;
            public double SumConnectionTimes;
            public double SumTransferTimes;

            public override string ToString()
            {
                return string.Format(
                    "[Data: Amount={0}, SumSize={1}, SumSpeed={2}, SumTimes={3}, SumWaitTimes={4}, SumConnectionTimes={5}, SumTransferTimes={6}]",
                    Amount, SumSize, SumSpeed, SumTimes, SumWaitTimes, SumConnectionTimes, SumTransferTimes);
            }

            public void Add(Data a)
            {
                Amount += a.Amount;
                SumSize += a.SumSize;
                SumSpeed += a.SumSpeed;
                SumTimes += a.SumTimes;
                SumWaitTimes += a.SumWaitTimes;
                SumConnectionTimes += a.SumConnectionTimes;
                SumTransferTimes += a.SumTransferTimes;
            }
        }

        public sealed class MRequests : Dictionary<int, Data>
        {
            public override string ToString()
            {
                var sb = new StringBuilder();
                var itr = this.GetEnumerator();
                while(itr.MoveNext())
                {
                    var item = itr.Current;
                    string s = string.Format(
                                   "[Request: key={0}, Value={1}]",
                                   item.Key, item.Value);
                    sb.Append(s);
                }
                itr.Dispose();

                return sb.ToString();
            }
        }

        public sealed class Stats
        {
            public double DataDownloaded;
            // in Kbytes
            public double SumDownloadSpeed;
            // in Kbytes
            public MRequests Requests;

            public override string ToString()
            {
                return string.Format(
                    "[Stats: DataDownloaded={0}, SumDownloadSpeed={1}, Requests={2}]",
                    DataDownloaded, SumDownloadSpeed, Requests);
            }
        }

        public sealed class MStats : Dictionary<string, Stats>
        {
            public override string ToString()
            {
                var sb = new StringBuilder();
                var itr = this.GetEnumerator();
                while(itr.MoveNext())
                {
                    var item = itr.Current;
                    string s = string.Format(
                                   "[Request: key={0}, Value={1}]",
                                   item.Key, item.Value);
                    sb.Append(s);
                }
                itr.Dispose();

                return sb.ToString();
            }
        }

        IHttpClient _client;
        readonly MStats _data;

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

        void NewTrace(Uri url, DateTime start, HttpResponse response)
        {
            if(url == null)
            {
                return;
            }
            Stats stats;
            if(!_data.TryGetValue(url.ToString(), out stats))
            {
                stats = new Stats();
                stats.Requests = new MRequests();
                _data[url.ToString()] = stats;
            }

            var dataDownloaded = response.DownloadSize / 1024.0;
            var downloadSpeed = response.DownloadSpeed / 1024.0;

            stats.DataDownloaded += dataDownloaded;
            stats.SumDownloadSpeed += downloadSpeed;

            Data data;
            if(!stats.Requests.TryGetValue(response.StatusCode, out data))
            {
                data = new Data();
                stats.Requests[response.StatusCode] = data;
            }

            data.Amount++;
            data.SumSize += dataDownloaded;
            data.SumSpeed += downloadSpeed;
            data.SumTimes += response.Duration;
            var end = TimeUtils.Now;
            data.SumWaitTimes += (end - start).TotalSeconds - response.Duration;
            data.SumConnectionTimes += response.ConnectionDuration;
            data.SumTransferTimes += response.TransferDuration;
        }
    }
}
