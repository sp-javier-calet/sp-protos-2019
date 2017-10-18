using System;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Photon.Hive.Plugin;
using SocialPoint.IO;
using SocialPoint.Base;
using log4net;
using SocialPoint.Network.ServerEvents;
using SocialPoint.Utils;
using SocialPoint.Attributes;

namespace SocialPoint.Network
{
    /**
     * more info:
     * https://doc.photonengine.com/en/onpremise/current/plugins/manual
     * https://doc.photonengine.com/en/onpremise/current/plugins/plugins-faq
     * https://doc.photonengine.com/en/onpremise/current/plugins/plugins-upload-guide
     */
    public abstract class NetworkServerPlugin : PluginBase, INetworkServer
    {
        string _pluginName;
        public override string Name
        {
            get
            {
                return _pluginName;
            }
        }

        bool INetworkServer.Running
        {
            get
            {
                return true;
            }
        }

        string INetworkServer.Id
        {
            get
            {
                return PluginHost.GameId;
            }
        }

        public HttpServerEventTracker PluginEventTracker { get; protected set; }

        List<INetworkServerDelegate> _delegates;
        INetworkMessageReceiver _receiver;
        UpdateScheduler _updateScheduler;
        UpdateScheduler _trackingsHttpClientScheduler;
        protected NetworkStatsServer _statsServer;
        protected bool _statsServerEnabled;
        ILog _log;
        protected object _timer;
        float _deltaTimeMsAverage;
        const float NAveragedSamples = 1000;
        long _lastTimestamp;
        long _lastUpdateTimestamp;
        protected IFileManager _fileManager;

#if LOAD_TESTS
        // WARNING: this will log A LOT, talk with backend after enabling it.
        const bool SendUpdateTrack = false;
#endif

        const int TimerUpdateInterval = 10;
        const float MaxUpdateDelay = 1.5f;

        static readonly TimeSpan TimeoutFlushingTrackings = TimeSpan.FromSeconds(10);
        static readonly TimeSpan SleepTimeWhileFlushingTrackings = TimeSpan.FromMilliseconds(100);

        const byte FailEventCode = 199;
        const byte EventContentParam = 245;
        const byte MaxPlayersKey = 255;
        const byte IsOpenKey = 253;
        const byte MasterClientIdKey = 248;

        const string BackendBaseUrlConfigKey = "BackendBaseUrl";
        const string ClientCanChangeBackendBaseUrlConfigKey = "ClientCanChangeBackendBaseUrl";

        const string BackendEnvKey = "be_env";
        const string ServerIdRoomProperty = "server";

        const string LoggerNameConfig = "LoggerName";
        const string PluginNameConfig = "PluginName";
        const string AssetsPathConfig = "AssetsPath";
        const string StatsServerEnabled = "StatsServerEnabled";
        const string FullErrorMsg = "Game is full.";
        const string ServerPresentErrorMsg = "This room already has a server.";
        const string ExceptionMetricName = "multiplayer.exception_raised";
        protected const string MetricSendIntervalConfig = "MetricSendInterval";
        protected const string MetricEnvironmentConfig = "MetricEnvironment";

        abstract protected int MaxPlayers { get; }
        abstract protected bool Full { get; }
        abstract protected int UpdateInterval { get; }

        public string BaseBackendUrl { get; private set; }

        protected IUpdateScheduler UpdateScheduler
        {
            get
            {
                return _updateScheduler;
            }
        }

        protected IUpdateScheduler TrackingsHttpClientScheduler
        {
            get
            {
                return _trackingsHttpClientScheduler;
            }
        }

        bool _clientCanChangeBackendBaseUrl;

        protected NetworkServerPlugin(string pluginName)
        {
            _pluginName = pluginName;
            UseStrictMode = true;
            _delegates = new List<INetworkServerDelegate>();
            _updateScheduler = new UpdateScheduler();
            _trackingsHttpClientScheduler = new UpdateScheduler();
        }

