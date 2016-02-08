using System;
using System.Collections.Generic;
using System.Text;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.QualityStats
{
    public class QualityStatsHttpClient : IHttpClient
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

        virtual public void Dispose()
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

        public struct Data
        {
            public int Amount;
            public double SumSize;
            public double SumTimes;
            public double SumWaitTimes;
            public double SumConnectionTimes;
            public double SumTransferTimes;

            public override string ToString()
            {
                return string.Format(
                    "[Data: Amount={0}, SumSize={1}, SumTimes={2}, SumWaitTimes={3}, SumConnectionTimes={4}, SumTransferTimes={5}]",
                    Amount, SumTimes, SumWaitTimes, SumConnectionTimes, SumTransferTimes);
            }

            public static Data operator+(Data a, Data b)
            {
                return new Data {
                    Amount = a.Amount + b.Amount,
                    SumSize = a.SumSize + b.SumSize,
                    SumTimes = a.SumTimes + b.SumTimes,
                    SumWaitTimes = a.SumWaitTimes + b.SumWaitTimes,
                    SumConnectionTimes = a.SumConnectionTimes + b.SumConnectionTimes,
                    SumTransferTimes = a.SumTransferTimes + b.SumTransferTimes,
                };
            }
        }

        public class MRequests : Dictionary<int, Data>
        {
            public override string ToString()
            {
                var sb = new StringBuilder();
                foreach(var item in this)
                {
                    string s = string.Format(
                                   "[Request: key={0}, Value={1}]",
                                   item.Key, item.Value);
                    sb.Append(s);
                }
                return sb.ToString();
            }
        }

        public struct Stats
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

        public class MStats : Dictionary<string, Stats>
        {
            public override string ToString()
            {
                var sb = new StringBuilder();
                foreach(var item in this)
                {
                    string s = string.Format(
                                   "[Request: key={0}, Value={1}]",
                                   item.Key, item.Value);
                    sb.Append(s);
                }
                return sb.ToString();
            }
        }

        IHttpClient _client;
        MStats _data;

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

            stats.DataDownloaded += (response.DownloadSize / 1024.0);
            stats.SumDownloadSpeed += (response.DownloadSpeed / 1024.0);

            Data data;
            if(!stats.Requests.TryGetValue(response.StatusCode, out data))
            {
                data = new Data();
            }

            data.Amount++;
            data.SumSize += response.DownloadSize / 1024.0;
            data.SumTimes += response.Duration;
            var end = TimeUtils.Now;
            data.SumWaitTimes += (end - start).TotalSeconds - response.Duration;
            data.SumConnectionTimes += response.ConnectionDuration;
            data.SumTransferTimes += response.TransferDuration;

            stats.Requests[response.StatusCode] = data;
        }
    }
}