        /*
         * to change the configuration values in the local build, edit:
         * deploy/LoadBalancing/GameServer/bin/Photon.LoadBalancing.dll.config
         */
        public override bool SetupInstance(IPluginHost host, Dictionary<string, string> config, out string errorMsg)
        {
            if(!base.SetupInstance(host, config, out errorMsg))
            {
                return false;
            }
            string configStr;
            string assetsPath;
            if(config.TryGetValue(PluginNameConfig, out configStr))
            {
                _pluginName = configStr;
            }
            if(config.TryGetValue(LoggerNameConfig, out configStr))
            {
                _log = LogManager.GetLogger(configStr);
            }
            if(config.TryGetValue(StatsServerEnabled, out configStr))
            {
                _statsServerEnabled = configStr.Equals("true") ? true : false;
                if(_statsServerEnabled)
                {
                    _statsServer = new NetworkStatsServer(this, _updateScheduler);
                }
            }
            if(PluginEventTracker != null)
            {
                PluginEventTracker.UpdateCommonTrackData += (dic => dic.SetValue("ver", AppVersion));
            }
            if(config.TryGetValue(AssetsPathConfig, out configStr))
            {
                assetsPath = configStr;
            }
            else
            {
                assetsPath = Path.GetDirectoryName(GetType().Assembly.Location);
            }
            _fileManager = new FileManagerWrapper(new StandaloneFileManager(),
                Path.Combine(assetsPath, "{0}.bytes"), true);

            SetupBackendUrl(config);

            _lastTimestamp = GetTimestampMilliseconds();
            _lastUpdateTimestamp = _lastTimestamp;

            return true;
        }

        protected void LogDebug(params object[] parms)
        {
            if(_log != null)
            {
                _log.Debug(parms);
            }
            else
            {
                PluginHost.LogDebug(parms);
            }
        }

        protected void LogWarn(params object[] parms)
        {
            if(_log != null)
            {
                _log.Warn(parms);
            }
            else
            {
                PluginHost.LogWarning(parms);
            }
        }

        protected void LogError(params object[] parms)
        {
            if(_log != null)
            {
                _log.Error(parms);
            }
            else
            {
                PluginHost.LogError(parms);
            }
        }

        void SendLog(LogLevel level, string message, AttrDic dic = null)
        {
            if(dic == null)
            {
                dic = new AttrDic();
            }
            dic.SetValue("match_id", PluginHost.GameId);
            var log = new ServerEvents.Log(level, message, dic);
            PluginEventTracker.SendLog(log, level == LogLevel.Error);
        }

        bool CheckServer(ICallInfo info)
        {
            if(Full)
            {
                info.Fail(FullErrorMsg);
                SendLog(LogLevel.Warning, FullErrorMsg);
            }
            else
            {
                info.Continue();
                return true;
            }
            return false;
        }

        byte GetClientId(string userId)
        {
            var actors = PluginHost.GameActors;
            for(var i = 0; i < actors.Count; i++)
            {
                var actor = actors[i];
                if(actor.UserId == userId)
                {
                    return GetClientId(actor.ActorNr);
                }
            }
            return 0;
        }

        byte GetClientId(int actorId)
        {
            return (byte)actorId;
        }

        public override void OnCloseGame(ICloseGameCallInfo info)
        {
            if(_timer != null)
            {
                PluginHost.StopTimer(_timer);
            }
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }
            info.Continue();
            FlushAllPendingTrackings();
        }

        public override void OnCreateGame(ICreateGameCallInfo info)
        {
            if(!CheckServer(info))
            {
                return;
            }

            CheckAndOverwriteBackendEnv(info);

            PluginHost.SetProperties(0, new Hashtable {
                { (int)MaxPlayersKey,MaxPlayers },
                { (int)MasterClientIdKey, 0 },
                { ServerIdRoomProperty, 0 },
            }, null, false);

            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }

            if(_statsServer != null)
            {
                _statsServer.Start();
            }

            SendLog(LogLevel.Info,"OnCreateGame");

            _deltaTimeMsAverage = UpdateInterval;
            var interval = Math.Min(TimerUpdateInterval, UpdateInterval);
            _timer = PluginHost.CreateTimer(TryUpdate, 0, interval);

            var clientId = GetClientId(info.UserId);
            OnClientConnected(clientId);
        }

        protected virtual void OnClientConnected(byte clientId)
        {
            try
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientConnected(clientId);
                }
                OnClientChanged();
            }
            catch(Exception e)
            {
                HandleException(e);
                LogError("OnClientConnected", e);
            }
        }

        public override void BeforeJoin(IBeforeJoinGameCallInfo info)
        {
            CheckServer(info);
        }

        public override void OnJoin(IJoinGameCallInfo info)
        {
            info.Continue();
            OnClientConnected(GetClientId(info.ActorNr));
        }

        public override void OnLeave(ILeaveGameCallInfo info)
        {
            OnClientDisconnected(GetClientId(info.ActorNr));
            info.Continue();
        }

        protected virtual void OnClientDisconnected(byte clientId)
        {
            try
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientDisconnected(clientId);
                }
                OnClientChanged();
            }
            catch(Exception e)
            {
                HandleException(e);
                LogError("OnClientDisconnected", e);
            }
        }

        protected virtual void OnClientChanged()
        {
            PluginHost.SetProperties(0,
                new Hashtable { { (int)IsOpenKey, !Full } }, null, false);
        }

        public override void OnSetProperties(ISetPropertiesCallInfo info)
        {
            if(info.Request.Properties.ContainsKey(ServerIdRoomProperty))
            {
                info.Fail(ServerPresentErrorMsg);
                LogWarn(ServerPresentErrorMsg);
            }
            else
            {
                info.Continue();
            }
        }

        public override void OnRaiseEvent(IRaiseEventCallInfo info)
        {
            info.Continue();

            if(_receiver == null)
            {
                return;
            }
            try
            {   
                var data = info.Request.Data as byte[];
                if(data != null)
                {
                    data = HttpEncoding.Decode(data, HttpEncoding.LZ4);

                    var stream = new MemoryStream(data);
                    var reader = new SystemBinaryReader(stream);
                    var netData = new NetworkMessageData
                    {
                        ClientIds = new List<byte>() { GetClientId(info.ActorNr) },
                        MessageType = info.Request.EvCode
                    };
                    _receiver.OnMessageReceived(netData, reader);
                }
            }
            catch(Exception e)
            {
                HandleException(e);
                var dic = new AttrDic();
                dic.SetValue("detail", e.StackTrace);
                SendLog(LogLevel.Error, e.Message, dic);
            }
        }

        protected static int GetConfigOption(Dictionary<string, string> config, string key, int def)
        {
            string sval;
            if(config.TryGetValue(key, out sval))
            {
                int val;
                if(int.TryParse(sval, out val))
                {
                    return val;
                }
            }
            return def;
        }

        protected static bool GetConfigOption(Dictionary<string, string> config, string key, bool def)
        {
            string sval;
            if(config.TryGetValue(key, out sval))
            {
                bool val;
                if(bool.TryParse(sval, out val))
                {
                    return val;
                }
            }
            return def;
        }

        void TryUpdate()
        {
            try
            {
                // Update delta time.
                long currentTimestamp = GetTimestampMilliseconds();
                long deltaTimeMs = currentTimestamp - _lastTimestamp;
                _lastTimestamp = currentTimestamp;

                long deltaUpdateTimeMs = currentTimestamp - _lastUpdateTimestamp;
                if(deltaUpdateTimeMs >= UpdateInterval)
                {
                    _lastUpdateTimestamp = _lastTimestamp;

                    // Update rolling average.
                    _deltaTimeMsAverage -= _deltaTimeMsAverage / NAveragedSamples;
                    _deltaTimeMsAverage += deltaUpdateTimeMs / NAveragedSamples;

                    var updateLogDic = new AttrDic();
                    updateLogDic.SetValue("match_id", PluginHost.GameId);
                    updateLogDic.SetValue("timestamp", currentTimestamp);
                    updateLogDic.SetValue("deltatime", deltaUpdateTimeMs);

#if LOAD_TESTS
                    if (SendUpdateTrack)
                    {
                        var updateLog = new ServerEvents.Log(LogLevel.Notice, "update_server", updateLogDic);
                        PluginEventTracker.SendLog(updateLog);
                    }
#endif

                    // Track when server update method takes much more time than expected.
                    if(deltaUpdateTimeMs >= _deltaTimeMsAverage * MaxUpdateDelay)
                    {
                        // NOTE: LogLevel is notice because backend has raised it to avoid logging debug/info stuff.
                        var slowUpdateLog = new ServerEvents.Log(LogLevel.Notice, "slow_update_server", updateLogDic);
                        PluginEventTracker.SendLog(slowUpdateLog);
                    }

                    // Perform update method passing it delta time in seconds.
                    float dt = deltaUpdateTimeMs * 0.001f;
                    Update(dt);
                }
            }
            catch(Exception e)
            {
                LogError("Update", e);
                HandleException(e);
            }
        }

        protected virtual void Update(float dt)
        {
            _updateScheduler.Update(dt, dt);
            _trackingsHttpClientScheduler.Update(dt, dt);
        }

        void BroadcastError(Error err)
        {
            PluginEventTracker.SendMetric(new Metric(MetricType.Counter, ExceptionMetricName, 1));
            var context = new AttrDic();
            if(err.Detail != string.Empty)
            {
                context.SetValue("detail", err.Detail);
            }
            SendLog(LogLevel.Error, err.Msg, context);
            var dic = new Dictionary<byte, object>();
            dic.Add(EventContentParam, err.ToString());
            BroadcastEvent(FailEventCode, dic);
            PluginHost.LogError(err.ToString());
        }

        protected void HandleException(Exception e)
        {
            BroadcastError(new Error(e.Message, e.StackTrace));
        }

        void INetworkServer.Start()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }
        }

        void INetworkServer.Stop()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }
            SendLog(LogLevel.Debug, "Stop");
            BroadcastError(new Error("server stopped"));
        }

        public void Fail(Error err)
        {
            BroadcastError(err);
        }

        INetworkMessage INetworkMessageSender.CreateMessage(NetworkMessageData info)
        {
            List<int> actors = null;
            if(info.ClientIds != null)
            {
                DebugUtils.Assert(info.ClientIds.Count > 0);
                actors = new List<int>();
                for(int i = 0; i < info.ClientIds.Count; ++i)
                {
                    actors.Add(info.ClientIds[i]);
                }
            }
            return new PluginNetworkMessage(PluginHost, info.MessageType, info.Unreliable, actors);
        }

        void INetworkServer.AddDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        void INetworkServer.RemoveDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        void INetworkServer.RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        int INetworkServer.GetTimestamp()
        {
            return System.Environment.TickCount;
        }

        long GetTimestampMilliseconds()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public bool LatencySupported
        {
            get
            {
                return false;
            }
        }

        protected object CreateInstanceFromAssembly(string assemblyName, string typeName)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(dir, assemblyName);
            var gameType = Assembly.LoadFile(path).GetType(typeName);
            return Activator.CreateInstance(gameType);
        }

        void SetupBackendUrl(Dictionary<string, string> config)
        {
            _clientCanChangeBackendBaseUrl = GetConfigOption(config, ClientCanChangeBackendBaseUrlConfigKey, 0) != 0;

            string baseUrl;
            if(config.TryGetValue(BackendBaseUrlConfigKey, out baseUrl))
            {
                BaseBackendUrl = baseUrl;
            }
        }

        void CheckAndOverwriteBackendEnv(ICreateGameCallInfo info)
        {
            if(_clientCanChangeBackendBaseUrl && info.Request.GameProperties != null && info.Request.GameProperties.ContainsKey(BackendEnvKey))
            {
                object url = info.Request.GameProperties[BackendEnvKey];
                if(url is string)
                {
                    BaseBackendUrl = url as string;
                }
            }
        }

        void FlushAllPendingTrackings()
        {
            var startingTime = DateTime.Now;

            // Update to send requests to HTTP client.
            PluginEventTracker.Update();
            
            // First update in case of using immediate scheduler for trackings. In this case the PluginEventTracker.HasPendingData() will return false.
            _trackingsHttpClientScheduler.Update(0.0f, 0.0f);
            while(PluginEventTracker.HasPendingData)
            {
                _trackingsHttpClientScheduler.Update(0.0f, 0.0f);
                System.Threading.Thread.Sleep(SleepTimeWhileFlushingTrackings);

                if(DateTime.Now - startingTime > TimeoutFlushingTrackings)
                {
                    break;
                }
            }
        }
    }
}
